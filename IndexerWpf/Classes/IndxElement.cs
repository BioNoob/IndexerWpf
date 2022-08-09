using IndexerWpf.Classes;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using IndexerWpf.Models;
using Newtonsoft.Json;
using System.Windows.Input;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

#pragma warning disable CS0660
namespace IndexerWpf.Classes
{
    //public class IndxElement : Proper
    //{
    //    public static int Identificator = 0;
    //    private string fullPath;
    //    private Type tp;
    //    private int id;
    //    private int? prnt;
    //    private bool isSelected;
    //    private bool isExpanded;

    //    public enum Type
    //    {
    //        folder,
    //        file
    //    }
    //    public string FullPath { get => fullPath; set { SetProperty(ref fullPath, value); } }


    //    public Type Tp
    //    {
    //        get => tp;
    //        set
    //        {
    //            SetProperty(ref tp, value);
    //            {
    //                switch (value)
    //                {
    //                    case Type.folder:
    //                        GetUriImg = "/Resources/папка.png";
    //                        break;
    //                    case Type.file:
    //                        GetUriImg = "/Resources/док.png";
    //                        break;
    //                    default:
    //                        GetUriImg = "";
    //                        break;
    //                }
    //            }
    //        }
    //    }

    //    //public IndxElement? Prnt { get; set; }
    //    public int? Prnt { get => prnt; set { SetProperty(ref prnt, value); } }

    //    public int Id { get => id; set { SetProperty(ref id, value); } }

    //    [JsonIgnore]
    //    public IndxElement Parent { get => ParentTree.AllFiles.FirstOrDefault(t => t.Id == Prnt); }//StaticModel.ElIndx.FirstOrDefault(t => t.Id == Prnt); }

    //    [JsonIgnore]
    //    public IndxElements ParentTree { set => SetProperty(ref parentTree, value); get => parentTree; }
    //    private IndxElements parentTree;

    //    [JsonIgnore]
    //    public string ParentName { get => Parent == null ? this.Name : Parent.Name; }
    //    [JsonIgnore]
    //    public bool IsExpanded
    //    {
    //        get => isExpanded;
    //        set
    //        {
    //            if (value != isExpanded)
    //                SetProperty(ref isExpanded, value);
    //            // Expand all the way up to the root.
    //            if (isExpanded && Parent != null)
    //                Parent.IsExpanded = value;
    //            if (Tp == Type.folder)
    //                if (value)
    //                    GetUriImg = "/Resources/опен.png";
    //                else
    //                    GetUriImg = "/Resources/папка.png";
    //        }
    //    }
    //    [JsonIgnore]
    //    public bool IsSelected
    //    {
    //        get => isSelected;
    //        set => SetProperty(ref isSelected, value);
    //    }

    //    public IndxElement()
    //    {
    //        ParentTree = null;
    //        Init();
    //    }
    //    public IndxElement(string path, IndxElements parent_tree)
    //    {
    //        FullPath = path;
    //        ParentTree = parent_tree;
    //        Init();
    //    }
    //    private void Init()
    //    {
    //        Id = Identificator++;
    //    }

    //    [JsonIgnore]
    //    public string Name => Path.GetFileName(FullPath);
    //    [JsonIgnore]
    //    public string Extension
    //    {
    //        get
    //        {
    //            if (Tp == Type.file)
    //            {
    //                var ext = Path.GetExtension(FullPath);
    //                if (string.IsNullOrWhiteSpace(ext))
    //                {
    //                    return "*";
    //                }
    //                else
    //                {
    //                    return ext.ToLower();
    //                }
    //            }
    //            else
    //                return "*";
    //        }
    //    }
    //    [JsonIgnore]
    //    public string DirPath => Path.GetDirectoryName(FullPath);

    //    public static bool operator ==(IndxElement a, IndxElement b)
    //    {
    //        if (a is null && b is null)
    //            return true;
    //        if (a is null)
    //            return false;
    //        if (b is null)
    //            return false;
    //        if (a.FullPath == b.FullPath && a.Tp == b.Tp)
    //            return true;
    //        else
    //            return false;


    //    }
    //    public static bool operator !=(IndxElement a, IndxElement b)
    //    {
    //        if (a is null && b is null)
    //            return false;
    //        if (a is null)
    //            return true;
    //        if (b is null)
    //            return true;
    //        if (a.FullPath == b.FullPath && a.Tp == b.Tp)
    //            return false;
    //        else
    //            return true;
    //    }
    //    public void OpenFolder()
    //    {
    //        Process.Start("explorer.exe", $"/select, {FullPath}");
    //    }
    //    public void OpenFile()
    //    {
    //        Process pr = new Process();
    //        pr.StartInfo.FileName = FullPath;
    //        pr.StartInfo.UseShellExecute = true;
    //        pr.Start();
    //    }

