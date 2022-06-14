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

namespace IndexerWpf.Models
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public IndxElements Indexes { get => indexes; set { StaticModel.ElIndx = value; SetProperty(ref indexes, value);  } }
        public IndxElement Selectedfile { get => selectedfile; set => SetProperty(ref selectedfile, value); }
        public int Prog_value { get => prog_val; set => SetProperty(ref prog_val, value); }
        public int Prog_value_max { get => prog_val; set => SetProperty(ref prog_val_max, value); }// { prog_val = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Prog_value_max")); } }
        string Def_path { get => def_path; set { def_path = value; /*curr_catalog_txt.Text = value;*/ } }
        

        private bool was_scanned = false;
        private bool is_scanned = false;

        private string def_path;
        private IndxElement selectedfile = new IndxElement();
        private IndxElements indexes = null;
        private int prog_val;
        private int prog_val_max;
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
                    Process.Start("explorer.exe", $"{def_path}");
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
                        {
                            //folder_tree.Nodes.Clear();
                            Indexes = DoScan(fbd.SelectedPath);
                            foreach (var item in Indexes.AllFiles)
                            {
                                var b = item.Items;
                            }
                            was_scanned = true;
                            //folder_tree.Nodes[0].Expand();
                            //select_current_cmb.Items.Add(new FileInfo(fbd.SelectedPath).Name);
                            //select_current_cmb.SelectedIndexChanged -= select_current_cmb_SelectedIndexChanged;
                            //select_current_cmb.SelectedIndex = select_current_cmb.Items.Count - 1;
                            //select_current_cmb.SelectedIndexChanged += select_current_cmb_SelectedIndexChanged;
                        }
                        else
                            return;
                    }
                },
                (obj) => !is_scanned
                ));
            }
        }
        public MainViewModel()
        {

        }
        private IndxElements DoScan(string path)
        {
            IndxElements ie = new IndxElements(path);
            DirectoryInfo di = new DirectoryInfo(path);
            //получаем количество файлов
            //progressBar1.Maximum = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories).Length;
            Prog_value_max = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories).Length;
            //file_cnt_lbl.Text = progressBar1.Maximum.ToString();
            //создаем корневую ноду по корневому каталогу
            //TreeNode tds = folder_tree.Nodes.Add(di.FullName, di.Name, "папка.png");
            //tds.SelectedImageKey = tds.ImageKey;
            IndxElement root = new IndxElement(di.FullName) { Tp = IndxElement.Type.folder, Prnt = null };
            ie.AllFiles.Add(root);
            //грузим файлы начального каталога
            foreach (var item in LoadFiles(path, root))
            {
                ie.AllFiles.Add(item);
            }
            foreach (var item in LoadSubDirectories(path, root))
            {
                ie.AllFiles.Add(item);
            }
            ie.VisualFolder = new ObservableCollection<IndxElement>(ie.AllFiles.Where(t=>t.Tp == IndxElement.Type.folder));

            //запускаем рекурсивный поиск
            //sel_ext_cmb.Items.Clear();
            //sel_ext_cmb.Items.Add("*");
            //суем все найденные расширения
            //sel_ext_cmb.Items.AddRange(ie.AllFiles.Select(t => t.Extension).Distinct().ToArray());
            //sel_ext_cmb.SelectedIndex = 0;
            GC.Collect();
            //progressBar1.CreateGraphics().DrawString("Done", new Font("Arial", (float)8.25, FontStyle.Regular), Brushes.Black, new PointF(progressBar1.Width / 2 - 10, progressBar1.Height / 2 - 7));
            return ie;
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
                //var a = td.Nodes.Add(fi.FullName, fi.Name, "док.png");
                //a.ToolTipText = parfolder.Name;
                //a.SelectedImageKey = a.ImageKey;
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
                //TreeNode tds = td.Nodes.Add(di.FullName, di.Name, "папка.png");
                IndxElement newparent = new IndxElement() { FullPath = di.FullName, Tp = IndxElement.Type.folder, Prnt = parfolder.Id };
                ie.Add(newparent);

                //tds.ToolTipText = newparent.Name;
                //tds.SelectedImageKey = tds.ImageKey;
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
                //int percent = (int)(((double)Prog_value / (double)Prog_value_max) * 100);
                //рисуем процентик
                //progressBar1.CreateGraphics().DrawString(percent.ToString() + "%", new Font("Arial", (float)8.25, FontStyle.Regular), Brushes.Black, new PointF(progressBar1.Width / 2 - 10, progressBar1.Height / 2 - 7));
                //Application.DoEvents();
            }
        }
    }
}
