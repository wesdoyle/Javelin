using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using Javelin.Helpers;
using Javelin.Search;
using Xunit;
using Xunit.Abstractions;

namespace Javelin.Tests.IntegrationTests.Search {
    public class TestSearchBenchmarks {
        private readonly string _testDirectory = 
            Directory.GetParent(Directory.GetCurrentDirectory())
                .Parent?.Parent?.FullName;
        
        private readonly ITestOutputHelper _output;

        public TestSearchBenchmarks(ITestOutputHelper output) {
            _output = output;
        }
        
        // TODO: run many times and get percentiles
        [Fact]
        public void Test_LinearSearch_vs_InvertedIndexLookup_InMemory() {
            
            // Setup the in-memory index for search
            var indexOnDiskPath = Path.Join(_testDirectory, "TestFixtures", "TestIndex");
            var sut = new BooleanSearchEngine(indexOnDiskPath);
            sut.LoadIndexFromDisk();
            
            // Setup the in-memory flat (forward) index for linear search
            var rawDataOnDiskPath = Path.Join("TestFixtures", "Data.zip");
            var forwardIndexer = new ForwardIndexer();
            forwardIndexer.BuildInMemoryForwardIndex(rawDataOnDiskPath);
            
            // Time the in-memory inverted index search
            var invStart = DateTime.Now;
            var invResult = sut.GetDocumentsContainingTerm("california");
            var invEnd = DateTime.Now;
            
            // Time the in-memory forward index (linear) search
            var fwdStart = DateTime.Now;
            var fwdResult = forwardIndexer.GetDocumentsContainingTerm(" california ");
            var fwdEnd = DateTime.Now;
            
            _output.WriteLine("Inverted Index Lookup time taken : " + (invEnd - invStart));
            _output.WriteLine("Forward Index Lookup time taken : " + (fwdEnd - fwdStart));
            
            // Note that grepping with a forward index will give us char-level substrings -
            // We will need to improve our linguistic preprocessing to identify instances
            // of the tokens we're looking for.  Like "Califorinian", "California's", "California-esque"
            // will be identified via grepping, but lost on our rudimentary indexer due to
            // (crude) whitespace tokenization.
        }
    }
}
