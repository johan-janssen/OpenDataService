using System.Reflection;
using OpenDataService.DataSources.Excel;
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
}