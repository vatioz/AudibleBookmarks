using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using AudibleBookmarks.Core.Messenger;
using AudibleBookmarks.Core.Models;
using Microsoft.Data.Sqlite;

namespace AudibleBookmarks.Core.Services
{
    public class DatabaseService
    {
        private SqliteConnection _connection;

        public void OpenSqliteConnection(string pathToLibrary)
        {
            try
            {
                _connection?.Close();

                //_connection = new SqliteConnection($"Data Source={pathToLibrary};Version=3;");
                _connection = new SqliteConnection($"Data Source={pathToLibrary};");
                _connection.Open();
            }
            catch (Exception ex)
            {
                _connection = null;
                PublishException(ex);
            }
        }

        private void PublishException(Exception ex)
        {
            TinyMessengerHub.Instance.Publish(new GenericTinyMessage<Exception>(this, ex));
        }

        private Dictionary<string, List<string>> GetNarrators()
        {
            if (_connection == null || _connection.State != ConnectionState.Open)
                return new Dictionary<string, List<string>>();

            var narratorDictionary = new Dictionary<string, List<string>>();

            try
            {
                var sqlNarrators = "select Asin, Narrator from BookNarrators";
                var commandNarrators = new SqliteCommand(sqlNarrators, _connection);
                var readerNarrators = commandNarrators.ExecuteReader();
                while (readerNarrators.Read())
                {
                    var asin = GetValue<string>(readerNarrators,"Asin");
                    var narrator = GetValue<string>(readerNarrators,"Narrator");
                    if (narratorDictionary.ContainsKey(asin))
                        narratorDictionary[asin].Add(narrator);
                    else
                        narratorDictionary.Add(asin, new List<string> { narrator });
                }
            }
            catch (Exception ex)
            {
                PublishException(new Exception("Error while loading narrator", ex));
            }
            return narratorDictionary;
        }

        private Dictionary<string, List<string>> GetAuthors()
        {
            if (_connection == null || _connection.State != ConnectionState.Open)
                return new Dictionary<string, List<string>>();
            var authorDictionary = new Dictionary<string, List<string>>();

            try
            {
                var sqlAuthors = "select Asin, Author from BookAuthors";
                var commandAuthors = new SqliteCommand(sqlAuthors, _connection);
                var readerAuthors = commandAuthors.ExecuteReader();
                while (readerAuthors.Read())
                {
                    var asin = GetValue<string>(readerAuthors,"Asin");
                    var author = GetValue<string>(readerAuthors,"Author");
                    if (authorDictionary.ContainsKey(asin))
                        authorDictionary[asin].Add(author);
                    else
                        authorDictionary.Add(asin, new List<string> { author });
                }
            }
            catch (Exception ex)
            {
                PublishException(new Exception("Error while loading author", ex));
            }
            return authorDictionary;
        }

        public IEnumerable<Book> GetBooks()
        {
            var authorDictionary = GetAuthors();
            var narratorDictionary = GetNarrators();

            if (_connection == null || _connection.State != ConnectionState.Open)
                return Enumerable.Empty<Book>();


            var books = new List<Book>();
            try
            {
                var sql = "select b.Asin, b.Title, b.Duration, b.FileName from Books b";
                var command = new SqliteCommand(sql, _connection);
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var asin = GetValue<string>(reader,"Asin");
                    var authors = authorDictionary.ContainsKey(asin)
                        ? authorDictionary[asin]
                        : Enumerable.Empty<string>();
                    var narrators = narratorDictionary.ContainsKey(asin)
                        ? narratorDictionary[asin]
                        : Enumerable.Empty<string>();

                    var book = new Book
                    {
                        Asin = asin,
                        Title = GetValue<string>(reader,"Title"),
                        IsDownloaded = GetValue<string>(reader,"FileName") != null,
                        RawLength = GetValue<long>(reader,"Duration"),
                        Authors = authors,
                        Narrators = narrators
                    };
                    books.Add(book);
                }
            }
            catch (Exception ex)
            {
                PublishException(new Exception("Error while loading book", ex));
            }

            return books;
        }

        public void LoadChapters(Book selectedBook)
        {
            if (_connection == null || selectedBook == null || selectedBook.Chapters.Count > 0)
                return;

            try
            {
                var sql = $"select StartTime, Name, Duration From Chapters Where Asin = '{selectedBook.Asin}'";
                var command = new SqliteCommand(sql, _connection);
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var ch = new Chapter
                    {
                        Title = GetValue<string>(reader,"Name"),
                        Duration = GetValue<long>(reader,"Duration"),
                        StartTime = GetValue<long>(reader,"StartTime")
                    };
                    selectedBook.Chapters.Add(ch);
                }

            }
            catch (Exception ex)
            {
                PublishException(new Exception("Error while loading chapter", ex));
            }
        }

        public void LoadBookmarks(Book selectedBook)
        {
            if (_connection == null || selectedBook == null || selectedBook.Bookmarks.Count > 0)
                return;

            try
            {
                var sql = $"select Position, StartPosition, Note, Title, LastModifiedTime From Bookmarks Where Asin = '{selectedBook.Asin}'";
                var command = new SqliteCommand(sql, _connection);
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    try
                    {
                        var position = GetValue<long>(reader,"Position");
                        var startPosition = GetValue<long>(reader,"StartPosition");
                        selectedBook.Bookmarks.Add(new Bookmark
                        {
                            Note = Sanitize(GetValue<string>(reader,"Note")),
                            Title = Sanitize(GetValue<string>(reader,"Title")),
                            Modified = DateTime.Parse(GetValue<string>(reader,"LastModifiedTime")),
                            End = position,
                            Start = startPosition,
                            Chapter = selectedBook.GetChapter(position)
                        });
                    }
                    catch (Exception ex)
                    {
                        PublishException(new Exception("Error while loading bookmark", ex));
                    }
                }
            }
            catch (Exception ex)
            {
                PublishException(new Exception("Error while loading bookmarks", ex));
            }
        }

        private T GetValue<T>(SqliteDataReader reader, string columnName)
        {
            var raw = reader[columnName];
            if (raw is DBNull)
                return default(T);

            return (T) raw;
        }

        private string Sanitize(string str)
        {
            if (str == null)
                return string.Empty;

            string decoded = str.Replace("&amp;", "&").Replace("&lt;", "<").Replace("&gt;", ">").Replace("&quot;", "\"").Replace("&apos;", "'");
            return decoded;
        }
    }
}
