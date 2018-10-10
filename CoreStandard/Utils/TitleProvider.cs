namespace AudibleBookmarks.Core.Utils
{
    public static class TitleProvider
    {
        private const string  _version = "v0.9-alpha";
        public static string GetTitleWithVersion()
        {
            return $"Audible Bookmarks [{_version}]";
        }
    }
}
