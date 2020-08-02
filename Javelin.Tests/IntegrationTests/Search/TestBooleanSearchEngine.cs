using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using Javelin.Search;
using Xunit;

namespace Javelin.Tests.IntegrationTests.Search {
    public class TestBooleanSearchEngine {
        private readonly string _testDirectory = 
            Directory.GetParent(Directory.GetCurrentDirectory())
                .Parent?.Parent?.FullName;
        
        [Fact]
        public void Test_Intersection_Returns_ExpectedDocuments() {
            var indexOnDiskPath = Path.Join(_testDirectory, "TestFixtures", "TestIndex");
            var sut = new SimpleBooleanSearchEngine(indexOnDiskPath);
            sut.LoadIndexFromDisk();
            var searchTerms = new List<string> {"red", "dry"};
            var result = sut.IntersectionQuery(searchTerms);
            result.Count.Should().Be(84);
        }
        
        [Fact]
        public void Test_GetDocumentsContainingTerm_Returns_ExpectedDocuments() {
            var indexOnDiskPath = Path.Join(_testDirectory, "TestFixtures", "TestIndex");
            var sut = new SimpleBooleanSearchEngine(indexOnDiskPath);
            sut.LoadIndexFromDisk();
            var result = sut.GetDocumentsContainingTerm("california");
            result.Postings.Count.Should().Be(21);
        }
    }
}