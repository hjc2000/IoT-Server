using BlazorApp1.MqttComponent;
using Microsoft.AspNetCore.Components;
using System.Text;
using static BlazorApp1.MqttComponent.Mqtt;

namespace BlazorApp1.Pages
{
	public partial class ESP32
	{
		Mqtt? _mqtt;

		[Parameter]
		public string ClientId { get; set; } = string.Empty;
		/*如果该页面不是使用程序跳转过来的，而是用户直接访问 URL 过来的
		 * 就重定向到 Login 页面*/
		[Inject]
		NavigationManager? Nav { get; set; }
		public static bool Initialized { get; set; } = false;
		protected override async Task OnInitializedAsync()
		{
			/*用户在该页面刷新后，会导致界面仍然停留在该页面，但是内存中的
			 所有数据都消失了，静态变量也恢复成初始值了。这个时候要重定向
			到登录页面重新登录*/
			if (!Initialized)
			{
				while (Nav == null)
				{
					await Task.Delay(100);
				}
				Nav.NavigateTo("/");
			}
		}

		double _temperature = 0;
		public double Temprature
		{
			get
			{
				return _temperature;
			}
			set
			{
				_temperature = value;
				StateHasChanged();
			}
		}
		string _log = "";
		public string Log
		{
			get { return _log; }
			set { _log = value; StateHasChanged(); }
		}

		string ImgUrl { get; set; } = "/img/开关——关.jpg";
		bool _switchState = false;
		/// <summary>
		/// 开关状态
		/// </summary>
		bool SwitchState
		{
			get
			{
				return _switchState;
			}
			set
			{
				_switchState = value;
				if (value)
				{
					ImgUrl = "/img/开关——开.jpg";
				}
				else
				{
					ImgUrl = "/img/开关——关.jpg";
				}
				StateHasChanged();
			}
		}
		bool _isOnline = false;
		string LightClass { get; set; } = "offlineLight";
		/// <summary>
		/// 是否在线
		/// </summary>
		bool IsOnline
		{
			get
			{
				return _isOnline;
			}
			set
			{
				_isOnline = value;
				if(value)
				{
					LightClass = "onlineLight";
				}
				else
				{
					LightClass = "offlineLight";
				}
				StateHasChanged();
			}
		}

		MsgBox? _msgbox;
		/// <summary>
		/// 获取到读取LED状态的响应后置为真，按钮点击后置为假
		/// </summary>
		bool _getLEDStateAck = false;
		/// <summary>
		/// 上一次点击按钮后工作流程还没走完
		/// </summary>
		bool _beingHandleClick = false;
		/// <summary>
		/// 按钮点击事件处理
		/// </summary>
		async void OnButtonClick()
		{
			if(!_beingHandleClick)
			{
				_getLEDStateAck = false;
				_beingHandleClick = true;
				if (SwitchState)//开关原本是开
				{
					//现在要把它关上
					Publish("set_LED_state", new byte[] { 0 });
				}
				else//开关原本是关
				{
					//现在要把它打开
					Publish("set_LED_state", new byte[] { 1 });
				}
				//一定要收到LED状态应答，除非设备断线了
				while (!_getLEDStateAck)
				{
					if (!_isOnline)
					{
						_beingHandleClick = false;
						break;//断线了就退出
					}
					await Task.Delay(100);
					Publish("read_LED_state", null);
				}
			}
			else
			{
				while(_msgbox==null)
				{
					await Task.Delay(1000);
				}
				_msgbox.Message = "点击太快了，上一次点击还没收到应答";
			}
		}
		/// <summary>
		/// 订阅ESP32的所有主题
		/// </summary>
		async void Subscribe()
		{
			string topicStr = string.Format("{0}/out/#", ClientId);
			while (_mqtt == null)
			{
				await Task.Delay(1000);
			}
			_mqtt.Subscribe(topicStr);
		}
		/// <summary>
		/// 发布主题
		/// </summary>
		/// <param name="topic"></param>
		/// <param name="payload"></param>
		async void Publish(string topic, byte[]? payload)
		{
			topic = string.Format("{0}/in/{1}", ClientId, topic);
			while (_mqtt == null)
			{
				await Task.Delay(1000);
			}
			if (payload != null)
			{
				_mqtt.Publish(topic, payload);
			}
			else
			{
				_mqtt.Publish(topic, Array.Empty<byte>());
			}
		}
		/// <summary>
		/// 接收到 MQTT 消息时被调用
		/// </summary>
		/// <param name="msg"></param>
		void OnReceive(Msg msg)
		{
			try
			{
				/**
				 * 可能会发生数组索引溢出异常，所以设置了try
				 */
				string[] subTopics = msg.Topic.Split(new char[] { '/' });
				int index = 2;//0和1分别是"客户端ID"和 "out"
				switch (subTopics[index++])
				{
				case "Temperature":
					{
						double temp = BitConverter.ToDouble(msg.Payload, 0);
						Temprature = temp;
						break;
					}
				case "Log":
					{
						Log = Encoding.UTF8.GetString(msg.Payload);
						break;
					}
				case "LedState":
					{
						if (msg.Payload[0] == 0)
						{
							SwitchState = false;
						}
						else
						{
							SwitchState = true;
						}
						_getLEDStateAck = true;
						_beingHandleClick = false;
						break;
					}
				case "IsOnline":
					{
						string state=Encoding.UTF8.GetString(msg.Payload).Trim();
						Console.WriteLine("在线状态" + state);
						if (state == "false")
						{
							IsOnline = false;
						}
						else
						{
							IsOnline = true;
						}
						break;
					}
				}
			}
			catch { }
		}
	}
}
