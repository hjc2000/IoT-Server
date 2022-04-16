using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace myIoTServer
{
    /// <summary>
    /// 创建TCP服务并监听
    /// </summary>
    public class MyTcpServer
    {
        TcpListener _tcpListener = null;

        /// <summary>
        /// 初始化服务器对象
        /// </summary>
        /// <param name="ip">要监听的IP</param>
        /// <param name="port">要监听的端口</param>
        public MyTcpServer(string ip, Int32 port)
        {
            //保存到字段，以便新线程能够获得信息
            _tcpListener = new TcpListener(IPAddress.Parse(ip), port);
            //在新线程中监听
            new Thread(new ThreadStart(ListenThread)).Start();
        }

        /// <summary>
        /// 监听，等待客户端连接的线程
        /// </summary>
        void ListenThread()
        {
            _tcpListener.Start();//开始监听
            while (true)
            {
                //创建一个新对象处理请求连接的客户端
                new TcpClientHandler(_tcpListener.AcceptTcpClient());//挂起，等待连接，然后创建新对象用来处理连接，然后继续等待连接
            }
        }
    }
}
