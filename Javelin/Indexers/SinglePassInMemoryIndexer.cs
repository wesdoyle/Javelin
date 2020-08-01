using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Javelin.Configuration;
using Javelin.Indexers.Interfaces;
using Javelin.Indexers.Models;
using Javelin.Serializers;
using Javelin.Tokenizers;

namespace Javelin.Indexers {

    /// <summary>
    /// Uses SPMI strategy for indexing data
    /// </summary>
    public class SinglePassInMemoryIndexer : IDocumentIndexer {
        
        // TODO IOptions
        private readonly IndexerConfig _config;
        
        private readonly ITokenizer _tokenizer;
        private readonly ISerializer<IndexSegment> _serializer;
        private readonly IFormatter _formatter ;

        public SinglePassInMemoryIndexer(
            IndexerConfig config, 
            ITokenizer tokenizer, 
            ISerializer<IndexSegment> serializer) {
            _tokenizer = tokenizer;
            _serializer = serializer;
            _config = config;
            _formatter = new BinaryFormatter();
        }
        
        /// <summary>
        /// Gets the lexicon term count of the in-memory index
        /// </summary>
        /// <returns></returns>
        public static long GetIndexVocabularySize(IndexSegment segment) => segment.Index.Keys.Count;
        
        
        /// <summary>
        /// Indexes the provided zip archive at `filePath` and writes
        /// segmented indices to disk
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="indexName"></param>
        public void BuildIndexForArchive(string filePath, string indexName) {
            using var file = File.OpenRead(filePath);
            using var zip = new ZipArchive(file, ZipArchiveMode.Read);
            var docId = 0;
            var indexId = 1;
            
            // TODO: Stream
            while (docId < zip.Entries.Count) {
                var segment = new IndexSegment(indexId);
                while (!IsSegmentSizeReached(segment)) {
                    docId++;
                    using var stream = zip.Entries[docId].Open();
                    IndexStream(stream, segment, docId);
                }
                FlushIndexSegment(indexName, segment);
                indexId++;
            }
        }
        
        
        /// <summary>
        /// Merges index segments
        /// </summary>
        /// <param name="path"></param>
        public void MergeIndices(string path) {
            throw new NotImplementedException();
        }
        

        /// <summary>
        /// Returns True if a Segment Size has reached threshold, otherwise returns false
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        private bool IsSegmentSizeReached(IndexSegment segment) {
            if (_config.SEGMENT_FLUSH_STRATEGY == SegmentFlushStrategy.AllocatedMemory) {
                return segment.SizeBytes >= _config.MAX_SIZE_BYTES_PER_SEGMENT;
            } 
            
            if (_config.SEGMENT_FLUSH_STRATEGY == SegmentFlushStrategy.PostingsCount) {
                return segment.DocumentCount >= _config.MAX_POSTING_COUNT_PER_SEGMENT;
            }
            
            throw new InvalidOperationException("Unknown or unspecified SEGMENT_FLUSH_STRATEGY.");
        }
        

        /// <summary>
        /// Estimates the memory required to store the given `segment`
        /// This is not the way to do this, but trying to get something
        /// simple working temporarily
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private long EstimateMemSize(IndexSegment segment) {
            using Stream stream = new MemoryStream();
            _formatter.Serialize(stream, segment.Index);
            var bytes = stream.Length;
            return bytes;
        }

        
        /// <summary>
        /// Using the provided fileName and _serializer,
        /// writes the currently tracked _invertedIndex to disk
        /// </summary>
        /// <param name="fileName"></param>
        private void FlushIndexSegment(string fileName, IndexSegment segment) {
            
            fileName += segment.Id.ToString("X");
            
            try {
                _serializer.WriteToFile(fileName, segment);
            } catch (Exception e) {
                Console.WriteLine("Error writing index to disk.");
                Console.WriteLine(e);
            }
        }
        
        
        /// <summary>
        /// Indexes text data from the Stream
        /// - Reads stream
        /// - Tokenizes text data
        /// - Updates the inverted index
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="docId"></param>
        private void IndexStream(Stream stream, IndexSegment segment, long docId) {
            using var reader = new StreamReader(stream);
            var documentText = reader.ReadToEnd();
            var tokens = _tokenizer.Tokenize(documentText);
            
            try {
                foreach (var token in tokens) {
                    var loweredToken = token.ToLowerInvariant();
                    if (segment.Index.ContainsKey(loweredToken)) {
                        segment.Index[loweredToken].Postings.Add(docId);
                    } else {
                        segment.Index[loweredToken] = new PostingList {
                            Postings = new List<long> { docId }
                        };
                    }
                }
                
                segment.DocumentCount++;
                segment.SizeBytes += EstimateMemSize(segment);
            
            } catch (Exception e) {
                Console.WriteLine("Error building index");
                Console.WriteLine(e);
            }
        }
    }
}