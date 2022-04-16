using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace myIoTServer
{
    /// <summary>
    /// 处理HTTP请求
    /// </summary>
    internal class HttpRequestHandler
    {
        /// <summary>
        /// 处理HTTP请求
        /// </summary>
        /// <param name="context">Context对象</param>
        /// <param name="rootPath">服务器根路径</param>
        public static void Handle(HttpListenerContext context, string rootPath)
        {
            //获取所请求的文件在本地的路径
            string path = GetFilePath(context, rootPath);
            Console.WriteLine(path);
            //根据文件的路径获取内容类型
            string contentType = GetContentType(path);
            //处理文件请求
            HandleFileRuquest(path, contentType, context);
        }

        /// <summary>
        /// 获取根据请求头获取请求的文件在本地的绝对路径
        /// </summary>
        /// <param name="context">http的Context对象</param>
        /// <param name="rootPath">服务器的根路径</param>
        /// <returns></returns>
        static string GetFilePath(HttpListenerContext context, string rootPath)
        {
            //获取请求内容
            HttpListenerRequest request = context.Request;

            //获取请求行中的URL
            string url = request.Url.ToString();

            //获取去除协议和主机名后的路径：相对路径
            int index = url.IndexOf(@"/", 8);
            string relativePath;
            relativePath = url.Substring(index, url.Length - index);

            //对相对路径进行一些修改
            if (relativePath.EndsWith("/"))
            {
                relativePath += "index.html";
            }
            relativePath = relativePath.Replace(@"/", @"\");//换成windows风格

            //合成绝对路径，用来访问本地文件系统
            string path = rootPath + relativePath;
            return path;
        }

        /// <summary>
        /// 根据文件的绝对路径获取content-type属性
        /// </summary>
        /// <param name="filePath">文件的绝对路径</param>
        /// <returns></returns>
        static string GetContentType(string filePath)
        {
            string format = MyString.TrimStringToTheEndFrom(filePath, ".");
            switch (format)//格式查询表
            {
                //文本
                case ".htm":
                    return "text/html";
                case ".html":
                    return "text/html";
                case ".js":
                    return "application/javascript";
                case ".mjs":
                    return "application/javascript";
                case ".css":
                    return "text/css";
                case ".xml":
                    return "text/xml";
                case ".pdf":
                    return "application/x-pdf";
                //图片
                case ".jpg":
                    return "image/jpeg";
                case ".png":
                    return "image/png";
                case ".gif":
                    return "image/gif";
                case ".ico":
                    return "image/x-icon";
                //压缩包
                case ".zip":
                    return "application/x-zip";
                case ".gz":
                    return "application/x-gzip";
                default:
                    return "text/plain";
            }
        }

        /// <summary>
        /// 处理HTTP的文件请求
        /// </summary>
        /// <param name="filePath">文件的完整路径</param>
        /// <param name="contentType">文件类型</param>
        /// <param name="context">http的context对象</param>
        /// <returns></returns>
        static bool HandleFileRuquest(string filePath, string contentType, HttpListenerContext context)
        {
            HttpListenerResponse response = context.Response;//获取等会要发送的响应内容对象
            response.StatusCode = (int)HttpStatusCode.OK;
            var outputStream = response.OutputStream;//获得流
            bool returnValue = false;
            if (File.Exists(filePath))
            {
                var fileStream = File.OpenRead(filePath);//打开文件
                var contentLength = fileStream.Length;//获取文件流长度
                response.ContentLength64 = contentLength;
                response.AddHeader("Content-Type", contentType);
                response.AddHeader("Content-Length", contentLength.ToString());
                fileStream.CopyTo(outputStream);
                fileStream.Close();//关闭文件
                returnValue = true;
            }
            else
            {
                string respinseString = "404 not found";
                byte[] sendBuff = Encoding.UTF8.GetBytes(respinseString);
                response.ContentLength64 = sendBuff.Length;
                response.AddHeader("Content-Type", @"text/plain");
                response.AddHeader("Content-Length", sendBuff.Length.ToString());
                outputStream.Write(sendBuff, 0, sendBuff.Length);
            }
            //释放资源
            response.Close();//这个方法会连流一起关闭
            return returnValue;
        }
    }

}
