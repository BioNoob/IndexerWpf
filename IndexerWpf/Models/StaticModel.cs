using IndexerWpf.Classes;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace IndexerWpf.Models
{
    public static class StaticModel
    {
        //private static WpfObservableRangeCollection<IndxElement> elIndx = new WpfObservableRangeCollection<IndxElement>();

        //public static WpfObservableRangeCollection<IndxElement> ElIndx { get => elIndx; set { elIndx = value; } }

        //public static WpfObservableRangeCollection<string> UnicExtentions { get => new WpfObservableRangeCollection<string>(ElIndx.Select(t => t.Extension).Distinct()); }

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
        public static bool IsValidRegex(string pattern)
        {
            if (string.IsNullOrWhiteSpace(pattern)) return false;

            try
            {
                Regex.Match("", pattern);
            }
            catch (ArgumentException)
            {
                return false;
            }

            return true;
        }
    }

}
