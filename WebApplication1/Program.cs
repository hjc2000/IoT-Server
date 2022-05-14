using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.StaticFiles;
using System.Net;
using System.Net.Sockets;
using System.Text;
using WebServerLib;
using Newtonsoft.Json;

//设置服务器的文件系统根路径
var options = new WebApplicationOptions()
{
	ContentRootPath = "C:\\Users\\huang\\source\\repos\\WpfApp2\\BlazorApp1\\bin\\Release\\net6.0\\browser-wasm\\publish\\wwwroot",
	WebRootPath = "C:\\Users\\huang\\source\\repos\\WpfApp2\\BlazorApp1\\bin\\Release\\net6.0\\browser-wasm\\publish\\wwwroot",
};
WebApplicationBuilder builder = WebApplication.CreateBuilder(options);
WebApplication app = builder.Build();
app.Urls.Clear();
app.Urls.Add("http://localhost:80");
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
/// <summary>
/// 用于登录验证
/// </summary>
app.MapPost("/loginAuth", async (HttpContext context) =>
 {
	 //允许来自本地的跨域请求
	 context.Response.Headers.Add("Access-Control-Allow-Origin", "http://localhost:5127");
	 byte[]? buff = await GetData.GetRequestContentAsBytes(context);
	 if (buff != null)
	 {
		 //将请求体内容反序列化为JSON
		 string body = Encoding.UTF8.GetString(buff);
		 Authentication.UserInfo? userinfo = JsonConvert.DeserializeObject<Authentication.UserInfo>(body);
		 //查询数据库，对用户信息进行验证
		 if (Authentication.DoAuth(userinfo))
		 {
			 context.Response.StatusCode = 200;
			 return;
		 }
	 }
	 context.Response.StatusCode = 404;
 });
/// <summary>
/// 获取用户名下的设备信息
/// </summary>
app.MapPost("/getDeviceIdList", async (HttpContext context) =>
{
	//允许来自本地的跨域请求
	context.Response.Headers.Add("Access-Control-Allow-Origin", "http://localhost:5127");
	byte[]? buff = await GetData.GetRequestContentAsBytes(context);
	if (buff != null)
	{
		//将请求体内容反序列化为JSON
		string bodyString = Encoding.UTF8.GetString(buff);
		Authentication.UserInfo? userinfo = JsonConvert.DeserializeObject<Authentication.UserInfo>(bodyString);
		//查询数据库，对用户信息进行验证
		if (Authentication.DoAuth(userinfo))
		{
			//查询数据库获取设备信息
			List<string>? devices = await GetData.GetDevices(userinfo?.Username);
			if (devices != null)
			{
				string deviceJson = JsonConvert.SerializeObject(devices);
				context.Response.StatusCode = 200;
				Console.WriteLine(deviceJson);
				Memory<byte> bodyMemory = Encoding.UTF8.GetBytes(deviceJson).AsMemory();
				context.Response.ContentLength = bodyMemory.Length;
				await context.Response.Body.WriteAsync(bodyMemory);
				return;
			}
		}
	}
	context.Response.StatusCode = 404;
});
/// <summary>
/// 处理WebSocket连接请求
/// </summary>
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
/// <summary>
/// MQTT客户端连接认证
/// </summary>
app.MapPost("/mqtt/auth", async (HttpContext context) =>
 {
	 byte[]? buff = await GetData.GetRequestContentAsBytes(context);
	 if (buff != null)//如果请求体不为空
	 {
		 string str = Encoding.UTF8.GetString(buff);
		 Authentication.MqttConnectAuthInfo? au = JsonConvert.DeserializeObject<Authentication.MqttConnectAuthInfo>(str);
		 if (au != null)
		 {
			 if (au.Clientid.StartsWith(au.Username))
			 {
				 Console.WriteLine("是网页");
				 /*客户端ID以连接时使用的用户名开头，说明是网页，
				  * 且客户端ID是合法的，接下来需要验证用户名、密码是否正确*/
				 Authentication.UserInfo userInfo = new()
				 {
					 Password = au.Password,
					 Username = au.Username,
				 };
				 if (Authentication.DoAuth(userInfo))
				 {
					 Console.WriteLine("MQTT连接请求验证通过");
					 context.Response.StatusCode = 200;
					 return;
				 }
			 }
			 else if (au.Clientid.StartsWith("esp32"))
			 {
				 //客户端ID以esp32开头的是物联网设备

				 /*接下来需要判断连接所使用的用户名名下是否有ID为该客户端
				  ID的设备*/
				 if (Authentication.TheUserHasTheDevice(au.Username, au.Clientid))
				 {
					 //接下来需要验证用户名密码是否正确
					 Authentication.UserInfo userInfo = new()
					 {
						 Password = au.Password,
						 Username = au.Username,
					 };
					 if (Authentication.DoAuth(userInfo))
					 {
						 context.Response.StatusCode = 200;
						 return;
					 }
				 }
			 }
		 }
	 }
	 context.Response.StatusCode = 404;
 });
