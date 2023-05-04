
using System.Linq;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using OpenDataService.DataSources;

namespace OpenDataService.Web.Extensions
{
    public class Part
    {
        public string Name { get; set; }
    }
    public class Product
    {
        public int Id { get; set;}
        public string Name { get; set;}
        public Part[] Parts { get; set; } = new Part[0];
    }

    internal class MyDataSource : IDataSource
    {
        public MyDataSource()
        {
            var modelBuilder = new ODataConventionModelBuilder();
            modelBuilder.EntitySet<Product>("Products");
            Model = modelBuilder.GetEdmModel();
        }
        public Type ClrType => typeof(Product);
        public IEdmModel Model { get; }
        public IQueryable Get()
        {
            return new[] {
                new Product { Id=1, Name="X", Parts = new [] {new Part {Name = "p1"}}},
                new Product { Id=2, Name="Y"}
            }.AsQueryable();
        }
    }
}
