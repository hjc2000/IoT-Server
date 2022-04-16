using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace myIoTServer
{
    /// <summary>
    /// 处理来自物联网设备的注册请求
    /// </summary>
    internal class AcceptRegister
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public AcceptRegister(string ip, int port)
        {
            IPEndPoint localEp = new IPEndPoint(IPAddress.Parse(ip), port);
            _udpClient = new UdpClient(localEp);
            _udpClient.JoinMulticastGroup(IPAddress.Parse("224.0.1.0"));//加入多播组
            new Thread(new ThreadStart(WaitForRegisterThread)).Start();//启动线程用来监听
        }

        UdpClient _udpClient;
        /// <summary>
        /// 多播组的远端终结点
        /// </summary>
        static IPEndPoint _multicastEp = new IPEndPoint(IPAddress.Parse("224.0.1.0"), 7777);

        /// <summary>
        /// 将数据发送给多播组的所有UDP客户端
        /// </summary>
        /// <param name="str"></param>
        public void ToAllUdpClient(string str)
        {
            byte[] sendBuff = Encoding.UTF8.GetBytes(str);
            _udpClient.Send(sendBuff, sendBuff.Length, _multicastEp);
        }

        /// <summary>
        /// 在新线程中等待注册请求
        /// </summary>
        async void WaitForRegisterThread()
        {
            while (true)
            {
                UdpReceiveResult result = await _udpClient.ReceiveAsync();
                IPEndPoint remoteEp = result.RemoteEndPoint;
                Console.Write("来自" + remoteEp.ToString() + "的消息：");
                string str = Encoding.UTF8.GetString(result.Buffer);
                string[] strArr = str.Split(new char[]{ '\n'});//根据换行符拆分字符串
                foreach(string eachStr in strArr)
                {
                    Console.WriteLine(eachStr);
                    if (eachStr == "registe")
                    {
                        byte[] sendBuff = Encoding.UTF8.GetBytes("yes\n");
                        _udpClient.Send(sendBuff, sendBuff.Length, remoteEp);//原路返回
                        Console.WriteLine("同意注册");
                    }
                }
            }
        }
    }
}
