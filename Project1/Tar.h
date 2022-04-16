#ifndef TAR_H
#define TAR_H
#include <stdint.h>
#include <list>
#include "D:\my_files\workspace\myLib\C++\CircularQueue.h"
#include <functional>
#include "D:\my_files\workspace\myLib\C++\Delegate.h"
using namespace std;

class Tar
{
private:
    //读缓冲区，分析完的数据放到这里
    CircularQueue<uint8_t> m_readBuff = CircularQueue<uint8_t>(100);

    uint8_t m_flag = 0; //用来标志工作阶段
    void AnalysisReadList(uint8_t data);
    uint8_t m_oldData = 0;
    uint8_t m_newData = 0;
    uint8_t m_count = 0;//对接收到的数据域字节数计数（减计数，在收到长度时赋值）

    /// <summary>
    /// 检测帧头，通过控制m_flag来控制解包程序的走向
    /// </summary>
    /// <param name=""></param>
    /// <returns></returns>
    void DetectFrameHeader(void);

    /// <summary>
    /// 获取帧的长度
    /// </summary>
    /// <param name=""></param>
    void GetFrameLength(void);

    /// <summary>
    /// 当前收到的数据是不是用来转义的被插入的0
    /// </summary>
    /// <param name=""></param>
    /// <returns></returns>
    bool IsInsertedZero(void);

    /// <summary>
    /// 正式接收数据
    /// </summary>
    /// <param name=""></param>
    void GetData(void);

public:
    Delegate m_utarEvent; //串口收集到一个帧后触发该事件
};
#endif // TAR_H
