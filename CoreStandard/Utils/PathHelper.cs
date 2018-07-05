using System;
using System.IO;
using System.Linq;

namespace AudibleBookmarks.Core.Utils
{
    public class PathHelper
    {

        // TODO this is Windows 10 specific

        public static string TryToGuessPathToLibrary()
        {
            var localAppData = Environment.GetEnvironmentVariable("LOCALAPPDATA");
            var pathToPackages = $"{localAppData}\\Packages";
            if (!Directory.Exists(pathToPackages))
                return string.Empty;
            var packages = Directory.EnumerateDirectories(pathToPackages);
            var audiblePackage = packages.FirstOrDefault(p => p.Contains("AudibleforWindowsPhone"));
            if (string.IsNullOrWhiteSpace(audiblePackage))
                return string.Empty;
            var pathToLibrary = $"{audiblePackage}\\LocalState\\library.db";
            if (File.Exists(pathToLibrary))
                return pathToLibrary;
            else
                return string.Empty;
        }
    }
}
