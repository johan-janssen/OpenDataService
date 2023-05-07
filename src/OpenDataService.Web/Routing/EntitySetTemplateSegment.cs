using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace OpenDataService.Web.Routing;

public class EntitySetTemplateSegment : ODataSegmentTemplate
{
    public override IEnumerable<string> GetTemplates(ODataRouteOptions options)
    {
        yield return "/{entityset}";
    }

    public override bool TryTranslate(ODataTemplateTranslateContext context)
    {
        if (!context.RouteValues.TryGetValueTyped("entityset", out string? entitySetName))
        {
            return false;
        }

        var edmEntitySet = context.Model.EntityContainer.EntitySets()
            .FirstOrDefault(e => string.Equals(entitySetName, e.Name, StringComparison.OrdinalIgnoreCase));

        if (edmEntitySet != null)
        {
            EntitySetSegment segment = new EntitySetSegment(edmEntitySet);
            context.Segments.Add(segment);
            return true;
        }

        return false;
    }
}