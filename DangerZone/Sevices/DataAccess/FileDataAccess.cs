using System.Text;
using System.Text.Json;

namespace DangerZone.Sevices.DataAccess
{
    /// <summary>
    /// File-system implementation of <see cref="IDataAccess{T}"/>.
    /// Stores each item as a JSON file under a base directory.
    /// </summary>
    public sealed class FileDataAccess<T> : IDataAccess<T>
    {
        private readonly string _baseDirectory;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };

        /// <summary>
        /// Creates a new instance. If <paramref name="baseDirectory"/> is null or empty,
        /// a default "Data" folder under the application's base directory is used.
        /// </summary>
        public FileDataAccess(string? baseDirectory = null)
        {
            _baseDirectory = string.IsNullOrWhiteSpace(baseDirectory)
                ? Path.Combine(AppContext.BaseDirectory, "Data")
                : baseDirectory;

            Directory.CreateDirectory(_baseDirectory);
        }

        /// <summary>
        /// Reads and deserializes the JSON file identified by <paramref name="id"/>.
        /// Throws <see cref="FileNotFoundException"/> when the file does not exist.
        /// </summary>
        public T GetData(string id)
        {
            var path = GetPath(id);

            if (!File.Exists(path))
                throw new FileNotFoundException($"Data file not found for id '{id}'.", path);

            var text = File.ReadAllText(path, Encoding.UTF8);
            return JsonSerializer.Deserialize<T>(text, _jsonOptions)!;
        }

        /// <summary>
        /// Serializes <paramref name="Data"/> and writes it to the file identified by <paramref name="id"/>.
        /// Overwrites existing file.
        /// </summary>
        public void SetData(string id, T Data)
        {
            if (Data == null) throw new ArgumentNullException(nameof(Data));

            var path = GetPath(id);
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            var text = JsonSerializer.Serialize(Data, _jsonOptions);
            File.WriteAllText(path, text, Encoding.UTF8);
        }

        private string GetPath(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Id must be provided.", nameof(id));

            // Prevent path traversal and rooted paths by using GetFileName and rejecting suspicious inputs.
            if (Path.IsPathRooted(id) || id.Contains(".."))
                throw new ArgumentException("Invalid id. Path traversal is not allowed.", nameof(id));

            var fileName = Path.GetFileName(id);
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("Id is invalid after sanitization.", nameof(id));

            if (string.IsNullOrEmpty(Path.GetExtension(fileName)))
                fileName += ".json";

            return Path.Combine(_baseDirectory, fileName);
        }
    }
}