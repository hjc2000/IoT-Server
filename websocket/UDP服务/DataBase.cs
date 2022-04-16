using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace myIoTServer
{
    /// <summary>
    /// 操作数据库的类
    /// </summary>
    public class DataBase
    {
        SqlConnection _sqlConnection;

        /// <summary>
        /// 构造函数
        /// </summary>
        public DataBase()
        {
            //连接数据库
            _sqlConnection = new SqlConnection(@"server=DESKTOP-NI9AEL6;database=设备信息;Trusted_Connection=SSPI");
            _sqlConnection.Open();
        }
        ~DataBase()
        {
            try
            {
                _sqlConnection.Close();
            }
            catch { }
        }

        /// <summary>
        /// 根据输入的命令，获取命令对象
        /// </summary>
        /// <param name="cmdStr"></param>
        /// <returns></returns>
        public SqlCommand GetCmd(string cmdStr)
        {
            return new SqlCommand(cmdStr, _sqlConnection);
        }

        /// <summary>
        /// 执行非查询命令
        /// </summary>
        /// <param name="cmd"></param>
        public int ExecuteCmdNonQuery(string cmd)
        {
            //执行完后会返回受影响的行数，不过不需要用到
            return GetCmd(cmd).ExecuteNonQuery();
        }

        /// <summary>
        /// 执行查询命令并获取阅读器
        /// </summary>
        /// <param name="selectStr"></param>
        /// <returns></returns>
        public SqlDataReader GetReader(string selectStr)
        {
            SqlCommand cmd = GetCmd(selectStr);
            return cmd.ExecuteReader();
        }
    }
}
