using Javelin.Indexers.Interfaces;
using Javelin.Indexers.Models;
using Javelin.Serializers;
using Javelin.Tokenizers;

namespace Javelin.Indexers {
    public class SinglePassInMemoryIndexer : IDocumentIndexer {
        private readonly ITokenizer _tokenizer;
        private readonly ISerializer<IndexSegment> _serializer;
        
        private IndexSegment _index;
        
        public SinglePassInMemoryIndexer() {
        }
    }
}