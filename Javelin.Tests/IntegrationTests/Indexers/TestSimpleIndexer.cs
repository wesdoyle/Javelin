using Javelin.Indexers;
using Javelin.Serializers;
using Javelin.Tokenizers;
using Xunit;

namespace Javelin.Tests.IntegrationTests.Indexers {
    public class TestSimpleIndexer {
        [Fact]
        public void Test_SimpleIndexer_Builds_SimpleInvertedIndex() {
            var tokenizer = new SimpleTokenizer();
            var serializer = new BinarySerializer<SimpleInvertedIndex>();
            var sut = new SimpleIndexer(tokenizer, serializer);
            sut.BuildIndexForArchive("./TestFixtures/Data.zip", "TestIndex");
        }
    }
}
