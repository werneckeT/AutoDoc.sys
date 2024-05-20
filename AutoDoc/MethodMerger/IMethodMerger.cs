using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoDoc.MethodMerger
{
    public interface IMethodMerger
    {
        string MergeMethods(string filePath,
            List<(string comment, string method, string indentation)> documentedMethods);
        string SaveMergedFile(string originalFilePath, string mergedContent);
    }
}
