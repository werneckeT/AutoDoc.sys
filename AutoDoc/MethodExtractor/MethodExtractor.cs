using System.Text.RegularExpressions;

namespace AutoDoc.MethodExtractor
{
    public class MethodExtractor : IMethodExtractor
    {
        public List<(string method, string indentation)> ExtractMethods(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File not found: {filePath}");
            }

            var lines = File.ReadAllLines(filePath);
            var methods = new List<(string method, string indentation)>();
            var currentMethod = new List<string>();
            var insideMethod = false;
            string? indentation = null;

            foreach (var line in lines)
            {
                var methodMatch = Regex.Match(line, @"^(\s*)(public|private|protected|internal)\s.*\s*\(.*\)\s*\{?$");
                var propertyMatch = Regex.Match(line, @"^(\s*)(public|private|protected|internal)\s.*\s*\{\s*get;\s*set;\s*\}$");

                if (methodMatch.Success)
                {
                    if (insideMethod)
                    {
                        methods.Add((string.Join(Environment.NewLine, currentMethod), indentation!));
                        currentMethod.Clear();
                    }
                    insideMethod = true;
                    indentation = methodMatch.Groups[1].Value;
                }
                else if (propertyMatch.Success)
                {
                    insideMethod = false;
                }

                if (insideMethod)
                {
                    currentMethod.Add(line);
                }

                if (insideMethod && line.Trim() == "}")
                {
                    insideMethod = false;
                    methods.Add((string.Join(Environment.NewLine, currentMethod), indentation!));
                    currentMethod.Clear();
                }
            }

            return methods;
        }
    }
}
