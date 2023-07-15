using System.Collections.Generic;
using System.Linq;

namespace OpenDataService.DataSources.Glob.AST
{
    internal sealed class Tree : GlobNode
    {
        public Segment[] Segments { get; }

        public Tree(IEnumerable<Segment> segments)
            : base(GlobNodeType.Tree)
        {
            Segments = segments.ToArray();
        }
    }
}
