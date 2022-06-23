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
    public class Proper : INotifyPropertyChanged
    {
        public Proper() { }
        public event PropertyChangedEventHandler PropertyChanged;
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

        protected bool SetProperty([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }
    }
    public class IndxElements : Proper
    {
        private ObservableCollection<IndxElement> allFiles;
        private string rootFolderPath;
        private string dateOfLastChange;

        public ObservableCollection<IndxElement> AllFiles { get => allFiles; set { SetProperty(ref allFiles, value); } }

        public ObservableCollection<IndxElement> VisualFolder { get => new ObservableCollection<IndxElement>(AllFiles.Where(t => t.Prnt == null)); }//visualFolder; set { SetProperty(ref visualFolder, value); } }

        public string RootFolderPath { get => rootFolderPath; set { SetProperty(ref rootFolderPath, value); } }

        public string DateOfLastChange { get => dateOfLastChange; set => SetProperty(ref dateOfLastChange, value); }

        public IndxElements()
        {

            RootFolderPath = string.Empty;
            Init();
        }

        private void Init()
        {
            AllFiles = new ObservableCollection<IndxElement>();
            IndxElement.Identificator = 0;
            StaticModel.LoadEndEvent += StaticModel_LoadEndEvent;
        }

        private void StaticModel_LoadEndEvent()
        {
            SetProperty(nameof(VisualFolder));
            SetProperty(nameof(Extentions));
        }

        public IndxElements(string path)
        {
            RootFolderPath = path;
            Init();
        }
        [JsonIgnore]
        public int TotalFiles { get => AllFiles.Count(t => t.Tp == IndxElement.Type.file); }
        [JsonIgnore]
        public ObservableCollection<string> Extentions { get => new ObservableCollection<string>(AllFiles.Select(t => t.Extension).Distinct()); }

        public static void SaveIndexes(IndxElements ie, string file_to_save)
        {
            ie.DateOfLastChange = DateTime.Now.ToString("G");
            File.WriteAllText(file_to_save, JsonConvert.SerializeObject(ie, Formatting.Indented));
        }
        public static IndxElements LoadInexes(string file_to_load)
        {
            if (File.Exists(file_to_load))
            {
                try
                {
                    var a = JsonConvert.DeserializeObject<IndxElements>(File.ReadAllText(file_to_load));
                    //Debug.WriteLine("DONE DESER");
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
    }
    public class IndxElement : Proper
    {
        public static int Identificator = 0;
        private string fullPath;
        private Type tp;
        private int id;
        private int? prnt;
        private bool isSelected;
        private bool isExpanded;

        public enum Type
        {
            folder,
            file
        }
        public string FullPath { get => fullPath; set { SetProperty(ref fullPath, value); } }


        public Type Tp { get => tp; set { SetProperty(ref tp, value); } }

        //public IndxElement? Prnt { get; set; }
        public int? Prnt { get => prnt; set { SetProperty(ref prnt, value); } }

        public int Id { get => id; set { SetProperty(ref id, value); } }

        [JsonIgnore]
        public IndxElement Parent { get => StaticModel.ElIndx.AllFiles.FirstOrDefault(t => t.Id == Prnt); }
        [JsonIgnore]
        public string ParentName { get => Parent == null ? this.Name : Parent.Name; }
        [JsonIgnore]
        public bool IsExpanded
        {
            get => isExpanded;
            set
            {
                if (value != isExpanded)
                    SetProperty(ref isExpanded, value);
                // Expand all the way up to the root.
                if (isExpanded && Parent != null)
                    Parent.IsExpanded = value;
            }
        }
        [JsonIgnore]
        public bool IsSelected
        {
            get => isSelected;
            set => SetProperty(ref isSelected, value);
        }

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
        public string Extension { get { if (Tp == Type.file) return new FileInfo(FullPath).Extension.ToLower(); else return "*"; } }
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
    }
}
