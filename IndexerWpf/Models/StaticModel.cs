using IndexerWpf.Classes;
using System.Collections.ObjectModel;

namespace IndexerWpf.Models
{
    public static class StaticModel
    {
        private static ObservableCollection<IndxElement> elIndx;

        public static ObservableCollection<IndxElement> ElIndx { get => elIndx; set { elIndx = value; } }

        //public delegate void ItemDrawed(IndxElement db);
        //public static event ItemDrawed RemoveItemEvent;
        //public static void InvokeRemoveItemEvent(IndxElement db)
        //{
        //    RemoveItemEvent?.Invoke(db);
        //}

        public delegate void LoadEnd();
        public static event LoadEnd LoadEndEvent;
        public static void InvokeLoadEndEvent()
        {
            LoadEndEvent?.Invoke();
        }
        
        public static event LoadEnd WindowClosing;
        public static void InvokeWindowClosing()
        {
            WindowClosing?.Invoke();
        }
        //private ObservableCollection<IndxElement> visualFolder;
        //public ObservableCollection<IndxElement> VisualFolder { get => visualFolder; set { visualFolder = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("visualFolder")); } }

    }

}
