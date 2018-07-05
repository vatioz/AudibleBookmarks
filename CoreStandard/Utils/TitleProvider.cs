namespace AudibleBookmarks.Core.Utils
{
    public static class TitleProvider
    {
        private const string  _version = "v0.4-alpha";
        public static string GetTitleWithVersion()
        {
            return $"Audible Bookmarks [{_version}]";
        }
    }
}
