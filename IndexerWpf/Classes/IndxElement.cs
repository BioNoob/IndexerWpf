﻿using IndexerWpf.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
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
    //public class SimpleIndxElement
    //{
    //    //private Func<int> get_id;
    //    ////private Action<int> set_id;
    //    //public int Id { get => get_id.Invoke(); /*set { set_id.Invoke(value); SetProperty(); }*/}
    //    private Func<bool> get_select;
    //    private Action<bool> set_select;
    //    private string extention;
    //    private string name;

    //    public bool IsSelected { get => get_select.Invoke(); set => set_select.Invoke(value); }
    //    public string Extension { get => extention; set => extention = value; }
    //    public string Name { get => name; set => name = value; }
    //    //private Func<IndxElementNew> get_prnt;
    //    //private Action<IndxElementNew> set_prnt;
    //    //public IndxElementNew Parent { get => get_prnt.Invoke(); set { set_prnt.Invoke(value); SetProperty(); } }
    //    //private Func<string> get_rootpath;
    //    ////private Action<string> set_parpath;
    //    //public string ParentPath { get => get_rootpath.Invoke(); /*set { set_parpath.Invoke(value); SetProperty(); }*/ }
    //    public SimpleIndxElement(IndxElementNew elem/*, string _rootFolderPath*/) : this(() => elem.IsSelected, n => { elem.IsSelected = n; }, elem.Name, elem.Extension)
    //    {
    //    }
    //    private SimpleIndxElement(Func<bool> get_s, Action<bool> set_s, string nm, string ext)
    //    {
    //        get_select = get_s;
    //        set_select = set_s;
    //        Name = nm;
    //        Extension = ext;
    //    }
    //}
    public class IndxElementBase : Proper, IEqualityComparer<IndxElementBase>, System.IEquatable<IndxElementNew>
    {
        public IndxElementBase()
        {
            //isScaned = false;
            isSelected = false;
            IsWasChecked = false;
            FileFolderExist = true;
        }
        public IndxElementBase(string _fullpath, Type tp) : this()//, int id)
        {
            //isScaned = true;
            FullPath = _fullpath;
            Tp = tp;
            //Id = id;//Identificator++;
        }
        public IndxElementBase(IndxElementBase elementBase) : this()
        {
            // isScaned = elementBase.isScaned;
            FullPath = elementBase.FullPath;
            Tp = elementBase.Tp;
            //Id = elementBase.Id;//Identificator++;
            IsSelected = elementBase.IsSelected;
            IsExpanded = elementBase.IsExpanded;
        }
        public enum Type
        {
            folder,
            file
        }
        private bool isSelected;
        private bool isExpanded;
        private string getUriImg;
        private string fullPath;
        private Type tp;
        //private int id;
        private bool fileFolderExist;
        private string fileFolderExistString;

        protected bool IsWasChecked { get; set; }

        //protected bool isScaned;
        //[JsonProperty(Order = -2)]
        //public int Id { get => id; set { SetProperty(ref id, value); } }
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
                //StaticModel.InvokeIdincreasedEvent(id, value);
                //if (isScaned)
                //{
                //    FileFolderExist = true;
                //    FileFolderExistString = "";
                //    return;
                //}


            }
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
                }
                if (Tp == Type.folder)
                    if (value)
                        GetUriImg = "/Resources/опен.png";
                    else
                        GetUriImg = "/Resources/папка.png";
            }
        }
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
        public bool FileFolderExist
        {
            get => fileFolderExist;
            set
            {

                SetProperty(ref fileFolderExist, value);
                if (!value)
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
        [JsonIgnore]
        public string FileFolderExistString
        {
            get => fileFolderExistString;
            set => SetProperty(ref fileFolderExistString, value);
        }
        private void CheckAvalibility()
        {
            if (!IsWasChecked)
            {
                FileFolderExist = Tp switch
                {
                    Type.folder => Directory.Exists(FullPath),
                    Type.file => File.Exists(FullPath),
                    _ => false,
                };
                IsWasChecked = true;
            }
        }


        public void OpenFolder()
        {
            //Directory.Exists(FullPath);
            //if(Tp != Type.folder)
            //Voyadger1.OpenFolderInExplorer(DirPath);
            Process.Start("explorer.exe", $"/select, \"{FullPath}\"");
            //Process.Start("explorer.exe", $"/select, {FullPath}");
        }
        public void OpenFile()
        {
            Process pr = new Process();
            //File.Exists(FullPath);
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
                    //CheckAvalibility();
                    //if (FileFolderExist)
                        OpenFolder();
                },
                (obj) => { CheckAvalibility(); return FileFolderExist;/* && Tp == Type.file;*/ }
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
                    //CheckAvalibility();
                    //if (FileFolderExist)
                        OpenFile();
                },
                (obj) => { if (Tp == Type.file) { CheckAvalibility(); return FileFolderExist;/* && Tp == Type.file;*/ } else return false; } 
                );
            }
        }

        public static bool operator ==(IndxElementBase a, IndxElementBase b)
        {
            if (a is null && b is null)
                return true;
            if (a is null)
                return false;
            if (b is null)
                return false;
            if (a.FullPath == b.FullPath && a.Tp == b.Tp)/* && a.Id == b.Id)*/
                return true;
            else
                return false;


        }
        public static bool operator !=(IndxElementBase a, IndxElementBase b)
        {
            if (a is null && b is null)
                return false;
            if (a is null)
                return true;
            if (b is null)
                return true;
            if (a.FullPath == b.FullPath && a.Tp == b.Tp) /*&& a.Id == b.Id)*/
                return false;
            else
                return true;
        }
        public override int GetHashCode()
        {
            // int w = Id.GetHashCode();
            int a = FullPath == null ? 0 : FullPath.GetHashCode();
            int s = Tp.GetHashCode();
            return /*w ^*/ a ^ s;
        }
        public bool Equals(IndxElementNew other)
        {
            // Would still want to check for null etc. first.
            return /*this.Id == other.Id &&*/
                   this.FullPath == other.FullPath &&
                   this.Tp == other.Tp;
        }
        public bool Equals([AllowNull] IndxElementBase x, [AllowNull] IndxElementBase y)
        {
            return /*x.Id == y.Id &&*/
                   x.FullPath == y.FullPath &&
                   x.Tp == y.Tp;
        }
        public int GetHashCode([DisallowNull] IndxElementBase obj)
        {
            //int w = Id.GetHashCode();
            int a = FullPath == null ? 0 : FullPath.GetHashCode();
            int s = Tp.GetHashCode();
            return /*w ^*/ a ^ s;
        }
        public override string ToString()
        {
            return Name;
        }
    }

    public class IndxElementNew : IndxElementBase
    {

        public IndxElementNew()
        {
            init();
            if (StaticModel.CancelToken.IsCancellationRequested)
                throw new ProcessingFileException(TypeOfError.CancelTask, null, null);
        }
        private void init()
        {
            //allLowerElements = null;
            Parent = null;
            childElements = null;
            //isScaned = false;
            this.PropertyChanged += IndxElementNew_PropertyChanged;
        }

        //Конструктор при сканировании
        public IndxElementNew(string _fullpath, Type tp, IndxElementNew prnt)/*, int id)*/
        {
            init();
            //isScaned = true;
            if (tp != Type.file)
                ChildElements = new WpfObservableRangeCollection<IndxElementNew>();
            else
                ChildElements = null;
            FullPath = _fullpath;
            Tp = tp;
            // Id = id;//Identificator++;
            Parent = prnt;
            IsWasChecked = true; 
            //allLowerElements = null;
        }


        private WpfObservableRangeCollection<IndxElementNew> childElements;
        private IndxElementNew parent;

        public WpfObservableRangeCollection<IndxElementNew> ChildElements { get => childElements; set => SetProperty(ref childElements, value); }

        //[JsonIgnore]
        //public int CountElements
        //{
        //    get
        //    {
        //        if (ChildElements != null)
        //            return ChildElements.Sum(t => t.CountElements) + 1;
        //        else return 1;
        //    }
        //}
        //private IEnumerable<IndxElementNew> Descendants(/*this IndxElementNew root*/)
        //{
        //    var nodes = new Stack<IndxElementNew>(new[] { this });
        //    while (nodes.Any())
        //    {
        //        IndxElementNew node = nodes.Pop();
        //        yield return node;
        //        if (node.ChildElements != null)
        //            foreach (var n in node.childElements) nodes.Push(n);
        //    }
        //}
        //private IEnumerable<IndxElementBase> Descendants() https://stackoverflow.com/questions/7062882/searching-a-tree-using-linq
        //{
        //    var st = new List<IndxElementBase>();
        //    if (ChildElements != null)
        //        foreach (var item in ChildElements)
        //        {
        //            st.Add(new IndxElementBase(this));
        //            st.AddRange(item.Descendants());

        //        }
        //    else
        //        st.Add(new IndxElementBase(this));
        //    return st;
        //}

        [JsonIgnore]
        public IndxElementNew Parent { get => parent; set => SetProperty(ref parent, value); }

        //[JsonIgnore]
        //public WpfObservableRangeCollection<IndxElementNew> AllLowerElements { get => allLowerElements ??= new WpfObservableRangeCollection<IndxElementNew>(Descendants()); }/*; set => SetProperty(ref allLowerElements, value); }*/
        private void IndxElementNew_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "IsExpanded":
                    if (IsExpanded && IsSelected)
                        StaticModel.InvokeItemIsExpandChange(this);
                    if (IsExpanded && Parent != null)
                        if (Parent.IsExpanded != IsExpanded)
                            Parent.IsExpanded = IsExpanded;
                    if (!IsExpanded && ChildElements != null)
                        ChildElements.ToList().ForEach(t => { if (t.IsExpanded != IsExpanded) t.IsExpanded = IsExpanded; });
                    //AllLowerElements.ToList().ForEach(t => { if (t.IsExpanded != IsExpanded) t.IsExpanded = IsExpanded; });
                    return;
                case "":
                default:
                    break;
            }
        }
    }
}
