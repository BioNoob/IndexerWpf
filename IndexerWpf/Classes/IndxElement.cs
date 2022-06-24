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
    public class IndxElements : Proper, IDisposable
    {
        private WpfObservableRangeCollection<IndxElement> allFiles;
        private string rootFolderPath;
        private string dateOfLastChange;
        private WpfObservableRangeCollection<string> extentions;
        [JsonIgnore]
        public int TotalFiles { get => AllFiles.Count(t => t.Tp == IndxElement.Type.file); }

        [JsonIgnore]
        public WpfObservableRangeCollection<string> Extentions { get => extentions; set { SetProperty(ref extentions, value); } }

        public string RootFolderPath { get => rootFolderPath; set { SetProperty(ref rootFolderPath, value); } }
        public string DateOfLastChange { get => dateOfLastChange; set => SetProperty(ref dateOfLastChange, value); }
        public WpfObservableRangeCollection<IndxElement> AllFiles { get => allFiles; set { SetProperty(ref allFiles, value); } }

        public IndxElements()
        {

            RootFolderPath = string.Empty;
            Init();
        }

        private void Init()
        {
            AllFiles = new WpfObservableRangeCollection<IndxElement>();
            Extentions = new WpfObservableRangeCollection<string>();
            StaticModel.LoadEndEvent += StaticModel_LoadEndEvent;
            IndxElement.Identificator = 0;
        }

        public void Clear()
        {
            AllFiles = new WpfObservableRangeCollection<IndxElement>();
            Extentions = new WpfObservableRangeCollection<string>();
            IndxElement.Identificator = 0;
            RootFolderPath = string.Empty;
        }

        private void StaticModel_LoadEndEvent()
        {
            Extentions.Clear();
            Extentions.AddRange(AllFiles.Select(t => t.Extension).Distinct());
        }

        public IndxElements(string path)
        {
            RootFolderPath = path;
            Init();
        }
        public void SaveIndexes(string file_to_save)
        {
            this.DateOfLastChange = DateTime.Now.ToString("G");
            File.WriteAllText(file_to_save, JsonConvert.SerializeObject(this, Formatting.Indented));
        }
        public void LoadInexes(string file_to_load)
        {
            if (File.Exists(file_to_load))
            {
                try
                {
                    var a = JsonConvert.DeserializeObject<IndxElements>(File.ReadAllText(file_to_load));
                    RootFolderPath = a.RootFolderPath;
                    DateOfLastChange = a.DateOfLastChange;
                    AllFiles = a.AllFiles;
                    a.Dispose();
                    //Debug.WriteLine("DONE DESER");
                    //return a;
                }
                catch (Exception e)
                {
                    //return null;
                    System.Windows.Forms.MessageBox.Show($"File {file_to_load} load error\n{e.Message}!", "Error load", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    throw;
                }

            }
            else
            {
                System.Windows.Forms.MessageBox.Show($"File {file_to_load} was deleted!", "Error load", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                throw new Exception();
                //return null;
            }
        }

        public void Dispose()
        {
            AllFiles = null;
            //VisualFolder = null;
            //Extentions = null;
            DateOfLastChange = null;
            RootFolderPath = null;
            //ID = -1;
            StaticModel.LoadEndEvent -= StaticModel_LoadEndEvent;
            GC.SuppressFinalize(this);
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
        public IndxElement Parent { get => StaticModel.ElIndx.FirstOrDefault(t => t.Id == Prnt); }
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
                SetProperty(nameof(GetUriImg));
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
        public string Name => Path.GetFileNameWithoutExtension(FullPath);//new FileInfo(FullPath).Name;
        [JsonIgnore]
        public string Extension
        {
            get
            {
                if (Tp == Type.file)
                {
                    var ext = Path.GetExtension(FullPath);
                    if (string.IsNullOrWhiteSpace(ext))
                    {
                        return "*";
                    }
                    else
                    {
                        return ext.ToLower();
                    }
                }
                else
                    return "*";
            }
        }
        [JsonIgnore]
        public string DirPath => Path.GetDirectoryName(FullPath);//new FileInfo(FullPath).DirectoryName;

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
        public IList<IndxElement> Items
        {
            get
            {
                IList<IndxElement> childNodes = new ObservableCollection<IndxElement>();
                if (Tp == Type.folder)
                {
                    var a = StaticModel.ElIndx.Where(t => t.Prnt == Id);
                    if (a != null)
                    {
                        foreach (var item in a)
                        {
                            childNodes.Add(item);
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
                        if (IsExpanded)
                            return "/Resources/опен.png";
                        else
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
