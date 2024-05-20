using AutoDoc.Chunker;
using AutoDoc.Config;
using AutoDoc.Core;
using AutoDoc.LLM;
using AutoDoc.Merger;
using AutoDoc.Scanner;
using System.Text.Json;
//static void CreateTestConfig(string configPath, string projectPath)
//{
//    var config = new AutoDocConfig
//    {
//        ProjectPath = projectPath,
//        ExcludedProjects = new List<string> { "bin", "obj" },
//        FileTypes = new List<string> { ".cs" },
//        DocLength = DocLengthEnum.Medium,
//        MaxTokensPerChunk = 500
//    };

//    var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
//    File.WriteAllText(configPath, json);
//}
try
{
    //CreateTestConfig("defaultConfig.json", @"./bin/debug/net8.0");
    var config = AutoDocConfig.Load("defaultConfig.json");

    IDirectoryScanner scanner = new DirectoryScanner();
    IFileChunker chunker = new FileChunker();
    ILlmClient llmClient = new LlmClient();
    IChunkMerger merger = new ChunkMerger();

    var processor = new AutoDocProcessor(scanner, chunker, llmClient, merger, config);
    await processor.ProcessAsync();

    Console.WriteLine("Project documentation completed successfully.");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}