using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace myIoTServer
{
    /// <summary>
    /// 处理WebSocket对象
    /// </summary>
    internal class WebSocketHandler
    {
        WebSocket _ws;

        byte _currentDeviceId = 0;
        /// <summary>
        /// 网页当前选择的设备ID
        /// </summary>
        public byte CurrendDeviceId
        {
            get { return _currentDeviceId; }
            set
            {
                try
                {
                    //退订旧设备的事件，退订一个没有订阅过的事件不会引发异常
                    TcpClientHandler._handlerDir[_currentDeviceId].SendDataToWebEvent -= SendDataToWebSocket;
                }
                catch { }//字典如果没找到指定键，get操作会引发异常
                _currentDeviceId = value;
                try
                {
                    TcpClientHandler._handlerDir[_currentDeviceId].SendDataToWebEvent += SendDataToWebSocket;
                    Console.WriteLine("web已选择设备，在线，ID=" + _currentDeviceId);
                }
                catch
                {
                    Console.WriteLine("web已选择设备，不在线，ID=" + _currentDeviceId);
                }
            }
        }

        static List<WebSocketHandler> _webSocketHandlerList=new List<WebSocketHandler>();

        /// <summary>
        /// 向所有web发送数据
        /// </summary>
        /// <param name="str"></param>
        /// <param name="end"></param>
        public static void Broadcast(string str, bool end)
        {
            foreach(WebSocketHandler handler in _webSocketHandlerList)
            {
                handler.SendDataToWebSocket(str, end);
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="wsContext">接收到的 HttpListenerWebSocketContext 对象</param>
        public WebSocketHandler(HttpListenerWebSocketContext wsContext)
        {
            //获取WebSocket对象
            _ws = wsContext.WebSocket;
            _webSocketHandlerList.Add(this);
            new Thread(new ThreadStart(WaitForDataFromWebSocketThread)).Start();
        }
        ~WebSocketHandler()
        {
            Dispose();
        }

        /// <summary>
        /// 将数据发送给该WebSocket对象。
        /// 将该类的对象储存到 List 中，想要发送数据时，遍历 List，使用线程池调用该函数
        /// </summary>
        /// <param name="param">会被强制转换为 string</param>
        async public void SendDataToWebSocket(string str,bool end)
        {
            if (!str.EndsWith("。"))
            {
                str = str + "。";//如果不以句号结尾，添加句号
            }
            byte[] sendBuff = Encoding.UTF8.GetBytes(str);
            ArraySegment<byte> arrSegment = new ArraySegment<byte>(sendBuff);
            CancellationTokenSource source = new CancellationTokenSource();
            CancellationToken token = source.Token;
            try
            {
                await _ws.SendAsync(arrSegment, WebSocketMessageType.Text, end, token);
            }
            catch
            {
                Dispose();
            }
        }

        /// <summary>
        /// 在新线程中等待数据
        /// </summary>
        async void WaitForDataFromWebSocketThread()
        {
            ArraySegment<byte> arrSegment = new ArraySegment<byte>(new byte[1024]);
            CancellationTokenSource source = new CancellationTokenSource();
            CancellationToken token = source.Token;
            try
            {
                while (true)
                {
                    await _ws.ReceiveAsync(arrSegment, token);
                    byte[] dataArr = arrSegment.ToArray();
                    string str = Encoding.UTF8.GetString(dataArr);
                    try
                    {
                        str = str.Remove(str.LastIndexOf("。"));//找到句号，没有找到会引发异常
                        WebJob.DoJob(this, str);
                    }
                    catch
                    {
                        Console.WriteLine("执行web发来的命令时发生异常");
                    }
                }
            }
            catch
            {
                Dispose();
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        void Dispose()
        {
            if (!_disposed)
            {
                _ws?.Dispose();
                try
                {
                    TcpClientHandler._handlerDir[(byte)_currentDeviceId].SendDataToWebEvent -= SendDataToWebSocket;
                }
                catch { }
                Console.WriteLine("客户端已断开WebSocket连接");
                _disposed = true;
            }
        }
        bool _disposed = false;
    }
}
