using System;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServerLib
{
    /// <summary>
    /// 操作数据库的类
    /// </summary>
    class DataBase
    {
        static SqlConnection GetConnection()
        {
            SqlConnection c = new SqlConnection(@"server=DESKTOP-NI9AEL6;database=设备信息;Trusted_Connection=SSPI");
            c.Open();
            return c;
        }
        /// <summary>
        /// 执行非查询命令
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns>受影响的行数</returns>
        static public int ExecuteCmdNonQuery(string cmd)
        {
            using (SqlConnection c = GetConnection())
            {
                int count = new SqlCommand(cmd, c).ExecuteNonQuery();
                return count;
            }
        }
        /// <summary>
        /// 执行查询命令并获取阅读器。用完了别忘记关闭
        /// </summary>
        /// <param name="selection"></param>
        /// <returns></returns>
        static public SqlDataReader GetReader(string selection)
        {
            SqlCommand cmd = new SqlCommand(selection, GetConnection());
            /**通过设置 CommandBehavior 使得 SqlDataReader 被关闭后 SqlConnection 
             * 被自动关闭
             */
            return cmd.ExecuteReader(System.Data.CommandBehavior.CloseConnection);
        }
    }
}

