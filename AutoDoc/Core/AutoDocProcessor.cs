using AutoDoc.Chunker;
using AutoDoc.Config;
using AutoDoc.LLM;
using AutoDoc.Merger;
using AutoDoc.Scanner;

namespace AutoDoc.Core
{
    public class AutoDocProcessor(
        IDirectoryScanner directoryScanner,
        IFileChunker fileChunker,
        ILlmClient llmClient,
        IChunkMerger chunkMerger,
        AutoDocConfig config)
    {
        public async Task ProcessAsync()
        {
            try
            {
                // Step 1: Scanning directories
                var files = directoryScanner.Scan(config.ProjectPath, config.ExcludedProjects ?? new List<string>(), config.FileTypes ?? new List<string>());

                foreach (var file in files)
                {
                    await ProcessFileAsync(file);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing project: {ex.Message}");
            }
        }

        private async Task ProcessFileAsync(string file)
        {
            while (true)
            {
                // Step 2: Chunking the file
                var maxCharsPerChunk = config.GetMaxCharsPerChunk();
                var chunks = fileChunker.ChunkFile(file, maxCharsPerChunk);

                var documentedChunks = new List<string>();

                foreach (var chunk in chunks)
                {
                    // Step 3: Documenting each chunk
                    var documentedChunk = await llmClient.GetDocumentationAsync(config.DocLength, chunk);

                    // Validate the response (this is done inside the LLM client)
                    documentedChunks.Add(documentedChunk);
                    Console.WriteLine($"Documented chunk: {documentedChunk}");
                    Console.WriteLine();
                }

                // Step 4: Merging the documented chunks
                var documentedFile = chunkMerger.MergeChunks(documentedChunks);

                // Step 5: Saving the documented file
                var newDocumentPath = chunkMerger.SaveMergedFile(file, documentedFile);

                // Step 6: Verify the line counts
                if (!VerifyLineCounts(file, newDocumentPath))
                {
                    Console.WriteLine($"Documentation failed for {file}. Retrying...");
                    // Retry the documentation process
                    continue;
                }

                break;
            }
        }

        private static bool VerifyLineCounts(string originalFilePath, string documentedFilePath)
        {
            try
            {
                var originalLines = File.ReadAllLines(originalFilePath).Length;
                var documentedLines = File.ReadAllLines(documentedFilePath).Length;

                return documentedLines >= originalLines * 0.9;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error verifying line counts: {ex.Message}");
                return false;
            }
        }
    }
}
