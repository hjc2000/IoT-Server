using System.Net.Http.Headers;
using System.Text;

namespace WebServerLib
{
    public class IsDeviceOnline
    {
        public static async void Lookup(string clientId)
        {
            HttpClient client = new HttpClient();
            string urlString = string.Format(@"http://localhost:8081/api/v4/clients/{0}", "server");
            client.BaseAddress = new Uri(urlString);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes("admin:public")));
            HttpResponseMessage msg = await client.GetAsync(urlString);
            Console.WriteLine(msg.StatusCode);
        }
    }
}