/// <summary>
/// MQTT订阅、发布认证
/// </summary>
app.MapPost("/mqtt/acl", async (HttpContext context) =>
{
	Console.WriteLine("订阅/发布认证");
	byte[]? buff = await GetData.GetRequestContentAsBytes(context);
	if (buff != null)//如果请求体不为空
	{
		string str = Encoding.UTF8.GetString(buff);
		Console.WriteLine(str);
		Authentication.MqttTopicAuthInfo? au = JsonConvert.DeserializeObject<Authentication.MqttTopicAuthInfo>(str);
		if (au != null)
		{
			if (au.Clientid.StartsWith(au.Username))
			{
				Console.WriteLine("是网页");
				int index = au.Topic.IndexOf('/');
				string deviceId = au.Topic.Substring(0, index);
				//检查该用户名下是否有该设备
				if (Authentication.TheUserHasTheDevice(au.Username, deviceId))
				{
					context.Response.StatusCode = 200;
					return;
				}
			}
			else if (au.Clientid.StartsWith("esp32"))
			{
				Console.WriteLine("是物联网设备");
				//检查是不是主场
				if (au.Topic.StartsWith(au.Clientid))
				{
					context.Response.StatusCode = 200;
					return;
				}
			}
		}
	}
	context.Response.StatusCode = 404;
});
/// <summary>
/// 将非文件请求重定向到根路径，通过含有点号来判断是一个文件。注意，必须
/// 放到文件中间件前面，紧挨着，否则会让路由中间件失效
/// 为什么要重定向？因为web选择了一个路由的路径后刷新页面，会导致浏览器直接从
/// 路由路径向服务器请求，而这里没有任何东西，会导致返回404
/// </summary>
app.Use(async (context, next) =>
{
	if (!context.Request.Path.ToString().Contains("."))
	{
		context.Request.Path = new PathString("/");
	}
	await next();
});
/// <summary>
/// 文件content-type提供者
/// </summary>
var provider = new FileExtensionContentTypeProvider();
/**添加mime表的内容。如果不添加，provider 的默认mime表内没有dll等文件的
 * content-type，会造成浏览器接收文件后以错误的方式处理
 */
provider.Mappings[".dll"] = "application/octet-stream";
provider.Mappings[".dat"] = "application/octet-stream";
provider.Mappings[".blat"] = "application/octet-stream";
/// <summary>
/// 使用默认文件中间件
/// </summary>
app.UseDefaultFiles();
/// <summary>
/// 使用静态文件中间件
/// </summary>
app.UseStaticFiles(new StaticFileOptions
{
	ContentTypeProvider = provider,//文件content-type提供者
	ServeUnknownFileTypes = true,//对未知类型的文件提供服务
	DefaultContentType = "application/octet-stream",//未知的文件类型的content-type
});
app.Run();
