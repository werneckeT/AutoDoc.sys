using AutoDoc.Config;
using RestSharp;
using System.Text.Json;

namespace AutoDoc.LLM
{
    public class LlmClient : ILlmClient
    {
        private readonly RestClient _client = new(LlmBaseUrl);
        private const string LlmBaseUrl = "http://localhost:4456/";
        private string? _modelIdentifier;

        public async Task<List<string?>> GetAvailableModelsAsync()
        {
            var request = new RestRequest("/v1/models");
            var response = await _client.ExecuteAsync(request);

            if (!response.IsSuccessful)
            {
                throw new Exception($"Error fetching models: {response.ErrorMessage}");
            }

            using var document = JsonDocument.Parse(response.Content ?? throw new InvalidOperationException("Invalid Llm response"));

            return document.RootElement.GetProperty("data").EnumerateArray()
                .Select(element => element.GetProperty("id").GetString())
                .Where(modelId => !string.IsNullOrEmpty(modelId)).ToList();
        }

        public async Task<string> GetDocumentationAsync(DocLengthEnum commentLength, string method)
        {
            return await GetDocumentationAsync(commentLength, method, 3);
        }

        private async Task<string> GetDocumentationAsync(DocLengthEnum commentLength, string method, int retry)
        {
            if (retry < 0)
            {
                throw new InvalidOperationException("LLM response validation failed.");
            }

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

            var prompt = GeneratePrompt(commentLength, method);

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
                        content = @"You are Autodoc.sys, a System specialized on Inline-Documentation of existing Source Code. 
                                    You will only respond with the documentation comment for the provided method. DO NOT INCLUDE ANY CODE. 
                                    DO NOT ADD OR REMOVE ANY SYNTAX. ONLY RETURN THE DOCUMENTATION COMMENT. SPECIALIZE THE COMMENTS ON THE ENVIRONMENT."
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

            var documentationComment = ExtractContentFromResponse(response.Content ?? throw new InvalidOperationException("Invalid Llm response"));

            if (!ValidateResponse(documentationComment))
            {
                Console.WriteLine("[ERROR] LLM response validation failed.\nRetry...");
                return await GetDocumentationAsync(commentLength, method, retry - 1);
            }

            return documentationComment;
        }

        private static string GeneratePrompt(DocLengthEnum commentLength, string method)
        {
            var commentLengthStr = commentLength.ToString().ToLower();
            return $"XML Comment length: {commentLengthStr}\nMethod:\n{method}";
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
            // Simple validation to ensure the response contains a comment
            return response.Trim().StartsWith("///");
        }
    }
}
