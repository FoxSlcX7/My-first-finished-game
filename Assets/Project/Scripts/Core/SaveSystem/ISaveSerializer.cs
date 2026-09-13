public interface ISaveSerializer
{
    string FileExtension { get; }
    void Save<T>(string filePath, T data);
    T Load<T>(string filePath);
}