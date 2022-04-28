using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.StaticFiles;
using System.Text;
using WebServerLib;

//new MQTTClient();
var options = new WebApplicationOptions()
{
    ContentRootPath = "C:\\Users\\huang\\source\\repos\\WpfApp2\\BlazorApp1\\bin\\Release\\net6.0\\browser-wasm\\publish\\wwwroot",
    WebRootPath = "C:\\Users\\huang\\source\\repos\\WpfApp2\\BlazorApp1\\bin\\Release\\net6.0\\browser-wasm\\publish\\wwwroot",
};
WebApplicationBuilder builder = WebApplication.CreateBuilder(options);
WebApplication app = builder.Build();
app.Urls.Clear();
app.Urls.Add("http://192.168.31.186:7235");
app.Urls.Add("http://127.0.0.1:80");
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
     Console.WriteLine(str);
     context.Response.StatusCode = 200;
 });
app.MapPost("/mqtt/acl", (HttpContext context) =>
{
    //MQTT客户端 Pub 和 Sub 认证
    context.Response.StatusCode = 200;//不做过多认证，始终允许
});
/// <summary>
/// 将非文件请求重定向到根路径，通过含有点号来判断是一个文件。注意，必须
/// 放到文件中间件前面，紧挨着，否则会让路由中间件失效
/// 
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
var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".dll"] = "application/octet-stream";
provider.Mappings[".dat"] = "application/octet-stream";
provider.Mappings[".blat"] = "application/octet-stream";
app.UseDefaultFiles();
app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = provider,
    ServeUnknownFileTypes = true,
    DefaultContentType = "application/octet-stream",
});
app.Run();



