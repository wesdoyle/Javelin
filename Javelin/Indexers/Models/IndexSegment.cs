using System;
using System.Collections.Generic;

namespace Javelin.Indexers.Models {
    /// <summary>
    /// Represents an inverted index for a segment of a corpus
    /// </summary>
    [Serializable]
    public class IndexSegment {
        public IndexSegment() {
            Index = new SortedDictionary<string, PostingList>();
        }
        
        public Guid Id { get; set; }
        public long SizeBytes { get; set; }
        public long DocumentCount { get; set; }
        public SortedDictionary<string, PostingList> Index { get; set; }
    }
    
    /// <summary>
    /// Contains a list of documentIds for an Index Term
    /// </summary>
    [Serializable]
    public class PostingList {
        public List<long> Postings { get; set; }
    }
    
    /// <summary>
    /// TODO: improvement
    /// Contains a Term and associated metadata for an Index 
    /// </summary>
    [Serializable]
    public class TermDictionary {
        public string Term { get; set; }
        public long DocumentFrequency { get; set; }
    }
}