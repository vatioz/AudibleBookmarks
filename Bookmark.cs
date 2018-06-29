using System;

namespace AudibleBookmarks
{
    public class Bookmark
    {
        public string Title { get; set; }
        public string Note { get; set; }
        public long Start { get; set; }
        public long End { get; set; }
        public TimeSpan PositionOverall => TimeSpan.FromTicks(End);

        public TimeSpan PositionChapter
        {
            get
            {
                var positionInChapter = End - Chapter.StartTime;
                return TimeSpan.FromTicks(positionInChapter);
            }
        }
        public Chapter Chapter { get; set; }
        public DateTime Modified { get; set; }
        public bool IsEmptyBookmark => string.IsNullOrWhiteSpace(Title) && string.IsNullOrWhiteSpace(Note);
    }
}