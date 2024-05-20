using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoDoc.MethodExtractor
{
    public interface IMethodExtractor
    {
        List<(string method, string indentation)> ExtractMethods(string filePath);
    }
}
