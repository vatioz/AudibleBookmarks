using System;
using System.IO;
using System.Linq;
using log4net;

namespace AudibleBookmarks.Core.Utils
{
    public class PathHelper
    {
        private static ILog _logger = LogManager.GetLogger(typeof(PathHelper));

        // TODO this is Windows 10 specific

        public static string TryToGuessPathToLibrary()
        {
            _logger.Info($"TryToGuessPathToLibrary()");

            var localAppData = Environment.GetEnvironmentVariable("LOCALAPPDATA");
            var pathToPackages = $"{localAppData}\\Packages";
            _logger.Debug($"local app data packages: {pathToPackages}");
            if (!Directory.Exists(pathToPackages))
                return string.Empty;

            var packages = Directory.EnumerateDirectories(pathToPackages);
            _logger.Debug($"Found {packages.Count()} packages");
            var audiblePackage = packages.FirstOrDefault(p => p.Contains("AudibleforWindowsPhone"));
            _logger.Info($"Found this audible package folder: {audiblePackage}");
            if (string.IsNullOrWhiteSpace(audiblePackage))
                return string.Empty;

            var pathToLibrary = $"{audiblePackage}\\LocalState\\library.db";
            if (File.Exists(pathToLibrary))
            {
                _logger.Info($"File was found: {pathToLibrary}");
                return pathToLibrary;
            }
            else
            {
                _logger.Info($"File was not found.");
                return string.Empty;
            }
        }
    }
}
