using AudibleBookmarks.Core.Messenger;
using AudibleBookmarks.Core.Models;
using log4net;
using log4net.Core;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace AudibleBookmarks.Core.Services
{
    public class DatabaseService
    {
        private static ILog _logger = LogManager.GetLogger(typeof(DatabaseService));
        private SqliteConnection _connection;

        public void OpenSqliteConnection(string pathToLibrary)
        {
            _logger.Info($"OpenSqliteConnection with file {pathToLibrary}");
            try
            {
                _logger.Info($"Closing any possibly open previous connection.");
                _connection?.Close();

                //_connection = new SqliteConnection($"Data Source={pathToLibrary};Version=3;");
                _connection = new SqliteConnection($"Data Source={pathToLibrary};");
                _connection.Open();
                _logger.Info("Connection open.");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while opening connection. Ex: {ex}");
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
            _logger.Info($"GetNarrators()");
            if (_connection == null || _connection.State != ConnectionState.Open)
                return new Dictionary<string, List<string>>();

            var narratorDictionary = new Dictionary<string, List<string>>();

            try
            {
                var sqlNarrators = "select Asin, Narrator from BookNarrators";
                var commandNarrators = new SqliteCommand(sqlNarrators, _connection);
                _logger.Info($"Querying DB with {sqlNarrators}");
                var readerNarrators = commandNarrators.ExecuteReader();
                while (readerNarrators.Read())
                {
                    var asin = GetValue<string>(readerNarrators, "Asin");
                    var narrator = GetValue<string>(readerNarrators, "Narrator");
                    if (narratorDictionary.ContainsKey(asin))
                    {
                        narratorDictionary[asin].Add(narrator);
                        _logger.Debug($"Added another narrator {narrator} for ASIN {asin}");
                    }
                    else
                    {
                        narratorDictionary.Add(asin, new List<string> { narrator });
                        _logger.Debug($"Added new narrator {narrator} for ASIN {asin}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while loading narrator. Ex: {ex}");
                PublishException(new Exception("Error while loading narrator", ex));
            }
            return narratorDictionary;
        }

        private Dictionary<string, List<string>> GetAuthors()
        {
            _logger.Info($"GetAuthors()");
            if (_connection == null || _connection.State != ConnectionState.Open)
                return new Dictionary<string, List<string>>();

            var authorDictionary = new Dictionary<string, List<string>>();

            try
            {
                var sqlAuthors = "select Asin, Author from BookAuthors";
                var commandAuthors = new SqliteCommand(sqlAuthors, _connection);
                _logger.Info($"Querying DB with {sqlAuthors}");
                var readerAuthors = commandAuthors.ExecuteReader();
                while (readerAuthors.Read())
                {
                    var asin = GetValue<string>(readerAuthors, "Asin");
                    var author = GetValue<string>(readerAuthors, "Author");
                    if (authorDictionary.ContainsKey(asin))
                    {
                        authorDictionary[asin].Add(author);
                        _logger.Debug($"Added another narrator {author} for ASIN {asin}");
                    }
                    else
                    {
                        authorDictionary.Add(asin, new List<string> { author });
                        _logger.Debug($"Added new narrator {author} for ASIN {asin}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while loading author. Ex: {ex}");
                PublishException(new Exception("Error while loading author", ex));
            }
            return authorDictionary;
        }

        public IEnumerable<Book> GetBooks()
        {
            _logger.Info($"GetBooks()");
            var authorDictionary = GetAuthors();
            var narratorDictionary = GetNarrators();

            if (_connection == null || _connection.State != ConnectionState.Open)
                return Enumerable.Empty<Book>();


            var books = new List<Book>();
            try
            {
                var sql = "select b.Asin, b.Title, b.Duration, b.DownloadState from Books b";
                var command = new SqliteCommand(sql, _connection);
                _logger.Info($"Querying DB with {sql}");
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var asin = GetValue<string>(reader, "Asin");
                    var authors = authorDictionary.ContainsKey(asin)
                        ? authorDictionary[asin]
                        : Enumerable.Empty<string>();
                    var narrators = narratorDictionary.ContainsKey(asin)
                        ? narratorDictionary[asin]
                        : Enumerable.Empty<string>();

                    var book = new Book
                    {
                        Asin = asin,
                        Title = GetValue<string>(reader, "Title"),
                        DownloadState = GetValue<long>(reader, "DownloadState"),
                        RawLength = GetValue<long>(reader, "Duration"),
                        Authors = authors,
                        Narrators = narrators
                    };
                    books.Add(book);
                    _logger.Debug($"Loaded new book {book}");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while loading book. Ex: {ex}");
                PublishException(new Exception("Error while loading book", ex));
            }

            return books;
        }

        public void LoadChapters(Book selectedBook)
        {
            _logger.Info($"LoadChapters()");
            if (_connection == null || selectedBook == null || selectedBook.Chapters.Count > 0)
                return;

            try
            {
                var sql = $"select StartTime, Name, Duration From Chapters Where Asin = '{selectedBook.Asin}'";
                var command = new SqliteCommand(sql, _connection);
                _logger.Info($"Querying DB with {sql}");
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var ch = new Chapter
                    {
                        Title = GetValue<string>(reader, "Name"),
                        Duration = GetValue<long>(reader, "Duration"),
                        StartTime = GetValue<long>(reader, "StartTime")
                    };
                    selectedBook.Chapters.Add(ch);
                    _logger.Debug($"Loaded new chapter for book {selectedBook.Title}: {ch}");
                }

            }
            catch (Exception ex)
            {
                _logger.Error($"Error while loading chapter. Ex: {ex}");
                PublishException(new Exception("Error while loading chapter", ex));
            }
        }

        public void LoadBookmarks(Book selectedBook)
        {
            _logger.Info($"LoadBookmarks()");
            if (_connection == null || selectedBook == null || selectedBook.Bookmarks.Count > 0)
                return;

            try
            {
                var sql = $"select Position, StartPosition, Note, Title, LastModifiedTime From Bookmarks Where Asin = '{selectedBook.Asin}'";
                var command = new SqliteCommand(sql, _connection);
                _logger.Info($"Querying DB with {sql}");
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    try
                    {
                        var position = GetValue<long>(reader, "Position");
                        var startPosition = GetValue<long>(reader, "StartPosition");
                        var bookmark = new Bookmark
                        {
                            Note = Sanitize(GetValue<string>(reader, "Note")),
                            Title = Sanitize(GetValue<string>(reader, "Title")),
                            Modified = DateTime.Parse(GetValue<string>(reader, "LastModifiedTime")),
                            End = position,
                            Start = startPosition,
                            Chapter = selectedBook.GetChapter(position)
                        };
                        selectedBook.Bookmarks.Add(bookmark);
                        _logger.Debug($"Loaded new bookmark for book {selectedBook.ShortTitle} at {bookmark.PositionOverall} (empty: {bookmark.IsEmptyBookmark})");
                        _logger.Logger.Log(null, Level.Fine, $"Bookmark details: {bookmark}", null);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Error while loading bookmark. Ex: {ex}");
                        PublishException(new Exception("Error while loading bookmark", ex));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while loading bookmarks. Ex: {ex}");
                PublishException(new Exception("Error while loading bookmarks", ex));
            }
        }

        private T GetValue<T>(SqliteDataReader reader, string columnName)
        {
            var raw = reader[columnName];
            if (raw is DBNull)
                return default(T);

            return (T)raw;
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
