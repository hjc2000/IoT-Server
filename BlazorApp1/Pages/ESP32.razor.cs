using BlazorApp1.MqttComponent;
using static BlazorApp1.MqttComponent.Mqtt;

namespace BlazorApp1.Pages
{
	public partial class ESP32
	{
		Mqtt? _mqtt;
		MqttOptions _options = new MqttOptions()
		{
			_username = "hjc",
			_password = "123456",
		};
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
				string[] subTopics = msg._topic.Split(new char[] { '/' });
				int index = 0;
				if (subTopics[index++] == "esp32-7C:9E:BD:65:DA:E0")
				{
					switch (subTopics[index++])
					{
					case "temperature":
						{
							double temp = BitConverter.ToDouble(msg._payload, 0);
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
		List<Person> _perList = new List<Person>()
		{
			new Person()
			{
				Name="张三",
				Age=15,
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
