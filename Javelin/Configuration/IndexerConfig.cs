namespace Javelin.Configuration {
    
    /// <summary>
    /// Determines the mechanism for flushing an index segment to disk
    /// </summary>
    public enum SegmentFlushStrategy {
        AllocatedMemory,
        PostingsCount
    }
    
    /// <summary>
    /// Basic configuration for an indexeer
    /// </summary>
    public class IndexerConfig {
        public long SegmentSizeBytes { get; set; }
        public SegmentFlushStrategy SEGMENT_FLUSH_STRATEGY = SegmentFlushStrategy.AllocatedMemory;
        public int MAX_POSTING_COUNT_PER_SEGMENT = 10_000;
        public long MAX_SIZE_BYTES_PER_SEGMENT = 60 * 1024 * 1024;
    }
}