using IndexerWpf.Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IndexerWpf.Models
{
    public class MainViewModel : Proper
    {
        public WpfObservableRangeCollection<string> ExistedIndexes { get => existedIndexs; set => SetProperty(ref existedIndexs, value); }
        public string SelectedExisted { get => selectedexisted; set { SetProperty(ref selectedexisted, value); if (!string.IsNullOrEmpty(value)) DoLoad(value); } }
        public IndxElements Indexes { get => indexes; set { StaticModel.ElIndx = value.AllFiles; SetProperty(ref indexes, value); } }
        public IndxElement Selectedfile { get => selectedfile; set => SetProperty(ref selectedfile, value); }
        public double Prog_value { get => prog_val; set => SetProperty(ref prog_val, value); }
        public double Prog_value_max { get => prog_val_max; set => SetProperty(ref prog_val_max, value); }// { prog_val = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Prog_value_max")); } }
        public string Def_path { get => Sets.FolderIndexesDefPath; }
        public string Search_text { get => search_text; set { SetProperty(ref search_text, value); DoSearch(value); } }
        public WpfObservableRangeCollection<IndxElement> Searched { get => searched; set => SetProperty(ref searched, value); }
        public string SelectedFilter { get => selectedFilter; set { SetProperty(ref selectedFilter, value); DoSearch(Search_text); } }
        public bool Was_Loaded { get => was_loaded; set => SetProperty(ref was_loaded, value); }
        public bool Is_scanned { get => is_scanned; set => SetProperty(ref is_scanned, !value); }
        public Settings Sets { get => sets; set => SetProperty(ref sets, value); }
        //[JsonIgnore]
        public WpfObservableRangeCollection<IndxElement> VisualFolder { get => visualfolder; set { SetProperty(ref visualfolder, value); } }


        private WpfObservableRangeCollection<IndxElement> visualfolder;


        private bool was_loaded = false;
        private bool is_scanned = false;
        private WpfObservableRangeCollection<string> existedIndexs;
        private IndxElement selectedfile = new IndxElement();
        private IndxElements indexes = null;
        private double prog_val;
        private double prog_val_max;
        private string search_text;
        private WpfObservableRangeCollection<IndxElement> searched;
        private string selectedFilter;
        private string selectedexisted;
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
                    Sets.LastIndex = SelectedExisted;
                    Sets.SaveSettings();
                    Environment.Exit(0);
                },
                (obj) => Is_scanned
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

            ExistedIndexes = new WpfObservableRangeCollection<string>();
            Indexes = new IndxElements();
            Searched = new WpfObservableRangeCollection<IndxElement>();
            VisualFolder = new WpfObservableRangeCollection<IndxElement>();

            StaticModel.LoadEndEvent += StaticModel_LoadEndEvent;
            fbd = new FolderBrowserDialog();
            fbd.ShowNewFolderButton = true;
            fbd.AutoUpgradeEnabled = true;
            fbd.RootFolder = Environment.SpecialFolder.Recent;
            Is_scanned = false;
            LoadListIndexes();

            SelectedExisted = Sets.LastIndex;

            Prog_value = 0;
        }

        private void LoadListIndexes()
        {
            ExistedIndexes.Clear();
            Indexes.Clear();
            VisualFolder.Clear();
            if (!Directory.Exists(Def_path))
                Directory.CreateDirectory(Def_path);
            else
            {
                var fls = Directory.GetFiles(Def_path);
                if (fls.Length > 0)
                {
                    foreach (var item in fls.Where(t => Path.GetExtension(t) == ".json"))
                        ExistedIndexes.Add(Path.GetFileNameWithoutExtension(item));
                }
            }
        }

        private void StaticModel_LoadEndEvent()
        {
            VisualFolder.Clear();
            VisualFolder.AddRange(Indexes.AllFiles.Where(t => t.Prnt == null));
        }

        private void DoLoad(string name_of)
        {
            string full_nm = $"{Def_path}\\{name_of}.json";
            try
            {
                Indexes.LoadInexes(full_nm);
                //Костыль
                Indexes = Indexes;
            }
            catch (Exception)
            {
                var ind = ExistedIndexes.IndexOf(name_of);
                ExistedIndexes.Remove(name_of);
                if (ind > 0)
                    SelectedExisted = ExistedIndexes.ElementAt(ind - 1);
                else
                    SelectedExisted = null;
                return;
            }

            StaticModel.InvokeLoadEndEvent();
            if (Indexes != null)
            {
                Was_Loaded = true;
                if (!string.IsNullOrEmpty(Search_text))
                    DoSearch(Search_text);
                Prog_value = Indexes.TotalFiles;
                Prog_value_max = Prog_value;
            }
        }
        //Сохранение полюбому в конце парсинга. Нет ситуации где бы оно не сохранилось. Если только отказаться перезаписывать
        private bool DoSave()
        {
            string to_save = Path.GetFileName(Indexes.RootFolderPath) /*new DirectoryInfo(Indexes.RootFolderPath).Name/* + DateTime.Now.ToString("ddMMy_HHmm")*/ + ".json";
            string full_to_save = Def_path + "\\" + to_save;
            if (new FileInfo(full_to_save).Exists)
                if (MessageBox.Show($"Overwrite File\n{to_save} ?\n(No = cancel save)", "Already exist", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    Indexes.SaveIndexes(full_to_save);
                else
                    return false;
            else
                Indexes.SaveIndexes(full_to_save);
            return true;

        }
        private void DoSearch(string text)
        {
            if (Indexes != null)
                if (!string.IsNullOrEmpty(text) && text.Length > 1)
                {
                    Searched.Clear();
                    List<IndxElement> res = new List<IndxElement>();
                    if (StaticModel.IsValidRegex(text))
                    {
                        var reg = new Regex(@$"^{text}$", RegexOptions.IgnoreCase);
                        res = Indexes.AllFiles.Where(t => reg.IsMatch(t.Name)).ToList();
                    }

                    if (SelectedFilter != "*" && SelectedFilter != null)
                    {
                        Searched.AddRange(res.Where(t => t.Extension == SelectedFilter));
                        Searched.AddRange(Indexes.AllFiles.Where(t => t.Name.Contains(text, StringComparison.OrdinalIgnoreCase) && t.Extension == SelectedFilter));
                    }
                    else
                    {
                        Searched.AddRange(res);
                        Searched.AddRange(Indexes.AllFiles.Where(t => t.Name.Contains(text, StringComparison.OrdinalIgnoreCase)));
                    }
                }
                else
                    Searched.Clear();
        }
        private async void Worker(string path)
        {
            Is_scanned = true;
            Was_Loaded = false;
            if(!await DoScan(path))
            {
                return;
            }
            //Debug.WriteLine("DONE");
            DoSave();
            var nm = Path.GetFileNameWithoutExtension(Indexes.RootFolderPath);
            if (!ExistedIndexes.Contains(nm))
                ExistedIndexes.Add(nm);
            SetProperty(ref selectedexisted, nm, "SelectedExisted");
            Was_Loaded = true;
            Is_scanned = false;
            StaticModel.InvokeLoadEndEvent();
        }
        private async Task<bool> DoScan(string path)
        {
            Search_text = string.Empty;
            Indexes.Dispose();
            Indexes = new IndxElements(path);
            //получаем количество файлов
            Prog_value = 0;
            var dirs = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
            //Directory.EnumerateFiles()
            Prog_value_max = dirs.Length;
            if(Prog_value_max > 10000)
            {
                MessageBox.Show($"Too many files ({Prog_value_max})\n Please use internal directories!", "Too many files", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Prog_value = 1;
                Prog_value_max = 1;
                return false;
            }
                
            //создаем корневую ноду по корневому каталогу
            IndxElement root = new IndxElement(Path.GetFullPath(path)) { Tp = IndxElement.Type.folder, Prnt = null };
            Indexes.AllFiles.Add(root);
            await Task.Run(() =>
            {
                foreach (var item in LoadFiles(path, root))
                {
                    Indexes.AllFiles.Add(item);
                }
                foreach (var item in LoadSubDirectories(path, root))
                {
                    Indexes.AllFiles.Add(item);
                }
            });
            GC.Collect();
            return true;
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
            string[] Files = Directory.GetFiles(dir, "*.*");
            foreach (string file in Files)
            {
                //FileInfo fi = new FileInfo(file);
                Path.GetFullPath(file);
                lie.Add(new IndxElement(Path.GetFullPath(file)/*fi.FullName*/) { Tp = IndxElement.Type.file, Prnt = parfolder.Id });
                UpdateProgress();

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
            string[] subdirectoryEntries = Directory.GetDirectories(dir);
            foreach (string subdirectory in subdirectoryEntries)
            {
                IndxElement newparent = new IndxElement() { FullPath = Path.GetFullPath(subdirectory), Tp = IndxElement.Type.folder, Prnt = parfolder.Id };
                ie.Add(newparent);
                ie.AddRange(LoadFiles(subdirectory, newparent));
                ie.AddRange(LoadSubDirectories(subdirectory,  newparent));
                UpdateProgress();
            }
            return ie;
        }
        private void UpdateProgress()
        {
            if (Prog_value < Prog_value_max)
            {
                Prog_value++;
            }
        }
    }
}
