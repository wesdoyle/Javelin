using System;
using System.Collections.Generic;
using System.Linq;
using Javelin.Indexers;
using Javelin.Serializers;

namespace Javelin.Search {
    /// <summary>
    /// A minimal search engine that can query a SimpleInvertedIndex
    /// </summary>
    public class BooleanSearchEngine {
        private readonly ISerializer<SimpleInvertedIndex> _serializer;
        
        private SimpleInvertedIndex _inMemoryIndex;
        private string _pathToIndex;

        public BooleanSearchEngine(
            ISerializer<SimpleInvertedIndex> serializer) {
            _serializer = serializer;
        }

        public List<long> GetDocumentsContainingTerm(string term) {
            try { return _inMemoryIndex.Index[term]; } 
            catch (KeyNotFoundException e) { return new List<long>(); }
        }

        /// <summary>
        /// Using the provided _serializer,
        /// loads an inverted index from disk into memory
        /// </summary>
        /// <param name="fileName"></param>
        public void LoadIndexFromDisk(string fileName) {
            try {
                _inMemoryIndex = _serializer.ReadFromFile(fileName);
            } catch (Exception e) {
                Console.WriteLine("Error reading index from disk.");
                Console.WriteLine(e);
            }
        }
    }
}