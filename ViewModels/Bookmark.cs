using System;

namespace AudibleBookmarks.ViewModels
{
    public class Bookmark
    {
        public string Title { get; set; }
        public string Note { get; set; }
        public long Start { get; set; }
        public long End { get; set; }

        public TimeSpan PositionOverallTS => TimeSpan.FromTicks(End);
        public string PositionOverall => $"{(int)PositionOverallTS.TotalHours:00}:{PositionOverallTS.Minutes:00}:{PositionOverallTS.Seconds:00}";

        public TimeSpan PositionChapterTS
        {
            get
            {
                var positionInChapter = End - Chapter.StartTime;
                return TimeSpan.FromTicks(positionInChapter);
            }
        }

        public string PositionChapter => PositionChapterTS.TotalHours > 0
            ? $"{(int) PositionChapterTS.TotalHours:00}:{PositionChapterTS.Minutes:00}:{PositionChapterTS.Seconds:00}"
            : $"{PositionChapterTS.Minutes:00}:{PositionChapterTS.Seconds:00}";

        public Chapter Chapter { get; set; }
        public DateTime Modified { get; set; }
        public bool IsEmptyBookmark => string.IsNullOrWhiteSpace(Title) && string.IsNullOrWhiteSpace(Note);
    }
}