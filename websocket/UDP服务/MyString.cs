using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace myIoTServer
{
    public class MyString
    {
        /// <summary>
        /// 从指定字符串处将字符串裁剪，直到结尾，包括指定的字符串。
        /// 对于指定的字符串，是从末尾开始往前查找，所以是从最后一个指定的字符串开始裁剪到结尾
        /// </summary>
        /// <param name="sourceString"></param>
        /// <param name="from"></param>
        /// <returns></returns>
        public static string TrimStringToTheEndFrom(string sourceString, string from)
        {
            int index = sourceString.LastIndexOf(from);
            if (!(index == -1))
            {
                return sourceString.Substring(index, sourceString.Length - index);
            }
            else
            {
                return "";//如果没找到指定的字符串，则返回空串
            }
        }

    }

    /*****************************************************************/


}
