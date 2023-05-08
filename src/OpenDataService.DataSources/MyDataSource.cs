
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Linq.Dynamic.Core;
using OpenDataService.DataSources;

namespace OpenDataService.DataSources
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

    public class EntitySet : IEntitySet, IEntitySet<Product>
    {
        public string Name => "Products";

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

    public class MyDataSource : IDataSource
    {
        private List<IEntitySet> entitySets = new List<IEntitySet>();
        public MyDataSource()
        {
            entitySets.Add(new EntitySet());
        }

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
