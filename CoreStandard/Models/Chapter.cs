namespace AudibleBookmarks.Core.Models
{
    public class Chapter
    {
        public string Title { get; set; }
        public long StartTime { get; set; }
        public long Duration { get; set; }

        public override string ToString()
        {
            return $"Chapter Title: {Title}, StartTime: {StartTime}, Duration: {Duration}";
        }
    }
}