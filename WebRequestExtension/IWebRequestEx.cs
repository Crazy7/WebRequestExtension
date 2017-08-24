using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebRequestExtension
{
    public interface IWebRequestEx
    {
        string UserAgent { get; set; }

        Task<string> GetAsync(string url, string referer = null, Encoding encoding = null, List<string> accessed = null);

        Task<string> PostAsync(string url, string postData, string referer = null, string contentType = null);
        Task<string> PostAsync(string url, dynamic postData, string referer = null, string contentType = null);
        Task<string> PostAsync(string url, IDictionary<string, object> postData, string referer = null, string contentType = null);
    }
}