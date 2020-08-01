using System.Threading.Tasks;

namespace Javelin.Serializers {
    public interface ISerializer<T> {
        public Task WriteToFile(string filePath, T objectToWrite, bool append = false);
        public Task<T> ReadFromFile(string filePath);
    }
}