namespace Hoba
{
    using System;
    using System.IO;
    using System.Collections.Generic;
    using System.Threading;

    public class RecyclableMemoryStreamManager
    {
        /// <summary>
        /// Generic delegate for handling events without any arguments.
        /// </summary>
        public delegate void EventHandler();

        /// <summary>
        /// Delegate for handling large buffer discard reports.
        /// </summary>
        /// <param name="reason">Reason the buffer was discarded.</param>
        //public delegate void LargeBufferDiscardedEventHandler(Events.MemoryStreamDiscardReason reason);

        /// <summary>
        /// Delegate for handling reports of stream size when streams are allocated
        /// </summary>
        /// <param name="bytes">Bytes allocated.</param>
        public delegate void StreamLengthReportHandler(long bytes);

        /// <summary>
        /// Delegate for handling periodic reporting of memory use statistics.
        /// </summary>
        /// <param name="smallPoolInUseBytes">Bytes currently in use in the small pool.</param>
        /// <param name="smallPoolFreeBytes">Bytes currently free in the small pool.</param>
        /// <param name="largePoolInUseBytes">Bytes currently in use in the large pool.</param>
        /// <param name="largePoolFreeBytes">Bytes currently free in the large pool.</param>
        public delegate void UsageReportEventHandler(
            long smallPoolInUseBytes, long smallPoolFreeBytes, long largePoolInUseBytes, long largePoolFreeBytes);

        public const int DefaultBlockSize = 128 * 1024;
        public const int DefaultLargeBufferMultiple = 1024 * 1024;
        public const int DefaultMaximumBufferSize = 128 * 1024 * 1024;

        private readonly int blockSize;
        private readonly long[] largeBufferFreeSize;
        private readonly long[] largeBufferInUseSize;

        private readonly int largeBufferMultiple;

        /// <summary>
        /// pools[0] = 1x largeBufferMultiple buffers
        /// pools[1] = 2x largeBufferMultiple buffers
        /// pools[2] = 3x(multiple)/4x(exponential) largeBufferMultiple buffers
        /// etc., up to maximumBufferSize
        /// </summary>
        private readonly Stack<byte[]>[] largePools;

        private readonly int maximumBufferSize;
        private readonly bool useExponentialLargeBuffer;

        private readonly Stack<byte[]> smallPool;

        private long smallPoolFreeSize;
        private long smallPoolInUseSize;

        /// <summary>
        /// Initializes the memory manager with the default block/buffer specifications.
        /// </summary>
        public RecyclableMemoryStreamManager()
            : this(DefaultBlockSize, DefaultLargeBufferMultiple, DefaultMaximumBufferSize, false) { }

        /// <summary>
        /// Initializes the memory manager with the given block requiredSize.
        /// </summary>
        /// <param name="blockSize">Size of each block that is pooled. Must be > 0.</param>
        /// <param name="largeBufferMultiple">Each large buffer will be a multiple of this value.</param>
        /// <param name="maximumBufferSize">Buffers larger than this are not pooled</param>
        /// <exception cref="ArgumentOutOfRangeException">blockSize is not a positive number, or largeBufferMultiple is not a positive number, or maximumBufferSize is less than blockSize.</exception>
        /// <exception cref="ArgumentException">maximumBufferSize is not a multiple of largeBufferMultiple</exception>
        public RecyclableMemoryStreamManager(int blockSize, int largeBufferMultiple, int maximumBufferSize)
            : this(blockSize, largeBufferMultiple, maximumBufferSize, false) { }

