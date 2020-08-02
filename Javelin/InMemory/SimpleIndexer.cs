using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Javelin.Indexers.Interfaces;
using Javelin.Indexers.Models;
using Javelin.Serializers;
using Javelin.Tokenizers;
    
namespace Javelin.Indexers {

    /// <summary>
    /// A rudimentary indexer 
    /// At a very high level, here is what we do to create an
    /// inverted index that we can use to make queries against:
    /// 
    ///     1.) Collect documents to be indexed
    ///     2.) Tokenize the documents
    ///     3.) Preprocess the tokens
    ///     4.) Create a Postings List
    ///     
    /// In this example, we will use a relatively small number of documents
    /// so that we can store the entire data structure in memory.
    /// This implementation is minimal and meant to give an initial idea of
    /// how a first pass at an inverted index can be constructed.
    /// There is/are no linguistic preprocessing, term frequency calculation,
    /// skip pointers, sorting, phrasing, compression, MapReduce, etc.
    /// </summary>
    public class SimpleIndexer : IDocumentIndexer {
        
        private readonly ITokenizer _tokenizer;
        private readonly ISerializer<IndexSegment> _serializer;
        
        private IndexSegment _indexSegment;

        public SimpleIndexer(
            ITokenizer tokenizer, 
            ISerializer<IndexSegment> serializer) {
            _tokenizer = tokenizer;
            _serializer = serializer;
        }

        /// <summary>
        /// Builds an in-memory index using a provided filePath to a zip file.
        /// The provided filePath should be a zip archive containing text files
        /// for index. Each file is considered a document for the posting list.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="indexName"></param>
        public void BuildIndexForArchive(string filePath, string indexName) {
            _indexSegment = new IndexSegment {
                Index = new SortedDictionary<string, PostingList>()
            };

            using var file = File.OpenRead(filePath);
            using var zip = new ZipArchive(file, ZipArchiveMode.Read);
                
            for (var docId = 1; docId < zip.Entries.Count; docId++) {
                using var stream = zip.Entries[docId].Open();
                IndexStream(stream, docId);
            }

            WriteIndexToDisk(indexName);
        }

        /// <summary>
        /// Gets the lexicon term count of the in-memory index
        /// </summary>
        /// <returns></returns>
        public long GetIndexVocabularySize() => _indexSegment.Index.Keys.Count;

        /// <summary>
        /// Indexes text data from the Stream
        /// - Reads stream
        /// - Tokenizes text data
        /// - Updates the inverted index
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="docId"></param>
        private void IndexStream(Stream stream, long docId) {
            using var reader = new StreamReader(stream);
            var documentText = reader.ReadToEnd();
            var tokens = _tokenizer.Tokenize(documentText);
            try {
                foreach (var token in tokens) {
                    var loweredToken = token.ToLowerInvariant();
                    if (_indexSegment.Index.ContainsKey(loweredToken)) {
                        _indexSegment.Index[loweredToken].Postings.Add(docId);
                    } else {
                        _indexSegment.Index[loweredToken] = new PostingList { Postings = new List<long> {docId }};
                    }
                }
            } catch (Exception e) {
                Console.WriteLine("Error building index");
                Console.WriteLine(e);
            }
        }

        /// <summary>
        /// Using the provided fileName and _serializer,
        /// writes the currently tracked _invertedIndex to disk
        /// </summary>
        /// <param name="fileName"></param>
        private void WriteIndexToDisk(string fileName) {
            try {
                _serializer.WriteToFile(fileName, _indexSegment);
            } catch (Exception e) {
                Console.WriteLine("Error writing index to disk.");
                Console.WriteLine(e);
            }
        }
        
        /// <summary>
        /// Using the provided _serializer,
        /// loads an inverted index from disk into memory
        /// </summary>
        /// <param name="fileName"></param>
        public async Task LoadIndexFromDisk(string fileName) {
            try {
                _indexSegment = await _serializer.ReadFromFile(fileName);
            } catch (Exception e) {
                Console.WriteLine("Error reading index from disk.");
                Console.WriteLine(e);
            }
        }
    }
}
