using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workflow.Application.Interfaces
{
    public interface IStorageService
    {
        Task<string> SaveAsync(Stream content, string fileName, string mimeType, CancellationToken ct = default);
        Task<Stream?> GetAsync(string url, CancellationToken ct = default);
        Task<bool> DeleteAsync(string url, CancellationToken ct = default);
    }

}
