using System;
using System.IO;
using System.Linq;
using IndexerWpf.Models;
using Newtonsoft.Json;
#pragma warning disable CS0660
#pragma warning disable CS0661
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
        public string GetName { get => Path.GetFileNameWithoutExtension(RootFolderPath); }
        [JsonIgnore]
        public int TotalFiles { get => RootElement.CountElements - 1; }//-1 = self; }
        [JsonIgnore]
        public bool IsSelected { get => isSelected; set { SetProperty(ref isSelected, value); IsSelectedChangedEvent?.Invoke(this, value); } }

        public string RootFolderPath { get => rootFolderPath; set { SetProperty(ref rootFolderPath, value); } }
        public string DateOfLastChange { get => dateOfLastChange; set => SetProperty(ref dateOfLastChange, value); }
        public IndxElementNew RootElement { get => allFiles; set { SetProperty(ref allFiles, value); } }

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
        public bool LoadInexes()//string file_to_load)
        {
            if (File.Exists(RootFolderPath))
            {
                try
                {
                    var a = JsonConvert.DeserializeObject<IndxElements>(File.ReadAllText(RootFolderPath));
                    RootFolderPath = a.RootFolderPath;
                    DateOfLastChange = a.DateOfLastChange;
                    RootElement = a.RootElement;
                    //StaticModel.ElIndx.AddRange(AllFiles);
                    var all_elem = RootElement.Descendants();
                    foreach (var elem in all_elem)
                    {
                        if(elem.ChildElements != null)
                        elem.ChildElements.ToList().ForEach(t => t.Parent = elem);
                    }
                    a.Dispose();
                    //Debug.WriteLine("DONE DESER");
                    return true;
                }
                catch (Exception e)
                {
                    //return null;
                    System.Windows.Forms.MessageBox.Show($"File {RootFolderPath} load error\n{e.Message}!", "Error load", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    throw;
                }

            }
            else
            {
                System.Windows.Forms.MessageBox.Show($"File {RootFolderPath} was deleted!", "Error load", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                throw new Exception();
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
    }
}
