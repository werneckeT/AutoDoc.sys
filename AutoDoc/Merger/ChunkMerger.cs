using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AutoDoc.Merger
{
    public class ChunkMerger : IChunkMerger
    {
        public string MergeChunks(List<string> chunks)
        {
            if (chunks == null || chunks.Count == 0)
                throw new ArgumentException("Chunks list cannot be null or empty.");

            var mergedContent = new StringBuilder();
            int openBraces = 0;
            int closeBraces = 0;

            for (int i = 0; i < chunks.Count; i++)
            {
                var chunk = chunks[i];
                if (i < chunks.Count - 1)
                {
                    // Remove the last closing brace if exists in the middle of chunks
                    chunk = RemoveTrailingClosingBrace(chunk);
                }

                mergedContent.Append(chunk);

                // Count the number of open and close braces
                openBraces += CountChar(chunk, '{');
                closeBraces += CountChar(chunk, '}');
            }

            // Ensure the last chunk has a closing brace if needed
            if (openBraces > closeBraces)
            {
                mergedContent.AppendLine("}");
            }

            return mergedContent.ToString();
        }

        private string RemoveTrailingClosingBrace(string chunk)
        {
            chunk = chunk.TrimEnd();
            if (chunk.EndsWith("}"))
            {
                return chunk.Substring(0, chunk.Length - 1);
            }
            return chunk;
        }

        private int CountChar(string text, char charToCount)
        {
            int count = 0;
            foreach (char c in text)
            {
                if (c == charToCount)
                {
                    count++;
                }
            }
            return count;
        }

        public string SaveMergedFile(string originalFilePath, string mergedContent)
        {
            if (string.IsNullOrEmpty(originalFilePath))
                throw new ArgumentException("Original file path cannot be null or empty.");

            if (string.IsNullOrEmpty(mergedContent))
                throw new ArgumentException("Merged content cannot be null or empty.");

            // Determine the directory and filename for the new file
            var directory = Path.GetDirectoryName(originalFilePath);
            var fileName = SanitizeFileName(Path.GetFileNameWithoutExtension(originalFilePath) + "_documented" + Path.GetExtension(originalFilePath));
            var newFilePath = Path.Combine(directory ?? throw new InvalidOperationException("Directory not found to combine to new path"), fileName);

            File.WriteAllText(newFilePath, mergedContent);
            return newFilePath;
        }

        private string SanitizeFileName(string fileName)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '_');
            }
            return fileName;
        }
    }
}
