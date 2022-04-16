using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace myIoTServer
{
    /// <summary>
    /// 从字节流中区分出帧，得到完整的帧后触发事件
    /// </summary>
    internal class FrameDivider
    {
        public event Action<byte[]> GetFullFrame;

        int _flag = 0;//阶段标志
        ushort _frameSize = 0;
        /// <summary>
        /// 帧长度
        /// </summary>
        ushort FramSize
        {
            get { return _frameSize; }
            set
            {
                _frameSize = value;
                _haveGet = 0;
            }
        }

        byte[] _sizeBuff = new byte[2]; //存放代表帧长度的两个字节的数组
        byte[] _dataBuff = null; //存放数据的数组

        /// <summary>
        /// 已经获取的帧中的字节数
        /// </summary>
        int _haveGet = 0;

        /// <summary>
        /// 剩余的待获取的字节数
        /// </summary>
        int LastToGet
        {
            get
            {
                return _frameSize - _haveGet;
            }
        }

        /// <summary>
        /// 读取一次数据后，不管读到多少，是不是刚好是完整的一个帧，都调用该方法，该方法自会处理，
        /// 不用用户操心
        /// </summary>
        /// <param name="buff">储存着待分析的数据的数组</param>
        /// <param name="offset">偏移量</param>
        /// <param name="count">有效数据的长度，因为不一定offset以后的数据都是有效的</param>
        public void InputData(byte[] buff, int offset, int count)
        {
            int index = offset;
            //设置下限
            if (index < 0)
            {
                index = 0;
            }

            int maxIndex = offset + count - 1;//最后一个元素的索引
            //设置上限
            if (maxIndex >= buff.Length)
            {
                maxIndex = buff.Length - 1;
            }


            while (index <= maxIndex)
            {
                switch (_flag)
                {
                    case 0://获取帧长度的第一个字节
                        {
                            _sizeBuff[0] = buff[index++];
                            _flag++;
                            break;
                        }
                    case 1://获取帧长度的第二个字节
                        {
                            _sizeBuff[1] = buff[index++];
                            FramSize = BitConverter.ToUInt16(_sizeBuff, 0);//获取到帧长度
                            if (FramSize > 0)
                            {
                                //帧不为空，进入下一步，获取数据
                                _dataBuff = new byte[FramSize];
                                _flag++;
                            }
                            else
                            {
                                //收到空的帧
                                _flag = 0;
                            }
                            break;
                        }
                    case 2://接收数据
                        {
                            if (LastToGet > 0)
                            {
                                int getInThisTime = Min(maxIndex - index + 1, LastToGet);
                                Buffer.BlockCopy(buff, index, _dataBuff, _haveGet, getInThisTime);
                                _haveGet += getInThisTime;
                                index += getInThisTime;
                            }
                            else
                            {
                                //收到完整的一个帧了，触发事件
                                GetFullFrame?.Invoke(_dataBuff);
                                _flag = 0;
                            }
                            break;
                        }
                }
            }
        }

        /// <summary>
        /// 返回两个数中的最小值
        /// </summary>
        /// <param name="num1"></param>
        /// <param name="num2"></param>
        /// <returns></returns>
        int Min(int num1, int num2)
        {
            if (num1 < num2)
            {
                return num1;
            }
            else
            {
                return num2;
            }
        }
    }
}
