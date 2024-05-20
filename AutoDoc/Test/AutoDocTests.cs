using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using AutoDoc.Config;
using AutoDoc.Chunker;
using AutoDoc.Core;
using AutoDoc.LLM;
using AutoDoc.Merger;
using AutoDoc.Scanner;

namespace AutoDoc.Test
{
    public class AutoDocTests
    {
        private readonly AutoDocConfig _config;
        private readonly AutoDocProcessor _processor;

        public AutoDocTests()
        {
            _config = new AutoDocConfig
            {
                ProjectPath = "testProject",
                ExcludedProjects = new List<string> { "bin", "obj" },
                FileTypes = new List<string> { ".cs" },
                DocLength = DocLengthEnum.Medium,
                MaxTokensPerChunk = 500
            };

            IDirectoryScanner scanner = new DirectoryScanner();
            IFileChunker chunker = new FileChunker();
            ILlmClient llmClient = new LlmClient();
            IChunkMerger merger = new ChunkMerger();
            _processor = new AutoDocProcessor(scanner, chunker, llmClient, merger, _config);

            CreateTestProject(_config.ProjectPath);
        }

        private static void CreateTestProject(string projectPath)
        {
            if (Directory.Exists(projectPath))
            {
                Directory.Delete(projectPath, true);
            }

            Directory.CreateDirectory(projectPath);

            // Simple class
            var simpleCode = @"
using System;

public class SimpleClass
{
    public void SimpleMethod()
    {
        Console.WriteLine(""Simple Method"");
    }
}
";
            File.WriteAllText(Path.Combine(projectPath, "SimpleClass.cs"), simpleCode);

            // Long class
            var longCode = "using System;\n\npublic class LongClass\n{\n";
            for (var i = 0; i < 1000; i++)
            {
                longCode += $"    public void Method{i}() {{ Console.WriteLine(\"Method {i}\"); }}\n";
            }
            longCode += "}\n";
            File.WriteAllText(Path.Combine(projectPath, "LongClass.cs"), longCode);
        }

        [Fact]
        public async Task Test_SimpleClass_Documentation()
        {
            await _processor.ProcessAsync();

            // Validate the documented SimpleClass.cs
            var documentedFilePath = Path.Combine(_config.ProjectPath, "SimpleClass_documented.cs");
            Assert.True(File.Exists(documentedFilePath));
            var documentedContent = await File.ReadAllTextAsync(documentedFilePath);
            Assert.Contains("///", documentedContent);
        }

        [Fact]
        public async Task Test_LongClass_Documentation()
        {
            await _processor.ProcessAsync();

            // Validate the documented LongClass.cs
            var documentedFilePath = Path.Combine(_config.ProjectPath, "LongClass_documented.cs");
            Assert.True(File.Exists(documentedFilePath));
            var documentedContent = await File.ReadAllTextAsync(documentedFilePath);
            Assert.Contains("///", documentedContent);
        }
    }
}
