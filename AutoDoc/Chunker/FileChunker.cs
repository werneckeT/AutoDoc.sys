using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AutoDoc.Chunker
{
    public class FileChunker : IFileChunker
    {
        public List<string> ChunkFile(string filePath, int maxCharsPerChunk)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File not found: {filePath}");
            }

            var lines = File.ReadAllLines(filePath);
            var chunks = new List<string>();
            var currentChunk = new StringBuilder();
            var currentChunkLength = 0;
            var insideClassOrMethod = false;

            foreach (var line in lines)
            {
                // Identify the start and end of a class or method
                if (line.Trim().StartsWith("public class") || line.Trim().StartsWith("private class") || line.Trim().StartsWith("protected class") ||
                    line.Trim().StartsWith("public") || line.Trim().StartsWith("private") || line.Trim().StartsWith("protected"))
                {
                    if (insideClassOrMethod && currentChunkLength + line.Length > maxCharsPerChunk)
                    {
                        // Split the chunk if the next line would exceed the max character count
                        chunks.Add(currentChunk.ToString());
                        currentChunk.Clear();
                        currentChunkLength = 0;
                    }

                    insideClassOrMethod = true;
                }

                currentChunk.AppendLine(line);
                currentChunkLength += line.Length;

                if (insideClassOrMethod && line.Trim() == "}")
                {
                    insideClassOrMethod = false;
                }
            }

            // Add the last chunk if any lines remain
            if (currentChunk.Length > 0)
            {
                chunks.Add(currentChunk.ToString());
            }

            Console.WriteLine($"Found {chunks.Count} chunks to document");
            return chunks;
        }
    }
}
