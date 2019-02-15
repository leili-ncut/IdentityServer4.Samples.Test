using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace OpenIdMvc
{
    public interface IGitHubClient
    {
        Task<string> GetData();
    }

    /// <summary>
    /// 自定义封装的HttpClient类
    /// </summary>
    public class GitHubClient : IGitHubClient
    {
        public HttpClient _client;

        public GitHubClient(HttpClient httpClient)
        {
            httpClient.BaseAddress = new Uri("https://api.github.com/");
            httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
            httpClient.DefaultRequestHeaders.Add("User-Agent", "HttpClientFactory-Sample");
            _client = httpClient;
        }

        public async Task<string> GetData()
        {
            return await _client.GetStringAsync("/");
        }
    }
}
