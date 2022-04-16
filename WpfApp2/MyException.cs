using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp2
{
    public class MyException:ApplicationException
    {
        public MyException(int message)
        {
            _messate = message;
        }
        public int _messate = 0;
        public int MyMessage
        {
            get
            {
                return _messate;
            }
        }
    }
}
