using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using IndexerWpf.Models;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;
#pragma warning disable CS0660
#pragma warning disable CS0661
namespace IndexerWpf.Classes
{
    public class IndxElements : INotifyPropertyChanged
    {
        private ObservableCollection<IndxElement> allFiles;
        private ObservableCollection<IndxElement> visualFolder;
        private string rootFolderPath;

        public ObservableCollection<IndxElement> AllFiles { get => allFiles; set { SetProperty(ref allFiles, value); } }

        public ObservableCollection<IndxElement> VisualFolder { get => visualFolder; set { SetProperty(ref visualFolder, value); } }

        public string RootFolderPath { get => rootFolderPath; set { SetProperty(ref rootFolderPath, value); } }
        
        public IndxElements()
        {

            RootFolderPath = string.Empty;
            Init();
        }

        private void Init()
        {
            AllFiles = new ObservableCollection<IndxElement>();
            VisualFolder = new ObservableCollection<IndxElement>();
            //StaticModel.RemoveItemEvent += StaticModel_RemoveItem;
        }

        public IndxElements(string path)
        {
            RootFolderPath = path;
            Init();
        }
        public int TotalFiles { get => AllFiles.Count(t => t.Tp == IndxElement.Type.file); }

        public ObservableCollection<string> Extentions { get => new ObservableCollection<string>(AllFiles.Select(t => t.Extension).Distinct()); }

        public event PropertyChangedEventHandler PropertyChanged;

        public static void SaveIndexes(IndxElements ie, string file_to_save)
        {
            File.WriteAllText(file_to_save, JsonConvert.SerializeObject(ie, Formatting.Indented));
        }
        public static IndxElements LoadInexes(string file_to_load)
        {
            if (File.Exists(file_to_load))
            {
                try
                {
                    var a = JsonConvert.DeserializeObject<IndxElements>(File.ReadAllText(file_to_load));
                    return a;
                }
                catch (Exception)
                {
                    return null;
                }

            }
            else
            {
                return null;
            }
        }
        protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (!Equals(field, newValue))
            {
                field = newValue;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                return true;
            }

            return false;
        }
    }
    public class IndxElement : INotifyPropertyChanged
    {
        public static int Identificator = 1;
        private string fullPath;
        private Type tp;
        private int id;
        private int? prnt;
        private bool founded;

        public enum Type
        {
            folder,
            file
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public string FullPath { get => fullPath; set { SetProperty(ref fullPath, value); } }


        public Type Tp { get => tp; set { SetProperty(ref tp, value); } }

        //public IndxElement? Prnt { get; set; }
        public int? Prnt { get => prnt; set { SetProperty(ref prnt, value); } }

        public int Id { get => id; set { SetProperty(ref id, value); } }

        [JsonIgnore]
        public bool Founded { get => founded; set => SetProperty(ref founded, value); }

        public IndxElement()
        {
            Init();
        }
        public IndxElement(string path)
        {
            FullPath = path;
            Init();
        }
        private void Init()
        {
            Id = Identificator++;
        }

        [JsonIgnore]
        public string Name => new FileInfo(FullPath).Name;
        [JsonIgnore]
        public string Extension { get { if (Tp == Type.file) return new FileInfo(FullPath).Extension; else return "*"; } }
        [JsonIgnore]
        public string DirPath => new FileInfo(FullPath).DirectoryName;

        public static bool operator ==(IndxElement a, IndxElement b)
        {
            if (a is null && b is null)
                return true;
            if (a is null)
                return false;
            if (b is null)
                return false;
            if (a.FullPath == b.FullPath && a.Tp == b.Tp)
                return true;
            else
                return false;


        }
        public static bool operator !=(IndxElement a, IndxElement b)
        {
            if (a is null && b is null)
                return false;
            if (a is null)
                return true;
            if (b is null)
                return true;
            if (a.FullPath == b.FullPath && a.Tp == b.Tp)
                return false;
            else
                return true;
        }
        public void OpenFolder()
        {
            Process.Start("explorer.exe", $"/select, {FullPath}");
        }
        public void OpenFile()
        {
            Process pr = new Process();
            pr.StartInfo.FileName = FullPath;
            pr.StartInfo.UseShellExecute = true;
            pr.Start();
        }

        [JsonIgnore]
        private CommandHandler _openfolder;
        [JsonIgnore]
        public CommandHandler OpenFolderCommand
        {
            get
            {
                return _openfolder ?? (_openfolder = new CommandHandler(obj =>
                {
                    OpenFolder();
                },
                (obj) => !string.IsNullOrEmpty(FullPath)
                ));
            }
        }
        [JsonIgnore]
        private CommandHandler _openfile;
        [JsonIgnore]
        public CommandHandler OpenFileCommand
        {
            get
            {
                return _openfile ?? (_openfile = new CommandHandler(obj =>
                {
                    OpenFile();
                },
                (obj) => !string.IsNullOrEmpty(FullPath) && Tp == Type.file
                ));
            }
        }
        public override string ToString()
        {
            return Name;
        }

        [JsonIgnore]
        public IList<object> Items
        {
            get
            {
                IList<object> childNodes = new ObservableCollection<object>();
                if (Tp == Type.folder)
                {
                    var a = StaticModel.ElIndx.AllFiles.Where(t => t.Prnt == Id);
                    if (a != null)
                    {
                        foreach (var item in a)
                        {
                            childNodes.Add(item);
                            //if(item.Tp == Type.folder)
                            //StaticModel.InvokeRemoveItemEvent(item);
                        }
                    }
                    return childNodes;
                }
                else
                    return null;
            }
        }
        [JsonIgnore]
        public string GetUriImg
        {
            get
            {
                switch (Tp)
                {
                    case Type.folder:
                        return "/Resources/папка.png";
                    case Type.file:
                        return "/Resources/док.png";
                    default:
                        return "";
                }
            }
        }

        protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (!Equals(field, newValue))
            {
                field = newValue;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                return true;
            }

            return false;
        }
    }
}
