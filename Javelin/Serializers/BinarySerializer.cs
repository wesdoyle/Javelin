using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Javelin.Indexers.Models;

namespace Javelin.Serializers {
    
    public class BinarySerializer<T> : ISerializer<T> where T: IndexSegment {
        
        public async Task WriteToFile(string filePath, T objectToWrite, bool append = false) {
            var fileMode = append ? FileMode.Append : FileMode.Create;
            await using var stream = File.Open(filePath, fileMode);
            var binaryFormatter = new BinaryFormatter();
            binaryFormatter.Serialize(stream, objectToWrite);
        }

        public async Task<T> ReadFromFile(string filePath) {
            await using var stream = File.Open(filePath, FileMode.Open);
            var binaryFormatter = new BinaryFormatter();
            return (T) binaryFormatter.Deserialize(stream);
        }
    }

}