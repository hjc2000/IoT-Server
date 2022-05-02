using System.Data.SqlClient;
using System.Text;

namespace WebServerLib
{
	/**
	 * 在这里处理服务器接收到的MQTT消息。服务器收到MQTT消息是因为客户端想要
	 * 请求服务器处理一些事，例如获取数据库的数据等。
	 */
	public static class MqttMsgHandler
	{
		static public void Handle(ServerMqttClient client, string topic, byte[] payload)
        {
			string[] subTopics=topic.Split(new char[] { '/' });
			if(subTopics.Length>=2 && subTopics[0]=="server")
            {
				switch(subTopics[1])
                {
					case "GetDeviceInfo": //获取用户名下的所有设备的信息
						{
							/**如果客户端发送过来的载荷无法被以UTF8解码，会抛异常。
							*/
							string username = Encoding.UTF8.GetString(payload);
							string selection = string.Format(@"SELECT * FROM [device] WHERE username='{0}';", username);
							using(SqlDataReader reader = DataBase.GetReader(selection))
							{

								while(reader.Read())
                                {
									string pbTopic = string.Format("{0}/{1}", username, subTopics[1]);
									try
									{
										string? device_id = reader["device_id"] as string;
										if (device_id != null)
										{
											byte[] pbPayload = Encoding.UTF8.GetBytes(device_id);
											client.PublishAsync(pbTopic, pbPayload);
										}
									}
									catch { }
								}
							}
							break;
                        }

				}
            }
        }
	}
}
