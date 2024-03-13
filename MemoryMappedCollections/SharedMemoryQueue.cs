using System;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Threading;

namespace MemoryMappedCollections
{
    public class SharedMemoryQueue<T> : IDisposable where T : struct
    {
        private string FileName;
        private string MutexName;

        public int Size { get; private set; }

        private int FrontLocation, RearLocation, CountLocation, RecordSize, MetaDataSize, MemorySize;

        private Mutex FileMutex;
        private MemoryMappedFile SharedFile;

        public SharedMemoryQueue(int size, string fileName, string mutexName)
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

        public int GetCount()
        {
            int count;
            using (var accessor = SharedFile.CreateViewAccessor(CountLocation, sizeof(Int32)))
            {
                count = accessor.ReadInt32(0);
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

                if ((front == 0 && rear == Size - 1) ||
                 (rear == (front - 1) % (Size - 1)))
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
                accessor.Write(MetaDataSize + (RecordSize * newRecordLocation), ref value);
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

                accessor.Read(MetaDataSize + (front * RecordSize), out value);

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
