using System;
using System.IO;
using System.Threading.Tasks;
using Javelin.Configuration;
using Javelin.Indexers.Interfaces;
using Javelin.Indexers.Models;

namespace Javelin.Indexers.Utils {
    public class SegmentMerger : ISegmentMerger {

        // TODO IOptions and inject JSON?
        private readonly IndexerConfig _config;

        private int _currentMergedSegmentId = 1;

        public SegmentMerger() {
            _config = new IndexerConfig();
        }

        /// <summary>
        /// Merges smaller segments on disk into a larger segment on disk
        /// Copies read FileStreams to a single write FileStream using the
        /// default buffer size, which should be approximately 81kb chunks
        /// TODO: manage segment sizes, no need to merge into single index
        /// TODO: benchmark performance with different buffer sizes
        /// </summary>
        public async Task MergeSegments() {

            var segmentFiles = Directory.GetFiles(_config.SEGMENT_DIRECTORY, $"{_config.SEGMENT_PREFIX}*");

            var mergedSegmentPath = Path.Join(
                _config.SEGMENT_DIRECTORY,
                $"{_config.MERGED_SEGMENT_PREFIX}{_currentMergedSegmentId}");

            await using var writeStream = File.OpenWrite(mergedSegmentPath);

            foreach (var fileName in segmentFiles) {
                await using var readStream = File.Open(fileName, FileMode.Open);
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
        public bool IsSegmentSizeReached(IndexSegment segment) {
            return _config.SEGMENT_FLUSH_STRATEGY switch {
                SegmentFlushStrategy.AllocatedMemory => segment.SizeBytes >= _config.MAX_SIZE_BYTES_PER_SEGMENT,
                SegmentFlushStrategy.PostingsCount => segment.DocumentCount >= _config.MAX_POSTING_COUNT_PER_SEGMENT,
                _ => throw new InvalidOperationException("Unknown or unspecified SEGMENT_FLUSH_STRATEGY.")
            };
        }
    }
}