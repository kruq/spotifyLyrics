using System;

namespace MyLyrics.ViewModels
{
    public class Suggestion
    {
        public string Title { get; private set; }
        public string Url { get; private set; }

        public Suggestion(string title, string url)
        {
            Title = title.Trim();
            Url = url.Trim();
        }

        public override string ToString()
        {
            return Title;
            ;
        }
    }
}