using BlazorApp1.MqttComponent;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using static BlazorApp1.MqttComponent.Mqtt;

namespace BlazorApp1.Pages
{
	public partial class Login
	{
		//和输入框绑定的属性
		public string Password
		{
			get { return _userInfo.Password; }
			set { _userInfo.Password = value; }
		}
		public string Username
		{
			get { return _userInfo.Username; }
			set { _userInfo.Username = value; }
		}
		/// <summary>
		/// 用于路由
		/// </summary>
		[Inject]
		NavigationManager? Nav { get; set; }
		/// <summary>
		/// 消息弹窗
		/// </summary>
		MsgBox? _msgbox;
		/// <summary>
		/// 点击登录按钮后执行得操作
		/// </summary>
		async void LoginButton()
		{
			bool authResult = await LoginAuthAsync();
			if (authResult)
			{
				//设置MQTT组件的默认设置
				Mqtt.DefaultOptions = new MqttOptions()
				{
					Username = _userInfo.Username,
					Password = _userInfo.Password,
				};
				DeviceList.UserInfomation = _userInfo;//设置用户信息
				DeviceList.Initialized = true;
				//跳转到页面
				while (Nav == null)
				{
					await Task.Delay(1000);
				}
				Nav.NavigateTo("/DeviceList");
			}
			else
			{
				while(_msgbox==null)
				{
					await Task.Delay(1000);
				}
				_msgbox.Message = "登录失败，请检查用户名和密码";
			}
		}
		/// <summary>
		/// 用户信息类
		/// </summary>
		public class UserInfo
		{
			public string Username { get; set; } = string.Empty;
			public string Password { get; set; } = string.Empty;
		}
		/// <summary>
		/// 用户信息字段
		/// </summary>
		UserInfo _userInfo = new UserInfo();
		/// <summary>
		/// 发送HTTP请求进行登录验证
		/// </summary>
		/// <returns></returns>
		async Task<bool> LoginAuthAsync()
		{
			bool result = false;
			HttpClient client = new();
			//string urlString = string.Format(@"{0}getDeviceInfo", Nav.BaseUri);
			string urlString = @"http://localhost:80";//末尾不需要带斜杠
			client.BaseAddress = new Uri(urlString);
			string json = JsonConvert.SerializeObject(_userInfo);
			HttpContent content = new StringContent(json);
			HttpResponseMessage msg = await client.PostAsync("/loginAuth", content);
			if (msg.StatusCode == System.Net.HttpStatusCode.OK)
			{
				result = true;
			}
			return result;
		}
	}
}
