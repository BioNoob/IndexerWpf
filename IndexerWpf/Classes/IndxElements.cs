using System;
using System.IO;
using System.Linq;
using IndexerWpf.Models;
using Newtonsoft.Json;
#pragma warning disable CS0660
#pragma warning disable CS0661
namespace IndexerWpf.Classes
{
    public class IndxElements : Proper, IDisposable
    {
        private WpfObservableRangeCollection<IndxElement> allFiles;
        private string rootFolderPath;
        private string dateOfLastChange;
        private bool isSelected;
        private WpfObservableRangeCollection<string> extentions;
        
        public event IsSelecetdChange IsSelectedChangedEvent;
        public delegate void IsSelecetdChange(IndxElements sender, bool state);

        [JsonIgnore]
        public string GetName { get => Path.GetFileNameWithoutExtension(RootFolderPath); }

        [JsonIgnore]
        public bool IsSelected { get => isSelected; set { SetProperty(ref isSelected, value); IsSelectedChangedEvent?.Invoke(this, value); } }
        [JsonIgnore]
        public int TotalFiles { get => AllFiles.Count(t => t.Tp == IndxElement.Type.file); }

        [JsonIgnore]
        public WpfObservableRangeCollection<string> Extentions { get => extentions; set { SetProperty(ref extentions, value); } }

        public string RootFolderPath { get => rootFolderPath; set { SetProperty(ref rootFolderPath, value); } }
        public string DateOfLastChange { get => dateOfLastChange; set => SetProperty(ref dateOfLastChange, value); }
        public WpfObservableRangeCollection<IndxElement> AllFiles { get => allFiles; set { SetProperty(ref allFiles, value); } }

        public IndxElements()
        {
            IsSelected = false;
            RootFolderPath = string.Empty;
            Init();
        }

        private void Init()
        {
            AllFiles = new WpfObservableRangeCollection<IndxElement>();
            Extentions = new WpfObservableRangeCollection<string>();
            StaticModel.LoadEndEvent += StaticModel_LoadEndEvent;
            IndxElement.Identificator = 0;
        }

        public void Clear()
        {
            AllFiles = new WpfObservableRangeCollection<IndxElement>();
            Extentions = new WpfObservableRangeCollection<string>();
            IndxElement.Identificator = 0;
            RootFolderPath = string.Empty;
        }

        private void StaticModel_LoadEndEvent()
        {
            Extentions.Clear();
            Extentions.AddRange(AllFiles.Select(t => t.Extension).Distinct());
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
        public void LoadInexes(string file_to_load)
        {
            if (File.Exists(file_to_load))
            {
                try
                {
                    var a = JsonConvert.DeserializeObject<IndxElements>(File.ReadAllText(file_to_load));
                    RootFolderPath = a.RootFolderPath;
                    DateOfLastChange = a.DateOfLastChange;
                    AllFiles = a.AllFiles;
                    a.Dispose();
                    //Debug.WriteLine("DONE DESER");
                    //return a;
                }
                catch (Exception e)
                {
                    //return null;
                    System.Windows.Forms.MessageBox.Show($"File {file_to_load} load error\n{e.Message}!", "Error load", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    throw;
                }

            }
            else
            {
                System.Windows.Forms.MessageBox.Show($"File {file_to_load} was deleted!", "Error load", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                throw new Exception();
                //return null;
            }
        }

        public void Dispose()
        {
            AllFiles = null;
            //VisualFolder = null;
            //Extentions = null;
            DateOfLastChange = null;
            RootFolderPath = null;
            //ID = -1;
            StaticModel.LoadEndEvent -= StaticModel_LoadEndEvent;
            GC.SuppressFinalize(this);
        }
    }
}
