using System.IO;
using FluentAssertions;
using Javelin.Indexers;
using Javelin.Serializers;
using Javelin.Tokenizers;
using Xunit;

namespace Javelin.Tests.TestFixtures {
    public class TestSimpleIndexer {
        
        private readonly string _testDirectory = 
            Directory.GetParent(Directory.GetCurrentDirectory())
                .Parent?.Parent?.FullName;
        
        [Fact]
        public void Test_VocabularySize_Is_Expected() {
            var tokenizer = new SimpleTokenizer();
            var serializer = new BinarySerializer<SimpleInvertedIndex>();
            var sut = new SimpleIndexer(tokenizer, serializer);
            var indexOnDiskPath = Path.Join(_testDirectory, "TestFixtures", "TestIndex");
            sut.BuildIndexForArchive("./TestFixtures/Data.zip", indexOnDiskPath);
            sut.LoadIndexFromDisk(indexOnDiskPath);
            sut.GetIndexVocabularySize().Should().Be(17_860L);
        }
    }
}