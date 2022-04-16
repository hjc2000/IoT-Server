using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;


namespace WpfApp2
{
    public class RenameFilesAndFolders: PropertyChengedRemind
    {
        string _rule;//重命名规则
        public string Rule
        {
            get
            {
                return _rule;
            }
            set
            {
                _rule = value;
                AnalyseRule();
                updataList();
            }
        }
        public string UserPath
        {
            get
            {
                return FilesOrFolders.Path;
            }
            set
            {
                FilesOrFolders.Path = value;
                Remind();
                updataList();
            }
        }
        void updataList()
        {
            FilesOrFoldersList = new ObservableCollection<FilesOrFolders>();
            RemindAt("FilesOrFoldersList");
            try
            {
                AddFilesOrFoldersToList(true);
                AddFilesOrFoldersToList(false);
            }
            catch (MyException ex)
            {
                if (ex.MyMessage == 1)
                {
                    MessageBox.Show("空路径！请选择文件夹");
                }
            }
            catch
            {
                MessageBox.Show("非法路径或路径不存在！");
            }
        }
        string TrimStringToTheEndFrom(string sourceString,string from)
        {
            //从指定字符串处将字符串裁剪，直到结尾。不包括指定的字符串
            int index = sourceString.LastIndexOf(from);
            if (!(index == -1))
            {
                return sourceString.Substring(index + 1, sourceString.Length - 1 - index);
            }
            else
            {
                return "";//如果没找到指定的字符串，则返回空串
            }
        }
        public ObservableCollection<FilesOrFolders> _filesOrFoldersList;
        public ObservableCollection<FilesOrFolders> FilesOrFoldersList
        {
            get 
            {
                return _filesOrFoldersList;
            }
            set
            {
                _filesOrFoldersList = value;
            }
        }
        void AddFilesOrFoldersToList(bool beFiles)
        {
            string[] url;
            string type;
            if(UserPath!=null&&UserPath.Length>0)
            {
                if (beFiles)
                {
                    type = "文件";
                    url = Directory.GetFiles(UserPath);
                }
                else
                {
                    url = Directory.GetDirectories(UserPath);
                    type = "文件夹";
                }
                foreach (string pickUrl in url)
                {
                    FilesOrFoldersList.Add(new FilesOrFolders(TrimStringToTheEndFrom(pickUrl, @"\"), type));
                }
            }
            else
            {
                throw (new MyException(1));
            }
        }
        public void HandleClick()
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            CommonFileDialogResult result = dialog.ShowDialog();
            if (result == CommonFileDialogResult.Ok)
            {
                UserPath = dialog.FileName;
            }
        }
        void AnalyseRule()
        {
            if(Rule.Length > 0)
            {
                for (int i = 0; i < FilesOrFoldersList.Count; i++)
                {
                    string oldName = FilesOrFoldersList[i].Name;
                    string path = FilesOrFolders.Path;
                    string oldUrl = path + oldName;
                    string subffix = @"." + TrimStringToTheEndFrom(oldName, @".");//获取文件格式
                    int index = oldName.LastIndexOf('.');
                    string oldNameWithoutSubffix = oldName.Remove(index);

                    string rule = Rule;//取得规则
                    if (rule.IndexOf('*') == -1)
                    {
                        rule += "*";
                    }
                    string newNameWithoutSubffix;//没有格式后缀的文件名
                    newNameWithoutSubffix = rule.Replace(@"*", i.ToString());
                    newNameWithoutSubffix = newNameWithoutSubffix.Replace("=", oldNameWithoutSubffix);

                    string newUrl = path + newNameWithoutSubffix + subffix;
                    File.Move(oldUrl, newUrl);
                }
            }
        }
    }
}
