using System;
using System.Text;
using System.Threading.Tasks;

namespace myIoTServer
{
    /// <summary>
    /// TcpClientHandler收到TCP发来的数据后会调用该类的DoJob函数。
    /// 该类的所有成员都是静态的
    /// </summary>
    internal class TcpJob
    {
        /// <summary>
        /// 该方法异步执行
        /// 接收一个数组，里面有完整的一个帧的数据，然后进行任务分配
        /// </summary>
        /// <param name="handler">TcpClientHandler在调用该函数时把this传递进来</param>
        /// <param name="tcpReceiveData">从TCP接受到的数据</param>
        public static async void DoJob(TcpClientHandler handler, byte[] tcpReceiveData)
        {
            //以异步方式执行
            await Task.Run(() =>
            {
                int offset = 0;
                //进行任务分配
                switch (tcpReceiveData[offset++])
                {
                    case 0:
                        PrintToWeb(handler, tcpReceiveData, offset);
                        break;
                    case 1://接收温度数据
                        ReceiveTemperature(handler, tcpReceiveData, offset);
                        break;
                    case 2://获取设备ID
                        GetDeviceInfo(handler, tcpReceiveData, offset);
                        break;
                }
            });
        }

        /// <summary>
        /// 获取温度数据
        /// </summary>
        static void ReceiveTemperature(TcpClientHandler handler, byte[] tcpReceiveData, int offset)
        {
            ushort temp=BitConverter.ToUInt16(tcpReceiveData, offset);
            double dTemp;

            //根据ushort的DS18B20的温度寄存器数据获取到double类型的温度
            Func<ushort, double> getDoubleTemp = (ushort usTemp) =>
             {
                 usTemp = (ushort)(usTemp & 0x07ff);//去除符号位
                 return usTemp / 16.0;
             };

            if ((short)temp > 0)
            {
                dTemp = getDoubleTemp(temp);
            }
            else
            {
                dTemp = -getDoubleTemp(temp);
            }

            string tempStr = dTemp.ToString("F4");//保留4位小数
            tempStr = "温度 " + tempStr;
            handler.SendDataToWeb(tempStr,true);
            handler.SendDataToTcpClient(new byte[] { 1 });
        }

        /// <summary>
        /// 在控制器与服务器建立TCP连接后获取设备信息。
        /// 会更新数据库
        /// </summary>
        static void GetDeviceInfo(TcpClientHandler handler, byte[] tcpReceiveData,int offset)
        {
            byte id = tcpReceiveData[offset++];
            handler.ClientID = id;
            string name = Encoding.UTF8.GetString(tcpReceiveData, offset, tcpReceiveData.Length - offset);
            handler.ClientName=name;

            Console.WriteLine("name={0} ID={1}", name, id);
        }

        /// <summary>
        /// 向网页的控制台打印来自ESP32的信息
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="tcpReceiveData"></param>
        /// <param name="offset"></param>
        static void PrintToWeb(TcpClientHandler handler,byte[] tcpReceiveData, int offset)
        {
            string str=Encoding.UTF8.GetString(tcpReceiveData, offset, tcpReceiveData.Length - offset);
            str = "打印 " + str;
            handler.SendDataToWeb(str,true);
        }
    }
}
