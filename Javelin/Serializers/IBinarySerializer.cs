using System.Threading.Tasks;
using Javelin.Indexers.Models;

namespace Javelin.Serializers {
    /// <summary>
    /// Serializes an IndexSegment
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ISerializer<T> where T: IndexSegment {
        public Task WriteToFile(string filePath, T objectToWrite, bool append = false);
        public Task<T> ReadFromFile(string filePath);
    }
}