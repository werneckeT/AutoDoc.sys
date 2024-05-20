namespace AutoDoc.CommentIndentationFixer
{
    public class CommentIndentationFixer : ICommentIndentationFixer
    {
        public void FixIndentation(string filePath)
        {
            var lines = File.ReadAllLines(filePath).ToList();
            var fixedLines = new List<string>();
            var commentLines = new List<string>();
            var isCommentBlock = false;
            var indentation = "";

            for (var i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                if (line.Trim().StartsWith("///"))
                {
                    if (!isCommentBlock)
                    {
                        isCommentBlock = true;
                        indentation = line[..line.IndexOf("///", StringComparison.Ordinal)];
                    }
                    commentLines.Add(line.Trim());
                }
                else
                {
                    if (isCommentBlock)
                    {
                        isCommentBlock = false;
                        fixedLines.AddRange(commentLines.Select(commentLine => indentation + commentLine));
                        commentLines.Clear();
                    }

                    // Check if the current line is an attribute and the next non-attribute line is a comment
                    if (line.Trim().StartsWith("["))
                    {
                        var j = i + 1;
                        // Skip all consecutive attribute lines
                        while (j < lines.Count && lines[j].Trim().StartsWith("["))
                        {
                            j++;
                        }
                        // If the next non-attribute line is a comment, skip the current line
                        if (j < lines.Count && lines[j].Trim().StartsWith("///"))
                        {
                            continue;
                        }
                    }
                    fixedLines.Add(line);
                }
            }

            // Add any remaining comment lines
            if (commentLines.Count != 0)
            {
                fixedLines.AddRange(commentLines.Select(commentLine => indentation + commentLine));
            }

            File.WriteAllLines(filePath, fixedLines);
        }
    }
}