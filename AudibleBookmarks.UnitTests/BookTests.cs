using AudibleBookmarks.Core.Models;
using NUnit.Framework;
using System;

namespace AudibleBookmarks.UnitTests
{
    [TestFixture]
    public class BookTests
    {
        [TestCase("00:01:10.9130000", "Chapter 1")]
        [TestCase("00:01:13.8850000", "Chapter 1")]
        [TestCase("00:09:35.2970000", "Chapter 1")]
        [TestCase("00:33:15.7080000", "Chapter 1")]
        [TestCase("00:36:29.6410000", "Chapter 1")]
        [TestCase("00:36:51.3290000", "Chapter 2")]
        [TestCase("00:38:34.0540000", "Chapter 2")]
        [TestCase("04:36:46.4920000", "Chapter 7")]
        [TestCase("07:32:51.1080000", "Chapter 10")]
        [TestCase("07:33:10.7990000", "Chapter 11")]
        [TestCase("08:38:15.7450000", "Chapter 11")]
        [TestCase("09:10:18.6360000", "Chapter 11")]
        public void GetChapter_SeveralBookmarks_FindsChapter(string position, string expectedChapterTitle)
        {
            var book = new Book();
            book.Chapters.Add(new Chapter { Title = "Chapter 1", StartTime = 0, Duration = 22062670000 });
            book.Chapters.Add(new Chapter { Title = "Chapter 2", StartTime = 22062670000, Duration = 26524150000 });
            book.Chapters.Add(new Chapter { Title = "Chapter 3", StartTime = 48586820000, Duration = 27388400000 });
            book.Chapters.Add(new Chapter { Title = "Chapter 4", StartTime = 75975220000, Duration = 34468630000 });
            book.Chapters.Add(new Chapter { Title = "Chapter 5", StartTime = 110443850000, Duration = 25981740000 });
            book.Chapters.Add(new Chapter { Title = "Chapter 6", StartTime = 136425590000, Duration = 19608790000 });
            book.Chapters.Add(new Chapter { Title = "Chapter 7", StartTime = 156034380000, Duration = 22164370000 });
            book.Chapters.Add(new Chapter { Title = "Chapter 8", StartTime = 178198750000, Duration = 32625430000 });
            book.Chapters.Add(new Chapter { Title = "Chapter 9", StartTime = 210824180000, Duration = 26542730000 });
            book.Chapters.Add(new Chapter { Title = "Chapter 10", StartTime = 237366910000, Duration = 34415220000 });
            book.Chapters.Add(new Chapter { Title = "Chapter 11", StartTime = 271782130000, Duration = 59178380000 });

            var positionTicks = TimeSpan.Parse(position).Ticks;
            var chapter = book.GetChapter(positionTicks);

            Assert.That(chapter.Title, Is.EqualTo(expectedChapterTitle));
        }
    }
}
