using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace myIoTServer
{
    internal class WebJob
    {
        static public void DoJob(WebSocketHandler handler,string str)
        {
            Task.Run(() =>
            {
                string[] strArr = str.Split(' ');
                int index = 0;
                switch (strArr[index++])
                {
                    case "选择设备":
                        {
                            SelectDevice(handler,strArr,index);
                            break;
                        }
                    case "获取设备":
                        {
                            GetAllDevice(handler);
                            break;
                        }
                    default:
                        Console.WriteLine(str);//处理不了的就打印
                        break;
                }
            });
        }

        /// <summary>
        /// 从数据库中获取设备，将设备信息发送给web
        /// </summary>
        static void GetAllDevice(WebSocketHandler handler)
        {
        }

        /// <summary>
        /// 选中一个物联网设备
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="strArr"></param>
        /// <param name="index"></param>
        static void SelectDevice(WebSocketHandler handler, string[] strArr,int index)
        {
            byte id = byte.Parse(strArr[index++]);
            handler.CurrendDeviceId = id;
        }
    }
}
