using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using OpenDataService.DataSources.Glob.AST;

namespace OpenDataService.DataSources.Glob
{
    public partial class Glob
    {
        public string Pattern { get; }

        private Tree? _root;
        private Segment[]? _segments;
        StringComparison stringComparison;

        public Glob(string pattern, StringComparison stringComparison=StringComparison.Ordinal)
        {
            this.stringComparison = stringComparison;
            this.Pattern = pattern;
            var parser = new Parser(this.Pattern);
            _root = parser.ParseTree();
            _segments = _root.Segments;
        }

        public bool IsMatch(string input)
        {
            var pathSegments = input.Split('/', '\\');
            return GlobEvaluator.Eval(_segments!, 0, pathSegments, 0, stringComparison);
        }
    }
}
