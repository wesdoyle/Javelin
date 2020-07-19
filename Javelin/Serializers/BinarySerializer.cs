using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Javelin.Serializers {
    
    public class BinarySerializer<T> : ISerializer<T> {
        public void WriteToFile(string filePath, T objectToWrite, bool append = false) {
            var fileMode = append ? FileMode.Append : FileMode.Create;
            using var stream = File.Open(filePath, fileMode);
            var binaryFormatter = new BinaryFormatter();
            binaryFormatter.Serialize(stream, objectToWrite);
        }

        public T ReadFromFile(string filePath) {
            using var stream = File.Open(filePath, FileMode.Open);
            var binaryFormatter = new BinaryFormatter();
            return (T) binaryFormatter.Deserialize(stream);
        }
    }

}