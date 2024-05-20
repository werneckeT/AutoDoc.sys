using AutoDoc.Config;
using RestSharp;
using System.Text.Json;

namespace AutoDoc.LLM
{
    public class LlmClient : ILlmClient
    {
        private readonly RestClient _client = new(LlmBaseUrl);
        private const string LlmBaseUrl = "http://localhost:4456/";
        private static readonly char[] Separator = ['\r', '\n'];
        private string? _modelIdentifier;

        public async Task<List<string?>> GetAvailableModelsAsync()
        {
            var request = new RestRequest("/v1/models");
            var response = await _client.ExecuteAsync(request);

            if (!response.IsSuccessful)
            {
                throw new Exception($"Error fetching models: {response.ErrorMessage}");
            }

            using var document =
                JsonDocument.Parse(response.Content ?? throw new InvalidOperationException("Invalid Llm response"));

            return document.RootElement.GetProperty("data").EnumerateArray()
                .Select(element => element.GetProperty("id").GetString())
                .Where(modelId => !string.IsNullOrEmpty(modelId)).ToList();
        }

        public async Task<string> GetDocumentationAsync(DocLengthEnum commentLength, string chunk)
        {
            if (string.IsNullOrEmpty(_modelIdentifier))
            {
                var availableModels = await GetAvailableModelsAsync();
                if (availableModels.Count == 0)
                {
                    throw new Exception("No models available.");
                }

                // For this example, we'll just use the first available model.
                _modelIdentifier = availableModels[0];
            }

            var prompt = GeneratePrompt(commentLength, chunk);

            var request = new RestRequest("v1/chat/completions", Method.Post);
            request.AddHeader("Content-Type", "application/json");
            request.AddJsonBody(new
            {
                model = _modelIdentifier,
                messages = new[]
                {
                    new
                    {
                        role = "system",
                        content =
                            @"  You are Autodoc.sys, a System specialized on Inline-Documentation of existing Source Code. 
                                You will only respond with Code, no additional explanation, message, or anything, as you 
                                are integrated into another system. You will be given a chunk of a source file. Your task 
                                is to document the chunk for the user. You MUST return only the chunk. IT IS FORBIDDEN FOR 
                                YOU TO CHANGE ANY CODE. YOUR ONLY TASK IS TO CREATE ADDITIONAL DOCUMENTATION INLINE. DO NOT ADD OR REMOVE ANY SYNTAX. YOUR ONLY TASK IS TO ADD DOCUMENTATION"
                    },
                    new { role = "user", content = prompt }
                },
                temperature = 0.7,
                max_tokens = -1,
                stream = false
            });

            request.Timeout = 500 * 1000;

            var response = await _client.ExecuteAsync(request);

            if (!response.IsSuccessful)
            {
                throw new Exception($"Error from LLM: {response.ErrorMessage}");
            }

            var documentedChunk =
                ExtractContentFromResponse(response.Content ??
                                           throw new InvalidOperationException("Invalid Llm response"));

            if (!ValidateResponse(documentedChunk))
            {
                throw new InvalidOperationException("LLM response validation failed.");
            }

            return documentedChunk;
        }

        private static string GeneratePrompt(DocLengthEnum commentLength, string chunk)
        {
            var commentLengthStr = commentLength.ToString().ToLower();
            return $"Comment length: {commentLengthStr}\nCode Chunk:\n{chunk}";
        }

        private static string ExtractContentFromResponse(string jsonResponse)
        {
            using var document = JsonDocument.Parse(jsonResponse);
            var content = document.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();
            return content ?? string.Empty;
        }

        private static bool ValidateResponse(string response)
        {
            var lines = response.Split(Separator, StringSplitOptions.RemoveEmptyEntries);

            return lines.All(line =>
                !line.StartsWith("You are") && !line.Contains("ONLY TASK") && !line.Contains("Comment length:"));
        }
    }
}