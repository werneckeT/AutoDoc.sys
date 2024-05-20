using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoDoc.Scanner
{
    public interface IDirectoryScanner
    {
        /// <summary>
        /// Scans the directory and returns a list of file paths matching the specified criteria.
        /// </summary>
        /// <param name="startPath">The starting directory path.</param>
        /// <param name="excludedSubfolders">Subfolders to exclude from scanning.</param>
        /// <param name="fileTypes">File types to include in the scanning.</param>
        /// <returns>A list of file paths that match the criteria.</returns>
        List<string> Scan(string startPath, List<string> excludedSubfolders, List<string> fileTypes);
    }
}
