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
    public class MyTcpListener
    {
        public static void AddListenr(string ip,Int32 port)
        {
            new HandleListenr(new TcpListener(IPAddress.Parse(ip), port));//创建一个新对象用来监听
        }
    }
    internal class HandleListenr
    {
        TcpListener _listener = null;
        public HandleListenr(TcpListener tcpListener)
        {
            _listener = tcpListener;//保存到字段，以便新线程能够获得信息
            Thread thread = new Thread(new ThreadStart(ListenInNewThread));
            thread.Start();
        }
        void ListenInNewThread()
        {
            _listener.Start();//开始监听
            while (true)
            {
                new HandleClient(_listener.AcceptTcpClient());//挂起，等待连接，然后创建新对象用来处理连接，然后继续等待连接
            }
        }

    }
    internal class HandleClient
    {
        TcpClient _client = null;
        public HandleClient(TcpClient client)
        {
            _client = client;
            Thread thread = new Thread(new ThreadStart(HandleClientInNewThread));
            thread.Start();
        }
        void HandleClientInNewThread()
        {
            byte[] readBuff = new byte[_client.ReceiveBufferSize];
            NetworkStream networkStream = _client.GetStream();
            try
            {
                //Enter the listening loop
                while (true)
                {
                    int numOfRead;
                    while ((numOfRead = networkStream.Read(readBuff, 0, _client.ReceiveBufferSize)) != 0)//不断地调用Read方法。如果远程客户端断线，会引起异常。如果没有断线，但是没有数据发过来，则numOfRead==0
                    {
                        string sendData = Encoding.UTF8.GetString(readBuff, 0, numOfRead);
                        Console.WriteLine(String.Format("Received: {0}", sendData));

                        // Process the data sent by the client.
                        sendData = sendData.ToUpper();

                        byte[] sendBuff = Encoding.UTF8.GetBytes(sendData);
                        // Send back a response.
                        networkStream.Write(sendBuff, 0, sendBuff.Length);
                        Console.WriteLine(String.Format("Sent: {0}", sendData));
                    }
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            catch
            {
                Console.WriteLine("断线");
                _client.Close();
                networkStream.Close();
                return;
            }
        }
    }
}
