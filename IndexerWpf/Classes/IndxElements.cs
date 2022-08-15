using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using IndexerWpf.Models;
using Newtonsoft.Json;
namespace IndexerWpf.Classes
{
    public class IndxElements : Proper, IDisposable, IEquatable<IndxElements>
    {
        private IndxElementNew rootElement;
        private string rootFolderPath;
        private string dateOfLastChange;
        private bool isSelected;
        private WpfObservableRangeCollection<IndxElementNew> allLowerElements;
        private int simpleCounter = 0;

        //private WpfObservableRangeCollection<SimpleIndxElement> allSimpleLowerElements;

        public event IsSelecetdChange IsSelectedChangedEvent;
        public delegate void IsSelecetdChange(IndxElements sender, bool state);
        //public event CountFilesChanged CountFilesChangedEvent;
        //public delegate void CountFilesChanged(int val);
        [JsonIgnore]
        public string GetName
        {
            get
            {
                //if (string.IsNullOrEmpty(RootFolderPath))
                //    return Path.GetFileNameWithoutExtension(JsonFileName);
                Regex pattern = new Regex(@":|\\+|\/+");
                return pattern.Replace(new DirectoryInfo(RootFolderPath).Name, "");
            }

        }
        [JsonIgnore]
        public int TotalFiles { get => RootElement.CountElements - 1; }//-1 = self; }
        [JsonIgnore]
        public bool IsSelected { get => isSelected; set { SetProperty(ref isSelected, value); IsSelectedChangedEvent?.Invoke(this, value); } }
        [JsonIgnore]
        public string JsonFileName { get; set; }
        [JsonProperty(Order = -1)]
        public string RootFolderPath { get => rootFolderPath; set { SetProperty(ref rootFolderPath, value); } }
        public string DateOfLastChange { get => dateOfLastChange; set => SetProperty(ref dateOfLastChange, value); }
        public IndxElementNew RootElement { get => rootElement; set { SetProperty(ref rootElement, value); if (value == null) allLowerElements = null; } }
        [JsonIgnore]
        public WpfObservableRangeCollection<IndxElementNew> AllLowerElements { get => allLowerElements ??= new WpfObservableRangeCollection<IndxElementNew>(Descendants()); }
        //[JsonIgnore]
        // public WpfObservableRangeCollection<SimpleIndxElement> AllSimpleLowerElements { get => allSimpleLowerElements ??= new WpfObservableRangeCollection<SimpleIndxElement>(AllLowerElements.ToList().Select(t => new SimpleIndxElement(t))); }
        private IEnumerable<IndxElementNew> Descendants(/*this IndxElementNew root*/)
        {
            var nodes = new Stack<IndxElementNew>(new[] { RootElement });
            while (nodes.Any())
            {
                IndxElementNew node = nodes.Pop();
                //yield return new SimpleIndxElement(() => node.Id, () => node.IsSelected, n => { node.IsSelected = n; }, () => RootFolderPath, () => node.Parent, n => { node.Parent = n; });
                yield return node;
                if (node.ChildElements != null)
                    foreach (var n in node.ChildElements) nodes.Push(n);
            }
        }
        [JsonIgnore]
        public bool IsCounterChange { get; set; }
        [JsonIgnore]
        public bool IsLoaded { get; set; }
        [JsonIgnore]
        //public int ChildIdentificatorCouner = 0;
        public int SimpleCounter { get => simpleCounter; set { simpleCounter = value; IsCounterChange = true; } }
        public IndxElements()
        {
            IsSelected = false;
            RootFolderPath = string.Empty;
            Init();
        }

