using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using OpenDataService.Web.Extensions;
using OpenDataService.DataSources;
using Microsoft.AspNetCore.OData.Query;
using System.Reflection.Emit;
using System.Reflection;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Formatter;
using System.Text;
using Microsoft.AspNetCore.Mvc.Formatters;
using System.Linq.Expressions;

namespace OpenDataService.Web.Controllers
{
    public class HandleAllController : ODataController
    {
        private IDataSourceProvider _provider;
        public HandleAllController(IDataSourceProvider provider)
        {
            var ab = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("x"), AssemblyBuilderAccess.Run);
            var mb = ab.DefineDynamicModule(ab.GetName().ToString());
            var tb = mb.DefineType("sometype", TypeAttributes.Public);
            var type = tb.CreateType();
            _provider = provider;
        }

        // Get entityset
        // odata/{datasource}/{entityset}
        public IActionResult Get()
        {
            var items = Query(Request);
            var collection = ClrToEdmEntities(Request, items);
            return Ok(collection);
        }

        // Get entityset(key) odata/{datasource}/{entityset}({key})
        public IActionResult GetByKey()
        {
            TryGetKeyValue(Request, out var keyValue);
            TryGetEntitysetName(Request, out var entitysetName);
            var items = Request.HttpContext.GetDataSource().GetEntitySet(entitysetName).Get();
            var filteredItems = items.Filter(keyValue.Value.Key, ExpressionType.Equal, keyValue.Value.Value);
            var item = filteredItems.Cast<object>().SingleOrDefault();

            if (item == null)
            {
                return NotFound();
            }

            var entity = ClrToEdmEntity(Request, item);
            return Ok(entity);
        }

        private EdmEntityObjectCollection ClrToEdmEntities(HttpRequest request, IQueryable items)
        {
            var path = request.ODataFeature().Path;
            var collectionType = (IEdmCollectionType)path.Last().EdmType;
            var collection = new EdmEntityObjectCollection(new EdmCollectionTypeReference(collectionType));
            var edmEntityTypeReference = collectionType.ElementType.AsEntity();

            var serializerContext = new Microsoft.AspNetCore.OData.Formatter.Serialization.ODataSerializerContext() { Request = Request, TimeZone = Request.GetTimeZoneInfo() };
            foreach (var r in items)
            {
                var resourceContext = new ResourceContext(serializerContext, edmEntityTypeReference, r);
                collection.Add(resourceContext.EdmObject as IEdmEntityObject);
            }

            return collection;
        }

        private IEdmEntityObject ClrToEdmEntity(HttpRequest request, object item)
        {
            var path = request.ODataFeature().Path;
            var entityType = (IEdmEntityType)path.Last().EdmType;
            

            var serializerContext = new Microsoft.AspNetCore.OData.Formatter.Serialization.ODataSerializerContext() { Request = Request, TimeZone = Request.GetTimeZoneInfo() };
            var resourceContext = new ResourceContext(serializerContext, new EdmEntityTypeReference(entityType, false), item);

            return (IEdmEntityObject)resourceContext.EdmObject;
        }

        private IQueryable Query(HttpRequest request)
        {
            var model = request.GetModel();
            var ds = request.HttpContext.GetDataSource();
            var path = request.ODataFeature().Path;
            
            if (!TryGetEntitysetName(request, out var entitysetName))
            {

            }
            
            var entitySet = ds.GetEntitySet(entitysetName);
            var items = entitySet.Get();
            var queryContext = new ODataQueryContext(model, entitySet.ClrType, path);
            var queryOptions = new ODataQueryOptions(queryContext, request);

            return queryOptions.ApplyTo(items);
        }

        private bool TryGetEntitysetName(HttpRequest request, out string entitysetName)
        {
            var path = request.ODataFeature().Path;
            var segment = path.FirstSegment as EntitySetSegment;
            if (segment == null)
            {
                entitysetName = string.Empty;
                return false;
            }
            entitysetName = segment.EntitySet.Name;
            return true;
        }

        private bool TryGetKeyValue(HttpRequest request, out KeyValuePair<string, object>? keyValue)
        {
            var path = request.ODataFeature().Path;
            var segment = path.SingleOrDefault(segment => segment is KeySegment);

            if (segment == null)
            {
                keyValue = null;
                return false;
            }
            keyValue = ((KeySegment)segment).Keys.First();
            return true;
        }

        // odata/{datasource}/{entityset}({key})/{navigation}
        public IActionResult GetNavigation(string datasource, string key, string navigation)
        {
            ODataPath path = Request.ODataFeature().Path;

            NavigationPropertySegment? property = path.Last() as NavigationPropertySegment;
            if (property == null)
            {
                return BadRequest("Not the correct navigation property access request!");
            }

            IEdmEntityType? entityType = property.NavigationProperty.DeclaringType as IEdmEntityType;
            //Set the SelectExpandClause on OdataFeature to include navigation property set in the $expand
            //SetSelectExpandClauseOnODataFeature(path, entityType);

            EdmEntityObject entity = new EdmEntityObject(entityType);
            return NotFound();
            // IDataSource ds = _provider.DataSources[datasource];

            // ds.Get(key, entity);

            // object value = ds.GetProperty(navigation, entity);

            // if (value == null)
            // {
            //     return NotFound();
            // }

            // IEdmEntityObject? nav = value as IEdmEntityObject;
            // if (nav == null)
            // {
            //     return NotFound();
            // }

            // return Ok(nav);
        }
    }
}