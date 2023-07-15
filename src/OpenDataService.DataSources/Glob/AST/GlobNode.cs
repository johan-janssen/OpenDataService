namespace OpenDataService.DataSources.Glob.AST
{
    internal abstract class GlobNode
    {
        protected GlobNode(GlobNodeType type)
        {
            this.Type = type;
        }

        public GlobNodeType Type { get; }
    }
}
