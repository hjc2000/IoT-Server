using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TCP服务
{
    internal class Program
    {
        static void Main(string[] args)
        {
            MyTcpListener.AddListenr("127.0.0.1", 8080);
            Console.WriteLine("服务启动");
        }
    }
}
