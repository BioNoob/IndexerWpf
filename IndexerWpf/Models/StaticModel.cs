using IndexerWpf.Classes;
using System;
using System.Text.RegularExpressions;

namespace IndexerWpf.Models
{
    public static class StaticModel
    {
        public delegate void ItemExpand(IndxElementNew item);
        public static event ItemExpand ItemIsExpandChange;
        public static void InvokeItemIsExpandChange(IndxElementNew item)
        {
            ItemIsExpandChange?.Invoke(item);
        }
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
