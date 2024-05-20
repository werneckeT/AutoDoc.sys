using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoDoc.Scanner
{
    public class DirectoryScanner : IDirectoryScanner
    {
        public List<string> Scan(string startPath, List<string> excludedSubfolders, List<string> fileTypes)
        {
            if (!Directory.Exists(startPath))
            {
                throw new DirectoryNotFoundException($"Start directory not found: {startPath}");
            }

            var files = new List<string>();
            var directories = new Stack<string>();
            directories.Push(startPath);

            while (directories.Count > 0)
            {
                var currentDirectory = directories.Pop();

                if (excludedSubfolders.Any(currentDirectory.Contains))
                    continue;

                foreach (var directory in Directory.GetDirectories(currentDirectory)) 
                    directories.Push(directory);

                files.AddRange(Directory.GetFiles(currentDirectory)
                    .Where(file => fileTypes.Contains(Path.GetExtension(file))));
            }

            return files;
        }
    }
}
