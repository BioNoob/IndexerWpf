using Newtonsoft.Json;
using System;
using System.IO;
using System.Drawing;

namespace IndexerWpf.Classes
{
    public class CustomPoint : Proper
    {
        public CustomPoint() { X = 0; Y = 0; }
        private int x;
        private int y;
        public int X { get => x; set => SetProperty(ref x, value); }
        public int Y { get => y; set => SetProperty(ref y, value); }
    }
    public class Settings : Proper
    {
        private CustomPoint windowSize;
        private CustomPoint windowLastPos;
        private string lastIndex;
        private string folderIndexesDefPath;
        [JsonIgnore]
        public CustomPoint WindowSise { get => windowSize; set => SetProperty(ref windowSize, value); }
        [JsonIgnore]
        public CustomPoint WindowLastPos { get => windowLastPos; set => SetProperty(ref windowLastPos, value); }
        [JsonIgnore]
        public string LastIndex { get => lastIndex; set => SetProperty(ref lastIndex, value); }
        public string FolderIndexesDefPath { get => folderIndexesDefPath; set => SetProperty(ref folderIndexesDefPath, value); }
        public Settings() 
        {
            WindowSise = new CustomPoint();
            WindowLastPos = new CustomPoint();
            FolderIndexesDefPath = string.Empty;
        }
        public void SaveSettings()
        {
            Properties.Settings.Default.LastPos = new Point(WindowLastPos.X, WindowLastPos.Y);
            Properties.Settings.Default.LastSize = new Size(WindowSise.X, WindowSise.Y);
            Properties.Settings.Default.LastIndex = LastIndex;
            Properties.Settings.Default.FolderIndexesDefPath = FolderIndexesDefPath;
            Properties.Settings.Default.Save();
            //File.WriteAllText(Directory.GetCurrentDirectory() +"\\settings.json", JsonConvert.SerializeObject(this, Formatting.Indented));
        }
        public void LoadSettings()
        {
            //if (File.Exists(Directory.GetCurrentDirectory() + "\\settings.json"))
            //{
            //    try
            //    {
            //        FolderIndexesDefPath = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(Directory.GetCurrentDirectory() + "\\settings.json")).FolderIndexesDefPath;
            //    }
            //    catch (Exception)
            //    {
            //        FolderIndexesDefPath = string.Empty;
            //    }
            //}
            //else
            //{
            //    FolderIndexesDefPath = string.Empty;
            //}
            FolderIndexesDefPath = Properties.Settings.Default.FolderIndexesDefPath;
            LastIndex = Properties.Settings.Default.LastIndex;
            WindowSise = new CustomPoint() {X = Properties.Settings.Default.LastSize.Width, Y = Properties.Settings.Default.LastSize.Height };
            WindowLastPos = new CustomPoint() { X = Properties.Settings.Default.LastPos.X, Y = Properties.Settings.Default.LastPos.Y };
        }
    }
}
