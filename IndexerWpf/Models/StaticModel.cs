using IndexerWpf.Classes;
using System;
using System.Collections.Generic;
using System.Text;

namespace IndexerWpf.Models
{
    public static class StaticModel
    {
        private static IndxElements elIndx;

        public static IndxElements ElIndx { get => elIndx; set { elIndx = value; } }

        public delegate void ItemDrawed(IndxElement db);
        public static event ItemDrawed RemoveItem;
        public static void InvokeRemoveItem(IndxElement db)
        {
            RemoveItem?.Invoke(db);
        }
        //private ObservableCollection<IndxElement> visualFolder;
        //public ObservableCollection<IndxElement> VisualFolder { get => visualFolder; set { visualFolder = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("visualFolder")); } }

    }
}
