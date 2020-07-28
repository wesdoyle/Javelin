using System;
using System.Collections.Generic;

namespace Javelin.Indexers {
    [Serializable]
    public class IndexSegment {
        public SortedDictionary<string, List<long>> Index { get; set; }
    }
}