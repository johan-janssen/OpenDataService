using System.Reflection;
using System.Linq;
using OpenDataService.DataSources;
using OpenDataService.DataSources.Dynamic;
using OpenDataService.DataSources.Extensions;
namespace OpenDataService.Tests.DataSources.Dynamic;

public class TestClone
{
    [SetUp]
    public void SetUp()
    {

    }

    class ClonedProduct { public int Id; public string Name; public Part[] Parts; }

    [Test]
    public void Clone()
    {
        var entitySet = new EntitySet();
        var cloned = entitySet.Clone<ClonedProduct>().ToArray();
        
        foreach (var (item, i) in entitySet.GetClr().WithIndex())
        {
            var a = item;
            var b = cloned[i];
            Assert.That(a.Id, Is.EqualTo(b.Id));
        }
    }
}

class Part
{
    public string Name { get; set; } = string.Empty;
}
class Product
{
    public int Id { get; set;}
    public string Name { get; set;} = string.Empty;
    public Part[] Parts { get; set; } = new Part[0];
}

class EntitySet : IEntitySet, IEntitySet<Product>
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

public class ClonedProduct
{

}