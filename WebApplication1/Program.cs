using System.Text;
using WebApplication1;
using WebServerLib;

new MQTTServer();
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
app.UseFileServer();//启用文件服务
app.Run();



