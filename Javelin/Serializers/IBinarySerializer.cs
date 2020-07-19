namespace Javelin.Serializers {
    public interface ISerializer<T> {
        public void WriteToFile(string filePath, T objectToWrite, bool append = false);
        public T ReadFromFile(string filePath);
    }
}