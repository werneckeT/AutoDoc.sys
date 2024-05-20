using AutoDoc.Config;
using AutoDoc.Core;
using AutoDoc.LLM;
using AutoDoc.Scanner;
using System.Text.Json;
using AutoDoc.CommentIndentationFixer;
using AutoDoc.MethodExtractor;
using AutoDoc.MethodMerger;

try
{
    var config = AutoDocConfig.Load("defaultConfig.json");

    IDirectoryScanner scanner = new DirectoryScanner();
    ILlmClient llmClient = new LlmClient();
    IMethodExtractor methodExtractor = new MethodExtractor();
    IMethodMerger merger = new MethodMerger();
    ICommentIndentationFixer commentIndentationFixer = new CommentIndentationFixer();

    var processor = new AutoDocProcessor(scanner, methodExtractor, llmClient, merger, commentIndentationFixer, config);
    await processor.ProcessAsync();

    Console.WriteLine("Project documentation completed successfully.");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}