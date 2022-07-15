using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using IndexerWpf.Models;
using Newtonsoft.Json;
#pragma warning disable CS0660
#pragma warning disable CS0661
namespace IndexerWpf.Classes
{
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
        public string Name => Path.GetFileName(FullPath);
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
        public string DirPath => Path.GetDirectoryName(FullPath);

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
