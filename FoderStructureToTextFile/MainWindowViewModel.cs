using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.WindowsAPICodePack.Dialogs;
using ZetaLongPaths;

namespace FoderStructureToTextFile
{
    public class MainWindowViewModel : ObservableObject
    {
        public MainWindowViewModel()
        {
            Filename = @"C:\Users\" + Environment.UserName + @"\Downloads\fk.txt";
        }
        public static string Filename { get; set; }

        private ICommand _openFolder;
        public ICommand OpenFolder
        {
            get
            {
                if (_openFolder == null)
                {
                    _openFolder = new RelayCommand(PerformOpenFolder);
                }

                return _openFolder;
            }
        }
        
        private void PerformOpenFolder(object commandParameter)
        {
            string ourPath = GetFolderWithStructure(out bool isCanceled1);
            if (isCanceled1) return;

            var di = new ZlpDirectoryInfo(ourPath);
            WalkDirectoryTree(di, 1);
        }

        private string GetFolderWithStructure(out bool isCanceled)
        {
            string str = @"C:\Users\" + Environment.UserName;
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.InitialDirectory = str;
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                str = dialog.FileName;
                isCanceled = false;
            }
            else
            {
                isCanceled = true;
            }
            return str;
        }

        private string FileForReport(out bool isCanceled)
        {
            string str = @"C:\Users\" + Environment.UserName + @"\Downloads";
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.InitialDirectory = str;
            dialog.IsFolderPicker = false;
            dialog.DefaultFileName = "report.txt";
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                str = dialog.FileName;
                if (!str.Contains(".txt")) str += ".txt";
                isCanceled = false;
            }
            else
            {
                isCanceled = true;
            }
            return str;
        }

        static void WalkDirectoryTree(ZlpDirectoryInfo root, int level)
        {
            ZlpDirectoryInfo[] subDirs = null;
            ZlpFileInfo[] files = null;

            try
            {
                subDirs = root.GetDirectories();
                files = root.GetFiles();
                foreach(var file in files)
                {
                    if (file.Name.Contains(".rfa"))
                    {
                        WriteToFile(Filename, file.Directory + "\t" + level.ToString() + "\t" + file.Name + "\t" + level.ToString());
                    }
                };
                foreach (ZlpDirectoryInfo dirInfo in subDirs)
                {
                    //WriteToFile(Filename, dirInfo.FullName + "\t" + level.ToString());
                    WalkDirectoryTree(dirInfo, level + 1);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());
            }
        }


        public static void WriteToFile(string fileName, string txt)
        {
            Encoding u16LE = Encoding.Unicode; // UTF-16 little endian
            
            if (!File.Exists(fileName))
            {
                try
                {
                    using (FileStream fs = File.Create(fileName))
                    {
                        byte[] byteString = new UnicodeEncoding().GetBytes("");
                        fs.Write(byteString, 0, byteString.Length);
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.ToString());
                }
            }
            using (StreamWriter writer = new StreamWriter(fileName, true))
            {
                writer.WriteLine(txt);
            }

        }

    }

    public class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
    public class RelayCommand : ICommand
    {
        private Action<object> _execute;
        private Func<object, bool> _canExecute;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }
    }
}
