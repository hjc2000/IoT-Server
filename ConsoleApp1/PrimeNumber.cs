using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    internal class PrimeNumber
    {
        /// <summary>
        /// 判断 factor 是不是 num 的因数
        /// </summary>
        /// <param name="num"></param>
        /// <param name="factor"></param>
        /// <returns></returns>
        public bool IsFactor(UInt64 num,UInt64 factor)
        {
            if(num%factor == 0)
            {
                //除得尽，说明是因数
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 储存素数的列表
        /// </summary>
        List<UInt64> _primeList = new List<UInt64>() { 2, 3 };

        public void GetPrime()
        {
            ulong output = _primeList.Last();
            Console.WriteLine(output);
        }
    }
}
