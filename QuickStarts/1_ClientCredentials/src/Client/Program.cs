using System;
using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel.Client;
using Newtonsoft.Json.Linq;

namespace Client
{
    class Program
    {
        public static void Main(string[] args) => MainAsync().GetAwaiter().GetResult();

        private static async Task MainAsync()
        {
            // 从元数据中发现端口
            var disco = await DiscoveryClient.GetAsync("http://localhost:5000");
            if (disco.IsError)
            {
                Console.WriteLine(disco.Error);
                return;
            }
            // 1. 客户端授权模式-- 请求令牌 
            var tokenClient = new TokenClient(disco.TokenEndpoint, "client", "secret");
            var tokenResponse = await tokenClient.RequestClientCredentialsAsync("api1");
            if (tokenResponse.IsError)
            {
                Console.WriteLine(tokenResponse.Error);
                return;
            }
            Console.WriteLine(tokenResponse.Json);

            // 调用api 
            var client = new HttpClient();
            client.SetBearerToken(tokenResponse.AccessToken);
            var response = await client.GetAsync("http://localhost:5001/api/identity");
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine(response.StatusCode);
            }
            else
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine(JArray.Parse(content));
            }

            //2. 资源所有者密码授权模式-- 请求令牌
            var tokenClientResource = new TokenClient(disco.TokenEndpoint, "ro.client", "secret");
            var tokenResponseResource = await tokenClientResource.RequestResourceOwnerPasswordAsync("alice", "password", "api1");//使用用户名密码
            if (tokenResponseResource.IsError)
            {
                Console.WriteLine(tokenResponseResource.Error);
                return;
            }
            Console.WriteLine(tokenResponseResource.Json);

            // 调用api 
            client = new HttpClient();
            client.SetBearerToken(tokenResponse.AccessToken);
            response = await client.GetAsync("http://localhost:5001/api/identity");
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine(response.StatusCode);
            }
            else
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine(JArray.Parse(content));
            }
            //3. 
        }


    }
}
