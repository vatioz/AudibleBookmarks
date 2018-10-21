using AudibleBookmarks.Core.Utils;
using KyleHughes.AboutDialog.WPF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Imaging;


namespace AudibleBookmarks.About
{
    public class AboutInfo : IVersionable
    {
        private const string MIT = "The MIT License (MIT){0}{0}{1}{0}{0}Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the \"Software\"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:{0}{0}The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.{0}{0}THE SOFTWARE IS PROVIDED \"AS IS\", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.";

        public BitmapImage Image { get; set; } = new BitmapImage(new Uri(Path.Combine(Environment.CurrentDirectory, "Assets", "logo.png")));
        public string Copy { get; set; } = "Credit goes also to the tester ErickCBass, who helped to make this app usable.";
        public string Product { get; set; } = "AudibleBookmarks";
        public string Title { get; set; } = "Audible Bookmarks";
        public string License
        {
            get { return string.Format(MIT, Environment.NewLine, Copyright); }
            set { }
        }

        public string Copyright
        {
            get { return $"Copyright \u00a9 {Author} {string.Join(", ", Years)}"; }
            set { }
        }

        public string Version { get; set; } = TitleProvider.Version;

        public string Description { get; set; } =
            "This app allows Audible users to export bookmark notes from their account";
        public string Author { get; set; } = "Petr Kubát";
        public string Owner { get; set; } = "Petr Kubát";
        public int[] Years { get; set; } = {2018};
        public bool ShowYearsAsRange { get; set; }
        public IList<Tuple<string, Uri>> Links {
            get
            {
                return new List<Tuple<string, Uri>>
                {
                    new Tuple<string, Uri>("Project website", new Uri("https://github.com/vatioz/AudibleBookmarks")),
                    new Tuple<string, Uri>("Latest releases", new Uri("https://github.com/vatioz/AudibleBookmarks/releases"))
                };
            }
            set { }
        }
    }
}
