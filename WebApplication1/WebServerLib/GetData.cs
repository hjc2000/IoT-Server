using Newtonsoft.Json.Linq;
using System.Data.SqlClient;
using System.Net.Http.Headers;
using System.Text;

namespace WebServerLib
{
	public class GetData
	{
		/// <summary>
		/// 获取用户名下的设备信息
		/// </summary>
		/// <returns></returns>
		public static async Task<List<string>?> GetDevices(string? username)
		{
			if (username != null)
			{
				List<string> devices = new List<string>();
				string selection = string.Format("SELECT * FROM [device] WHERE username='{0}';", username);
				using (SqlDataReader reader = DataBase.GetReader(selection))
				{
					while (await reader.ReadAsync())
					{
						string? device = reader["device_id"] as string;
						if (device != null)
						{
							devices.Add(device);
						}
					}
				}
				return devices;
			}
			else
			{
				return null;
			}
		}
		/// <summary>
		/// 接收请求体的内容。注意，请求体的内容不能大于int能表示的最大值，
		/// 如果大于了，不要使用这个方法
		/// </summary>
		/// <param name="context"></param>
		/// <returns>byte[]对象</returns>
		public static async Task<byte[]?> GetRequestContentAsBytes(HttpContext context)
		{
			if (context.Request.ContentLength <= int.MaxValue)
			{
				//准备接收请求体
				int length = (int)context.Request.ContentLength;
				byte[] buffer = new byte[length];
				int offset = 0;
				//等到读取完所有字节
				while (offset < length)
				{
					offset += await context.Request.Body.ReadAsync(buffer.AsMemory(offset, length - offset));
				}
				//接收完毕
				return buffer;
			}
			return null;
		}
	}
}
