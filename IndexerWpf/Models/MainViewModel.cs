using IndexerWpf.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace IndexerWpf.Models
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<string> ExistedIndexes { get => existedIndexs; set => SetProperty(ref existedIndexs, value); }
        public string SelectedExisted { get => selectedexisted; set { SetProperty(ref selectedexisted, value); DoLoad(value); } }
        public IndxElements Indexes { get => indexes; set { StaticModel.ElIndx = value; SetProperty(ref indexes, value); } }
        public IndxElement Selectedfile { get => selectedfile; set => SetProperty(ref selectedfile, value); }
        public double Prog_value { get => prog_val; set => SetProperty(ref prog_val, value); }
        public double Prog_value_max { get => prog_val_max; set => SetProperty(ref prog_val_max, value); }// { prog_val = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Prog_value_max")); } }
        string Def_path { get => def_path; set { def_path = value; } }
        public string Search_text { get => search_text; set { SetProperty(ref search_text, value); DoSearch(value); } }
        public ObservableCollection<IndxElement> Searched { get => searched; set => SetProperty(ref searched, value); }
        public string SelectedFilter { get => selectedFilter; set { SetProperty(ref selectedFilter, value); DoSearch(Search_text); } }
        public bool Was_scanned { get => was_scanned; set => SetProperty(ref was_scanned, value); }
        public bool Is_scanned { get => is_scanned; set => SetProperty(ref is_scanned, value); }

        private bool was_scanned = false;
        private bool is_scanned = false;
        private ObservableCollection<string> existedIndexs;
        private string def_path;
        private IndxElement selectedfile = new IndxElement();
        private IndxElements indexes = null;
        private double prog_val;
        private double prog_val_max;
        private string search_text;
        private ObservableCollection<IndxElement> searched;
        private string selectedFilter;
        private string selectedexisted;

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
        private CommandHandler _openfolder;
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
        private CommandHandler _startscan;
        public CommandHandler StartScanCommand
        {
            get
            {
                return _startscan ?? (_startscan = new CommandHandler(obj =>
                {
                    FolderBrowserDialog fbd = new FolderBrowserDialog();
                    if (fbd.ShowDialog() == DialogResult.OK)
                    {
                        if (MessageBox.Show($"Do index of\n\n{fbd.SelectedPath}", "New Indexer", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                            Worker(fbd.SelectedPath);
                        else
                            return;
                    }
                },
                (obj) => !Is_scanned
                ));
            }
        }

        public MainViewModel()
        {
            Def_path = Directory.GetCurrentDirectory() + "\\indexes";
            if (!Directory.Exists(Def_path))
                Directory.CreateDirectory(Def_path);
            else
            {
                var fls = Directory.GetFiles(Def_path);
                if (fls.Length > 0)
                {
                    foreach (var item in fls.Where(t => new FileInfo(t).Extension == ".json"))
                    {
                        //FileInfo di = new FileInfo(item);
                        //if (new FileInfo(item).Extension == ".json")
                        //select_current_cmb.Items.Add(new FileInfo(item).Name);
                        ExistedIndexes.Add(new FileInfo(item).Name);
                    }
                    //Properties.Settings.Default.Last_load = select_current_cmb.SelectedItem.ToString();
                    //Properties.Settings.Default.Save();
                }
            }

            Prog_value = 0;
            Searched = new ObservableCollection<IndxElement>();
            Indexes = null;
            //Prog_value_max = 1;

        }

        private void DoLoad(string name_of)
        {
            if(!DoSave())
            {

            }
            else
            {
                string full_nm = $"{Def_path}\\{name_of}";
                Indexes = IndxElements.LoadInexes(full_nm);
            }
        }
        private bool DoSave()
        {
            if (was_scanned)
            {
                var res = MessageBox.Show("File not saved! Save file?", "Not saved", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (res == DialogResult.Yes)
                {
                    string to_save = new DirectoryInfo(Indexes.RootFolderPath).Name/* + DateTime.Now.ToString("ddMMy_HHmm")*/ + ".json";
                    IndxElements.SaveIndexes(Indexes, Def_path + "\\" + to_save);
                    return true;
                }
                else if (res == DialogResult.Cancel)
                    return false;
                else
                    return true;
            }
            else
                return true;
        }
    
        private void DoSearch(string text)
        {
            if (Indexes != null)
                if (text.Length > 2)
                {
                    Searched.Clear();
                    if (SelectedFilter != "*" && SelectedFilter != null)
                    {
                        var b = Indexes.AllFiles.Where(t => t.Name.Contains(text, StringComparison.OrdinalIgnoreCase) && t.Extension == SelectedFilter);
                        foreach (var item in b)
                        {
                            Searched.Add(item);//(item.FullPath, item.Name, item.Tp == IndxElement.Type.file ? "док.png" : "папка.png").Tag = item;
                        }
                    }
                    else
                    {
                        var b = Indexes.AllFiles.Where(t => t.Name.Contains(text, StringComparison.OrdinalIgnoreCase));
                        foreach (var item in b)
                        {
                            Searched.Add(item);//search_result_lbx_.Items.Add(item.FullPath, item.Name, item.Tp == IndxElement.Type.file ? "док.png" : "папка.png").Tag = item;
                        }
                    }
                }
                else
                    Searched.Clear();
        }
        private async void Worker(string path)
        {
            Is_scanned = true;
            await DoScan(path);
            //Debug.WriteLine("DONE");
            Was_scanned = true;
            PropertyChanged?.Invoke(Indexes, new PropertyChangedEventArgs(nameof(Indexes.Extentions)));
            Is_scanned = false;
        }
        private async Task DoScan(string path)
        {
            //Indexes = new IndxElements(path);
            //if (Indexes == null)
            Indexes = new IndxElements(path);
            DirectoryInfo di = new DirectoryInfo(path);
            //получаем количество файлов
            prog_val = 0;
            Prog_value_max = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories).Length;
            //создаем корневую ноду по корневому каталогу
            IndxElement root = new IndxElement(di.FullName) { Tp = IndxElement.Type.folder, Prnt = null };
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
            //грузим файлы начального каталога
            Indexes.VisualFolder = new ObservableCollection<IndxElement>(Indexes.AllFiles.Where(t => t.Tp == IndxElement.Type.folder && t.Prnt == null));
            GC.Collect();
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
                FileInfo fi = new FileInfo(file);
                lie.Add(new IndxElement(fi.FullName) { Tp = IndxElement.Type.file, Prnt = parfolder.Id });
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
                DirectoryInfo di = new DirectoryInfo(subdirectory);
                IndxElement newparent = new IndxElement() { FullPath = di.FullName, Tp = IndxElement.Type.folder, Prnt = parfolder.Id };
                ie.Add(newparent);
                ie.AddRange(LoadFiles(subdirectory, /*tds,*/ newparent));
                ie.AddRange(LoadSubDirectories(subdirectory, /*tds,*/ newparent));
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
