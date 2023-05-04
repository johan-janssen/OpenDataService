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
            var edmEntityType = edmEntityTypeReference.EntityDefinition();

            var segment = path.FirstSegment as EntitySetSegment;
            var entitysetName = segment.EntitySet.Name;
            IEdmNavigationSource source = segment?.EntitySet;

            var model = Request.GetModel();

            SetSelectExpandClauseOnODataFeature(path, edmEntityType);

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
        public IEdmEntityObject GetByKey(string datasource, string entityset, string key)
        {
            // Get entity type from path.
            ODataPath path = Request.ODataFeature().Path;
            IEdmEntityType entityType = (IEdmEntityType)path.Last().EdmType;

            //Set the SelectExpandClause on OdataFeature to include navigation property set in the $expand
            SetSelectExpandClauseOnODataFeature(path, entityType);

            // Create an untyped entity object with the entity type.
            EdmEntityObject entity = new EdmEntityObject(entityType);

            IDataSource ds = _provider.DataSources[datasource];
            //ds.Get(key, entity);

            return entity;
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
            SetSelectExpandClauseOnODataFeature(path, entityType);

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

        private IDictionary<string, string> ToDictionary(IQueryCollection collection)
        {
            IDictionary<string, string> options = new Dictionary<string, string>();
            foreach (var k in Request.Query.Keys)
            {
                options.Add(k, collection[k]);
            }
            return options;
        }

        /// <summary>
        /// Set the <see cref="SelectExpandClause"/> on ODataFeature.
        /// Without this, the response does not contains navigation property included in $expand
        /// </summary>
        /// <param name="odataPath">OData Path from the Request</param>
        /// <param name="edmEntityType">Entity type on which the query is being performed</param>
        /// <returns></returns>
        private void SetSelectExpandClauseOnODataFeature(ODataPath odataPath, IEdmType edmEntityType)
        {
            IDictionary<string, string> options = new Dictionary<string, string>();
            foreach (var k in Request.Query.Keys)
            {
                options.Add(k, Request.Query[k]);
            }

            //At this point, we should have valid entity segment and entity type.
            //If there is invalid entity in the query, then OData routing should return 404 error before executing this api
            var segment = odataPath.FirstSegment as EntitySetSegment;
            IEdmNavigationSource source = segment?.EntitySet;
            ODataQueryOptionParser parser = new(Request.GetModel(), edmEntityType, source, options);
            //Set the SelectExpand Clause on the ODataFeature otherwise  Odata formatter won't show the expand and select properties in the response.
            Request.ODataFeature().SelectExpandClause = parser.ParseSelectAndExpand();
        }
    }
}
