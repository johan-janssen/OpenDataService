using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace OpenDataService.Web.Routing;

public class NavigationTemplateSegment : ODataSegmentTemplate
{
    public override IEnumerable<string> GetTemplates(ODataRouteOptions options)
    {
        yield return "/{navigation}";
    }

    public override bool TryTranslate(ODataTemplateTranslateContext context)
    {
        if (!context.RouteValues.TryGetValueTyped("navigation", out string? navigationName))
        {
            return false;
        }

        KeySegment keySegment = (KeySegment)context.Segments.Last();
        IEdmEntityType entityType = (IEdmEntityType)keySegment.EdmType;

        IEdmNavigationProperty? navigationProperty = entityType.NavigationProperties().FirstOrDefault(n => n.Name == navigationName);
        if (navigationProperty != null)
        {
            var navigationSource = keySegment.NavigationSource;
            IEdmNavigationSource targetNavigationSource = navigationSource.FindNavigationTarget(navigationProperty);

            NavigationPropertySegment seg = new NavigationPropertySegment(navigationProperty, targetNavigationSource);
            context.Segments.Add(seg);
            return true;
        }

        return false;
    }
}