using System.Text;
using WebApplication1;

new MQTTServer();

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
WebApplication app = builder.Build();

RequestDelegate requestDelegate = async (HttpContext context) =>
  {
      HttpResponse re = context.Response;
      re.StatusCode = 200;
      re.ContentType = "text/plain";
      Stream body = re.Body;
      string hello = "hollo world";
      byte[] helloByte = Encoding.UTF8.GetBytes(hello);
      await body.WriteAsync(helloByte, 0, helloByte.Length);
  };
app.MapGet("/", requestDelegate);

app.Run();
