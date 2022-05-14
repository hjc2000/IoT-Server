using Newtonsoft.Json;
using System.Data.SqlClient;

namespace WebServerLib
{
	/// <summary>
	/// 用于网页登录验证和MQTT验证
	/// </summary>
	public class Authentication
	{
		//承载待验证信息的类
		public class MqttConnectAuthInfo
		{
			[JsonProperty("clientid")]
			public string Clientid { get; set; } = string.Empty;
			[JsonProperty("username")]
			public string Username { get; set; } = string.Empty;
			[JsonProperty("password")]
			public string Password { get; set; } = string.Empty;
		}
		public class MqttTopicAuthInfo
		{
			[JsonProperty("clientid")]
			public string Clientid { get; set; } = string.Empty;
			[JsonProperty("username")]
			public string Username { get; set; } = string.Empty;
			/*不知道什么原因，emqx在订阅/发布认证时发过来的password是null
			 可能是因为启用http认证方式后，emqx就不再负责储存密码了，它认为
			这是HTTP服务器应该干的事*/
			[JsonProperty("password")]
			public string Password { get; set; } = string.Empty;
			[JsonProperty("topic")]
			public string Topic { get; set; } = string.Empty;
		}
		public class UserInfo
		{
			public string Username { get; set; } = string.Empty;
			public string Password { get; set; } = string.Empty;
		}
		/// <summary>
		/// 查询数据库，看用户名密码是否正确，正确
		/// </summary>
		/// <param name="userinfo">UserInfo对象</param>
		/// <returns>验证是否通过</returns>
		public static bool DoAuth(UserInfo? userinfo)
		{
			bool result = false;//验证通过赋值为true
			if (userinfo != null)
			{
				string selection = string.Format("SELECT * FROM [user] WHERE [username]='{0}';", userinfo.Username);
				using (SqlDataReader reader = DataBase.GetReader(selection))
				{
					if (reader.Read())
					{
						//找到该用户名
						if (userinfo.Password == reader["password"] as string)
						{
							//检查密码正确
							result = true;
						}
					}
				}
			}
			return result;
		}
		/// <summary>
		/// 对ESP32订阅/发布主题的行为进行认证
		/// </summary>
		/// <param name="clientId"></param>
		/// <param name="topic"></param>
		/// <returns></returns>
		public static bool ESP32TopicAuth(string clientId,string topic)
		{
			string[] subTopics = topic.Split('/');
			if (subTopics[0] == clientId)
			{
				return true;
			}
			return false;
		}
		/// <summary>
		/// 对网页客户端的订阅/发布主题的请求进行认证
		/// </summary>
		/// <param name="username">MQTT客户端连接时使用的用户名</param>
		/// <param name="topic">MQTT客户端要订阅/发布的主题</param>
		/// <returns></returns>
		public static bool WebTopicAuth(string username, string topic)
		{
			string[] subTopics = topic.Split('/');//获取该用户要通信的设备的ID
			string selection = string.Format("SELECT * FROM [device] WHERE username='{0} AND device_id='{1}';", username, subTopics[0]);
			using (SqlDataReader reader = DataBase.GetReader(selection))
			{
				if (reader.Read())//如果有该条记录，则认证通过
				{
					return true;
				}
			}
			return false;
		}
		/// <summary>
		/// 通过查询数据库，验证该用户是否有该设备
		/// </summary>
		/// <param name="username"></param>
		/// <param name="deviceId"></param>
		/// <returns></returns>
		public static bool TheUserHasTheDevice(string username,string deviceId)
		{
			string selection = string.Format("SELECT * FROM [device] WHERE username='{0}' AND device_id='{1}';", username, deviceId);
			using (SqlDataReader reader = DataBase.GetReader(selection))
			{
				if (reader.Read())//如果有该条记录，则认证通过
				{
					return true;
				}
			}
			return false;
		}
	}
}
