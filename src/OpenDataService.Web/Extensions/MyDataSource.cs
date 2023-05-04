
using System.Collections;
using System.Linq;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using OpenDataService.DataSources;

namespace OpenDataService.Web.Extensions
{
    public class Part
    {
        public string Name { get; set; } = string.Empty;
    }
    public class Product
    {
        public int Id { get; set;}
        public string Name { get; set;} = string.Empty;
        public Part[] Parts { get; set; } = new Part[0];
    }

    internal class EntitySet : IEntitySet, IEntitySet<Product>
    {
        public EntitySet(IEdmCollectionType edmType)
        {
            EdmType = edmType;
        }
        public string Name => "Products";

        public IEdmCollectionType EdmType { get; set; }

        public Type ClrType => typeof(Product);

        public IQueryable Get()
        {
            return GetClr();
        }

        public IQueryable<Product> GetClr()
        {
            return new[] {
                new Product { Id=1, Name="X", Parts = new [] {new Part {Name = "p1"}}},
                new Product { Id=2, Name="Y"}
            }.AsQueryable();
        }
    }

    internal class MyDataSource : IDataSource
    {
        private List<IEntitySet> entitySets = new List<IEntitySet>();
        public MyDataSource()
        {
            var modelBuilder = new ODataConventionModelBuilder();
            modelBuilder.EntitySet<Product>("Products");
            Model = modelBuilder.GetEdmModel();
            entitySets.Add(new EntitySet((IEdmCollectionType)Model.FindDeclaredEntitySet("Products").Type));
        }
        public IEdmModel Model { get; }

        public IEntitySet GetEntitySet(string name)
        {
            return entitySets.Single(s => s.Name == name);
        }

        public IEnumerator<IEntitySet> GetEnumerator()
        {
            return entitySets.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return entitySets.GetEnumerator();
        }
    }
}
