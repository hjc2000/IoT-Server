using System.Text;
using WebApplication1;
using WebServerLib;

new MQTTServer();
string rootPath = @"D:/my_files/workspace/前端/CLI/vue_test/dist";

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
WebApplication app = builder.Build();

RequestDelegate requestDelegate = async (HttpContext context) =>
  {
      HttpResponse response = context.Response;
      HttpRequest request = context.Request;
      Console.WriteLine(request.Path);
      response.StatusCode = 200;
      response.ContentType = Mime.GetContextType("a.txt");
      Console.WriteLine(response.ContentType);
      Stream body = response.Body;
      string hello = "hollo world";
      byte[] helloByte = Encoding.UTF8.GetBytes(hello);
      await body.WriteAsync(helloByte, 0, helloByte.Length);
  };
app.MapGet("/", requestDelegate);

app.Run();
