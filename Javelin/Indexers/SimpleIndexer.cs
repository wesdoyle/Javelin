using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Javelin.Serializers;
using Javelin.Tokenizers;
    
namespace Javelin.Indexers {

    /// <summary>
    /// A rudimentary search indexer
    /**
    At a very high level, here is what we do to create an
    inverted index that we can use to make queries against:
        1.) Collect documents to be indexed
        2.) Tokenize the documents
        3.) Preprocess the tokens
        4.) Create a Postings List
        
    In this example, we will use a relatively small number of documents
    so that we can store the entire data structure in memory.
    This implementation is minimal and meant to give an initial idea of
    how a first pass at an inverted index can be constructed.
    There is/are no linguistic preprocessing, term frequency calculation,
    skip pointers, sorting, phrasing, compression, MapReduce, etc.
    
    **/
    /// </summary>
    public class SimpleIndexer : IDocumentIndexer {
        
        private readonly ITokenizer _tokenizer;
        private readonly ISerializer<SimpleInvertedIndex> _serializer;
        
        private SimpleInvertedIndex _invertedIndex;

        public SimpleIndexer(
            ITokenizer tokenizer, 
            ISerializer<SimpleInvertedIndex> serializer) {
            _tokenizer = tokenizer;
            _serializer = serializer;
        }

        public void BuildIndexForArchive(string filePath, string indexName) {
            
            _invertedIndex = new SimpleInvertedIndex();
            
            using (var file = File.OpenRead(filePath)) {
                using var zip = new ZipArchive(file, ZipArchiveMode.Read);
                for (var docId = 1; docId < zip.Entries.Count; docId++) {
                    using var stream = zip.Entries[docId].Open();
                    IndexStream(stream, docId);
                }
            }

            try {
                WriteIndexToDisk(indexName);
            } catch {
                Console.WriteLine("Error writing index to disk.");
            }
        }

        private void IndexStream(Stream stream, long docId) {
            using var reader = new StreamReader(stream);
            var documentText = reader.ReadToEnd();
            var tokens = _tokenizer.Tokenize(documentText);
                
            foreach (var token in tokens) {
                if (_invertedIndex.Index.ContainsKey(token)) {
                    _invertedIndex.Index[token].Add(docId);
                } else {
                    _invertedIndex.Index[token] = new List<long> { docId };
                }
            }
        }

        private void WriteIndexToDisk(string fileName) {
            _serializer.WriteToFile(fileName, _invertedIndex);
        }
        
        public void LoadIndexFromDisk(string fileName) {
            _invertedIndex = _serializer.ReadFromFile(fileName);
        }
    }
}
