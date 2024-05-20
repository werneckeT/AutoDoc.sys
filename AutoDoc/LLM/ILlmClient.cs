using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoDoc.Config;

namespace AutoDoc.LLM
{
    public interface ILlmClient
    {
        /// <summary>
        /// Fetches the available models from the LLM server.
        /// </summary>
        /// <returns>A list of available model IDs.</returns>
        Task<List<string?>> GetAvailableModelsAsync();

        /// <summary>
        /// Sends a code chunk to the LLM for documentation and returns the documented chunk.
        /// </summary>
        /// <param name="commentLength">The length of the comments (small, medium, large).</param>
        /// <param name="chunk">The code chunk to be documented.</param>
        /// <returns>The documented code chunk.</returns>
        Task<string> GetDocumentationAsync(DocLengthEnum commentLength, string chunk);
    }
}
