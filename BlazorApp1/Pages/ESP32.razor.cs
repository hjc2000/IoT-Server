using BlazorApp1.MqttComponent;
using Microsoft.AspNetCore.Components;
using static BlazorApp1.MqttComponent.Mqtt;

namespace BlazorApp1.Pages
{
	public partial class ESP32
	{
		Mqtt? _mqtt;
		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			if (firstRender)
			{
				while (_mqtt == null)
				{
					await Task.Delay(1000);
				}
				_mqtt.TryConnect();
			}

		}

		/*如果该页面不是使用程序跳转过来的，而是用户直接访问 URL 过来的
		 * 就重定向到 Login 页面*/
		[Inject]
		NavigationManager? Nav { get; set; }
		public static bool _esp32Initialized = false;
		protected override async Task OnInitializedAsync()
		{
			/*用户在该页面刷新后，会导致界面仍然停留在该页面，但是内存中的
			 所有数据都消失了，静态变量也恢复成初始值了。这个时候要重定向
			到登录页面重新登录*/
			if(!_esp32Initialized)
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

		void ToggleLed()
		{
			_mqtt?.Publish("esp32-7C:9E:BD:65:DA:E0/command/msp", new byte[] { 1 });
		}

		void OnReceive(Msg msg)
		{
			try
			{
				/**
				 * 可能会发生数组索引溢出异常，所以设置了try
				 */
				string[] subTopics = msg.Topic.Split(new char[] { '/' });
				int index = 0;
				if (subTopics[index++] == "esp32-7C:9E:BD:65:DA:E0")
				{
					switch (subTopics[index++])
					{
					case "temperature":
						{
							double temp = BitConverter.ToDouble(msg.Payload, 0);
							Temprature = temp;
							break;
						}
					}
				}

			}
			catch
			{

			}
		}
		class Person
		{
			public string Name { get; set; } = "佚名";
			public int Age { get; set; } = 0;
		}
		List<Person> _perList = new()
		{
			new Person()
			{
				Name = "张三",
				Age = 15,
			},
			new Person(),
			new Person(),
		};
		void OutputClick(Person person)
		{
			person.Age++;
			_perList.Add(new Person());
		}
	}
}