        /// <summary>
        /// Initializes the memory manager with the given block requiredSize.
        /// </summary>
        /// <param name="blockSize">Size of each block that is pooled. Must be > 0.</param>
        /// <param name="largeBufferMultiple">Each large buffer will be a multiple/exponential of this value.</param>
        /// <param name="maximumBufferSize">Buffers larger than this are not pooled</param>
        /// <param name="useExponentialLargeBuffer">Switch to exponential large buffer allocation strategy</param>
        /// <exception cref="ArgumentOutOfRangeException">blockSize is not a positive number, or largeBufferMultiple is not a positive number, or maximumBufferSize is less than blockSize.</exception>
        /// <exception cref="ArgumentException">maximumBufferSize is not a multiple/exponential of largeBufferMultiple</exception>
        public RecyclableMemoryStreamManager(int blockSize, int largeBufferMultiple, int maximumBufferSize, bool useExponentialLargeBuffer)
        {
            if (blockSize <= 0)
            {
                throw new ArgumentOutOfRangeException("blockSize", "blockSize must be a positive number");
            }

            if (largeBufferMultiple <= 0)
            {
                throw new ArgumentOutOfRangeException("largeBufferMultiple", "largeBufferMultiple must be a positive number");
            }

            if (maximumBufferSize < blockSize)
            {
                throw new ArgumentOutOfRangeException("maximumBufferSize", "maximumBufferSize must be at least blockSize");
            }

            this.blockSize = blockSize;
            this.largeBufferMultiple = largeBufferMultiple;
            this.maximumBufferSize = maximumBufferSize;
            this.useExponentialLargeBuffer = useExponentialLargeBuffer;

            if (!this.IsLargeBufferSize(maximumBufferSize))
            {
                throw new ArgumentException(String.Format("maximumBufferSize is not {0} of largeBufferMultiple",
                                                          useExponentialLargeBuffer ? "an exponential" : "a multiple"),
                                            "maximumBufferSize");
            }

            smallPool = new Stack<byte[]>();
            var numLargePools = useExponentialLargeBuffer
                                    ? ((int)Math.Log(maximumBufferSize / largeBufferMultiple, 2) + 1)
                                    : (maximumBufferSize / largeBufferMultiple);

            // +1 to store size of bytes in use that are too large to be pooled
            largeBufferInUseSize = new long[numLargePools + 1];
            largeBufferFreeSize = new long[numLargePools];

            largePools = new Stack<byte[]>[numLargePools];

            for (var i = 0; i < largePools.Length; ++i)
            {
                largePools[i] = new Stack<byte[]>();
            }

            //Events.Writer.MemoryStreamManagerInitialized(blockSize, largeBufferMultiple, maximumBufferSize);
        }

        /// <summary>
        /// The size of each block. It must be set at creation and cannot be changed.
        /// </summary>
        public int BlockSize
        {
            get { return blockSize; }
        }

        /// <summary>
        /// All buffers are multiples/exponentials of this number. It must be set at creation and cannot be changed.
        /// </summary>
        public int LargeBufferMultiple
        {
            get { return largeBufferMultiple; }
        }

        /// <summary>
        /// Number of bytes in small pool not currently in use
        /// </summary>
        public long SmallPoolFreeSize
        {
            get { return smallPoolFreeSize; }
        }

        /// <summary>
        /// Number of bytes currently in use by stream from the small pool
        /// </summary>
        public long SmallPoolInUseSize
        {
            get { return smallPoolInUseSize; }
        }

        /// <summary>
        /// Number of bytes in large pool not currently in use
        /// </summary>
        public long LargePoolFreeSize
        {
            get
            {
                long sum = 0;
                foreach (long freeSize in largeBufferFreeSize)
                {
                    sum += freeSize;
                }

                return sum;
            }
        }

        /// <summary>
        /// Number of bytes currently in use by streams from the large pool
        /// </summary>
        public long LargePoolInUseSize
        {
            get
            {
                long sum = 0;
                foreach (long inUseSize in largeBufferInUseSize)
                {
                    sum += inUseSize;
                }

                return sum;
            }
        }

        /// <summary>
        /// How many blocks are in the small pool
        /// </summary>
        public long SmallBlocksFree
        {
            get { return smallPool.Count; }
        }

        /// <summary>
        /// How many buffers are in the large pool
        /// </summary>
        public long LargeBuffersFree
        {
            get
            {
                long free = 0;
                foreach (var pool in this.largePools)
                {
                    free += pool.Count;
                }
                return free;
            }
        }

        /// <summary>
        /// How many bytes of small free blocks to allow before we start dropping
        /// those returned to us.
        /// </summary>
        public long MaximumFreeSmallPoolBytes { get; set; }

        /// <summary>
        /// How many bytes of large free buffers to allow before we start dropping
        /// those returned to us.
        /// </summary>
        public long MaximumFreeLargePoolBytes { get; set; }

        /// <summary>
        /// Maximum stream capacity in bytes. Attempts to set a larger capacity will
        /// result in an exception.
        /// </summary>
        /// <remarks>A value of 0 indicates no limit.</remarks>
        public long MaximumStreamCapacity { get; set; }

        /// <summary>
        /// Whether to save callstacks for stream allocations. This can help in debugging.
        /// It should NEVER be turned on generally in production.
        /// </summary>
        public bool GenerateCallStacks { get; set; }

        /// <summary>
        /// Whether dirty buffers can be immediately returned to the buffer pool. E.g. when GetBuffer() is called on
        /// a stream and creates a single large buffer, if this setting is enabled, the other blocks will be returned
        /// to the buffer pool immediately.
        /// Note when enabling this setting that the user is responsible for ensuring that any buffer previously
        /// retrieved from a stream which is subsequently modified is not used after modification (as it may no longer
        /// be valid).
        /// </summary>
        public bool AggressiveBufferReturn { get; set; }

