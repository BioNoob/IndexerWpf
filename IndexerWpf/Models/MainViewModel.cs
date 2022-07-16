using IndexerWpf.Classes;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;

namespace IndexerWpf.Models
{
    public class MainViewModel : Proper
    {
        public WpfObservableRangeCollection<IndxElement> SelectedElements { get => selectedElements; set => SetProperty(ref selectedElements, value); }
        public WpfObservableRangeCollection<IndxElements> ListOfIndexes { get => selectedIndexes; set => SetProperty(ref selectedIndexes, value); }
        public WpfObservableRangeCollection<IndxElements> ListOfSelectedIndexes { get => new WpfObservableRangeCollection<IndxElements>(ListOfIndexes.Where(t => t.IsSelected)); }
        public WpfObservableRangeCollection<string> ListOfExtentionsSelectedIndexes { get { return StaticModel.UnicExtentions; } }

        public string Copyied_items { get => copyieditems; set => SetProperty(ref copyieditems, value); }

        //public WpfObservableRangeCollection<string> ExistedIndexes { get => existedIndexs; set => SetProperty(ref existedIndexs, value); }
        public string SelectedIndexsString { get => selectedIndexsString; set { SetProperty(ref selectedIndexsString, value); /*if (!string.IsNullOrEmpty(value))*/ DoLoad(); } }
        //public IndxElements Indexes { get => indexes; set { StaticModel.ElIndx = value.AllFiles; SetProperty(ref indexes, value); } }
        public IndxElement Selectedfile { get => selectedfile; set => SetProperty(ref selectedfile, value); }
        public double Prog_value { get => prog_val; set => SetProperty(ref prog_val, value); }
        //public double Prog_value_max { get => prog_val_max; set => SetProperty(ref prog_val_max, value); }// { prog_val = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Prog_value_max")); } }
        public string Def_path { get => Sets.FolderIndexesDefPath; }
        public string Search_text { get => search_text; set { SetProperty(ref search_text, value); DoSearch(value); } }
        public WpfObservableRangeCollection<IndxElement> Searched { get => searched; set => SetProperty(ref searched, value); }
        public string SelectedFilter { get => selectedFilter; set { SetProperty(ref selectedFilter, value); DoSearch(Search_text); } }
        public bool Was_Loaded { get => was_loaded; set => SetProperty(ref was_loaded, value); }
        public bool Is_scanned { get => is_scanned; set { SetProperty(ref is_scanned, !value); } }
        public Settings Sets { get => sets; set => SetProperty(ref sets, value); }
        public bool ShowPopUp { get => showpopup; set => SetProperty(ref showpopup, value); }
        //[JsonIgnore]
        public WpfObservableRangeCollection<IndxElement> VisualFolder { get => visualfolder; set { SetProperty(ref visualfolder, value); } }
        private string copyieditems;
        private WpfObservableRangeCollection<IndxElement> selectedElements;
        private WpfObservableRangeCollection<IndxElements> selectedIndexes;
        private WpfObservableRangeCollection<IndxElement> visualfolder;
        private bool was_loaded = false;
        private bool is_scanned = false;
        private bool showpopup = false;
        private IndxElement selectedfile = new IndxElement();
        //private IndxElements indexes = null;
        private double prog_val;
        //private double prog_val_max;
        private string search_text;
        private WpfObservableRangeCollection<IndxElement> searched;
        private string selectedFilter;
        private string selectedIndexsString;
        private Settings sets;
        private CommandHandler _openfolder;
        private FolderBrowserDialog fbd;

