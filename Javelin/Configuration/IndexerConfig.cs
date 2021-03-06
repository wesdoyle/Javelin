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
        public SegmentFlushStrategy SEGMENT_FLUSH_STRATEGY = SegmentFlushStrategy.PostingsCount;
        public int MAX_POSTING_COUNT_PER_SEGMENT = 8_000;
        public int MAX_DOCS_PER_POSTING_LIST = 8_000;
        public long MAX_SIZE_BYTES_PER_SEGMENT = 60 * 1024 * 1024;
        public string SEGMENT_DIRECTORY = "/Users/wes/.javelin/index";
        public string SEGMENT_PREFIX = "segment_";
        public string MERGED_SEGMENT_PREFIX = "merged_segment_";
    }
}