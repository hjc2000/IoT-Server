using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp2
{
    public class FilesOrFoldersTemplate
    {
        public FilesOrFoldersTemplate(string name, string type)
        {
            _name = name;
            _type = type;
        }
        //默认路径为桌面
        static string _path;
        static public string Path//文件或文件夹路径
        {
            get
            {
                return _path;
            }
            set
            {
                _path = value.Replace(@"/", @"\");//替换成windows的路径分隔符
                if (_path.Length != 0)
                {
                    if (!_path.EndsWith(@"\"))
                    {
                        _path += @"\";
                    }
                }
            }
        }
        public string _name;
        public string Name//不包含路径的文件或文件夹名
        {
            get
            {
                return _name;
            }
            set
            {
                File.Move(_path + _name, _path + value);//重命名
                _name = value;
            }
        }
        public string _type;
        public string Type
        {
            get
            {
                return _type;
            }
        }
    }
}
