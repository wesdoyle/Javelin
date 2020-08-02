using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Javelin.Configuration;
using Javelin.Indexers.Interfaces;
using Javelin.Indexers.Models;
using Javelin.Indexers.Utils;
using Javelin.Serializers;
using Javelin.Tokenizers;

namespace Javelin.Indexers {

    /// <summary>
    /// Uses SPIMI strategy for indexing data
    /// </summary>
    public class SinglePassInMemoryIndexer : IDocumentIndexer {
        
        // TODO IOptions and inject JSON?
        private readonly IndexerConfig _config;

        private readonly ITokenizer _tokenizer;
        private readonly ISerializer<IndexSegment> _serializer;
        private readonly IFormatter _formatter ;
        private readonly ISegmentMerger _segMerge;

        public SinglePassInMemoryIndexer(
            ITokenizer tokenizer, 
            ISerializer<IndexSegment> serializer) {
            _config = new IndexerConfig();
            _tokenizer = tokenizer;
            _serializer = serializer;
            _formatter = new BinaryFormatter();
            _segMerge = new SegmentMerger();
        }
        
        /// <summary>
        /// Gets the lexicon term count of the in-memory index
        /// </summary>
        /// <returns></returns>
        public static long GetSegmentVocabularySize(IndexSegment segment) => segment.Index.Keys.Count;
        
        /// <summary>
        /// Indexes the provided zip archive at `filePath` and writes
        /// segmented indices to disk
        /// </summary>
        /// <param name="filePath"></param>
        public async Task BuildIndexForArchive(string filePath) {
            await using var file = File.OpenRead(filePath);
            using var zip = new ZipArchive(file, ZipArchiveMode.Read);
            var docId = 0;
            var indexId = 1;
            var fileCount = zip.Entries.Count;
            
            // TODO: Stream / use cancellation token to exit while loop in async method
            while (true) {
                var segment = new IndexSegment(indexId);
                
                while (!_segMerge.IsSegmentSizeReached(segment)) {
                    try {
                        await using var stream = zip.Entries[docId].Open();
                        await BuildSegment(stream, segment, docId);
                        docId++;
                        if (docId > fileCount - 1) {
                            break;
                        }
                    } catch (ArgumentOutOfRangeException e) {
                        Console.WriteLine($"docId not found: {docId}");
                    }
                }

                await FlushIndexSegment(segment);
                
                indexId++;

                if (docId > fileCount - 1) { break; }
            }

            await _segMerge.MergeSegments();
        }
        
        /// <summary>
        /// Estimates the memory required to store the given `segment`
        /// This is not the way to do this, but trying to get something
        /// simple working temporarily
        /// 
        /// This is way too slow to use efficiently.
        /// TODO: How to monitor the size in MB of the segment during indexing?
        /// Fairly non-trivial to implement properly. See Lucene Estimator for an example.
        /// https://github.com/apache/lucenenet/blob/master/src/Lucene.Net/Util/RamUsageEstimator.cs
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private async Task<long> EstimateMemSize(IndexSegment segment) {
            await using Stream stream = new MemoryStream();
            _formatter.Serialize(stream, segment.Index);
            return stream.Length;
        }
        
        
        /// <summary>
        /// Indexes text data from the Stream
        /// - Reads stream
        /// - Tokenizes text data
        /// - Updates the inverted index
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="docId"></param>
        private async Task BuildSegment(Stream stream, IndexSegment segment, long docId) {
            using var reader = new StreamReader(stream);
            var documentText = await reader.ReadToEndAsync();
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
                
                // TODO: How to measure efficiently at runtime
                // segment.SizeBytes += await EstimateMemSize(segment);
            
            } catch (Exception e) {
                Console.WriteLine("Error building index");
                Console.WriteLine(e);
            }
        }


        /// <summary>
        /// Writes the IndexSegment instance to disk
        /// </summary>
        private async Task FlushIndexSegment(IndexSegment segment) {
            var fileName = Path.Join(_config.SEGMENT_DIRECTORY, _config.SEGMENT_PREFIX);
            fileName += $"{segment.Id:X}";
            
            try {
                await _serializer.WriteToFile(fileName, segment);
            } catch (Exception e) {
                Console.WriteLine("Error writing index to disk.");
                Console.WriteLine(e);
            }
        }
    }
}