        public CommandHandler OpenIndexFolderCommand
        {
            get
            {
                return _openfolder ?? (_openfolder = new CommandHandler(obj =>
                {
                    Process.Start("explorer.exe", $"{Def_path}");
                },
                (obj) => !string.IsNullOrEmpty(Def_path)
                ));
            }
        }
        private CommandHandler _closewindow;
        public CommandHandler CloseWindowCommand
        {
            get
            {
                return _closewindow ?? (_closewindow = new CommandHandler(obj =>
                {
                    Sets.LastIndex = SelectedIndexsString;
                    Sets.SaveSettings();
                    Environment.Exit(0);
                },
                (obj) => Is_scanned
                ));
            }
        }
        private CommandHandler _savefiles;
        public CommandHandler SaveFilesCommand
        {
            get
            {
                return _savefiles ?? (_savefiles = new CommandHandler(obj =>
                {
                    StringCollection paths = new StringCollection();
                    var buf = StaticModel.ElIndx.Where(t => t.IsSelected).Select(t => t.Name).ToArray();
                    paths.AddRange(StaticModel.ElIndx.Where(t => t.IsSelected).Select(t => t.FullPath).ToArray());
                    Copyied_items = string.Empty;
                    Copyied_items = string.Join("\n", buf);
                    Clipboard.SetFileDropList(paths);
                    ShowPopUp = true;
                },
                (obj) => StaticModel.ElIndx.Where(t => t.IsSelected).Count() > 0//Searched.Where(t => t.IsSelected).Count() > 0
                ));
            }
        }
        private CommandHandler _changedir;
        public CommandHandler ChangeDirCommand
        {
            get
            {
                return _changedir ?? (_changedir = new CommandHandler(obj =>
                {

                    fbd.SelectedPath = Def_path;
                    if (fbd.ShowDialog() == DialogResult.OK)
                    {
                        Sets.FolderIndexesDefPath = fbd.SelectedPath;
                        LoadListIndexes();
                    }
                },
                (obj) => true
                ));
            }
        }
        private CommandHandler _startscan;
        public CommandHandler StartScanCommand
        {
            get
            {
                return _startscan ?? (_startscan = new CommandHandler(obj =>
                {
                    if (fbd.ShowDialog() == DialogResult.OK)
                    {
                        if (MessageBox.Show($"Do index of\n\n{fbd.SelectedPath}", "New Indexer", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                            Worker(fbd.SelectedPath);
                        else
                            return;
                    }
                },
                (obj) => Is_scanned
                ));
            }
        }



        public MainViewModel()
        {
            Sets = new Settings();
            Sets.LoadSettings();
            if (string.IsNullOrEmpty(Sets.FolderIndexesDefPath))
                Sets.FolderIndexesDefPath = Directory.GetCurrentDirectory() + "\\indexes";

            ListOfIndexes = new WpfObservableRangeCollection<IndxElements>();
            ListOfIndexes.CollectionChanged += ListOfIndexes_CollectionChanged;
            Copyied_items = string.Empty;
            Searched = new WpfObservableRangeCollection<IndxElement>();
            VisualFolder = new WpfObservableRangeCollection<IndxElement>();
            SelectedElements = new WpfObservableRangeCollection<IndxElement>();

            StaticModel.LoadEndEvent += StaticModel_LoadEndEvent;
            fbd = new FolderBrowserDialog();
            fbd.ShowNewFolderButton = true;
            fbd.AutoUpgradeEnabled = true;
            fbd.RootFolder = Environment.SpecialFolder.Recent;
            Is_scanned = false;
            LoadListIndexes();

            var lds = GetSelectedIndexes(Sets.LastIndex);
            foreach (var item in lds)
            {
                var a = ListOfIndexes.SingleOrDefault(t => t.GetName == item);
                if (a != null)
                    a.IsSelected = true;

            }
            //SelectedIndexsString = Sets.LastIndex;

            //Prog_value = 0;
        }

        private void ListOfIndexes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (var Indx in e.NewItems)
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    StaticModel.ElIndx.AddRange((Indx as IndxElements).AllFiles);
                }
                else if (e.Action == NotifyCollectionChangedAction.Remove)
                {
                    foreach (var element in (Indx as IndxElements).AllFiles)
                    {
                        StaticModel.ElIndx.Remove(element);
                    }
                }
            }

        }

        private void LoadListIndexes()
        {
            ListOfIndexes.Clear();
            //Indexes.Clear();
            VisualFolder.Clear();
            if (!Directory.Exists(Def_path))
                Directory.CreateDirectory(Def_path);
            else
            {
                var fls = Directory.GetFiles(Def_path);
                if (fls.Length > 0)
                {
                    foreach (var item in fls.Where(t => Path.GetExtension(t) == ".json"))
                    {
                        var q = new IndxElements(item);
                        q.IsSelectedChangedEvent += Q_IsSelectedChangedEvent;
                        ListOfIndexes.Add(q); //Path.GetFileNameWithoutExtension(item));
                    }

                }
            }
        }

        private void Q_IsSelectedChangedEvent(IndxElements sender, bool state)
        { //ВОЗМОЖНО ПРИ СНЯТИИ ВЫДЕЛЕНИЯ ЧИСТИТЬ ЭЛЕМЕНТЫ ЧТОБ ПАМЯТЬ ЖИЛА
            if (state)
                if (string.IsNullOrEmpty(SelectedIndexsString))
                    SelectedIndexsString += sender.GetName;
                else
                    SelectedIndexsString += ", " + sender.GetName;
            else
            {

                var sselectedIndexsString = Regex.Replace(SelectedIndexsString, $@"(, )?{sender.GetName}", "");
                sselectedIndexsString = Regex.Replace(sselectedIndexsString, $@"^(, )", "");
                SelectedIndexsString = sselectedIndexsString;
            }

        }

