using System;
using System.Text;

namespace AudibleBookmarks.Core.Models
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

        public int PositionChapterPercentage
        {
            get
            {
                return GetPositionPercentage(Chapter.Duration, Chapter.StartTime, End);
            }
        }

        public static int GetPositionPercentage(long chapterDuration, long chapterStart, long bookmarkPosition)
        {
            var positionInChapter = bookmarkPosition - chapterStart;
            var positionPercentage = positionInChapter * 100 / chapterDuration;
            return (int)positionPercentage;
        }

        public string PositionVisualization
        {
            get
            {
                return GetPositionVisualisation(PositionChapterPercentage);
            }
        }

        public static string GetPositionVisualisation(int percentage)
        {
            var vis = new StringBuilder("----------");
            var index = (int)Math.Round(percentage / 10.0, MidpointRounding.AwayFromZero);

            if(index != 0)
                vis[index - 1] = '*';

            return vis.ToString();
        }

        public string PositionChapter => PositionChapterTS.TotalHours > 0
            ? $"{(int) PositionChapterTS.TotalHours:00}:{PositionChapterTS.Minutes:00}:{PositionChapterTS.Seconds:00}"
            : $"{PositionChapterTS.Minutes:00}:{PositionChapterTS.Seconds:00}";

        public Chapter Chapter { get; set; }
        public DateTime Modified { get; set; }
        public bool IsEmptyBookmark => string.IsNullOrWhiteSpace(Title) && string.IsNullOrWhiteSpace(Note);

        public override string ToString()
        {
            return $"Bookmark Title: {Title}, PositionChapter: {PositionChapter}, PositionOverallTS: {PositionOverallTS}";
        }
    }
}