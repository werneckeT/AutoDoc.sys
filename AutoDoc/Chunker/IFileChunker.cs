using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoDoc.Chunker
{
    public interface IFileChunker
    {
        /// <summary>
        /// Chunks the file into smaller parts based on character count.
        /// </summary>
        /// <param name="filePath">The path to the file to be chunked.</param>
        /// <param name="maxCharsPerChunk">The maximum number of characters per chunk.</param>
        /// <returns>A list of file chunks.</returns>
        List<string> ChunkFile(string filePath, int maxCharsPerChunk);
    }
}