        private void StaticModel_LoadEndEvent()
        {
            VisualFolder.Clear();
            foreach (var item in ListOfSelectedIndexes)
            {
                VisualFolder.AddRange(item.AllFiles.Where(t => t.Prnt == null));
            }
            SetProperty(nameof(ListOfExtentionsSelectedIndexes));
            //var t = ListOfExtentionsSelectedIndexes;
            //VisualFolder.AddRange(Indexes.AllFiles.Where(t => t.Prnt == null));
        }

        //https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/how-to-iterate-file-directories-with-the-parallel-class?redirectedfrom=MSDN ??
        private List<string> GetSelectedIndexes(string name_of)
        {
            List<string> paths = new List<string>();
            if (name_of.Contains(","))
            {
                //name_of = name_of.Replace(" ", "");
                paths.AddRange(name_of.Split(", "));
            }
            else
                paths.Add(name_of);
            return paths;
        }
        private void DoLoad(/*string name_of*/)
        {
            //List<string> paths = GetSelectedIndexes(name_of);
            //StaticModel.ElIndx.Clear();
            List<IndxElement> t = new List<IndxElement>();
            foreach (var item in ListOfSelectedIndexes)
            {
                try
                {
                    if (item.AllFiles.Count <= 0)
                        item.LoadInexes();
                    else
                        StaticModel.ElIndx.AddRange(item.AllFiles);
                }
                catch (Exception)
                {
                    ListOfIndexes.Remove(item);
                    item.IsSelected = false;
                    return;
                }
                t.AddRange(item.AllFiles);
            }
            StaticModel.ElIndx = new WpfObservableRangeCollection<IndxElement>(StaticModel.ElIndx.Distinct());
            var B = StaticModel.ElIndx.Except(t);
            StaticModel.ElIndx.RemoveRange(B);
            StaticModel.InvokeLoadEndEvent();
            Was_Loaded = true;
            if (!string.IsNullOrEmpty(Search_text))
                DoSearch(Search_text);
            //Prog_value_max = Indexes.TotalFiles;
            Prog_value = StaticModel.ElIndx.Count;

            //foreach (var item in paths)
            //{
            //    //string full_nm = $"{Def_path}\\{item.Trim()}.json";
            //    try
            //    {

            //        Indexes.LoadInexes(full_nm);
            //        //Костыль
            //        Indexes = Indexes;
            //    }
            //    catch (Exception)
            //    {
            //        //И ТУУУУУУТ
            //        //var ind = ExistedIndexes.IndexOf(name_of);
            //        //ExistedIndexes.Remove(name_of);
            //        //if (ind > 0)
            //        //    SelectedExisted = ExistedIndexes.ElementAt(ind - 1);
            //        //else
            //        //    SelectedExisted = null;
            //        //return;
            //    }

            //    StaticModel.InvokeLoadEndEvent();
            //    if (Indexes != null)
            //    {
            //        Was_Loaded = true;
            //        if (!string.IsNullOrEmpty(Search_text))
            //            DoSearch(Search_text);
            //        Prog_value_max = Indexes.TotalFiles;
            //        Prog_value = Indexes.TotalFiles;

            //    }
            //}
        }
        //Сохранение полюбому в конце парсинга. Нет ситуации где бы оно не сохранилось. Если только отказаться перезаписывать
        private bool DoSave(IndxElements elements)
        {
            string to_save = Path.GetFileName(elements.RootFolderPath) /*new DirectoryInfo(Indexes.RootFolderPath).Name/* + DateTime.Now.ToString("ddMMy_HHmm")*/ + ".json";
            string full_to_save = Def_path + "\\" + to_save;
            if (new FileInfo(full_to_save).Exists)
                if (MessageBox.Show($"Overwrite File\n{to_save} ?\n(No = cancel save)", "Already exist", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    elements.SaveIndexes(full_to_save);
                else
                    return false;
            else
                elements.SaveIndexes(full_to_save);
            return true;

        }
        private void DoSearch(string text)
        {
            if (StaticModel.ElIndx != null)
                if (!string.IsNullOrEmpty(text) && text.Length > 1)
                {
                    Searched.Clear();
                    List<IndxElement> res = new List<IndxElement>();
                    if (StaticModel.IsValidRegex(text))
                    {
                        var reg = new Regex(@$"^{text}$", RegexOptions.IgnoreCase);
                        res = StaticModel.ElIndx.Where(t => reg.IsMatch(t.Name)).ToList();
                    }

                    if (SelectedFilter != "*" && SelectedFilter != null)
                    {
                        Searched.AddRange(res.Where(t => t.Extension == SelectedFilter));
                        Searched.AddRange(StaticModel.ElIndx.Where(t => t.Name.Contains(text, StringComparison.OrdinalIgnoreCase) && t.Extension == SelectedFilter));
                    }
                    else
                    {
                        Searched.AddRange(res);
                        Searched.AddRange(StaticModel.ElIndx.Where(t => t.Name.Contains(text, StringComparison.OrdinalIgnoreCase)));
                    }
                }
                else
                    Searched.Clear();
        }
        private async void Worker(string path)
        {
            Is_scanned = true;
            Was_Loaded = false;
            var indx = await DoScan(path);
            if (indx == null)
            {
                Was_Loaded = true;
                Is_scanned = false;
                StaticModel.InvokeLoadEndEvent();
                return;
            }
            indx.IsSelected = true;
            ListOfIndexes.Add(indx);
            //Debug.WriteLine("DONE");
            DoSave(indx);

            //ТУУУУТ!
            //var nm = Path.GetFileNameWithoutExtension(Indexes.RootFolderPath);

            //if (!ExistedIndexes.Contains(nm))
            //    ExistedIndexes.Add(nm);
            //SetProperty(ref selectedexisted, nm, "SelectedExisted");


            Was_Loaded = true;
            Is_scanned = false;
            StaticModel.InvokeLoadEndEvent();
        }
        private async Task<IndxElements> DoScan(string path)
        {
            Search_text = string.Empty;
            //Indexes.Dispose();
            var indexes = new IndxElements(path);
            //получаем количество файлов
            Prog_value = 0;


            //var dirs = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);

            //Prog_value_max = dirs.Length;

            //var fileNames = Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories);

            //Prog_value_max = fileNames.Count();
            //if (Prog_value_max > 100000)
            //{
            //    MessageBox.Show($"Too many files ({Prog_value_max})\n Please use internal directories!", "Too many files", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    Prog_value = 1;
            //    Prog_value_max = 1;
            //    return false;
            //}

            //создаем корневую ноду по корневому каталогу
            IndxElement root = new IndxElement(Path.GetFullPath(path)) { Tp = IndxElement.Type.folder, Prnt = null };
            indexes.AllFiles.Add(root);
            await Task.Run(() =>
            {
                foreach (var item in LoadFiles(path, root))
                {
                    indexes.AllFiles.Add(item);
                }
                foreach (var item in LoadSubDirectories(path, root))
                {
                    indexes.AllFiles.Add(item);
                }
            });
            //ListOfIndexes.Add(indexes);
            GC.Collect();
            return indexes;
        }
        /// <summary>
        /// Ищем файлы в указанном каталоге
        /// </summary>
        /// <param name="dir">каталог</param>
        /// <param name="td">предыдущая нода</param>
        /// <returns>Лист найденных элементов</returns>
        private List<IndxElement> LoadFiles(string dir, /*TreeNode td,*/ IndxElement parfolder)
        {
            List<IndxElement> lie = new List<IndxElement>();
            try
            {
                var Files = Directory.EnumerateFiles(dir, "*.*");
                foreach (string file in Files)
                {
                    //FileInfo fi = new FileInfo(file);
                    Path.GetFullPath(file);
                    lie.Add(new IndxElement(Path.GetFullPath(file)/*fi.FullName*/) { Tp = IndxElement.Type.file, Prnt = parfolder.Id });
                    UpdateProgress();

                }
            }
            catch (UnauthorizedAccessException)
            {

                return lie;

            }

            return lie;
        }
        /// <summary>
        /// Парсим поддериктории (Рекурсивная штука)
        /// </summary>
        /// <param name="dir">каталог</param>
        /// <param name="td">предыдущая нода</param>
        /// <returns>Лист найденных элементов</returns>
        private List<IndxElement> LoadSubDirectories(string dir, /*TreeNode td, */IndxElement parfolder)
        {
            List<IndxElement> ie = new List<IndxElement>();
            //string[] subdirectoryEntries = Directory.GetDirectories(dir);
            try
            {
                var subs = Directory.EnumerateDirectories(dir);

                foreach (string subdirectory in subs)
                {
                    IndxElement newparent = new IndxElement() { FullPath = Path.GetFullPath(subdirectory), Tp = IndxElement.Type.folder, Prnt = parfolder.Id };
                    ie.Add(newparent);
                    ie.AddRange(LoadFiles(subdirectory, newparent));
                    ie.AddRange(LoadSubDirectories(subdirectory, newparent));
                    UpdateProgress();
                }
            }
            catch (UnauthorizedAccessException)
            {

                return ie;

            }

            return ie;
        }
        private void UpdateProgress()
        {
            //if (Prog_value < Prog_value_max)
            //{
            Prog_value++;
            //}
        }
    }
}
