using IndexerWpf.Classes;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IndexerWpf.Models
{
    public class MainViewModel : Proper
    {
        public WpfObservableRangeCollection<IndxElements> ListOfIndexes { get => selectedIndexes; set => SetProperty(ref selectedIndexes, value); }
        public WpfObservableRangeCollection<IndxElements> ListOfSelectedIndexes { get => new WpfObservableRangeCollection<IndxElements>(ListOfIndexes.Where(t => t.IsSelected)); }
        public WpfObservableRangeCollection<string> ListOfExtentionsSelectedIndexes => new WpfObservableRangeCollection<string>(ListOfElementsInSelectedIndexes.Select(t => t.Extension).Distinct());
        public WpfObservableRangeCollection<IndxElementNew> ListOfElementsInSelectedIndexes
        {
            get
            {
                List<IndxElementNew> list = new List<IndxElementNew>();
                foreach (var item in ListOfSelectedIndexes.SelectMany(t => t.RootElement.ChildElements))
                {
                    list.AddRange(item.AllLowerElements);//Descendants());
                }
                return new WpfObservableRangeCollection<IndxElementNew>(list);
            }
        }
        public WpfObservableRangeCollection<IndxElementNew> ListOfRootInSelectedIndexes => new WpfObservableRangeCollection<IndxElementNew>(ListOfSelectedIndexes.Select(t => t.RootElement));
        public string Copyied_items { get => copyieditems; set => SetProperty(ref copyieditems, value); }
        public string SelectedIndexsString { get => selectedIndexsString; set { SetProperty(ref selectedIndexsString, value); /*if (!string.IsNullOrEmpty(value))*/ DoLoad(); } }
        public double Prog_value { get => prog_val; set => SetProperty(ref prog_val, value); }
        public string Def_path { get => Sets.FolderIndexesDefPath; }
        public string Search_text { get => search_text; set { SetProperty(ref search_text, value); DoSearch(value); } }
        public WpfObservableRangeCollection<IndxElementNew> Searched { get => searched; set => SetProperty(ref searched, value); }
        public string SelectedFilter { get => selectedFilter; set { SetProperty(ref selectedFilter, value); DoSearch(Search_text); } }
        public bool Is_scanned { get => is_scanned; set { SetProperty(ref is_scanned, !value); } }
        public bool Is_LongOperation { get => is_LongOperation; set { SetProperty(ref is_LongOperation, !value); } }
        public Settings Sets { get => sets; set => SetProperty(ref sets, value); }
        public bool ShowPopUp { get => showpopup; set => SetProperty(ref showpopup, value); }
        private string copyieditems;
        private WpfObservableRangeCollection<IndxElements> selectedIndexes;
        private WpfObservableRangeCollection<IndxElementNew> searched;
        private bool is_scanned = false;
        private bool is_LongOperation = false;
        public bool ignore_scanned = false;
        private bool showpopup = false;
        private double prog_val;
        private string search_text;
        private string selectedFilter;
        private string selectedIndexsString;
        private Settings sets;
        private CommandHandler _openfolder;
        private readonly FolderBrowserDialog fbd;

        public CommandHandler OpenIndexFolderCommand
        {
            get
            {
                return _openfolder ??= new CommandHandler(obj =>
                {
                    Process.Start("explorer.exe", $"{Def_path}");
                },
                (obj) => !string.IsNullOrEmpty(Def_path)
                );
            }
        }
        private CommandHandler _closewindow;
        public CommandHandler CloseWindowCommand
        {
            get
            {
                return _closewindow ??= new CommandHandler(obj =>
                {
                    Sets.LastIndex = SelectedIndexsString;
                    Sets.SaveSettings();
                    Environment.Exit(0);
                },
                (obj) => Is_scanned
                );
            }
        }
        private CommandHandler _savefiles;
        public CommandHandler SaveFilesCommand
        {
            get
            {
                return _savefiles ??= new CommandHandler(obj =>
                {
                    StringCollection paths = new StringCollection();
                    var buf = ListOfElementsInSelectedIndexes.Where(t => t.IsSelected).Select(t => t.Name).ToArray();
                    paths.AddRange(ListOfElementsInSelectedIndexes.Where(t => t.IsSelected).Select(t => t.FullPath).ToArray());
                    Copyied_items = string.Empty;
                    Copyied_items = string.Join("\n", buf);
                    Clipboard.SetFileDropList(paths);
                    ShowPopUp = true;
                },
                (obj) => true
                );
            }
        }
        private CommandHandler _changedir;
        public CommandHandler ChangeDirCommand
        {
            get
            {
                return _changedir ??= new CommandHandler(obj =>
                {

                    fbd.SelectedPath = Def_path;
                    if (fbd.ShowDialog() == DialogResult.OK)
                    {
                        Sets.FolderIndexesDefPath = fbd.SelectedPath;
                        LoadListIndexes();
                    }
                },
                (obj) => true
                );
            }
        }
        public CancellationTokenSource CancelToken = new CancellationTokenSource();
        private CommandHandler _cancelActionCommandn;
        public CommandHandler CancelActionCommand
        {
            get
            {
                return _cancelActionCommandn ??= new CommandHandler(obj =>
                {
                    CancelToken?.Cancel();
                },
                (obj) => true//Is_LongOperation
                );
            }
        }
        private CommandHandler _startscan;
        public CommandHandler StartScanCommand
        {
            get
            {
                return _startscan ??= new CommandHandler(obj =>
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
                );
            }
        }
        private CommandHandler _showontree;
        public CommandHandler ShowOnTreeCommand
        {
            get
            {
                return _showontree ??= new CommandHandler(obj =>
                {
                    //Is_scanned = true;
                    //_= await Task.Run(() =>
                    //{
                    ListOfElementsInSelectedIndexes.Where(t => !t.IsSelected).ToList().ForEach(t => { if (!t.IsExpanded) t.IsExpanded = false; }); ;
                    ListOfElementsInSelectedIndexes.Where(t => t.IsSelected).ToList().ForEach(t => t.IsExpanded = true);
                    //return true;
                    //}
                    //);

                },
                (obj) => true
                );
            }
        }
        public MainViewModel()
        {
            Sets = new Settings();
            Sets.LoadSettings();
            if (string.IsNullOrEmpty(Sets.FolderIndexesDefPath))
                Sets.FolderIndexesDefPath = Directory.GetCurrentDirectory() + "\\indexes";

            ListOfIndexes = new WpfObservableRangeCollection<IndxElements>();
            //ListOfIndexes.CollectionChanged += ListOfIndexes_CollectionChanged;
            Copyied_items = string.Empty;
            Searched = new WpfObservableRangeCollection<IndxElementNew>();

            StaticModel.LoadEndEvent += StaticModel_LoadEndEvent;
            fbd = new FolderBrowserDialog
            {
                ShowNewFolderButton = true,
                AutoUpgradeEnabled = true,
                RootFolder = Environment.SpecialFolder.Recent
            };
            Is_scanned = false;
        }

        public void LoadListIndexes()
        {
            ListOfIndexes.Clear();
            if (!Directory.Exists(Def_path))
                Directory.CreateDirectory(Def_path);
            else
            {
                var fls = Directory.GetFiles(Def_path);
                if (fls.Length > 0)
                {
                    foreach (var item in fls.Where(t => Path.GetExtension(t) == ".json"))
                    {
                        var q = new IndxElements() { JsonFileName = item };
                        q.IsSelectedChangedEvent += Q_IsSelectedChangedEvent;
                        ListOfIndexes.Add(q);
                    }

                }
            }
        }

        private void Q_IsSelectedChangedEvent(IndxElements sender, bool state)
        {
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
            SetProperty(nameof(ListOfExtentionsSelectedIndexes));
            SetProperty(nameof(ListOfRootInSelectedIndexes));
        }

        //https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/how-to-iterate-file-directories-with-the-parallel-class?redirectedfrom=MSDN ??
        public List<string> GetSelectedIndexes(string name_of)
        {
            List<string> paths = new List<string>();
            if (name_of.Contains(","))
                paths.AddRange(name_of.Split(", "));
            else
                paths.Add(name_of);
            return paths;
        }
        private void DoLoad()
        {
            if (!ignore_scanned)
                Is_scanned = true;
            List<Task> TaskList = new List<Task>();
            foreach (var item in ListOfSelectedIndexes)
            {
                try
                {
                    if (item.RootElement.CountElements - 1 <= 0)
                    {
                        var LastTask = new Task(() => item.LoadInexes(CancelToken), CancelToken.Token);

                        LastTask.Start();
                        TaskList.Add(LastTask);
                    }
                    //_ = await Task.Run(() => item.LoadInexes());

                    //    .ContinueWith(t =>
                    //{
                    //    if (t.Exception != null) throw t.Exception.InnerException;
                    //}, default
                    //  , TaskContinuationOptions.OnlyOnFaulted
                    //  , TaskScheduler.FromCurrentSynchronizationContext());
                }
                catch (ProcessingFileException e)
                {
                    switch (e.ErrorType)
                    {
                        case ProcessingFileException.TypeOfError.Deleted:
                            MessageBox.Show($"File {e.Path_to_Json} was deleted!", "Error load", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            break;
                        case ProcessingFileException.TypeOfError.Invalid:
                            if (MessageBox.Show($"File {e.Path_to_Json} load error\n{e.Message}\nDelete a file?", "Error load", MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.Yes)
                                FileSystem.DeleteFile(e.Path_to_Json, UIOption.AllDialogs, RecycleOption.SendToRecycleBin);
                            //File.Delete(e.Path_to_Json);
                            break;
                        case ProcessingFileException.TypeOfError.CancelTask:
                            if (!ignore_scanned)
                                Is_scanned = false; 
                            item.IsSelected = false;
                            return;
                        default:
                            break;
                    }
                    if (!ignore_scanned)
                        Is_scanned = false;
                    ListOfIndexes.Remove(item);
                    item.IsSelected = false;
                    return;
                }
            }
            Task.WaitAll(TaskList.ToArray());
            StaticModel.InvokeLoadEndEvent();
            if (!string.IsNullOrEmpty(Search_text))
                DoSearch(Search_text);
            Prog_value = ListOfElementsInSelectedIndexes.Count;
            if (!ignore_scanned)
                Is_scanned = false;
        }
        //Сохранение полюбому в конце парсинга. Нет ситуации где бы оно не сохранилось. Если только отказаться перезаписывать
        private bool DoSave(IndxElements elements)
        {
            string to_save = Path.GetFileName(elements.RootFolderPath) /*new DirectoryInfo(Indexes.RootFolderPath).Name/* + DateTime.Now.ToString("ddMMy_HHmm")*/ + ".json";
            string full_to_save = Def_path + "\\" + to_save;
            elements.JsonFileName = full_to_save;
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
            if (ListOfElementsInSelectedIndexes != null)
                if (!string.IsNullOrEmpty(text) && text.Length > 1)
                {
                    Searched.Clear();
                    List<IndxElementNew> res = new List<IndxElementNew>();
                    if (StaticModel.IsValidRegex(text))
                    {
                        var reg = new Regex(@$"{text}", RegexOptions.IgnoreCase);
                        res = ListOfElementsInSelectedIndexes.Where(t => reg.IsMatch(t.Name)).ToList();
                    }

                    if (SelectedFilter != "*" && SelectedFilter != null)
                    {
                        Searched.AddRange(res.Where(t => t.Extension == SelectedFilter));
                        Searched.AddRange(ListOfElementsInSelectedIndexes.Where(t => t.Name.Contains(text, StringComparison.OrdinalIgnoreCase) && t.Extension == SelectedFilter));
                    }
                    else
                    {
                        Searched.AddRange(res);
                        var res_simple = ListOfElementsInSelectedIndexes.Where(t => t.Name.Contains(text, StringComparison.OrdinalIgnoreCase)).ToList();
                        Searched.AddRange(res_simple);
                    }
                    Searched = new WpfObservableRangeCollection<IndxElementNew>(Searched.Distinct());
                }
                else
                    Searched.Clear();
        }
        private async void Worker(string path)
        {
            Is_LongOperation = true;
            Is_scanned = true;
            IndxElements indx = null;
            try
            {
                indx = await DoScan(path);
            }
            catch (ProcessingFileException)
            {
                indx = null; 
            }

            if (indx == null)
            {
                Is_scanned = false;
                StaticModel.InvokeLoadEndEvent();
                Is_LongOperation = false;
                return;
            }
            Is_LongOperation = false;
            var _ = Task.Run(() => DoSave(indx));
            if (!ListOfIndexes.Contains(indx))
            {
                ListOfIndexes.Add(indx);
                indx.IsSelectedChangedEvent += Q_IsSelectedChangedEvent;
            }
            indx.IsSelected = true;


            Is_scanned = false;
            StaticModel.InvokeLoadEndEvent();
        }
        private async Task<IndxElements> DoScan(string path)
        {
            var indexes = new IndxElements(path);
            //получаем количество файлов
            //Prog_value = 0;
            //создаем корневую ноду по корневому каталогу
            IndxElementNew root = new IndxElementNew(Path.GetFullPath(path), IndxElementNew.Type.folder, null);
            indexes.RootElement = root;
            try
            {
                await Task.Run(() =>
                {
                    indexes.RootElement.ChildElements.AddRange(LoadFiles(path, root));
                    indexes.RootElement.ChildElements.AddRange(LoadSubDirectories(path, root));
                    GC.Collect();
                }, CancelToken.Token);
            }
            catch (ProcessingFileException e)
            {
                throw e;
            }
            return indexes;
        }
        /// <summary>
        /// Ищем файлы в указанном каталоге
        /// </summary>
        /// <param name="dir">каталог</param>
        /// <param name="td">предыдущая нода</param>
        /// <returns>Лист найденных элементов</returns>
        private List<IndxElementNew> LoadFiles(string dir, IndxElementNew parfolder)
        {
            List<IndxElementNew> lie = new List<IndxElementNew>();
            try
            {
                var Files = Directory.EnumerateFiles(dir, "*.*");
                foreach (string file in Files)
                {
                    if (CancelToken.IsCancellationRequested)
                        throw new ProcessingFileException(ProcessingFileException.TypeOfError.CancelTask, null);
                    Path.GetFullPath(file);
                    lie.Add(new IndxElementNew(Path.GetFullPath(file), IndxElementNew.Type.file, parfolder));
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
        private List<IndxElementNew> LoadSubDirectories(string dir, IndxElementNew parfolder)
        {
            List<IndxElementNew> ie = new List<IndxElementNew>();
            try
            {
                var subs = Directory.EnumerateDirectories(dir);

                foreach (string subdirectory in subs)
                {
                    if (CancelToken.IsCancellationRequested)
                        throw new ProcessingFileException(ProcessingFileException.TypeOfError.CancelTask, null);
                    IndxElementNew newparent = new IndxElementNew(Path.GetFullPath(subdirectory), IndxElementNew.Type.folder, parfolder);
                    newparent.ChildElements.AddRange(LoadFiles(subdirectory, newparent));
                    newparent.ChildElements.AddRange(LoadSubDirectories(subdirectory, newparent));
                    ie.Add(newparent);
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
            Prog_value++;
        }
    }
}
