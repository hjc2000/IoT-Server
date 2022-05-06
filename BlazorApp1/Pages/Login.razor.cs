using BlazorApp1.MqttComponent;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Headers;
using System.Text;
using static BlazorApp1.MqttComponent.Mqtt;

namespace BlazorApp1.Pages
{
	public partial class Login
	{
		string _password = "";
		string _username = "";
		public string Password
		{
			get { return _password; }
			set { _password = value; }
		}
		public string Username
		{
			get { return _username; }
			set { _username = value; }
		}

		Mqtt? _mqtt;

		[Inject]
		NavigationManager? Nav { get; set; }

		//事件处理
		async void LoginButton()
		{
			/*用户点击了之后就设置MQTT组件的默认设置，以后所有实例化的MQTT组件
			 都使用该设置*/
			Mqtt.DefaultOptions = new MqttOptions()
			{
				Username = _username,
				Password = _password,
			};
			/*组件没加载完成时，_mqtt 可能为 null，等待，直到不为null*/
			while (_mqtt == null)
			{
				await Task.Delay(1000);
			}
			_mqtt.TryConnect();
		}
		/// <summary>
		/// MQTT连接成功，表示用户登录成功
		/// </summary>
		async void OnConnect()
		{
			ESP32._esp32Initialized = true;
			while (Nav == null)
			{
				await Task.Delay(100);
			}
			Nav.NavigateTo("esp32");
		}

	}

	public partial class Login
    {
		async void TestButton()
        {
			HttpClient client = new HttpClient();
			string urlString = string.Format(@"http://localhost:8081/api/v4/clients/{0}", "server");
			client.BaseAddress = new Uri(urlString);
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",Convert.ToBase64String(Encoding.ASCII.GetBytes("admin:public")));
            HttpResponseMessage msg = await client.GetAsync(urlString);
			Console.WriteLine(msg.StatusCode);
		}

	}
}
