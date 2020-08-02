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
using Javelin.Serializers;
using Javelin.Tokenizers;

namespace Javelin.InMemory {

    /// <summary>
    /// Uses SPMI strategy for indexing data
    /// </summary>
    public class SinglePassInMemoryIndexer : IDocumentIndexer {
        
        // TODO IOptions and inject JSON?
        private readonly IndexerConfig _config;

        private int _currentMergedSegmentId = 1;
        
        private readonly ITokenizer _tokenizer;
        private readonly ISerializer<IndexSegment> _serializer;
        private readonly IFormatter _formatter ;

        public SinglePassInMemoryIndexer(
            ITokenizer tokenizer, 
            ISerializer<IndexSegment> serializer) {
            _tokenizer = tokenizer;
            _serializer = serializer;
            _formatter = new BinaryFormatter();
            _config = new IndexerConfig();
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
                
                while (!IsSegmentSizeReached(segment)) {
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

            await MergeSegments();
        }
        
        
        /// <summary>
        /// Merges smaller segments on disk into a larger segment on disk
        /// Copies read FileStreams to a single write FileStream using the
        /// default buffer size, which should be approximately 81kb chunks
        /// TODO: manage segment sizes, no need to merge into single index
        /// TODO: benchmark performance with different buffer sizes
        /// </summary>
        private async Task MergeSegments() {
            
            var segmentFiles = Directory.GetFiles(_config.SEGMENT_DIRECTORY, $"{_config.SEGMENT_PREFIX}*");
            
            var mergedSegmentPath = Path.Join(
                _config.SEGMENT_DIRECTORY,
                $"{_config.MERGED_SEGMENT_PREFIX}{_currentMergedSegmentId}");
            
            await using FileStream writeStream = File.OpenWrite(mergedSegmentPath);
            
            foreach (var fileName in segmentFiles) {
                await using FileStream readStream = File.Open(fileName, FileMode.Open);
                await readStream.CopyToAsync(writeStream);
            }

            // Currently waits for merge operation to complete before deleting segments
            foreach (var fileName in segmentFiles) {
                File.Delete(fileName);
            }

            _currentMergedSegmentId++;
        }
        

        /// <summary>
        /// Returns True if a Segment Size has reached threshold, otherwise returns false
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        private bool IsSegmentSizeReached(IndexSegment segment) {
            return _config.SEGMENT_FLUSH_STRATEGY switch {
                SegmentFlushStrategy.AllocatedMemory => segment.SizeBytes >= _config.MAX_SIZE_BYTES_PER_SEGMENT,
                SegmentFlushStrategy.PostingsCount => segment.DocumentCount >= _config.MAX_POSTING_COUNT_PER_SEGMENT,
                _ => throw new InvalidOperationException("Unknown or unspecified SEGMENT_FLUSH_STRATEGY.")
            };
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
        /// Using the provided fileName and _serializer,
        /// writes the currently tracked _invertedIndex to disk
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