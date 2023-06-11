using System.Linq.Dynamic.Core;
using System.Reflection;
using OpenDataService.DataSources.Excel;
using OpenDataService.DataSources.Dynamic;
namespace OpenDataService.Tests.DataSources.Excel;

public class TestExcelDataSource
{
    private byte[] excelFile = new byte[0];

    [SetUp]
    public void SetUp()
    {
        var assembly = GetType().Assembly;
        using (var excelFileStream = assembly.GetManifestResourceStream("OpenDataService.Tests.data.excelfile.xlsx")!)
        {
            using (var reader = new BinaryReader(excelFileStream))
            {
                excelFile = reader.ReadBytes((int)excelFileStream.Length);
            }
        }
    }

    [Test]
    public void HasSheets()
    {
        ExcelDataSource dataSource = new ExcelDataSource(new MemoryStream(excelFile));
        Assert.That("Tab1" == dataSource.Sheets.ElementAt(0).Name);
        Assert.That("Set2" == dataSource.Sheets.ElementAt(1).Name);
    }

    [Test]
    public void HasColumnDefinitions()
    {
        ExcelDataSource dataSource = new ExcelDataSource(new MemoryStream(excelFile));
        Assert.That(dataSource.Sheets.ElementAt(0).Columns.Select(c => c.Name).ToArray(), Is.EqualTo(new [] { "Id", "Name" }));
        Assert.That(dataSource.Sheets.ElementAt(1).Columns.Select(c => c.Name).ToArray(), Is.EqualTo(new [] { "Col1", "Name", "Integers", "Floats", "Floats scientific" }));
    }

    [Test]
    public void HasColumnDefinitionTypes()
    {
        ExcelDataSource dataSource = new ExcelDataSource(new MemoryStream(excelFile));
        Assert.That(dataSource.Sheets.ElementAt(0).Columns.Select(c => c.Type).ToArray(), Is.EqualTo(new [] { typeof(int), typeof(string) }));
        Assert.That(dataSource.Sheets.ElementAt(1).Columns.Select(c => c.Type).ToArray(), Is.EqualTo(new [] { typeof(string), typeof(string), typeof(int), typeof(float), typeof(float) }));
    }

    [Test]
    public void HasEntitySets()
    {
        ExcelDataSource dataSource = new ExcelDataSource(new MemoryStream(excelFile));
        Assert.That(dataSource.GetEntitySet("Tab1").Name, Is.EqualTo("Tab1"));
        Assert.That(dataSource.GetEntitySet("Set2").Name, Is.EqualTo("Set2"));
    }

    [Test]
    public void HasEntitySetTypes()
    {
        ExcelDataSource dataSource = new ExcelDataSource(new MemoryStream(excelFile));
        Assert.That(dataSource.GetEntitySet("Tab1").ClrType.Name, Is.EqualTo("Tab1"));
        Assert.That(dataSource.GetEntitySet("Set2").ClrType.Name, Is.EqualTo("Set2"));
    }

    class Tab1Record { public int Id {get;set;} public string Name {get;set;} = string.Empty; }
    [Test]
    public void HasEntitySetData()
    {
        ExcelDataSource dataSource = new ExcelDataSource(new MemoryStream(excelFile));
        Assert.That(dataSource.GetEntitySet("Tab1").Get().Count(), Is.EqualTo(4));
        Assert.That(dataSource.GetEntitySet("Set2").Get().Count(), Is.EqualTo(3));

        var tab1Data = dataSource.GetEntitySet("Tab1").Clone<Tab1Record>().ToList();

        Assert.That(tab1Data[0].Id, Is.EqualTo(1));
        Assert.That(tab1Data[1].Id, Is.EqualTo(2));
        Assert.That(tab1Data[2].Id, Is.EqualTo(3));
        Assert.That(tab1Data[3].Id, Is.EqualTo(4));
    }
}