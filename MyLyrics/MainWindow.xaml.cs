using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MyLyrics.ViewModels;

namespace MyLyrics
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string _url = @"http://www.tekstowo.pl/piosenka,alter_bridge,blackbird.html";

        public SongViewModel ViewModel { get; set; }

        public ICommand Test => new CommandHandler(ChangeFontSize);

        private void ChangeFontSize(object parameter)
        {
            Title = DateTime.Now.ToString();

        }

        public MainWindow()
        {
            InitializeComponent();

            ViewModel = new SongViewModel();
            DataContext = ViewModel;

            ViewModel.Url = _url;

            var task = Task.Run(async () => await CheckSpotifyProcess());
        }

        private async Task CheckSpotifyProcess()
        {
            var lastTitle = string.Empty;
            while (true)
            {
                var windowTitle = Process.GetProcessesByName("spotify").SingleOrDefault(x => x.MainWindowTitle.Contains("-"))?.MainWindowTitle;
                if (!string.IsNullOrWhiteSpace(windowTitle) && lastTitle != windowTitle)
                {

                    var windowTitleParts = windowTitle.Split('-');

                    if (windowTitleParts.Length > 1)
                    {
                        ViewModel.ArtistSearch = windowTitleParts[0].Trim();
                        ViewModel.SongSearch = windowTitleParts[1].Trim();
                        ViewModel.ChangeUrl();
                        lastTitle = windowTitle;
                    }
                }
                await Task.Delay(2000);
            }

        }
    }
}
