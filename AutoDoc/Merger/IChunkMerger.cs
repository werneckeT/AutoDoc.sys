using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoDoc.Merger
{
    public interface IChunkMerger
    {
        /// <summary>
        /// Merges the documented chunks into a single file content.
        /// </summary>
        /// <param name="chunks">The list of documented chunks.</param>
        /// <returns>The merged file content.</returns>
        string MergeChunks(List<string> chunks);

        /// <summary>
        /// Saves the merged file content to a new file.
        /// </summary>
        /// <param name="originalFilePath">The original file path.</param>
        /// <param name="mergedContent">The merged file content.</param>
        string SaveMergedFile(string originalFilePath, string mergedContent);
    }
}
