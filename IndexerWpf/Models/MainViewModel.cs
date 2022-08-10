using IndexerWpf.Classes;
using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public WpfObservableRangeCollection<IndxElementBase> ListOfElementsInSelectedIndexes
        {
            get
            {
                List<IndxElementBase> list = new List<IndxElementBase>();
                foreach (var item in ListOfSelectedIndexes.SelectMany(t => t.RootElement.ChildElements))
                {
                    list.AddRange(item.AllLowerElements);//Descendants());
                }
                return new WpfObservableRangeCollection<IndxElementBase>(list);
            }
        }
        public WpfObservableRangeCollection<IndxElementNew> ListOfRootInSelectedIndexes => new WpfObservableRangeCollection<IndxElementNew>(ListOfSelectedIndexes.Select(t => t.RootElement));
        public string Copyied_items { get => copyieditems; set => SetProperty(ref copyieditems, value); }
        public string SelectedIndexsString { get => selectedIndexsString; set { SetProperty(ref selectedIndexsString, value); /*if (!string.IsNullOrEmpty(value))*/  } }
        public double Prog_value { get => prog_val; set => SetProperty(ref prog_val, value); }
        public string Def_path { get => Sets.FolderIndexesDefPath; }
        public string Search_text { get => search_text; set { SetProperty(ref search_text, value); DoSearch(value); } }
        public IEnumerable<IndxElementBase> Searched { get => searched; set => SetProperty(ref searched, value); }
        public string SelectedFilter { get => selectedFilter; set { SetProperty(ref selectedFilter, value); DoSearch(Search_text); } }
        public bool Is_scanned { get => is_scanned; set { SetProperty(ref is_scanned, !value); } }
        public bool Is_LongOperation { get => is_LongOperation; set { SetProperty(ref is_LongOperation, value); } }
        public Settings Sets { get => sets; set => SetProperty(ref sets, value); }
        public bool ShowPopUp { get => showpopup; set => SetProperty(ref showpopup, value); }
        private string copyieditems;
        private WpfObservableRangeCollection<IndxElements> selectedIndexes;
        private IEnumerable<IndxElementBase> searched;
        private bool is_scanned = false;
        private bool is_LongOperation = false;
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
                        SelectedIndexsString = "";
                        StaticModel_LoadEndEvent();
                    }
                },
                (obj) => true
                );
            }
        }
        public CancellationTokenSource CancelToken { get; set; } = new CancellationTokenSource();
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
            Searched = new List<IndxElementNew>();

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
                        var qq = File.ReadLines(item).Take(3).ToList(); //вроде быстро
                        qq.Add("}");
                        var qqq = string.Join(Environment.NewLine, qq);
                        var indx = JsonConvert.DeserializeObject<IndxElements>(qqq.TrimEnd(','));
                        indx.JsonFileName = item;
                        indx.IsSelectedChangedEvent += Q_IsSelectedChangedEvent;
                        //var q = new IndxElements() { JsonFileName = item };
                        //q.IsSelectedChangedEvent += Q_IsSelectedChangedEvent;
                        //q.CountFilesChangedEvent += Q_CountFilesChangedEvent;
                        //ListOfIndexes.Add(q);
                        if(!ListOfIndexes.Contains(indx))
                        ListOfIndexes.Add(indx);
                    }

                }
            }
        }

        //private void Q_CountFilesChangedEvent(int val)
        //{
        //    Prog_value = val;
        //    //SetProperty(nameof(Prog_value));
        //}

        public void Q_IsSelectedChangedEvent(IndxElements sender, bool state)
        {
            if (state)
            {
                if (string.IsNullOrEmpty(SelectedIndexsString))
                    SelectedIndexsString += sender.GetName;
                else
                    SelectedIndexsString += ", " + sender.GetName;
                //Prog_value += sender.TotalFiles;
                DoLoad();
            }
            else
            {
                //SetProperty(nameof(ListOfRootInSelectedIndexes));
                var sselectedIndexsString = Regex.Replace(SelectedIndexsString, $@"(, )?{sender.GetName}", "");
                sselectedIndexsString = Regex.Replace(sselectedIndexsString, $@"^(, )", "");
                SelectedIndexsString = sselectedIndexsString;
                //Prog_value -= sender.TotalFiles;
                StaticModel_LoadEndEvent();
            }
            //Prog_value = ListOfSelectedIndexes.Sum(t => t.TotalFiles);
        }

        private void StaticModel_LoadEndEvent()
        {
            SetProperty(nameof(ListOfExtentionsSelectedIndexes));
            SetProperty(nameof(ListOfRootInSelectedIndexes));
            Prog_value = ListOfSelectedIndexes.Sum(t => t.TotalFiles);
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
        //public System.Timers.Timer StartCountWatcher()
        //{
        //    var a  = new System.Timers.Timer() { AutoReset = true, Interval = 10 };
        //    a.Elapsed += A_Elapsed;
        //    return a;
        //}
        public System.Threading.Timer StartCountWatcher(IndxElements elem = null)
        {
            var a = new System.Threading.Timer(A_Elapsed, elem, 10, 10);
            return a;
        }

        private void A_Elapsed(object sender)
        {
            //Prog_value = ListOfSelectedIndexes.Sum(t => t.TotalFiles);
            //Prog_value = IndxElementNew.Identificator;
            //Debug.WriteLine(IndxElementNew.Identificator);
            if (sender != null)
                Prog_value += (sender as IndxElements).SimpleCounter;
            else
                Prog_value = ListOfSelectedIndexes.Sum(t => t.SimpleCounter);
        }

        public async void DoLoad()
        {
            //if (!ignore_scanned)
            if(!Is_scanned)
            Is_scanned = true;
            List<Task<bool>> TaskList = new List<Task<bool>>();
            Is_LongOperation = true;
            if (CancelToken.IsCancellationRequested) CancelToken = new CancellationTokenSource();
            foreach (var item in ListOfSelectedIndexes)
            {
                //try
                //{
                if (item.RootElement == null || item.RootElement.CountElements - 1 <= 0)
                {
                    //var LastTask = new Task(() => item.LoadInexes(CancelToken));
                    //LastTask.Start();
                    TaskList.Add(item.LoadInexes(CancelToken));
                }
                //_ = await Task.Run(() => item.LoadInexes());

                //    .ContinueWith(t =>
                //{
                //    if (t.Exception != null) throw t.Exception.InnerException;
                //}, default
                //  , TaskContinuationOptions.OnlyOnFaulted
                //  , TaskScheduler.FromCurrentSynchronizationContext());
                //}

            }
            //}
            //timer.Start();
            var timer = StartCountWatcher();
            var tsj = Task.WhenAll(TaskList);
            try
            {
                _ = await tsj;
            }
            catch (ProcessingFileException e)
            {
                timer.Dispose();
                switch (e.ErrorType)
                {
                    case TypeOfError.Deleted:
                        MessageBox.Show($"{e.GetMessage()}", "File deleted", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                    case TypeOfError.Invalid:
                        if (MessageBox.Show($"{e.GetMessage()}\nDelete a file?", "Error load", MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.Yes)
                            FileSystem.DeleteFile(e.Path_to_Json, UIOption.AllDialogs, RecycleOption.SendToRecycleBin);
                        //File.Delete(e.Path_to_Json);
                        break;
                    case TypeOfError.CancelTask:
                        //Is_scanned = false;
                        //(e.Source as IndxElements).IsSelected = false;
                        Prog_value -= e.Source.TotalFiles;
                        //(e.Source as IndxElements).Dispose();
                        //Is_LongOperation = false;
                        return;
                }

                Is_scanned = false;
                ListOfIndexes.Remove(e.Source);
                e.Source.IsSelected = false;
                e.Source.Dispose();
                Is_LongOperation = false;
                return;
            }
            //Task.WaitAll(TaskList.ToArray());


            //timer.Stop();
            timer.Dispose();
            if (!string.IsNullOrEmpty(Search_text))
                DoSearch(Search_text);
            //Prog_value = ListOfElementsInSelectedIndexes.Count;
            StaticModel.InvokeLoadEndEvent();
            Is_LongOperation = false;
            Is_scanned = false;
        }
        private void DoSearch(string text)
        {
            if (ListOfElementsInSelectedIndexes != null)
                if (!string.IsNullOrEmpty(text) && text.Length > 1)
                {
                    var watch = Stopwatch.StartNew();
                    Debug.WriteLine($"search started {watch.ElapsedMilliseconds}");
                    //Searched.Clear();
                    IEnumerable<IndxElementBase> res = new List<IndxElementBase>();
                    if (StaticModel.IsValidRegex(text))
                    {
                        var reg = new Regex(@$"{text}", RegexOptions.IgnoreCase);
                        res = ListOfElementsInSelectedIndexes.Where(t => reg.IsMatch(t.Name));//.ToList();
                        Debug.WriteLine($"founded {watch.ElapsedMilliseconds}");
                    }
                    if (SelectedFilter != "*" && SelectedFilter != null)
                    {
                        res = res.Intersect(res.Where(t => t.Extension == SelectedFilter));  //= Searched.AddRange(res.Where(t => t.Extension == SelectedFilter));
                                                                                             //res = res.Concat(ListOfElementsInSelectedIndexes.Where(t => t.Name.Contains(text, StringComparison.OrdinalIgnoreCase) && t.Extension == SelectedFilter));
                                                                                             //Searched.AddRange(ListOfElementsInSelectedIndexes.Where(t => t.Name.Contains(text, StringComparison.OrdinalIgnoreCase) && t.Extension == SelectedFilter));
                        Debug.WriteLine($"filtered {watch.ElapsedMilliseconds}");
                    }
                    //else
                    //{
                    //    //Searched.AddRange(res);
                    //    var res_simple = ListOfElementsInSelectedIndexes.Where(t => t.Name.Contains(text, StringComparison.OrdinalIgnoreCase)).ToList();
                    //    res.Concat(res_simple);
                    //    //Searched.AddRange(res_simple);
                    //}
                    //Searched = Searched.Distinct().ToList();//new WpfObservableRangeCollection<IndxElementNew>(Searched.Distinct());
                    
                    Searched = res;//new List<IndxElementNew>(res.Count());
                    var q = res.ToList();
                    //Debug.WriteLine($"malloc {watch.ElapsedMilliseconds}");
                    //Searched.AddRange(res);
                    //(res as List<IndxElementNew>).ForEach(t => Searched.Add(t));
                    //Searched = new HashSet<IndxElementNew>(res);
                    Debug.WriteLine($"done {watch.ElapsedMilliseconds}");
                    //Searched = new List<IndxElementNew>(res.Distinct());
                    //Searched = res.Distinct().ToList();
                }
                else
                    Searched = null;
            //Searched.Clear();
        }
        private async void Worker(string path)
        {

            Is_LongOperation = true;
            Is_scanned = true;
            IndxElements indx = new IndxElements(path);
            var timer = StartCountWatcher(indx);
            //indx.CountFilesChangedEvent += Q_CountFilesChangedEvent;
            if (CancelToken.IsCancellationRequested) CancelToken = new CancellationTokenSource();
            try
            {
                bool t = await Task.Run(() => indx.DoScan(CancelToken));
            }
            catch (ProcessingFileException)
            {
                Prog_value -= indx.TotalFiles;
                indx = null;
                timer.Dispose();
                Is_scanned = false;
                StaticModel.InvokeLoadEndEvent();
                Is_LongOperation = false;
                return;
            }
            Is_LongOperation = false;
            var _ = await Task.Run(() => indx.DoSave(Def_path));
            if (!ListOfIndexes.Contains(indx))
            {
                ListOfIndexes.Add(indx);
                indx.IsSelectedChangedEvent += Q_IsSelectedChangedEvent;
            }
            indx.IsSelected = true;
            Is_scanned = false;
            timer.Dispose();
            StaticModel.InvokeLoadEndEvent();
        }
    }
}
