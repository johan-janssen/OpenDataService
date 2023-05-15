using System.Reflection;
using OpenDataService.DataSources.Dynamic;
namespace OpenDataService.Tests.DataSources.Dynamic;

public class TestTypeGenerator
{
    private byte[] excelFile = new byte[0];

    [SetUp]
    public void SetUp()
    {

    }

    [Test]
    public void GenerateType()
    {
        var generator = new TypeGenerator();
        var types = generator.Generate(
            new AssemblyDefinition("Hello.World", new[] {
                new TypeDefinition("Foo", new[] {
                    new PropertyDefinition("Baz", typeof(int))
                })
            })
            );
        Assert.That(types.Count, Is.EqualTo(1));
        var type = types.Values.First();
        Assert.That(type.Namespace, Is.EqualTo("Hello.World"));
        Assert.That(type.Name, Is.EqualTo("Foo"));
        Assert.That(type.GetField("Baz")?.FieldType, Is.EqualTo(typeof(int)));
    }
    
    [Test]
    public void GenerateTypeTwice_SurprisesMeThatThisWorks()
    {
        var generator = new TypeGenerator();
        var firstTime = generator.Generate(
            new AssemblyDefinition("Hello.World", new[] {
                new TypeDefinition("Foo", new[] {
                    new PropertyDefinition("Baz", typeof(int))
                })
            })
            );

        var secondTime = generator.Generate(
            new AssemblyDefinition("Hello.World", new[] {
                new TypeDefinition("Foo", new[] {
                    new PropertyDefinition("Baz2", typeof(int))
                })
            })
            );
        
        var firstType = firstTime.Values.First();
        var secondType = secondTime.Values.First();

        Assert.That(firstType.GetFields().Length, Is.EqualTo(1));
        Assert.That(secondType.GetFields().Length, Is.EqualTo(1));
        Assert.That(firstType.AssemblyQualifiedName, Is.EqualTo(secondType.AssemblyQualifiedName));
        var firstInstance = Activator.CreateInstance(firstType);
        var secondInstance = Activator.CreateInstance(secondType);
        firstType?.GetField("Baz")?.SetValue(firstInstance, 1);
        secondType?.GetField("Baz2")?.SetValue(secondInstance, 1);

        Assert.That(firstType?.GetField("Baz")?.GetValue(firstInstance), Is.EqualTo(secondType?.GetField("Baz2")?.GetValue(secondInstance)));
    }
}