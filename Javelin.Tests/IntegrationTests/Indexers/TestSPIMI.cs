using System;
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
        
        private readonly IndexerConfig _config = new IndexerConfig();
        
        [Fact]
        public async Task Test_Creates_Merged_Index_File() {
            var testStartTime = DateTime.UtcNow.ToFileTimeUtc();
            var tokenizer = new EnglishTokenizer();
            var serializer = new BinarySerializer<IndexSegment>();
            
            var sut = new SinglePassInMemoryIndexer(tokenizer, serializer);
            await sut.BuildIndexForArchive("./TestFixtures/Data.zip");
            var mergedSegmentPath = Path.Combine(_config.SEGMENT_DIRECTORY, _config.MERGED_SEGMENT_PREFIX + "1");
            
            var mergedSegmentExists = File.Exists(mergedSegmentPath);
            var mergedSegmentLastWriteTime= File.GetLastWriteTime(mergedSegmentPath).ToFileTimeUtc();
            
            mergedSegmentExists.Should().BeTrue();
            mergedSegmentLastWriteTime.Should().BeGreaterThan(testStartTime);
        }
    }
}
