using AutoDoc.CommentIndentationFixer;
using AutoDoc.Config;
using AutoDoc.LLM;
using AutoDoc.MethodExtractor;
using AutoDoc.MethodMerger;
using AutoDoc.Scanner;
using System;

namespace AutoDoc.Core
{
    public class AutoDocProcessor(
        IDirectoryScanner directoryScanner,
        IMethodExtractor methodExtractor,
        ILlmClient llmClient,
        IMethodMerger methodMerger,
        ICommentIndentationFixer commentIndentationFixer,
        AutoDocConfig config)
    {
        public async Task ProcessAsync()
        {
            try
            {
                // Step 1: Scanning directories
                var files = directoryScanner.Scan(config.ProjectPath, config.ExcludedProjects ?? [], config.FileTypes ?? []);

                var totalFiles = files.Count;
                var processedFiles = 0;

                foreach (var file in files)
                {
                    processedFiles++;
                    await ProcessFileAsync(file, processedFiles, totalFiles);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing project: {ex.Message}");
            }
        }

        private async Task ProcessFileAsync(string file, int processedFiles, int totalFiles)
        {
            while (true)
            {
                // Step 2: Extracting methods from the file
                var methods = methodExtractor.ExtractMethods(file);

                if (methods.Count == 0)
                {
                    Console.WriteLine($"No methods found in file {file}, {processedFiles}/{file}");
                    return;
                }

                var documentedMethods = new List<(string comment, string method, string indentation)>();
                var totalMethods = methods.Count;
                var processedMethods = 0;

                Console.WriteLine($"Processing file {processedFiles}/{totalFiles}: {file}");

                foreach (var (method, indentation) in methods)
                {
                    processedMethods++;
                    Console.WriteLine($"Processing method {processedMethods}/{totalMethods} in file {file}");

                    // Step 3: Documenting each method
                    var documentedComment = await llmClient.GetDocumentationAsync(config.DocLength, method);

                    // Validate the response (this is done inside the LLM client)
                    documentedMethods.Add((documentedComment, method, indentation));

                    // Display progress
                    var percentComplete = (int)((double)processedMethods / totalMethods * 100);
                    Console.WriteLine($"Documented method {processedMethods}/{totalMethods} ({percentComplete}% complete): {method.TrimStart()[..Math.Min(method.Length, 40)]}...");
                }

                Console.WriteLine($"Completed documentation for file {processedFiles}/{totalFiles}: {file}");

                // Step 4: Merging the documented methods
                var documentedFile = methodMerger.MergeMethods(file, documentedMethods);

                // Step 5: Saving the documented file
                var documentedFilePath = methodMerger.SaveMergedFile(file, documentedFile);

                // Step 6: Fixing indentation
                commentIndentationFixer.FixIndentation(documentedFilePath);

                // Step 7: Verify the line counts
                if (!VerifyLineCounts(file, documentedFilePath))
                {
                    Console.WriteLine($"Documentation failed for {file}. Retrying...");
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
