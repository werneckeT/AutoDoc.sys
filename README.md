# AutoDoc

AutoDoc is a .NET application designed to automate the process of documenting your codebase. It scans your project, extracts methods, generates documentation for each method, and merges the documentation back into your codebase.

## Components

### 1. AutoDocConfig
This class is responsible for loading and validating the configuration for the AutoDoc application. The configuration includes the project path, excluded projects, file types, and documentation length.

### 2. IDirectoryScanner and DirectoryScanner
These components are responsible for scanning the directories of your project and identifying the files that need to be processed.

### 3. IMethodExtractor and MethodExtractor
These components are responsible for extracting methods from the identified files.

### 4. ILlmClient and LlmClient
These components are responsible for generating documentation for each extracted method.

### 5. IMethodMerger and MethodMerger
These components are responsible for merging the generated documentation back into your codebase.

### 6. ICommentIndentationFixer and CommentIndentationFixer
These components are responsible for fixing the indentation of the comments in your codebase to ensure consistency.

### 7. AutoDocProcessor
This is the main component that orchestrates the entire process. It uses all the other components to scan the directories, extract methods, generate documentation, and merge the documentation back into your codebase.

## Usage

To use AutoDoc, you need to provide a configuration file. If no configuration file is provided, the application will look for a file named `defaultConfig.json`.

Here is an example of how to run AutoDoc:

```sh
dotnet run --project ./path-to-your-project AutoDoc --config ./path-to-your-config.json
```
If the process completes successfully, you will see the message "Project documentation completed successfully." If there are any errors during the process, they will be printed to the console.

# Example Configuration
```json
{
  "ProjectPath": "C:\\Users\\USER\\source\\repos\\REPO1",
  "ExcludedProjects": [
    "bin",
    "obj",
    "Migrations",
    "ApiClients"
  ],
  "FileTypes": [
    ".cs"
  ],
  "DocLength": 2,
  "MaxTokensPerChunk": 1000
}
```
