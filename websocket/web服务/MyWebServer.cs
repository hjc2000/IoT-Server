using System;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace myIoTServer
{
    public class MyWebServer
    {        
        HttpListener _httpListenr;
        string _rootPath;
        //储存服务器根路径时会进行格式化操作
        string RootPath
        {
            get
            {
                return _rootPath;
            }
            set
            {
                _rootPath = value;
                _rootPath = _rootPath.Replace(@"/", @"\");//换成windows风格的路径
                if (_rootPath.EndsWith(@"\"))
                {
                    _rootPath = _rootPath.Remove(_rootPath.Length - 1);//移除末尾的斜杠
                }
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="rootPath">根路径</param>
        /// <param name="listenUrl">要监听的URL，类似于：@"http://192.168.31.186:80/"</param>
        public MyWebServer(string rootPath, string listenUrl)
        {
            if (!listenUrl.EndsWith(@"/"))
            {
                listenUrl += "/";
            }
            _httpListenr = new HttpListener();
            _httpListenr.Prefixes.Add(listenUrl);
            _httpListenr.Start();
            RootPath = rootPath;
            //创建一个新线程，用来获取Context
            new Thread(new ThreadStart(GetContextThread)).Start();
        }

        /// <summary>
        /// 在新线程中等待HTTP请求，获取Context对象
        /// </summary>
        void GetContextThread()
        {
            while (true)
            {
                HttpListenerContext context = _httpListenr.GetContext();
                HandleContextAsync(RootPath, context);//以异步方式处理请求
            }
        }

        static async void HandleContextAsync(string rootPath, HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;//获取请求内容
            try
            {
                //尝试一下，不知道是不是websocket请求，如果不是的话引发异常，进入catch
                HttpListenerWebSocketContext ws = await context.AcceptWebSocketAsync("chat");
                //是websocket请求，交给HandleWebSocket类去处理
                new WebSocketHandler(ws);
            }
            catch
            {
                Console.WriteLine("");

                await Task.Run(() => HttpRequestHandler.Handle(context, rootPath));
            }
        }
    }
}
