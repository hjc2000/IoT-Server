using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace myIoTServer
{
    /*****************************************************************/
    /// <summary>
    /// 处理请求连接的客户端
    /// </summary>
    internal class TcpClientHandler
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="client"></param>
        public TcpClientHandler(TcpClient client)
        {
            _client = client;
            _client.ReceiveTimeout = 1000 * 10;
            _client.SendTimeout = 1000 * 10;
            _networkStream = _client.GetStream();
            //创建新线程用来处理客户端
            new Thread(new ThreadStart(ReceiveDataThread)).Start();
            Console.WriteLine("已收到TCP连接请求");
        }
        ~TcpClientHandler()
        {
            Dispose();
        }

        /// <summary>
        /// 本对象连接着的TCP客户端
        /// </summary>
        TcpClient _client = null;

        /// <summary>
        /// 本对象连接着的TCP客户端的网络流
        /// </summary>
        NetworkStream _networkStream;

        byte _clientID = 0;
        bool _idHasBeenSet = false;
        /// <summary>
        /// 本对象连接着的TCP客户端的ID
        /// </summary>
        public byte ClientID
        {
            get { return _clientID; }
            set
            {
                _clientID = value;
                try
                {
                    /*释放字典中过期值的资源。释放资源后该值会从字典中消失*/
                    _handlerDir[_clientID].Dispose();
                }
                catch { }//如果字典中不存在该值会引起异常
                _handlerDir.Add(_clientID, this);
                _idHasBeenSet = true;
            }
        }


        /// <summary>
        /// 用来储存这个类的对象，键是UInt64类型的，储存着是设备ID，对应着与该
        /// 设备连接的TcpClientHandler
        /// </summary>
        static public Dictionary<byte, TcpClientHandler> _handlerDir = new Dictionary<byte, TcpClientHandler>();

        public delegate void SendDataToWebDelegate(string str, bool end);
        public event SendDataToWebDelegate SendDataToWebEvent;

        /// <summary>
        /// 将数据发送给订阅了SendDataToWebEvent事件的网页
        /// </summary>
        /// <param name="str"></param>
        /// <param name="end"></param>
        public void SendDataToWeb(string str, bool end)
        {
            SendDataToWebEvent?.Invoke(str, end);
        }

        void AfterGetFrame(byte[] buff)
        {
            TcpJob.DoJob(this, buff);
        }

        /// <summary>
        /// 接收客户端数据的线程
        /// </summary>
        void ReceiveDataThread()
        {
            FrameDivider dv = new FrameDivider();
            dv.GetFullFrame += AfterGetFrame;
            byte[] tcpReceiveData = new byte[1024];

            try
            {
                while (true)//在死循环中不断监听客户端发过来的数据
                {
                    int count = _networkStream.Read(tcpReceiveData, 0, tcpReceiveData.Length);
                    dv.InputData(tcpReceiveData, 0, count);
                }
            }
            catch
            {
                Dispose();
            }
        }

        /// <summary>
        /// 发送数据给TCP客户端。
        /// web端通过查询字典，找到与特定设备连接的TcpClientHandler对象，调用
        /// 这个对象的该方法，将数据发送给物联网设备
        /// </summary>
        /// <param name="sendBuff"></param>
        public void SendDataToTcpClient(byte[] sendBuff)
        {
            try
            {
                byte[] len = BitConverter.GetBytes((UInt16)sendBuff.Length);
                _networkStream.Write(len, 0, len.Length);//发送长度
                _networkStream.Write(sendBuff, 0, sendBuff.Length);//发送数据
            }
            catch
            {
                Dispose();
            }
        }

        /// <summary>
        /// 用来释放资源
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _client.Close();
                _client.Dispose();
                _networkStream.Close();
                _networkStream.Dispose();
                Console.WriteLine("TCP客户端断线");
                //将自己从字典中删除
                _handlerDir.Remove(ClientID);
            }
        }
        bool _disposed = false;
    }
}
