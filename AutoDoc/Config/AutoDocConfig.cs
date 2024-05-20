using System.Text.Json;

namespace AutoDoc.Config
{
    public class AutoDocConfig
    {
        /// <summary>
        /// The path to the project to document.
        /// </summary>
        public required string ProjectPath { get; set; }

        /// <summary>
        /// Paths to exclude from the documentation.
        /// </summary>
        public List<string>? ExcludedProjects { get; set; }

        /// <summary>
        /// The file types to include in the documentation.
        /// </summary>
        public List<string>? FileTypes { get; set; }

        /// <summary>
        /// Length of the generated comments.
        /// </summary>
        public DocLengthEnum DocLength { get; set; }

        /// <summary>
        /// Maximum number of tokens per chunk.
        /// </summary>
        public int MaxTokensPerChunk { get; set; }

        /// <summary>
        /// Loads the configuration from a file or other source.
        /// </summary>
        /// <param name="path">The path to the configuration file.</param>
        /// <returns>An instance of AutoDocConfig with the loaded settings.</returns>
        /// <exception cref="FileNotFoundException">Thrown when the configuration file is not found.</exception>
        /// <exception cref="InvalidDataException">Thrown when the configuration data is invalid.</exception>
        public static AutoDocConfig Load(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("Config file not found", path);

            var json = File.ReadAllText(path);
            var config = JsonSerializer.Deserialize<AutoDocConfig>(json) ?? throw new InvalidDataException("Config file is empty.");
            ValidateConfig(config);

            return config;
        }

        /// <summary>
        /// Validates the configuration data.
        /// </summary>
        /// <param name="config">The configuration to validate.</param>
        /// <exception cref="InvalidDataException">Thrown when the configuration data is invalid.</exception>
        private static void ValidateConfig(AutoDocConfig config)
        {
            if (string.IsNullOrEmpty(config.ProjectPath))
            {
                throw new InvalidDataException("ProjectPath is required.");
            }

            if (config.FileTypes == null || config.FileTypes.Count == 0)
            {
                throw new InvalidDataException("At least one FileType is required.");
            }

            if (!Enum.IsDefined(typeof(DocLengthEnum), config.DocLength))
            {
                throw new InvalidDataException("Invalid DocLength value.");
            }

            if (config.MaxTokensPerChunk <= 0)
            {
                throw new InvalidDataException("MaxTokensPerChunk must be greater than zero.");
            }
        }

        /// <summary>
        /// Converts the maximum token count to an approximate character count.
        /// </summary>
        /// <returns>The approximate character count for the max token count.</returns>
        public int GetMaxCharsPerChunk()
        {
            const double avgCharsPerToken = 4.0;
            return (int)(MaxTokensPerChunk * avgCharsPerToken);
        }
    }
}
