using System.IO.MemoryMappedFiles;
using System.Threading;
using System.Runtime.InteropServices;
using ZeroFormatter;
using System.Threading.Tasks;

namespace MemoryMappedCollections
{
    public struct RecordMetadata
    { 
        public bool IsFull { get; set; }
        public int RecordArraySize { get; set; }
    }
    public class SharedMemorySerializedMultiBuffer<T> where T : struct
    {
        private short NumberOfRecords;
        
        private Mutex[] Mutexes;
        
        private int MetaDataSize, MemorySize;
        
        private MemoryMappedFile SharedFile;
        
        private int SingleRecordMetadataSize;

        private int MaxMemoryPerRecord;

        private short LastReadLocation;
        private short LastWriteLocation;

        private const int mutexWaitTime = 10;

        private int TasksPerReadWrite;

        /// <summary>
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="mutexName"></param>
        /// <param name="maxMemoryPerRecord"></param>
        /// <param name="useSerializer"></param>
        public SharedMemorySerializedMultiBuffer(string fileName, string mutexName, int maxMemoryPerRecord, short numberOfRecords, short tasksPerReadWrite)
        {
            NumberOfRecords = numberOfRecords;

            TasksPerReadWrite = tasksPerReadWrite;

            //2 ints because of the previous record the reader or writer interacted with

            SingleRecordMetadataSize = Marshal.SizeOf<RecordMetadata>();

            MetaDataSize = numberOfRecords * SingleRecordMetadataSize;
            MemorySize = MetaDataSize + maxMemoryPerRecord * numberOfRecords;
            MaxMemoryPerRecord = maxMemoryPerRecord;

            Mutexes = new Mutex[NumberOfRecords];

            for (int i = 0; i < numberOfRecords; i++)
            {
                string currentMutexName = $"{mutexName}_{i}";
                if (!Mutex.TryOpenExisting(currentMutexName, out var mutex))
                {
                    Mutexes[i] = new Mutex(false, currentMutexName);
                }
                else
                {
                    Mutexes[i] = mutex;
                }
            }

            SharedFile = MemoryMappedFile.CreateOrOpen(fileName, MemorySize);
        }

        private static int[] PartitionInteger(int total, int n)
        {
            int[] parts = new int[n];
            int baseValue = total / n;
            int remainder = total % n;

            for (int i = 0; i < n; i++)
            {
                parts[i] = baseValue + (i < remainder ? 1 : 0);
            }

            return parts;
        }

        private void AddOneInternal(ref T value, RecordMetadata recordMetaData, short recordIndex)
        {
            var serializedData = ZeroFormatterSerializer.Serialize(value);

            var writers = new Task[TasksPerReadWrite];
            var partitions = PartitionInteger(serializedData.Length, TasksPerReadWrite);
            int previousPartitionsTotalSize = 0;

            for (int i = 0; i < TasksPerReadWrite; i++)
            {
                int localPartitionTotalSize = previousPartitionsTotalSize;
                int localIndex = i;
                int localRecordIndex = recordIndex;
                int partitionSize = partitions[localIndex];
                var task = Task.Run(() =>
                {
                    using (var accessor = SharedFile.CreateViewAccessor(MetaDataSize + recordIndex * MaxMemoryPerRecord + localPartitionTotalSize, partitionSize))
                    {
                        accessor.WriteArray(0, serializedData, localPartitionTotalSize, partitionSize);
                    }
                });

                writers[localIndex] = task;

                previousPartitionsTotalSize += partitionSize;
            }

            recordMetaData.RecordArraySize = serializedData.Length;
            recordMetaData.IsFull = true;

            Task.WhenAll(writers).Wait();

            using (var accessor = SharedFile.CreateViewAccessor(SingleRecordMetadataSize * recordIndex, SingleRecordMetadataSize))
            {
                accessor.Write(0, ref recordMetaData);
            }

            Mutexes[recordIndex].ReleaseMutex();
        }

        private void TakeOneInternal(out T value, RecordMetadata recordMetadata, short recordIndex)
        {
            byte[] serializedData = new byte[recordMetadata.RecordArraySize];

            var readers = new Task[TasksPerReadWrite];
            var partitions = PartitionInteger(serializedData.Length, TasksPerReadWrite);
            int previousPartitionsTotalSize = 0;

            for (int i = 0; i < TasksPerReadWrite; i++)
            {
                int localPartitionTotalSize = previousPartitionsTotalSize;
                int localIndex = i;
                int localRecordIndex = recordIndex;
                int partitionSize = partitions[localIndex];
                var task = Task.Run(() =>
                {
                    using (var accessor = SharedFile.CreateViewAccessor(MetaDataSize + (localRecordIndex * MaxMemoryPerRecord) + localPartitionTotalSize, partitionSize))
                    {
                        accessor.ReadArray(0, serializedData, localPartitionTotalSize, partitionSize);
                    }
                });

                readers[localIndex] = task;

                previousPartitionsTotalSize += partitionSize;
            }

            Task.WhenAll(readers).Wait();

            value = ZeroFormatterSerializer.Deserialize<T>(serializedData);

            recordMetadata.IsFull = false;

            using (var accessor = SharedFile.CreateViewAccessor(recordIndex * SingleRecordMetadataSize, SingleRecordMetadataSize))
            {
                accessor.Write(0, ref recordMetadata);
            }

            Mutexes[recordIndex].ReleaseMutex();
        }

        public void AddOne(ref T value)
        {
            var accessor = SharedFile.CreateViewAccessor(0, MetaDataSize);

            for (short i = 0; i < NumberOfRecords; i++)
            {
                if (Mutexes[i].WaitOne(mutexWaitTime))
                {
                    accessor.Read(i * SingleRecordMetadataSize, out RecordMetadata recordMetaData);
                    if (!recordMetaData.IsFull)
                    {
                        accessor.Dispose();
                        AddOneInternal(ref value, recordMetaData, i);
                        return;
                    }
                    else
                    {
                        Mutexes[i].ReleaseMutex();
                    }
                }
            }

            while (true)
            {
                LastWriteLocation = (short)((LastWriteLocation + 1) % NumberOfRecords);

                if (Mutexes[LastWriteLocation].WaitOne(mutexWaitTime))
                {
                    accessor.Read(LastWriteLocation * SingleRecordMetadataSize, out RecordMetadata recordMetaData);
                    accessor.Dispose();
                    AddOneInternal(ref value, recordMetaData, LastWriteLocation);
                    break;
                }
            }
        }

        public void TakeOne(out T value)
        {
            var accessor = SharedFile.CreateViewAccessor(0, MetaDataSize);

            while (true)
            {
                LastReadLocation = (short)((LastReadLocation + 1) % NumberOfRecords);

                if (Mutexes[LastReadLocation].WaitOne(mutexWaitTime))
                {
                    accessor.Read(LastReadLocation * SingleRecordMetadataSize, out RecordMetadata recordMetaData);

                    if (recordMetaData.IsFull)
                    {
                        accessor.Dispose();
                        TakeOneInternal(out value, recordMetaData, LastReadLocation);
                        break;
                    }
                    else
                    {
                        Mutexes[LastReadLocation].ReleaseMutex();
                    }
                }
            }
        }
    }
}
