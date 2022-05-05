using BlazorApp1.MqttComponent;
using Microsoft.AspNetCore.Components;
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

		//事件处理
		async void LoginButton()
		{
			/*用户点击了之后就设置MQTT组件的默认设置，以后所有实例化的MQTT组件
			 都使用该设置*/
			Mqtt._defaultOptions = new MqttOptions()
			{
				Username = _username,
				Password = _password,
			};
			await Task.Run(() =>
			{
				//组件没加载完成时，_mqtt 可能为 null
				while (_mqtt == null) { }//等待，直到不为null
				_mqtt?.TryConnect();
			});
		}
		/// <summary>
		/// MQTT连接成功，表示用户登录成功
		/// </summary>
		void OnConnect()
		{
			ESP32._esp32Initialized = true;
			_nav.NavigateTo("esp32");
		}

	}
}
