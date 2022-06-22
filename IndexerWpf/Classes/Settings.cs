using Newtonsoft.Json;
using System;
using System.IO;
using System.Windows;

namespace IndexerWpf.Classes
{
    public class Settings
    {
        public Size WindowSise { get; set; }
        public Point WindowLastPos { get; set; }
        public string FolderIndexesDefPath { get; set; }
        public Settings() 
        {
            WindowSise = new Size();
            WindowLastPos = new Point();
            FolderIndexesDefPath = string.Empty;
        }
        public void SaveSettings()
        {
            File.WriteAllText(Directory.GetCurrentDirectory() +"\\settings.json", JsonConvert.SerializeObject(this, Formatting.Indented));
        }
        public static Settings LoadSettings()
        {
            if (File.Exists(Directory.GetCurrentDirectory() + "\\settings.json"))
            {
                try
                {
                    var a = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(Directory.GetCurrentDirectory() + "\\settings.json"));
                    return a;
                }
                catch (Exception)
                {
                    return null;
                }

            }
            else
            {
                return null;
            }
        }
    }
}