        private void Init()
        {
            IsLoaded = false;
            RootElement = null;
            //RootElement = new IndxElementNew();
            //RootElement.Id = 0;
            SimpleCounter = 0;
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
        public async Task<bool> LoadInexes(CancellationTokenSource cts)//string file_to_load)
        {
            var token = cts.Token;
            if (File.Exists(JsonFileName))
            {
                try
                {
                    //ПРОВЕРИТЬ НА КАНСЕЛ ТОКЕН. DONE
                    await Task.Run(() =>
                    {
                        //token.Register(() => token.ThrowIfCancellationRequested());//throw new ProcessingFileException(TypeOfError.CancelTask, null, this));
                        StaticModel.IdincreasedEvent += StaticModel_IdincreasedEvent;
                        //var qq = File.ReadLines(JsonFileName); //вроде быстро
                        //var watch = Stopwatch.StartNew();
                        //Debug.WriteLine($"{JsonFileName} started {watch.ElapsedMilliseconds}");
                        IndxElements a = null;
                        using (StreamReader file = File.OpenText(JsonFileName))
                        {
                            JsonSerializer serializer = new JsonSerializer();

                            a = (IndxElements)serializer.Deserialize(file, typeof(IndxElements));
                            //Debug.WriteLine($"{JsonFileName} deserialize {watch.ElapsedMilliseconds}"); //25mb file done by 4sec
                            file.Close();
                        }

                        //string json = await File.ReadAllTextAsync(JsonFileName);
                        //Debug.WriteLine($"{JsonFileName} readed {watch.ElapsedMilliseconds}");
                        //var a = await Task.Run(() => JsonConvert.DeserializeObject<IndxElements>(json)); //25mb file done by 12sec
                        //Debug.WriteLine($"{JsonFileName} deserialize {watch.ElapsedMilliseconds}");
                        //watch.Stop();

                        //RootFolderPath = a.RootFolderPath;
                        //DateOfLastChange = a.DateOfLastChange;
                        RootElement = a.RootElement;
                        if (string.IsNullOrEmpty(RootElement.FullPath))
                            throw new ProcessingFileException(TypeOfError.Invalid, JsonFileName, this);
                        //StaticModel.ElIndx.AddRange(AllFiles);
                        var all_elem = AllLowerElements;//Descendants();
                        foreach (var elem in all_elem)
                        {
                            //if (token.IsCancellationRequested)
                            //    throw new ProcessingFileException(TypeOfError.CancelTask, null, this);
                            if (elem.ChildElements != null)
                                elem.ChildElements.ToList().ForEach(t => t.Parent = elem);
                            //UpdateProgress();
                        }
                        // UpdateProgress();
                        _ = AllLowerElements;
                        //_ = AllSimpleLowerElements;
                        a.Dispose();
                    }, token);
                    IsLoaded = true;
                    return true;
                }
                catch (ProcessingFileException e)
                {
                    if (e.ErrorType == TypeOfError.CancelTask)
                        throw new ProcessingFileException(TypeOfError.CancelTask, null, this);
                    else
                        throw e;
                }
                finally
                {
                    StaticModel.IdincreasedEvent -= StaticModel_IdincreasedEvent;
                }
            }
            else
            {
                throw new ProcessingFileException(TypeOfError.Deleted, JsonFileName, this);
                //return null;
            }
        }

        private void StaticModel_IdincreasedEvent(int val, string el)
        {
            //ЕСЛИ ПЕРЕИМЕНУЮТ JSON файл вся логика пойдет в пезду (не пошла. Но элемент с неверным названием нужно удалить!// вроде все норм
            //if (val == 0 && el.Contains(GetName))
            //    this.RootFolderPath = el;
            if (!string.IsNullOrEmpty(RootFolderPath) && el.Contains(RootFolderPath))
                if(SimpleCounter <= val)
                SimpleCounter = val;
        }

        public bool DoScan(CancellationTokenSource token)
        {
            //var indexes = new IndxElements(path);
            //получаем количество файлов
            //Prog_value = 0;
            //создаем корневую ноду по корневому каталогу
            try
            {
                RootElement = new IndxElementNew(Path.GetFullPath(RootFolderPath), IndxElementNew.Type.folder, null, SimpleCounter);
                LoadFiles(RootFolderPath, RootElement, token);
                LoadSubDirectories(RootFolderPath, RootElement, token);
                //RootElement.ChildElements.AddRange(LoadFiles(RootFolderPath, RootElement, token));
                //RootElement.ChildElements.AddRange(LoadSubDirectories(RootFolderPath, RootElement, token));
                GC.Collect();
            }
            catch (ProcessingFileException e)
            {
                throw e;
            }
            return true;
        }
        public void Dispose()
        {
            RootElement = null;
            DateOfLastChange = null;
            RootFolderPath = null;
            IsSelectedChangedEvent = null;
            //CountFilesChangedEvent = null;
            GC.SuppressFinalize(this);
        }
        public bool Equals(IndxElements other)
        {
            return this.RootElement == other.RootElement &&
                   this.RootFolderPath == other.RootFolderPath;
        }
        public override string ToString()
        {
            return GetName;
        }
        /// <summary>
        /// Ищем файлы в указанном каталоге
        /// </summary>
        /// <param name="dir">каталог</param>
        /// <param name="td">предыдущая нода</param>
        /// <returns>Лист найденных элементов</returns>
        private void LoadFiles(string dir, IndxElementNew parfolder, CancellationTokenSource token)
        {
            //List<IndxElementNew> lie = new List<IndxElementNew>();
            try
            {
                var Files = Directory.EnumerateFiles(dir, "*.*");
                foreach (string file in Files)
                {
                    if (token.IsCancellationRequested)
                        throw new ProcessingFileException(TypeOfError.CancelTask, null, this);
                    parfolder.ChildElements.Add(new IndxElementNew(Path.GetFullPath(file), IndxElementNew.Type.file, parfolder, ++SimpleCounter));
                    //SimpleCounter++;
                }

            }
            catch (UnauthorizedAccessException)
            {
                //return lie;
            }
            // return lie;
        }
        /// <summary>
        /// Парсим поддериктории (Рекурсивная штука)
        /// </summary>
        /// <param name="dir">каталог</param>
        /// <param name="td">предыдущая нода</param>
        /// <returns>Лист найденных элементов</returns>
        private void LoadSubDirectories(string dir, IndxElementNew parfolder, CancellationTokenSource token)
        {
            //List<IndxElementNew> ie = new List<IndxElementNew>();
            try
            {
                var subs = Directory.EnumerateDirectories(dir);
                foreach (string subdirectory in subs)
                {
                    if (token.IsCancellationRequested)
                        throw new ProcessingFileException(TypeOfError.CancelTask, null, this);
                    IndxElementNew newparent = new IndxElementNew(Path.GetFullPath(subdirectory), IndxElementNew.Type.folder, parfolder, ++SimpleCounter);
                    parfolder.ChildElements.Add(newparent);
                    //SimpleCounter++;
                    LoadFiles(subdirectory, newparent, token);
                    LoadSubDirectories(subdirectory, newparent, token);
                    //UpdateProgress();
                    //newparent.ChildElements.AddRange();
                    //newparent.ChildElements.AddRange(;
                    //ie.Add(newparent);
                    //UpdateProgress();
                }
            }
            catch (UnauthorizedAccessException)
            {
                //return ie;
            }
            //return ie;
        }
        private void UpdateProgress()
        {
            //CountFilesChangedEvent?.Invoke(TotalFiles);
        }
        //Сохранение полюбому в конце парсинга. Нет ситуации где бы оно не сохранилось. Если только отказаться перезаписывать
        public bool DoSave(string folder_to_saave_path)
        {
            string to_save = GetName /*new DirectoryInfo(Indexes.RootFolderPath).Name/* + DateTime.Now.ToString("ddMMy_HHmm")*/ + ".json";
            string full_to_save = folder_to_saave_path + "\\" + to_save;
            JsonFileName = full_to_save;
            if (new FileInfo(full_to_save).Exists)
                if (MessageBox.Show($"Overwrite File\n{to_save} ?\n(No = cancel save)", "Already exist", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    SaveIndexes(full_to_save);
                else
                    return false;
            else
                SaveIndexes(full_to_save);
            return true;

        }
    }
}
