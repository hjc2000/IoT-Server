
namespace myIoTServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string ip = @"192.168.31.186";
            //string ip = @"192.168.31.156";
            string url = string.Format(@"http://{0}:80/", ip);
            string rootPath = @"D:\my_files\workspace\前端\CLI\vue_test\dist";

            // 如果web服务启动失败，在CMD输入
            // netsh http add urlacl url=http://192.168.31.156:80/ user=Everyone
            new MyWebServer(rootPath, url);
            new AcceptRegister(ip, 8848);
            new MyTcpServer(ip, 8848);

            //服务器启动时将所有数据库中的设备的在线状态设置为0
            new DataBase().GetCmd(@"UPDATE 基本信息 SET 在线状态 = 0").ExecuteNonQuery();
        }
    }
}
