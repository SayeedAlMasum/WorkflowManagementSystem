namespace Workflow.Infrastructure.Services
{
    public interface IStorageService
    {
        Task<string> SaveAsync(Stream content, string fileName, string mimeType, CancellationToken ct = default);
        Task<Stream?> GetAsync(string url, CancellationToken ct = default);
        Task<bool> DeleteAsync(string url, CancellationToken ct = default);
    }

    public class LocalStorageService : IStorageService
    {
        private readonly string _root;

        public LocalStorageService(string rootPath)
        {
            _root = rootPath;
            Directory.CreateDirectory(_root);
        }

        public async Task<string> SaveAsync(Stream content, string fileName, string mimeType, CancellationToken ct = default)
        {
            var safeName = $"{Guid.NewGuid()}_{Path.GetFileName(fileName)}";
            var fullPath = Path.Combine(_root, safeName);

            using (var fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
            {
                await content.CopyToAsync(fs, ct);
            }

            // Return relative URL
            return $"/uploads/{safeName}";
        }

        public async Task<Stream?> GetAsync(string url, CancellationToken ct = default)
        {
            var fileName = url.Replace("/uploads/", "");
            var fullPath = Path.Combine(_root, fileName);

            if (!File.Exists(fullPath))
                return null;

            var memory = new MemoryStream();
            using (var fs = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
            {
                await fs.CopyToAsync(memory, ct);
            }
            memory.Position = 0;
            return memory;
        }

        public Task<bool> DeleteAsync(string url, CancellationToken ct = default)
        {
            var fileName = url.Replace("/uploads/", "");
            var fullPath = Path.Combine(_root, fileName);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }
    }
}