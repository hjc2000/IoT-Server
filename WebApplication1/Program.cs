using System.Text;
using WebServerLib;

new MQTTClient();
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
WebApplication app = builder.Build();
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

app.UseFileServer();//启用文件服务
app.Run();



