using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace OpenDataService.Web.Routing;

public class EntitySetWithKeyTemplateSegment : ODataSegmentTemplate
{
    public override IEnumerable<string> GetTemplates(ODataRouteOptions options)
    {
        yield return "({key})";
    }

    public override bool TryTranslate(ODataTemplateTranslateContext context)
    {
        if (!context.RouteValues.TryGetValue("entityset", out object? entitysetNameObj) || !(entitysetNameObj is string))
        {
            return false;
        }

        if (!context.RouteValues.TryGetValue("key", out object? keyObj) || !(keyObj is string))
        {
            return false;
        }

        string entitySetName = (string)entitysetNameObj;
        string keyValue = (string)keyObj;
        var edmEntitySet = context.Model.EntityContainer.EntitySets()
            .FirstOrDefault(e => string.Equals(entitySetName, e.Name, StringComparison.OrdinalIgnoreCase));

        if (edmEntitySet != null)
        {
            EntitySetSegment entitySet = new EntitySetSegment(edmEntitySet);
            IEdmEntityType entityType = entitySet.EntitySet.EntityType();

            IEdmProperty keyProperty = entityType.Key().First();

            object newValue = ODataUriUtils.ConvertFromUriLiteral(keyValue, ODataVersion.V4, context.Model, keyProperty.Type);

            // for non FromODataUri, so update it, for example, remove the single quote for string value.
            context.UpdatedValues["key"] = newValue;

            // For FromODataUri, let's refactor it later.
            string prefixName = ODataParameterValue.ParameterValuePrefix + "key";
            context.UpdatedValues[prefixName] = new ODataParameterValue(newValue, keyProperty.Type);

            IDictionary<string, object> keysValues = new Dictionary<string, object>();
            keysValues[keyProperty.Name] = newValue;

            KeySegment keySegment = new KeySegment(keysValues, entityType, entitySet.EntitySet);

            context.Segments.Add(keySegment);

            return true;
        }

        return false;
    }
}