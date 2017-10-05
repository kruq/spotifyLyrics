using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows.Input;
using HtmlAgilityPack;

namespace MyLyrics.ViewModels
{
    public class SongViewModel : INotifyPropertyChanged
    {
        private string _title;
        private string _lyric;
        private string _url;
        private string _artistSearch;
        private string _songSearch;
        private int _fontSize = 10;
        private Suggestion _selectedSuggestion;
        private ObservableCollection<Suggestion> _suggestions;

        public ICommand OnSearchCommand => new CommandHandler(DownloadSuggestions);

        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                OnPropertyChanged();
            }
        }

        public int FontSize
        {
            get { return _fontSize; }
            set
            {
                _fontSize = value;
                OnPropertyChanged();
            }
        }

        public string Lyric
        {
            get { return _lyric; }
            set
            {
                _lyric = value;
                OnPropertyChanged();
            }
        }

        public Suggestion SelectedSuggestion
        {
            get { return _selectedSuggestion; }
            set
            {
                _selectedSuggestion = value;
                OnPropertyChanged();
                SelectedSuggestionChange(value);
            }
        }

        public ObservableCollection<Suggestion> Suggestions
        {
            get { return _suggestions; }
            set
            {
                _suggestions = value;
                OnPropertyChanged();
            }
        }

        public string Url
        {
            get { return _url; }
            set
            {
                _url = value;
                OnPropertyChanged();
                DownloadSong();
            }
        }

        public void ChangeUrl()
        {
            if (!string.IsNullOrWhiteSpace(SongSearch) && !string.IsNullOrWhiteSpace(ArtistSearch))
            {
                var artist = ArtistSearch.Replace(' ', '_').Replace("'", "_");
                var song = SongSearch.Replace(' ', '_').Replace("'", "_");

                Url = $"http://www.tekstowo.pl/piosenka,{artist},{song}.html";
            }
        }

        public string ArtistSearch
        {
            get { return _artistSearch; }
            set
            {
                _artistSearch = value;
                OnPropertyChanged();
            }
        }

        public string SongSearch
        {
            get { return _songSearch; }
            set
            {
                _songSearch = value;
                OnPropertyChanged();
            }
        }

        public ICommand UpFontSize => new CommandHandler(ChangeFontSize);

        private void ChangeFontSize(object parameter)
        {
            FontSize = Math.Max(1, FontSize + int.Parse(parameter.ToString()));

        }

        public ICommand DownFontSize => new CommandHandler(ChangeFontSize);


        private void DownloadSuggestions(object parameter = null)
        {
            try
            {
                var url =
                    $"http://www.tekstowo.pl/szukaj,wykonawca,{ArtistSearch},tytul,{SongSearch}.html";
                var client = new HttpClient();
                var result = client.GetStringAsync(url);

                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(result.Result);



                var content =
                    htmlDocument.DocumentNode.Descendants()
                        .FirstOrDefault(
                            x => x.Name == "h2")
                        .ParentNode
                        .ChildNodes.Where(x => x.Attributes.Contains("class") && x.Attributes["class"].Value == "box-przeboje")
                        .SelectMany(x => x.Elements("a"))
                        .Where(IsTextValid)
                        .Select(x => new Suggestion(x.GetAttributeValue("title", string.Empty), x.GetAttributeValue("href", string.Empty)));

                //.Select(x => x.InnerText.Trim()).Select(x => x.Substring(x.IndexOf(".", StringComparison.Ordinal)+1).Trim())
                //.Where(x => !regex.IsMatch(x));

                Suggestions = new ObservableCollection<Suggestion>(content);
            }
            catch (AggregateException)
            {

            }
        }

        private bool IsTextValid(HtmlNode htmlNode)
        {
            var regex = new Regex(@"\(\d+\)$");
            var text = htmlNode.InnerText?.Trim();
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }
            text = text.Substring(text.IndexOf(".", StringComparison.CurrentCulture) + 1).Trim();
            return !regex.IsMatch(text);
        }

        private void DownloadSong()
        {

            try
            {
                var client = new HttpClient();
                var result = client.GetStringAsync(_url);

                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(result.Result);

                var content =
                    htmlDocument.DocumentNode.Descendants()
                        .FirstOrDefault(
                            x => x.Attributes.Contains("class") && x.Attributes["class"].Value.Contains("song-text"))
                        .InnerText;
                content =
                    content.Replace("Tekst piosenki:", "")
                        .Replace("Poznaj historię zmian tego tekstu", "")
                        .Replace("&nbsp;", "")
                        .Trim();

                var title =
                    htmlDocument.DocumentNode.Descendants()
                        .FirstOrDefault(
                            x => x.Attributes.Contains("class") && x.Attributes["class"].Value.Contains("belka"))
                        .ChildNodes.SingleOrDefault(x => x.Name == "strong")
                        .InnerText;

                Lyric = content;
                Title = title;
            }
            catch (AggregateException ex)
            {
                var firstOrDefault = ex.InnerExceptions.FirstOrDefault();
                if (firstOrDefault != null && firstOrDefault.Message.Contains("404"))
                {
                    DownloadSuggestions();
                    if (Suggestions.Any())
                    {
                        SelectedSuggestionChange(Suggestions.FirstOrDefault());
                    }
                }
            }
        }

        private void SelectedSuggestionChange(Suggestion suggestion)
        {
            var url = suggestion?.Url;
            //var title = suggestion?.Title;

            if (string.IsNullOrWhiteSpace(url))
            {
                return;
            }
            //if (!string.IsNullOrEmpty(title))
            //{
            //    var splitedSuggestion = title.Split('-');
            //    if (splitedSuggestion.Count() != 2) return;
            //    ArtistSearch = splitedSuggestion[0].Trim().ToLower();
            //    SongSearch = splitedSuggestion[1].Trim().ToLower();
            //}
            Url = @"http://www.tekstowo.pl" + url;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
