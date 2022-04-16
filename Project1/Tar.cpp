#include "Tar.h"

void Tar::DetectFrameHeader(void)
{
    if ((m_newData == 85) && (m_oldData == 85))
    {
        m_flag = 1;
    }
}

void Tar::GetFrameLength(void)
{
    m_count = m_newData;
    //如果数据长度是85，会导致帧重启，所以使用0代替85，数据长度不可能
    //真的是0，因为至少会有1字节的功能码，即使后面不跟数据
    if (m_count == 0)
    {
        m_count = 85;
    }
}

bool Tar::IsInsertedZero(void)
{
    if (m_newData == 0 && m_oldData == 85)
    {
        return true;
    }
    return false;
}

void Tar::GetData(void)
{
    if (!IsInsertedZero())
    {
        //如果这个数据不是被插入的用来转义的0
        m_readBuff.push_back(m_newData);
        m_count--;
        if (m_count == 0)
        {
            /*已经收集到一个帧了*/
            m_utarEvent(&m_readBuff);
            m_flag = 0;//回到等待帧头的状态
        }
    }
}

void Tar::AnalysisReadList(uint8_t data)
{
    m_newData = data;
    DetectFrameHeader();
    switch (m_flag)
    {
    case 0: //等待帧头的到来
    {
        break;
    }
    case 1://已经接收到帧头，此时应该重启帧
    {
        m_readBuff.clear();
        m_flag++;
        break;
    }
    case 2://接收帧长度
    {
        GetFrameLength();
        m_flag++;
        break;
    }
    case 3://接收数据
    {
        GetData();
        break;
    }
    }
    m_oldData = m_newData;
}
