using System;

namespace OpenDataService.DataSources.Glob
{
    public class GlobPatternException : Exception
    {
        internal GlobPatternException(string message)
            : base(message)
        {
        }
    }
}
