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

    class ClonedProduct { public int Id=-1; public string Name=string.Empty; public Part[] Parts=new Part[0]; }

    [Test]
    public void Clone()
    {
        var entitySet = new EntitySet();
        var cloned = entitySet.Clone<ClonedProduct>().ToList();
        
        foreach (var (item, i) in entitySet.GetClr().WithIndex())
        {
            var a = item;
            var b = cloned[i];
            Assert.That(a.Id, Is.EqualTo(b.Id));
            Assert.That(a.Name, Is.EqualTo(b.Name));
            Assert.That(a.Parts.Length, Is.EqualTo(b.Parts.Length));
            foreach (var (partA, idx) in a.Parts.WithIndex())
            {
                Assert.That(partA.Name, Is.EqualTo(b.Parts[idx].Name));
            }
        }
    }

    class BadClonedProductMissingField { /*public int Id=-1;*/ public string Name=string.Empty; public Part[] Parts=new Part[0]; }
    [Test]
    public void CannotCloneBecauseOfMissingField()
    {
        var entitySet = new EntitySet();
        Assert.Throws(typeof(Exception), () => entitySet.Clone<BadClonedProductMissingField>().ToArray());
    }

    class BadClonedProductMistmatch { public string Id=string.Empty; public string Name=string.Empty; public Part[] Parts=new Part[0]; }
    [Test]
    public void CannotCloneBecauseOfMismatchingTypes()
    {
        var entitySet = new EntitySet();
        Assert.Throws(typeof(Exception), () => entitySet.Clone<BadClonedProductMistmatch>().ToArray());
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