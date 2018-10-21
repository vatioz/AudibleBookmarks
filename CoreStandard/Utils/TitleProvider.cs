namespace AudibleBookmarks.Core.Utils
{
    public static class TitleProvider
    {
        public const string  Version = "0.10-alpha";
        public static string GetTitleWithVersion()
        {
            return $"Audible Bookmarks [v{Version}]";
        }
    }
}
