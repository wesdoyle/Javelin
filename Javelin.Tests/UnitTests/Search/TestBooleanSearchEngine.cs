using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using Javelin.Indexers.Models;
using Javelin.Search;
using Xunit;

namespace Javelin.Tests.UnitTests.Search {
    public class TestBooleanSearchEngine {
        private readonly string _testDirectory = 
            Directory.GetParent(Directory.GetCurrentDirectory())
                .Parent?.Parent?.FullName;
        
        [Theory]
        [InlineData("red", "blue", 3)]
        [InlineData("red", "green", 0)]
        [InlineData("green", "blue", 1)]
        public void Test_Intersection_Returns_ExpectedDocuments(string t1, string t2, long expectedCount) {
            var index = new SortedDictionary<string, PostingList>() {
                 ["red"] = new PostingList{ Postings =  new List<long> {1, 2, 3, 4, 5 }},
                 ["blue"] = new PostingList{ Postings =  new List<long> {2, 3, 4, 7}},
                 ["green"] = new PostingList{ Postings =  new List<long> {6, 7}},
            };
            
            var inMemoryIndex = new IndexSegment() { Index = index };
            var sut = new BooleanSearchEngine();
            sut.LoadIndexFromMemory(inMemoryIndex);
            
            var searchTerms = new List<string> {t1, t2};
            var result = sut.IntersectionQuery(searchTerms);

            result.Count.Should().Be((int)expectedCount);
        }
        
        [Fact]
        public void Test_GetDocumentsContainingTerm_Returns_ExpectedDocuments() {
            var index = new SortedDictionary<string, PostingList>() {
                 ["red"] = new PostingList{ Postings =  new List<long> {1, 2, 3, 4, 5 }},
                 ["blue"] = new PostingList{ Postings =  new List<long> {2, 3, 4, 7}},
                 ["green"] = new PostingList{ Postings =  new List<long> {6, 7}},
            };
            
            var inMemoryIndex = new IndexSegment() { Index = index };
            var sut = new BooleanSearchEngine();
            sut.LoadIndexFromMemory(inMemoryIndex);

            var result = sut.GetDocumentsContainingTerm("blue");
            var expectedDocs = new PostingList { Postings = new List<long> {2, 3, 4, 7}};

            result.Postings.Count.Should().Be(4);
            result.Should().BeEquivalentTo(expectedDocs);
        }
    }
}
