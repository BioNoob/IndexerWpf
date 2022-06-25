using System.Diagnostics;
using System.Windows;

namespace IndexerWpf
{
    /// <summary>
    /// Логика взаимодействия для RegexHelpWindow.xaml
    /// </summary>
    public partial class RegexHelpWindow : Window
    {
        public RegexHelpWindow()
        {
            InitializeComponent();
            this.MouseLeftButtonDown += delegate { this.DragMove(); };
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }
    }
}