    //    [JsonIgnore]
    //    private CommandHandler _openfolder;
    //    [JsonIgnore]
    //    public CommandHandler OpenFolderCommand
    //    {
    //        get
    //        {
    //            return _openfolder ?? (_openfolder = new CommandHandler(obj =>
    //            {
    //                OpenFolder();
    //            },
    //            (obj) => !string.IsNullOrEmpty(FullPath)
    //            ));
    //        }
    //    }
    //    [JsonIgnore]
    //    private CommandHandler _openfile;
    //    [JsonIgnore]
    //    public CommandHandler OpenFileCommand
    //    {
    //        get
    //        {
    //            return _openfile ?? (_openfile = new CommandHandler(obj =>
    //            {
    //                OpenFile();
    //            },
    //            (obj) => !string.IsNullOrEmpty(FullPath) && Tp == Type.file
    //            ));
    //        }
    //    }
    //    public override string ToString()
    //    {
    //        return Name;
    //    }

    //    private IList<IndxElement> items = new ObservableCollection<IndxElement>();
    //    [JsonIgnore]
    //    public IList<IndxElement> Items
    //    {
    //        get
    //        {
    //            return items;
    //        }
    //        set
    //        {
    //            SetProperty(ref items, value);
    //        }
    //    }

    //    public IList<IndxElement> buildtree()
    //    {
    //        IList<IndxElement> childNodes = new ObservableCollection<IndxElement>();
    //        if (Tp == Type.folder)
    //        {
    //            //var a = StaticModel.ElIndx.Where(t => t.Prnt == Id);
    //            var a = ParentTree.AllFiles.Where(t => t.Prnt == id);
    //            if (a != null)
    //            {
    //                foreach (var item in a)
    //                {
    //                    childNodes.Add(item);
    //                }
    //            }
    //            return childNodes;
    //        }
    //        else
    //            return null;
    //    }

    //    private string getUriImg;
    //    [JsonIgnore]
    //    public string GetUriImg
    //    {
    //        set => SetProperty(ref getUriImg, value);
    //        get => getUriImg;
    //    }
    //}



    public class IndxElementNew : Proper, IEqualityComparer<IndxElementNew>, System.IEquatable<IndxElementNew>
    {
        public enum Type
        {
            folder,
            file
        }
        public IndxElementNew()
        {
            //Id = Identificator++;
            allLowerElements = null;
            Parent = null;
            childElements = null;
        }
        public IndxElementNew(string _fullpath, Type tp, IndxElementNew prnt, int id)
        {
            isScaned = true;
            if (tp != Type.file)
                ChildElements = new WpfObservableRangeCollection<IndxElementNew>();
            else
                ChildElements = null;
            FullPath = _fullpath;
            Tp = tp;
            Id = id;//Identificator++;
            Parent = prnt;
            allLowerElements = null;
        }
        private string fullPath;
        private Type tp;
        private int id;
        private string getUriImg;
        private WpfObservableRangeCollection<IndxElementNew> childElements;
        private WpfObservableRangeCollection<IndxElementNew> allLowerElements;
        private IndxElementNew parent;
        private bool isSelected;
        private bool isExpanded;
        private bool isScaned = false;
        [JsonProperty(Order = -2)]
        public int Id { get => id; set { SetProperty(ref id, value); } }
        public Type Tp
        {
            get => tp;
            set
            {
                SetProperty(ref tp, value);
                GetUriImg = value switch
                {
                    Type.folder => "/Resources/папка.png",
                    Type.file => "/Resources/док.png",
                    _ => "",
                };
            }
        }
        [JsonProperty(Order = -1)]
        public string FullPath
        {
            get => fullPath;
            set
            {
                SetProperty(ref fullPath, value);
                StaticModel.InvokeIdincreasedEvent(id, value);
                if (isScaned)
                {
                    FileFolderExist = true;
                    FileFolderExistString = "";
                    return;
                }
                FileFolderExist = Tp switch
                {
                    Type.folder => Directory.Exists(FullPath),
                    Type.file => File.Exists(FullPath),
                    _ => false,
                };
                if (!FileFolderExist)
                    FileFolderExistString = Tp switch
                    {
                        Type.folder => "Folder not found",
                        Type.file => "File not found",
                        _ => "",
                    };
                else
                    FileFolderExistString = "";
            }
        }
        public WpfObservableRangeCollection<IndxElementNew> ChildElements { get => childElements; set => SetProperty(ref childElements, value); }

