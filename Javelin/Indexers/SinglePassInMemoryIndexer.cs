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

    public enum SegmentFlushStrategy {
        AllocatedMemory,
        PostingsCount
    }
    
    /// <summary>
    /// Uses SPMI strategy for indexing data
    /// </summary>
    public class SinglePassInMemoryIndexer : IDocumentIndexer {
        private readonly ITokenizer _tokenizer;
        private readonly ISerializer<IndexSegment> _serializer;
        private readonly IndexerConfig _config;

        private readonly SegmentFlushStrategy SEGMENT_FLUSH_STRATEGY = SegmentFlushStrategy.AllocatedMemory;
        
        private readonly int MAX_POSTING_COUNT_PER_SEGMENT = 10_000;
        private readonly long MAX_SIZE_BYTES_PER_SEGMENT = 60 * 1024 * 1024;

        public SinglePassInMemoryIndexer(IndexerConfig config) {
            _config = config;
        }
        
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
            
            // TODO: Stream
            while (docId < zip.Entries.Count) {
                
                var segment = new IndexSegment();
                
                while (!IsSegmentSizeReached(segment)) {
                    docId++;
                    using var stream = zip.Entries[docId].Open();
                    IndexStream(stream, segment, docId);
                }
                
                FlushIndexSegment(indexName, segment);
            }
        }

        /// <summary>
        /// Returns True if a Segment Size has reached threshold, otherwise returns false
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        private bool IsSegmentSizeReached(IndexSegment segment) {
            if (SEGMENT_FLUSH_STRATEGY == SegmentFlushStrategy.AllocatedMemory) {
                return EstimateMemSize(segment) >= MAX_SIZE_BYTES_PER_SEGMENT;
            } 
            
            if (SEGMENT_FLUSH_STRATEGY == SegmentFlushStrategy.PostingsCount) {
                return segment.DocumentCount >= MAX_POSTING_COUNT_PER_SEGMENT;
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
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, segment.Index);
            return stream.Length;
        }

        /// <summary>
        /// Gets the lexicon term count of the in-memory index
        /// </summary>
        /// <returns></returns>
        public static long GetIndexVocabularySize(IndexSegment segment) => segment.Index.Keys.Count;
        
        /// <summary>
        /// Using the provided fileName and _serializer,
        /// writes the currently tracked _invertedIndex to disk
        /// </summary>
        /// <param name="fileName"></param>
        private void FlushIndexSegment(string fileName, IndexSegment segment) {
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
                            Postings = new List<long> {docId }
                        };
                    }
                }
            } catch (Exception e) {
                Console.WriteLine("Error building index");
                Console.WriteLine(e);
            }
        }
    }
}