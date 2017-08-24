using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WebRequestExtension.Extensions;

namespace WebRequestExtension
{
    public class WebRequestEx : IWebRequestEx
    {
        const string DefaultUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/60.0.3112.101 Safari/537.36";
        private readonly CookieContainer _cookieContainer;

        static WebRequestEx()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public string UserAgent { get; set; }


        public WebRequestEx(CookieContainer cookieContainer)
        {
            _cookieContainer = cookieContainer ?? new CookieContainer();
        }

        public async Task<string> GetAsync(string url, string referer = null, Encoding encoding = null, List<string> accessed = null)
        {
            try
            {
                using (var response = await GetResponseAsync(url, false, referer))
                using (var resStream = response.GetResponseStream())
                using (var reader = new StreamReader(resStream, encoding ?? Encoding.Default))
                {
                    var content = await reader.ReadToEndAsync();
                    return content;
                }
            }
            catch (WebException ex) when (ex.Message.Contains("302"))
            {
                var location = ex.Response?.Headers?[HttpResponseHeader.Location];
                if (!string.IsNullOrEmpty(location))
                {
                    var uri = new Uri(url).AbsolutePath;

                    if (accessed == null)
                    {
                        accessed = new List<string> { uri };
                    }
                    else
                    {
                        if (accessed.Contains(uri)) throw new ArgumentException($"Encounter loop : {url}");
                        accessed.Add(uri);
                    }

                    return await GetAsync(location, accessed: accessed);
                }

                throw ex;
            }
        }

        public async Task<string> PostAsync(string url, dynamic postData, string referer = null, string contentType = null)
        {
            if(!(postData is IDictionary<string, object> dict))
            {
                throw new NotSupportedException("postData is not " + nameof(IDictionary<string, object>));
            }

            return await PostAsync(url, dict, referer, contentType);
        }

        public async Task<string> PostAsync(string url, IDictionary<string, object> postData, string referer = null, string contentType = null)
        {
            var postString = postData.ToPostString(contentType);
            
            return await PostAsync(url, postString, referer, contentType);
        }

        public async Task<string> PostAsync(string url, string postData, string referer = null, string contentType = null)
        {
            var request = CreateWebRequest(url, referer);
            request.Method = "POST";
            request.ContentType = contentType ?? "application/x-www-form-urlencoded";
            request.ContentLength = postData.Length;

            using (var reqStream = await request.GetRequestStreamAsync())
            using (var reqStreamWriter = new StreamWriter(reqStream))
            {
                await reqStreamWriter.WriteAsync(postData);
                await reqStreamWriter.FlushAsync();
            }

            using (var response = await request.GetResponseAsync())
            using (var resStream = response.GetResponseStream())
            using (var resReader = new StreamReader(resStream, Encoding.GetEncoding("GBK")))
            {
                var responseBody = await resReader.ReadToEndAsync();

                return responseBody;
            }
        }


        private async Task<WebResponse> GetResponseAsync(string url, bool ignore302Excpetion = true, string referer = null)
        {
            var request = CreateWebRequest(url, referer);

            request.Method = "GET";
            request.AllowAutoRedirect = true;

            try
            {
                return await request.GetResponseAsync();
            }
            catch (WebException ex) when (ex.Message.Contains("302"))
            {
                if (ignore302Excpetion)
                {
                    return ex.Response;
                }

                throw ex;
            }
        }
        
        private HttpWebRequest CreateWebRequest(string url, string referer = null)
        {
            var request = WebRequest.CreateHttp(url);
            request.UserAgent = UserAgent ?? DefaultUserAgent;
            request.CookieContainer = _cookieContainer;

            if (!string.IsNullOrEmpty(referer))
            {
                request.Headers.Add(HttpRequestHeader.Referer, referer);
            }

            return request;
        }
    }
}
