using Models;
using Qplix.CodeChallenge.Interfaces;

namespace Qplix.CodeChallenge.Utils;

public class FileReader<T> where T : IEntity, new()
{
    private const int HEADER = 1;

    private readonly  string _fileName;
    private readonly  string _directory;

    public FileReader(string directory, string fileName)
    {
        _fileName = fileName;
        _directory = directory;
    }
    public List<T> ReadFile()
    {
        var filePath =$"{_directory}{_fileName}";
        
        var lines = File.ReadLines(filePath);

        var list = new List<T>();
        
        foreach(string line in lines.Skip(HEADER))
        {
            var entity = new T();
            entity.ParseLine(line);
            
            list.Add(entity);
        }

        return list;    
    }
}