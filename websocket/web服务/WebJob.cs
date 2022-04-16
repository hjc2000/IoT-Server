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
            DataBase database = new DataBase();
            SqlDataReader sqlDataReader = database.GetReader(@"SELECT * FROM 基本信息");
            while (sqlDataReader.Read())
            {
                string strToWeb = string.Format("设备信息 {0},{1},{2}", sqlDataReader["设备名称"], (int)sqlDataReader["ID"], (bool)sqlDataReader["在线状态"]);
                handler.SendDataToWebSocket(strToWeb, true);
            }
            sqlDataReader.Close();
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
