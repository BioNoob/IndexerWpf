using System;
using System.IO;
using System.Linq;
using System.Threading;
using IndexerWpf.Models;
using Newtonsoft.Json;
namespace IndexerWpf.Classes
{
    public class IndxElements : Proper, IDisposable, IEquatable<IndxElements>
    {
        private IndxElementNew allFiles;
        private string rootFolderPath;
        private string dateOfLastChange;
        private bool isSelected;

        public event IsSelecetdChange IsSelectedChangedEvent;
        public delegate void IsSelecetdChange(IndxElements sender, bool state);

        [JsonIgnore]
        public string GetName { get => string.IsNullOrEmpty(RootFolderPath) ? Path.GetFileNameWithoutExtension(JsonFileName) : Path.GetDirectoryName(RootFolderPath); }
        [JsonIgnore]
        public int TotalFiles { get => RootElement.CountElements - 1; }//-1 = self; }
        [JsonIgnore]
        public bool IsSelected { get => isSelected; set { SetProperty(ref isSelected, value); IsSelectedChangedEvent?.Invoke(this, value); } }

        public string RootFolderPath { get => rootFolderPath; set { SetProperty(ref rootFolderPath, value); } }
        public string DateOfLastChange { get => dateOfLastChange; set => SetProperty(ref dateOfLastChange, value); }
        public IndxElementNew RootElement { get => allFiles; set { SetProperty(ref allFiles, value); } }
        [JsonIgnore]
        public string JsonFileName { get; set; }
        public IndxElements()
        {
            IsSelected = false;
            RootFolderPath = string.Empty;
            Init();
        }

        private void Init()
        {
            RootElement = new IndxElementNew();
            IndxElementNew.Identificator = 0;
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
        public bool LoadInexes(CancellationTokenSource token)//string file_to_load)
        {
            if (File.Exists(JsonFileName))
            {
                try
                {
                    var a = JsonConvert.DeserializeObject<IndxElements>(File.ReadAllText(JsonFileName));
                    RootFolderPath = a.RootFolderPath;
                    DateOfLastChange = a.DateOfLastChange;
                    RootElement = a.RootElement;
                    if (string.IsNullOrEmpty(RootElement.FullPath))
                        throw new ProcessingFileException(ProcessingFileException.TypeOfError.Invalid, JsonFileName);
                    //StaticModel.ElIndx.AddRange(AllFiles);
                    var all_elem = RootElement.AllLowerElements;//Descendants();
                    foreach (var elem in all_elem)
                    {
                        if(token.IsCancellationRequested)
                            throw new ProcessingFileException(ProcessingFileException.TypeOfError.CancelTask, null);
                        if (elem.ChildElements != null)
                            elem.ChildElements.ToList().ForEach(t => t.Parent = elem);
                    }
                    a.Dispose();
                    //Debug.WriteLine("DONE DESER");
                    return true;
                }
                catch (ProcessingFileException e)
                {
                    //return null;
                    throw e;
                }

            }
            else
            {
                throw new ProcessingFileException(ProcessingFileException.TypeOfError.Deleted, JsonFileName);
                //return null;
            }
        }

        public void Dispose()
        {
            RootElement = null;
            //VisualFolder = null;
            //Extentions = null;
            DateOfLastChange = null;
            RootFolderPath = null;
            //ID = -1;
            //StaticModel.LoadEndEvent -= StaticModel_LoadEndEvent;
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
    }
}
