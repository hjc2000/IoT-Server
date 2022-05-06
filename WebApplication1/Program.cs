using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.StaticFiles;
using System.Data.SqlClient;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using WebServerLib;

//设置服务器的文件系统根路径
var options = new WebApplicationOptions()
{
	ContentRootPath = "C:\\Users\\huang\\source\\repos\\WpfApp2\\BlazorApp1\\bin\\Release\\net6.0\\browser-wasm\\publish\\wwwroot",
	WebRootPath = "C:\\Users\\huang\\source\\repos\\WpfApp2\\BlazorApp1\\bin\\Release\\net6.0\\browser-wasm\\publish\\wwwroot",
};
WebApplicationBuilder builder = WebApplication.CreateBuilder(options);
WebApplication app = builder.Build();
app.Urls.Clear();
app.Urls.Add("http://127.0.0.1:80");
foreach (var ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
{
	// 下面的判断过滤 IPv4 地址
	if (ip.AddressFamily == AddressFamily.InterNetwork)
	{
		string locallIp = ip.ToString();
		app.Urls.Add(string.Format("http://{0}:7235", locallIp));
	}
}
app.UseWebSockets();
app.UseRouting();//使用路由中间件
app.Map("/ws", async (HttpContext context) =>
{
	if (context.WebSockets.IsWebSocketRequest)
	{
		Console.WriteLine("收到websocket请求");
		//接收 WebSocket 对象后传递给 Echo 函数去维护 WebSocket 句柄
		var webSocket = await context.WebSockets.AcceptWebSocketAsync("chat");
		ArraySegment<byte> arrSegment = new ArraySegment<byte>(new byte[1024]);
		CancellationTokenSource source = new CancellationTokenSource();
		CancellationToken token = source.Token;
		while (true)
		{
			await webSocket.ReceiveAsync(arrSegment, token);
			byte[] dataArr = arrSegment.ToArray();
			string str = Encoding.UTF8.GetString(dataArr);
			try
			{
				str = str.Remove(str.LastIndexOf("。"));//找到句号，没有找到会引发异常
				Console.WriteLine(str);
			}
			catch
			{
				Console.WriteLine("执行web发来的命令时发生异常");
			}
		}
	}
});
app.MapPost("/mqtt/auth", async (HttpContext context) =>
 {
	 //MQTT客户端连接认证
	 int length = 0;
	 if (context.Request.ContentLength != null)
	 {
		 length = (int)context.Request.ContentLength;
	 }
	 byte[] body = new byte[length];
	 await context.Request.Body.ReadAsync(body, 0, body.Length);
	 string str = Encoding.UTF8.GetString(body);
	 MqttAuthentication? au = JsonSerializer.Deserialize<MqttAuthentication>(str, new JsonSerializerOptions()
	 {
		 PropertyNameCaseInsensitive = true,//反序列化不区分大小写
	 });
	 int statueCode = 404;
	 if (au != null)
	 {
		 //其他客户端认证通道
		 string selection = string.Format("SELECT * FROM [user] WHERE [username]='{0}';", au.Username);
		 using (SqlDataReader reader = DataBase.GetReader(selection))
		 {
			 if (reader.Read())
			 {
				 //找到该用户名
				 if (au.Password == reader["password"] as string)
				 {
					 Console.WriteLine("密码是：" + au.Password);
					 //检查密码正确
					 statueCode = 200;
				 }
			 }
		 }
	 }
	 context.Response.StatusCode = statueCode;
 });
app.MapPost("/mqtt/acl", (HttpContext context) =>
{
	//MQTT客户端 Pub 和 Sub 认证
	context.Response.StatusCode = 200;//不做过多认证，始终允许
});
/**将非文件请求重定向到根路径，通过含有点号来判断是一个文件。注意，必须
 * 放到文件中间件前面，紧挨着，否则会让路由中间件失效
 * 为什么要重定向？因为web选择了一个路由的路径后刷新页面，会导致浏览器直接从
 * 路由路径向服务器请求，而这里没有任何东西，会导致返回404
 */
app.Use(async (context, next) =>
{
	if (!context.Request.Path.ToString().Contains("."))
	{
		context.Request.Path = new PathString("/");
	}
	await next();
});
//文件content-type提供者
var provider = new FileExtensionContentTypeProvider();
/**添加mime表的内容。如果不添加，provider 的默认mime表内没有dll等文件的
 * content-type，会造成浏览器接收文件后以错误的方式处理
 */
provider.Mappings[".dll"] = "application/octet-stream";
provider.Mappings[".dat"] = "application/octet-stream";
provider.Mappings[".blat"] = "application/octet-stream";
app.UseDefaultFiles();//使用默认文件中间件
app.UseStaticFiles(new StaticFileOptions
{
	ContentTypeProvider = provider,//文件content-type提供者
	ServeUnknownFileTypes = true,//对未知类型的文件提供服务
	DefaultContentType = "application/octet-stream",//未知的文件类型的content-type
});
app.Run();
