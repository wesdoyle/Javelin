using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Javelin.Configuration;
using Javelin.Indexers;
using Javelin.Indexers.Models;
using Javelin.Serializers;
using Javelin.Tokenizers;
using Xunit;

namespace Javelin.Tests.IntegrationTests.Indexers {
    public class TestSPIMI {
        
        private readonly string _testDirectory = 
            Directory.GetParent(Directory.GetCurrentDirectory())
                .Parent?.Parent?.FullName;
        
        [Fact]
        public async Task Test_VocabularySize_Is_Expected() {
            var tokenizer = new EnglishTokenizer();
            var serializer = new BinarySerializer<IndexSegment>();
            
            var indexerConfig = new IndexerConfig {
                SEGMENT_FLUSH_STRATEGY = SegmentFlushStrategy.AllocatedMemory,
                MAX_SIZE_BYTES_PER_SEGMENT = 60 * 1024 * 1024
            };
            
            var sut = new SinglePassInMemoryIndexer(indexerConfig, tokenizer, serializer);
            var indexOnDiskPath = Path.Join(_testDirectory, "TestFixtures", "TestIndex");
            await sut.BuildIndexForArchive("./TestFixtures/Data.zip", indexOnDiskPath);
        }
    }
}
