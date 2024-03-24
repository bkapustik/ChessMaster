using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Threading;
using ProtoBuf;

namespace MemoryMappedCollections
{
    public class SharedMemoryQueue<T> : IDisposable where T : struct
    {
        private string FileName;
        private string MutexName;

        private bool UseSerializer = false;

        public int Size { get; private set; }

        private int FrontLocation, RearLocation, CountLocation, RecordSize, MetaDataSize, ArraySizeLocation, MemorySize;

        private Mutex FileMutex;
        private MemoryMappedFile SharedFile;

        public SharedMemoryQueue(int size, string fileName, string mutexName, bool useSerializer = false)
        {
            Size = size;
            FrontLocation = 0;
            RearLocation = sizeof(Int32);
            CountLocation = sizeof(Int32) * 2;
            RecordSize = Marshal.SizeOf(typeof(T));
            MetaDataSize = sizeof(Int32) * 3;
            MemorySize = MetaDataSize + (Size * RecordSize);
            FileName = fileName;
            MutexName = mutexName;
            UseSerializer = useSerializer;

            if (!Mutex.TryOpenExisting(MutexName, out var fileMutex))
            {
                FileMutex = new Mutex(false, MutexName);
            }
            else
            {
                FileMutex = fileMutex;
            }

            SharedFile = MemoryMappedFile.CreateOrOpen(FileName, MemorySize);
        }

        /// <summary>
        /// Size is capped at 1
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="mutexName"></param>
        /// <param name="memorySize"></param>
        /// <param name="useSerializer"></param>
        public SharedMemoryQueue(string fileName, string mutexName, int memorySize, bool useSerializer)
        {
            Size = 1;
            FrontLocation = 0;
            RearLocation = sizeof(Int32);
            CountLocation = sizeof(Int32) * 2;
            ArraySizeLocation = sizeof(Int32) * 3;
            MetaDataSize = sizeof(Int32) * 4 + sizeof(long);
            MemorySize = MetaDataSize + memorySize;
            FileName = fileName;
            MutexName = mutexName;

            if (!Mutex.TryOpenExisting(MutexName, out var fileMutex))
            {
                FileMutex = new Mutex(false, MutexName);
            }
            else
            {
                FileMutex = fileMutex;
            }

            SharedFile = MemoryMappedFile.CreateOrOpen(FileName, MemorySize);

            UseSerializer = useSerializer;
        }

        public int GetCount()
        {
            int count;
            using (var accessor = SharedFile.CreateViewAccessor(0, MemorySize))
            {
                count = accessor.ReadInt32(CountLocation);
            }
            return count;
        }

        public void Dispose()
        {
            SharedFile.Dispose();
        }

        public bool TryEnqueue(ref T value)
        {
            FileMutex.WaitOne();
            int front, rear, currentCount;
            using (var accessor = SharedFile.CreateViewAccessor(0, MemorySize))
            {
                front = accessor.ReadInt32(0);
                rear = accessor.ReadInt32(RearLocation);
                currentCount = accessor.ReadInt32(CountLocation);

                if (currentCount == Size)
                {
                    FileMutex.ReleaseMutex();
                    return false;
                }

                int newRecordLocation;
                if (front == -1)
                {
                    accessor.Write(FrontLocation, 0);
                    rear = 0;
                    newRecordLocation = 0;
                }

                else if (rear == Size - 1 && front != 0)
                {
                    rear = 0;
                    newRecordLocation = 0;
                }

                else
                {
                    rear++;
                    newRecordLocation = rear;
                }

                accessor.Write(RearLocation, rear);

                if (UseSerializer)
                {
                    byte[] serializedData;
                    using (var ms = new MemoryStream())
                    {
                        Serializer.Serialize(ms, value);
                        serializedData = ms.ToArray();
                    }
                    accessor.WriteArray(MetaDataSize, serializedData, 0, serializedData.Length);

                    accessor.Write(ArraySizeLocation, serializedData.Length);
                }
                else
                {
                    accessor.Write(MetaDataSize + (RecordSize * newRecordLocation), ref value);
                }

                accessor.Write(CountLocation, currentCount + 1);
            }
            FileMutex.ReleaseMutex();
            return true;
        }

        public bool TryDequeue(out T value)
        {
            FileMutex.WaitOne();
            int front, rear, currentCount;
            using (var accessor = SharedFile.CreateViewAccessor(0, MemorySize))
            {
                front = accessor.ReadInt32(0);
                rear = accessor.ReadInt32(RearLocation);
                currentCount = accessor.ReadInt32(CountLocation);

                if (front == -1)
                {
                    value = default;
                    FileMutex.ReleaseMutex();
                    return false;
                }

                if (UseSerializer)
                {
                    accessor.Read(ArraySizeLocation, out int arraySize);
                    byte[] serializedData = new byte[arraySize];
                    accessor.ReadArray<byte>(MetaDataSize, serializedData, 0, arraySize);

                    using (var ms = new MemoryStream(serializedData))
                    {
                        value = Serializer.Deserialize<T>(ms);
                    }
                }
                else
                {
                    accessor.Read(MetaDataSize + (front * RecordSize), out value);
                }

                if (front == rear)
                {
                    front = -1;
                    rear = -1;
                }

                else if (front == Size - 1)
                {
                    front = 0;
                }
                else
                {
                    front = front + 1;
                }

                accessor.Write(FrontLocation, front);
                accessor.Write(RearLocation, rear);
                accessor.Write(CountLocation, currentCount - 1);

                FileMutex.ReleaseMutex();
                return true;
            }
        }
    }
}
