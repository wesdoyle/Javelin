using System.Threading.Tasks;
using Javelin.Indexers.Models;

namespace Javelin.Indexers.Interfaces {
    public interface ISegmentMerger {
        Task MergeSegments();
        bool IsSegmentSizeReached(IndexSegment segment);
    }
}