        [JsonIgnore]
        public int CountElements
        {
            get
            {
                if (ChildElements != null)
                    return ChildElements.Sum(t => t.CountElements) + 1;
                else return 1;
            }
        }
        private bool fileFolderExist;
        [JsonIgnore]
        public bool FileFolderExist
        {
            get => fileFolderExist;
            set => SetProperty(ref fileFolderExist, value);
        }
        private string fileFolderExistString;
        [JsonIgnore]
        public string FileFolderExistString
        {
            get => fileFolderExistString;
            set => SetProperty(ref fileFolderExistString, value);
        }
        [JsonIgnore]
        public bool IsSelected
        {
            get => isSelected;
            set => SetProperty(ref isSelected, value);
        }
        [JsonIgnore]
        public bool IsExpanded
        {
            get => isExpanded;
            set
            {
                if (value != isExpanded)
                {
                    SetProperty(ref isExpanded, value);
                    if (value && IsSelected)
                        StaticModel.InvokeItemIsExpandChange(this);
                }

                // Expand all the way up to the root.

                if (isExpanded && Parent != null)
                    if (Parent.IsExpanded != value)
                        Parent.IsExpanded = value;
                if (!isExpanded && ChildElements != null)
                    AllLowerElements.ToList().ForEach(t => { if (t.isExpanded != value) t.IsExpanded = value; });
                if (Tp == Type.folder)
                    if (value)
                        GetUriImg = "/Resources/опен.png";
                    else
                        GetUriImg = "/Resources/папка.png";
            }
        }
        [JsonIgnore]
        public IndxElementNew Parent { get => parent; set => SetProperty(ref parent, value); }
        [JsonIgnore]
        public string GetUriImg
        {
            set => SetProperty(ref getUriImg, value);
            get => getUriImg;
        }
        [JsonIgnore]
        public string Name
        {
            get
            {
                if (Tp == Type.file)
                    //if (string.IsNullOrEmpty(FullPath))
                    //return Path.GetFileNameWithoutExtension(FullPath);
                    return Path.GetFileName(FullPath);
                else
                {
                    //Regex pattern = new Regex(@":|\\+|\/+");
                    return new Regex(@":|\\+|\/+").Replace(new DirectoryInfo(FullPath).Name, "");
                }
                //var a = Path.GetPathRoot(RootFolderPath);

            }
        }
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
                    else if (ext == Name)
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
        [JsonIgnore]
        public WpfObservableRangeCollection<IndxElementNew> AllLowerElements { get => allLowerElements ??= new WpfObservableRangeCollection<IndxElementNew>(Descendants()); }/*; set => SetProperty(ref allLowerElements, value); }*/
        public void OpenFolder()
        {
            Directory.Exists(FullPath);
            Process.Start("explorer.exe", $"/select, {FullPath}");
        }
        public void OpenFile()
        {
            Process pr = new Process();
            File.Exists(FullPath);
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
                return _openfolder ??= new CommandHandler(obj =>
                {
                    OpenFolder();
                },
                (obj) => FileFolderExist
                );
            }
        }
        [JsonIgnore]
        private CommandHandler _openfile;
        [JsonIgnore]
        public CommandHandler OpenFileCommand
        {
            get
            {
                return _openfile ??= new CommandHandler(obj =>
                {
                    OpenFile();
                },
                (obj) => FileFolderExist && Tp == Type.file
                );
            }
        }
        private IEnumerable<IndxElementNew> Descendants(/*this IndxElementNew root*/)
        {
            var nodes = new Stack<IndxElementNew>(new[] { this });
            while (nodes.Any())
            {
                IndxElementNew node = nodes.Pop();
                yield return node;
                if (node.ChildElements != null)
                    foreach (var n in node.childElements) nodes.Push(n);
            }
        }
        public static bool operator ==(IndxElementNew a, IndxElementNew b)
        {
            if (a is null && b is null)
                return true;
            if (a is null)
                return false;
            if (b is null)
                return false;
            if (a.FullPath == b.FullPath && a.Tp == b.Tp && a.Id == b.Id)
                return true;
            else
                return false;


        }
        public static bool operator !=(IndxElementNew a, IndxElementNew b)
        {
            if (a is null && b is null)
                return false;
            if (a is null)
                return true;
            if (b is null)
                return true;
            if (a.FullPath == b.FullPath && a.Tp == b.Tp && a.Id == b.Id)
                return false;
            else
                return true;
        }
        public override int GetHashCode()
        {
            int w = Id.GetHashCode();
            int a = FullPath == null ? 0 : FullPath.GetHashCode();
            int s = Tp.GetHashCode();
            return w ^ a ^ s;
        }
        public bool Equals(IndxElementNew other)
        {
            // Would still want to check for null etc. first.
            return this.Id == other.Id &&
                   this.FullPath == other.FullPath &&
                   this.Tp == other.Tp;
        }
        public bool Equals([AllowNull] IndxElementNew x, [AllowNull] IndxElementNew y)
        {
            return x.Id == y.Id &&
                   x.FullPath == y.FullPath &&
                   x.Tp == y.Tp;
        }
        public int GetHashCode([DisallowNull] IndxElementNew obj)
        {
            int w = Id.GetHashCode();
            int a = FullPath == null ? 0 : FullPath.GetHashCode();
            int s = Tp.GetHashCode();
            return w ^ a ^ s;
        }
        public override string ToString()
        {
            return Name;
        }
    }
}
