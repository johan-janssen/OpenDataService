namespace OpenDataService.DataSources.Glob.AST
{
    internal abstract class Segment : GlobNode
    {
        protected Segment(GlobNodeType type)
            : base(type)
        {
        }
    }
}
