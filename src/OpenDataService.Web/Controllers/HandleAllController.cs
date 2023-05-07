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
        public EdmEntityObjectCollection Get()
        {
            ODataPath path = Request.ODataFeature().Path;
            IEdmCollectionType collectionType = (IEdmCollectionType)path.Last().EdmType;
            IEdmEntityTypeReference edmEntityTypeReference = collectionType.ElementType.AsEntity();

            var segment = path.FirstSegment as EntitySetSegment;
            var entitysetName = segment.EntitySet.Name;

            var model = Request.GetModel();

            IDataSource ds = Request.HttpContext.GetDataSource();
            var entitySet = ds.GetEntitySet(entitysetName);
            var collection = new EdmEntityObjectCollection(new EdmCollectionTypeReference(collectionType));
            var items = entitySet.Get();
            var queryContext = new ODataQueryContext(model, entitySet.ClrType, path);
            var queryOptions = new ODataQueryOptions(queryContext, Request);

            items = queryOptions.ApplyTo(items);
            var serializerContext = new Microsoft.AspNetCore.OData.Formatter.Serialization.ODataSerializerContext() { Request = Request, TimeZone = Request.GetTimeZoneInfo() };
            foreach (var r in items)
            {
                var resourceContext = new ResourceContext(serializerContext, edmEntityTypeReference, r);
                collection.Add(resourceContext.EdmObject as IEdmEntityObject);
            }
            return collection;
        }

        // Get entityset(key) odata/{datasource}/{entityset}({key})
        public IActionResult GetByKey(string datasource, string entityset, string key)
        {
            // Get entity type from path.
            ODataPath path = Request.ODataFeature().Path;
            IEdmEntityType entityType = (IEdmEntityType)path.Last().EdmType;

            //Set the SelectExpandClause on OdataFeature to include navigation property set in the $expand
            //SetSelectExpandClauseOnODataFeature(path, entityType);

            // Create an untyped entity object with the entity type.
            EdmEntityObject entity = new EdmEntityObject(entityType);

            IDataSource ds = _provider.DataSources[datasource];
            //ds.Get(key, entity);

            return Ok(entity);
            //return entity;
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