        /// <summary>
        /// Removes and returns a single block from the pool.
        /// </summary>
        /// <returns>A byte[] array</returns>
        internal byte[] GetBlock()
        {
            byte[] block;
            if (smallPool.Count > 0)
            {
                block = smallPool.Pop();
                Interlocked.Add(ref smallPoolFreeSize, -BlockSize);
            }
            else
            {
                block = new byte[BlockSize];
            }

            Interlocked.Add(ref smallPoolInUseSize, BlockSize);
            return block;
        }

        /// <summary>
        /// Returns a buffer of arbitrary size from the large buffer pool. This buffer
        /// will be at least the requiredSize and always be a multiple/exponential of largeBufferMultiple.
        /// </summary>
        /// <param name="requiredSize">The minimum length of the buffer</param>
        /// <param name="tag">The tag of the stream returning this buffer, for logging if necessary.</param>
        /// <returns>A buffer of at least the required size.</returns>
        internal byte[] GetLargeBuffer(int requiredSize, string tag)
        {
            requiredSize = RoundToLargeBufferSize(requiredSize);

            var poolIndex = GetPoolIndex(requiredSize);

            byte[] buffer;
            if (poolIndex < largePools.Length)
            {
                if (largePools[poolIndex].Count > 0)
                {
                    buffer = largePools[poolIndex].Pop();
                    Interlocked.Add(ref largeBufferFreeSize[poolIndex], -buffer.Length);
                }
                else
                {
                    buffer = new byte[requiredSize];
                }
            }
            else
            {
                // Buffer is too large to pool. They get a new buffer.

                // We still want to track the size, though, and we've reserved a slot
                // in the end of the inuse array for nonpooled bytes in use.
                poolIndex = largeBufferInUseSize.Length - 1;

                // We still want to round up to reduce heap fragmentation.
                buffer = new byte[requiredSize];
            }

            Interlocked.Add(ref largeBufferInUseSize[poolIndex], buffer.Length);

            return buffer;
        }

        private int RoundToLargeBufferSize(int requiredSize)
        {
            if (useExponentialLargeBuffer)
            {
                int pow = 1;
                while (largeBufferMultiple * pow < requiredSize)
                {
                    pow <<= 1;
                }
                return largeBufferMultiple * pow;
            }
            else
            {
                return ((requiredSize + LargeBufferMultiple - 1) / LargeBufferMultiple) * LargeBufferMultiple;
            }
        }

        private bool IsLargeBufferSize(int value)
        {
            return (value != 0) && (useExponentialLargeBuffer
                                        ? (value == RoundToLargeBufferSize(value))
                                        : (value % LargeBufferMultiple) == 0);
        }

        private int GetPoolIndex(int length)
        {
            if (this.useExponentialLargeBuffer)
            {
                int index = 0;
                while ((this.largeBufferMultiple << index) < length)
                {
                    ++index;
                }
                return index;
            }
            else
            {
                return length / this.largeBufferMultiple - 1;
            }
        }

        /// <summary>
        /// Returns the buffer to the large pool
        /// </summary>
        /// <param name="buffer">The buffer to return.</param>
        /// <param name="tag">The tag of the stream returning this buffer, for logging if necessary.</param>
        /// <exception cref="ArgumentNullException">buffer is null</exception>
        /// <exception cref="ArgumentException">buffer.Length is not a multiple/exponential of LargeBufferMultiple (it did not originate from this pool)</exception>
        internal void ReturnLargeBuffer(byte[] buffer, string tag)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }

            if (!IsLargeBufferSize(buffer.Length))
            {
                throw new ArgumentException(
                    String.Format("buffer did not originate from this memory manager. The size is not {0} of ",
                                  useExponentialLargeBuffer ? "an exponential" : "a multiple") +
                    largeBufferMultiple);
            }

            var poolIndex = GetPoolIndex(buffer.Length);

            if (poolIndex < largePools.Length)
            {
                if ((largePools[poolIndex].Count + 1) * buffer.Length <= MaximumFreeLargePoolBytes ||
                    MaximumFreeLargePoolBytes == 0)
                {
                    largePools[poolIndex].Push(buffer);
                    System.Threading.Interlocked.Add(ref largeBufferFreeSize[poolIndex], buffer.Length);
                }
                else
                {
                    
                }
            }
            else
            {
                // This is a non-poolable buffer, but we still want to track its size for inuse
                // analysis. We have space in the inuse array for this.
                poolIndex = largeBufferInUseSize.Length - 1;
            }

