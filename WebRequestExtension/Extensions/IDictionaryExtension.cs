using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace WebRequestExtension.Extensions
{
    public static class IDictionaryExtension
    {
        public static string ToPostString(this IDictionary<string, object> postData, string contentType = null)
        {
            string postString = null;
            if (contentType?.Equals("application/json", StringComparison.OrdinalIgnoreCase) ?? false)
            {
                postString = JsonConvert.SerializeObject(postData);
            }
            else
            {
                postString = string.Join("&", postData.Select(_ => _.Key + "=" + Uri.EscapeDataString((_.Value?.ToString() ?? string.Empty))));
            }

            return postString;
        }
    }
}
