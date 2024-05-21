using System.Text;
using System.Text.RegularExpressions;

namespace AutoDoc.MethodMerger
{
    public class MethodMerger : IMethodMerger
    {
        public string MergeMethods(string filePath, List<(string comment, string method, string indentation)> documentedMethods)
        {
            if (documentedMethods == null || documentedMethods.Count == 0)
                throw new ArgumentException("Documented methods list cannot be null or empty.");

            var lines = File.ReadAllLines(filePath);
            var documentedContent = new StringBuilder();
            var methodIndex = 0;

            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                if (methodIndex < documentedMethods.Count && IsMethodDeclaration(line))
                {
                    var (comment, _, indentation) = documentedMethods[methodIndex];
                    var indentedComment = IndentComment(comment, indentation);

                    // Find the start of the attributes
                    var attributeStartIndex = i;
                    while (attributeStartIndex > 0 && IsAttribute(lines[attributeStartIndex - 1]))
                    {
                        attributeStartIndex--;
                    }

                    // Insert the comment above the attributes
                    documentedContent.AppendLine(indentedComment);
                    for (var j = attributeStartIndex; j <= i; j++)
                    {
                        documentedContent.AppendLine(lines[j]);
                    }

                    methodIndex++;
                }
                else
                {
                    documentedContent.AppendLine(line);
                }
            }

            return documentedContent.ToString();
        }

        private bool IsMethodDeclaration(string line)
        {
            return Regex.IsMatch(line.Trim(), @"^(public|private|protected|internal)\s.*\s*\(.*\)\s*\{?$");
        }

        private bool IsAttribute(string line)
        {
            return line.Trim().StartsWith("[");
        }

        private string IndentComment(string comment, string indentation)
        {
            var indentedComment = new StringBuilder();
            foreach (var line in comment.Split(new[] { Environment.NewLine }, StringSplitOptions.None))
            {
                indentedComment.AppendLine(indentation + line.Trim());
            }
            return indentedComment.ToString().TrimEnd();
        }

        public string SaveMergedFile(string originalFilePath, string mergedContent)
        {
            if (string.IsNullOrEmpty(originalFilePath))
                throw new ArgumentException("Original file path cannot be null or empty.");

            if (string.IsNullOrEmpty(mergedContent))
                throw new ArgumentException("Merged content cannot be null or empty.");

            // Determine the directory and filename for the new file
            var directory = Path.GetDirectoryName(originalFilePath);
            var fileName = SanitizeFileName(Path.GetFileNameWithoutExtension(originalFilePath) /*+ "_documented"*/ + Path.GetExtension(originalFilePath));
            var newFilePath = Path.Combine(directory ?? throw new InvalidOperationException("Directory not found to combine to new path"), fileName);

            File.WriteAllText(newFilePath, mergedContent);
            return newFilePath;
        }

        private string SanitizeFileName(string fileName)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '_');
            }
            return fileName;
        }
    }
}
