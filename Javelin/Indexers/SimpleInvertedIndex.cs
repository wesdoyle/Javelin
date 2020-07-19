using System;
using System.Collections.Generic;

namespace Javelin.Indexers {
    [Serializable]
    public class SimpleInvertedIndex {
        public Dictionary<string, List<long>> Index { get; set; }
    }
}