using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class BookmarkTests
    {
        [TestCase(0,  "----------")]        
        [TestCase(3,  "----------")]
        [TestCase(10, "*---------")]
        [TestCase(11, "*---------")]
        [TestCase(14, "*---------")]
        [TestCase(15, "-*--------")]
        [TestCase(44, "---*------")]
        [TestCase(45, "----*-----")]
        [TestCase(54, "----*-----")]
        [TestCase(94, "--------*-")]
        [TestCase(95, "---------*")]
        [TestCase(100,"---------*")]
        public void PositionVisualization(int position, string expectedVisualization)
        {
            var vizualization = AudibleBookmarks.Core.Models.Bookmark.GetPositionVisualisation(position);
            Assert.That(vizualization, Is.EqualTo(expectedVisualization));
        }

        [TestCase(0, 100, 50, 50)]
        [TestCase(0, 100, 20, 20)]
        [TestCase(0, 100, 80, 80)]
        [TestCase(0, 100, 99, 99)]
        [TestCase(5000, 1000, 5500, 50)]        
        public void GetPositionPercentage(long chapterStart, long chapterDuration, long bookmarkPosition, int expectedPercentage)
        {
            var percentage = AudibleBookmarks.Core.Models.Bookmark.GetPositionPercentage(chapterDuration, chapterStart, bookmarkPosition);
            Assert.That(percentage, Is.EqualTo(expectedPercentage));
        }
    }
}