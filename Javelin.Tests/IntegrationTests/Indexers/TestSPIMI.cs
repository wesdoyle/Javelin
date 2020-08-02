using System.Threading.Tasks;
using Javelin.Indexers;
using Javelin.Indexers.Models;
using Javelin.Serializers;
using Javelin.Tokenizers;
using Xunit;

namespace Javelin.Tests.IntegrationTests.Indexers {
    public class TestSPIMI {
        
        [Fact]
        public async Task Test_VocabularySize_Is_Expected() {
            var tokenizer = new EnglishTokenizer();
            var serializer = new BinarySerializer<IndexSegment>();
            
            var sut = new SinglePassInMemoryIndexer(tokenizer, serializer);
            await sut.BuildIndexForArchive("./TestFixtures/Data.zip");
        }
    }
}
