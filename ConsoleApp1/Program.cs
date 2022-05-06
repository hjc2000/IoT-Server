using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ConsoleApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            LookUp();
			Console.ReadLine();
        }

        static async void LookUp()
        {
            HttpClient client = new HttpClient();
            string urlString = string.Format(@"http://localhost:8081/api/v4/clients/{0}", "server");
            client.BaseAddress = new Uri(urlString);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes("admin:public")));
            HttpResponseMessage msg = await client.GetAsync(urlString);
            Console.WriteLine(msg.StatusCode);
            string con = await msg.Content.ReadAsStringAsync();
			JObject jo= JObject.Parse(con);
			Console.WriteLine(jo["data"][0]["connected"].ToString());
		}

/*      
{
	"data": [{
		"send_msg.dropped": 0,
		"send_oct": 65,
		"recv_msg.qos0": 0,
		"subscriptions_cnt": 1,
		"max_inflight": 32,
		"username": "server",
		"recv_cnt": 30,
		"mqueue_dropped": 0,
		"inflight": 0,
		"clientid": "server",
		"recv_msg.dropped": 0,
		"zone": "external",
		"connected": true,
		"recv_msg.qos1": 0,
		"send_msg.qos1": 0,
		"is_bridge": false,
		"clean_start": true,
		"max_subscriptions": 0,
		"send_msg.dropped.too_large": 0,
		"mailbox_len": 0,
		"reductions": 22232,
		"awaiting_rel": 0,
		"proto_name": "MQTT",
		"send_msg.qos0": 0,
		"send_pkt": 30,
		"recv_msg.qos2": 0,
		"max_mqueue": 1000,
		"recv_oct": 137,
		"send_msg.dropped.queue_full": 0,
		"mountpoint": "undefined",
		"send_msg.qos2": 0,
		"send_msg.dropped.expired": 0,
		"recv_pkt": 30,
		"send_msg": 0,
		"max_awaiting_rel": 100,
		"proto_ver": 4,
		"keepalive": 15,
		"created_at": "2022-05-05 23:32:35",
		"ip_address": "127.0.0.1",
		"send_cnt": 30,
		"node": "emqx@127.0.0.1",
		"heap_size": 2586,
		"port": 64513,
		"connected_at": "2022-05-05 23:32:35",
		"mqueue_len": 0,
		"recv_msg": 0,
		"expiry_interval": 0
	}],
	"code": 0
}
*/
	}
}
