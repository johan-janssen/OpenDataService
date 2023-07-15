using System.Collections.Generic;
using System.Linq;

namespace OpenDataService.DataSources.Glob.AST
{
    internal sealed class DirectorySegment : Segment
    {
        public SubSegment[] SubSegments { get; }

        public DirectorySegment(IEnumerable<SubSegment> subSegments)
            : base(GlobNodeType.DirectorySegment)
        {
            SubSegments = subSegments.ToArray();
        }

        public bool MatchesSegment(string pathSegment, StringComparison stringComparison)
        {
            return MatchesSubSegment(SubSegments, 0, -1, pathSegment, 0, stringComparison);
        }

        private bool MatchesSubSegment(SubSegment[] segments, int segmentIndex, int literalSetIndex, string pathSegment, int pathIndex, StringComparison stringComparison)
        {
            var nextSegment = segmentIndex + 1;
            if (nextSegment > segments.Length)
                return pathIndex == pathSegment.Length;

            var head = segments[segmentIndex];
            if (head is LiteralSet ls)
            {
                if (literalSetIndex == -1)
                {
                    for (int i = 0; i < ls.Literals.Length; i++)
                    {
                        if (MatchesSubSegment(segments, segmentIndex, i, pathSegment, pathIndex, stringComparison))
                            return true;
                    }

                    return false;
                }

                head = ls.Literals[literalSetIndex];
            }

            switch (head)
            {
                // match zero or more chars
                case StringWildcard _:
                    return MatchesSubSegment(segments, nextSegment, -1, pathSegment, pathIndex, stringComparison) // zero
                           || (pathIndex < pathSegment.Length &&
                               MatchesSubSegment(segments, segmentIndex, -1, pathSegment, pathIndex + 1, stringComparison)); // or one+

                case CharacterWildcard _:
                    return pathIndex < pathSegment.Length && MatchesSubSegment(segments, nextSegment, -1, pathSegment, pathIndex + 1, stringComparison);

                case Identifier ident:
                    var len = ident.Value.Length;
                    if (len + pathIndex > pathSegment.Length)
                        return false;

                    if (!SubstringEquals(pathSegment, pathIndex, ident.Value, stringComparison))
                        return false;

                    return MatchesSubSegment(segments, nextSegment, -1, pathSegment, pathIndex + len, stringComparison);

                case CharacterSet set:
                    if (pathIndex == pathSegment.Length)
                        return false;

                    var inThere = set.Matches(pathSegment[pathIndex], stringComparison);
                    return inThere && MatchesSubSegment(segments, nextSegment, -1, pathSegment, pathIndex + 1, stringComparison);

                default:
                    return false;
            }
        }

        private bool SubstringEquals(string segment, int segmentIndex, string search, StringComparison stringComparison) =>
            string.Compare(segment, segmentIndex, search, 0, search.Length, stringComparison) == 0;
    

        public override string ToString()
        {
            return string.Join("", SubSegments.Select(x => x.ToString()));
        }
    }
}