            Interlocked.Add(ref this.largeBufferInUseSize[poolIndex], -buffer.Length);
        }

        /// <summary>
        /// Returns the blocks to the pool
        /// </summary>
        /// <param name="blocks">Collection of blocks to return to the pool</param>
        /// <param name="tag">The tag of the stream returning these blocks, for logging if necessary.</param>
        /// <exception cref="ArgumentNullException">blocks is null</exception>
        /// <exception cref="ArgumentException">blocks contains buffers that are the wrong size (or null) for this memory manager</exception>
        internal void ReturnBlocks(ICollection<byte[]> blocks, string tag)
        {
            if (blocks == null)
            {
                throw new ArgumentNullException("blocks");
            }

            var bytesToReturn = blocks.Count * BlockSize;
            Interlocked.Add(ref smallPoolInUseSize, -bytesToReturn);

            foreach (var block in blocks)
            {
                if (block == null || block.Length != BlockSize)
                {
                    throw new ArgumentException("blocks contains buffers that are not BlockSize in length");
                }
            }

            foreach (var block in blocks)
            {
                if (MaximumFreeSmallPoolBytes == 0 || SmallPoolFreeSize < MaximumFreeSmallPoolBytes)
                {
                    Interlocked.Add(ref smallPoolFreeSize, BlockSize);
                    smallPool.Push(block);
                }
                else
                {
                    //ReportBlockDiscarded();
                    break;
                }
            }
        }

        /// <summary>
        /// Retrieve a new MemoryStream object with no tag and a default initial capacity.
        /// </summary>
        /// <returns>A MemoryStream.</returns>
        public MemoryStream GetStream()
        {
            return new RecyclableMemoryStream(this);
        }

        /// <summary>
        /// Retrieve a new MemoryStream object with the given tag and a default initial capacity.
        /// </summary>
        /// <param name="tag">A tag which can be used to track the source of the stream.</param>
        /// <returns>A MemoryStream.</returns>
        public MemoryStream GetStream(string tag)
        {
            return new RecyclableMemoryStream(this, tag);
        }

        /// <summary>
        /// Retrieve a new MemoryStream object with the given tag and at least the given capacity.
        /// </summary>
        /// <param name="tag">A tag which can be used to track the source of the stream.</param>
        /// <param name="requiredSize">The minimum desired capacity for the stream.</param>
        /// <returns>A MemoryStream.</returns>
        public MemoryStream GetStream(string tag, int requiredSize)
        {
            return new RecyclableMemoryStream(this, tag, requiredSize);
        }

        /// <summary>
        /// Retrieve a new MemoryStream object with the given tag and at least the given capacity, possibly using
        /// a single continugous underlying buffer.
        /// </summary>
        /// <remarks>Retrieving a MemoryStream which provides a single contiguous buffer can be useful in situations
        /// where the initial size is known and it is desirable to avoid copying data between the smaller underlying
        /// buffers to a single large one. This is most helpful when you know that you will always call GetBuffer
        /// on the underlying stream.</remarks>
        /// <param name="tag">A tag which can be used to track the source of the stream.</param>
        /// <param name="requiredSize">The minimum desired capacity for the stream.</param>
        /// <param name="asContiguousBuffer">Whether to attempt to use a single contiguous buffer.</param>
        /// <returns>A MemoryStream.</returns>
        public MemoryStream GetStream(string tag, int requiredSize, bool asContiguousBuffer)
        {
            if (!asContiguousBuffer || requiredSize <= BlockSize)
            {
                return GetStream(tag, requiredSize);
            }

            return new RecyclableMemoryStream(this, tag, requiredSize, GetLargeBuffer(requiredSize, tag));
        }

        /// <summary>
        /// Retrieve a new MemoryStream object with the given tag and with contents copied from the provided
        /// buffer. The provided buffer is not wrapped or used after construction.
        /// </summary>
        /// <remarks>The new stream's position is set to the beginning of the stream when returned.</remarks>
        /// <param name="tag">A tag which can be used to track the source of the stream.</param>
        /// <param name="buffer">The byte buffer to copy data from.</param>
        /// <param name="offset">The offset from the start of the buffer to copy from.</param>
        /// <param name="count">The number of bytes to copy from the buffer.</param>
        /// <returns>A MemoryStream.</returns>
        public MemoryStream GetStream(string tag, byte[] buffer, int offset, int count)
        {
            RecyclableMemoryStream stream = null;
            try
            {
                stream = new RecyclableMemoryStream(this, tag, count);
                stream.Write(buffer, offset, count);
                stream.Position = 0;
                return stream;
            }
            catch
            {
                if(stream != null)
                    stream.Dispose();

                return null;
            }
        }
    }
}