using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using HtmlAgilityPack;
using Lyrics.Annotations;

namespace Lyrics.ViewModels
{
    public class SongViewModel : INotifyPropertyChanged
    {
        private string _title;
        private string _lyric;
        private string _url;
        private string _artistSearch;
        private string _songSearch;
        private string _selectedSuggestion;
        private ObservableCollection<string> _suggestions;


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

        public string Lyric
        {
            get { return _lyric; }
            set
            {
                _lyric = value;
                OnPropertyChanged();
            }
        }

        public string SelectedSuggestion
        {
            get { return _selectedSuggestion; }
            set
            {
                _selectedSuggestion = value;
                OnPropertyChanged();
                SelectedSuggestionChange(value);
            }
        }

        public ObservableCollection<string> Suggestions
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
                Url = $"http://www.tekstowo.pl/piosenka,{ArtistSearch.Replace(' ', '_')},{SongSearch.Replace(' ', '_')}.html";
            }
        }

        public string ArtistSearch
        {
            get { return _artistSearch; }
            set
            {
                _artistSearch = value;
                OnPropertyChanged();
                ChangeUrl();
            }
        }

        public string SongSearch
        {
            get { return _songSearch; }
            set
            {
                _songSearch = value;
                OnPropertyChanged();
                ChangeUrl();
            }
        }

        private void DownloadSuggestions()
        {
            try
            {
                var url =
                    $"http://www.tekstowo.pl/szukaj,wykonawca,{ArtistSearch?.Replace(' ', '_')},tytul,{SongSearch?.Replace(' ', '_')}.html";
                var client = new HttpClient();
                var result = client.GetStringAsync(url);

                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(result.Result);

                var content =
                    htmlDocument.DocumentNode.Descendants()
                        .FirstOrDefault(
                            x => x.Name == "h2")
                        .ParentNode.ChildNodes.Where(
                            x => x.Attributes.Contains("class") && x.Attributes["class"].Value == "box-przeboje").Select(x => x.InnerText.Trim()).Select(x => x.Substring(x.IndexOf(".", StringComparison.Ordinal)+1).Trim());

                Suggestions = new ObservableCollection<string>(content);
            }
            catch (AggregateException)
            {

            }
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
                            x => x.Attributes.Contains("class") && x.Attributes["class"].Value.Contains("belka short"))
                        .ChildNodes.SingleOrDefault(x => x.Name == "strong")
                        .InnerText;

                Lyric = content;
                Title = title;
            }
            catch (AggregateException)
            {

            }
        }

        private void SelectedSuggestionChange(string suggestion)
        {
            var splitedSuggestion = suggestion.Split('-');
            if (splitedSuggestion.Count() != 2) return;
            ArtistSearch = splitedSuggestion[0].Trim().ToLower();
            SongSearch = splitedSuggestion[1].Trim().ToLower();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
