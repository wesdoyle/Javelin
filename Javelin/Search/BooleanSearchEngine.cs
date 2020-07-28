using System;
using System.Collections.Generic;
using System.Linq;
using Javelin.Indexers;
using Javelin.Serializers;

namespace Javelin.Search {
    /// <summary>
    /// A minimal search engine that can query an in-memory IndexSegment
    /// </summary>
    public class BooleanSearchEngine {
        private readonly ISerializer<IndexSegment> _serializer;
        
        private IndexSegment _inMemoryIndex;
        private readonly string _pathToIndex;

        public BooleanSearchEngine(
            string pathToIndex = null,
            ISerializer<IndexSegment> serializer = null) {
            _pathToIndex = pathToIndex;
            _serializer = serializer ?? new BinarySerializer<IndexSegment>();
        }

        public BooleanSearchEngine() { }

        /// <summary>
        /// Returns a list of documentIds pointing to documents containing
        /// the provided term
        /// </summary>
        /// <param name="term">The term to query</param>
        /// <returns></returns>
        public List<long> GetDocumentsContainingTerm(string term) {
            try { return _inMemoryIndex.Index[term]; } 
            catch (KeyNotFoundException e) { return new List<long>(); }
        }

        /// <summary>
        /// Execute a Boolean AND query on the provided terms
        /// Intersects posting lists for terms
        /// </summary>
        /// <param name="terms"></param>
        /// <returns></returns>
        public List<long> IntersectionQuery(List<string> terms) {
            HashSet<long> hashSet = null;

            // Get the posting lists for each term
            var postingLists = terms
                .Select(GetDocumentsContainingTerm)
                .Where(documents => documents.Any());

            // Use C# HashSet intersection to intersect the posting lists
            foreach (var list in postingLists) {
                if (hashSet == null) {
                    hashSet = new HashSet<long>(list);
                } else {
                    hashSet.IntersectWith(list);
                }
            } 
            
            return hashSet == null 
                ? new List<long>() 
                : hashSet.ToList();
        }

        /// <summary>
        /// Using the provided _serializer,
        /// loads an inverted index from disk into memory
        /// </summary>
        public void LoadIndexFromDisk() {
            try {
                _inMemoryIndex = _serializer.ReadFromFile(_pathToIndex);
            } catch (Exception e) {
                Console.WriteLine("Error reading index from disk.");
                Console.WriteLine(e);
            }
        }

        /// <summary>
        /// Sets the _inMemoryIndex to the provided index value
        /// </summary>
        /// <param name="index"></param>
        public void LoadIndexFromMemory(IndexSegment index) {
            _inMemoryIndex = index;
        }
    }
}