using System.IO;
using OpenDataService.DataSources;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace OpenDataService.DataSources.Excel;

public class ColumnDefinition
{
    public ColumnDefinition(int index, string name, Type type)
    {
        Index = index;
        Name = name;
        Type = type;
    }

    public int Index { get; }
    public string Name { get; }
    public Type Type { get; }
}

public class Sheet
{
    public Sheet(string name, IEnumerable<ColumnDefinition> columns)
    {
        Name = name;
        Columns = columns;
    }

    public string Name { get; }
    public IEnumerable<ColumnDefinition> Columns { get; }
}

public class ExcelDataSource
{
    public ExcelDataSource(Stream stream)
    {
        Sheets = Read(stream);
    }
    public IEnumerable<Sheet> Sheets { get; private set;}
    private IEnumerable<Sheet> Read(Stream stream)
    {
        var sheets = new List<Sheet>();

        using (var doc = SpreadsheetDocument.Open(stream, false))
        {
            var workbook = doc.WorkbookPart;

            if (workbook == null)
            {
                throw new Exception();
            }

            foreach (var sheetPart in workbook.WorksheetParts)
            {
                string name = GetSheet(workbook, sheetPart).Name!.Value!;
                SheetData sheetData = sheetPart.Worksheet.Elements<SheetData>().First();

                Row columnRow = sheetData.Elements<Row>().First();
                IEnumerable<Row> dataRows = sheetData.Elements<Row>().Skip(1);
                var dataRaw = dataRows.Select(row => row.Elements<Cell>().Select(cell => GetCellValue(cell, workbook)).ToArray()).ToArray();
                sheets.Add(new Sheet(name, GetColumns(columnRow, workbook, dataRaw)));
                var worksheet = sheetPart.Worksheet;
                Console.WriteLine(sheetData.ToString());
            }
        }

        return sheets;
    }

    private List<ColumnDefinition> GetColumns(Row row, WorkbookPart workbook, object[][] datarows)
    {
        var columns = new List<ColumnDefinition>();

        foreach (var item in row.Elements<Cell>().Select((cell, i) => (cell, i)))
        {
            var type = FindLeastComplexTypeInColumn(datarows, item.i);
            string value = GetCellValue(item.cell, workbook).ToString() ?? string.Empty;
            columns.Add(new ColumnDefinition(item.i, value, type));
        }

        
        return columns;
    }
    private class Foor
    {
        public Type Type = typeof(object);
        public int Count;
    }
    private Type FindLeastComplexTypeInColumn(object[][] dataRows, int columnIndex)
    {
        var typesOrderedByComplexity = new []
        {
            new Foor { Type = typeof(string), Count = 0 },
            new Foor { Type = typeof(bool), Count = 0 },
            new Foor { Type = typeof(float), Count = 0 },
            new Foor { Type = typeof(int), Count = 0 },
        };
        var unmatchedTypeCount = 0;

        foreach (var row in dataRows)
        {
            var value = row[columnIndex];
            var foundType = typesOrderedByComplexity.SingleOrDefault(t => t.Type == value.GetType());
            if (foundType != null)
            {
                foundType.Count++;
            }
            else
            {
                unmatchedTypeCount++;
            }
        }

        var leastComplexType = typesOrderedByComplexity.FirstOrDefault(t => t.Count > 0);

        if (leastComplexType != null)
        {
            return leastComplexType.Type;
        }
        return typeof(string);
    }

    private object GetCellValue(Cell cell, WorkbookPart workbook)
    {
        if (cell.CellValue == null || cell.CellValue.Text == null)
        {
            return string.Empty;
        }
        var text = cell.CellValue.Text;

        switch (cell.DataType?.Value)
        {
            case CellValues.Boolean:
                if (bool.TryParse(text, out var boolValue))
                {
                    return boolValue;
                }
                else
                {
                    throw new Exception();
                }
            case CellValues.Number:
                if (int.TryParse(text, out var intValue))
                {
                    return intValue;
                }
                else if (float.TryParse(text, out var floatValue))
                {
                    return floatValue;
                }
                else
                {
                    throw new Exception();
                }
            case CellValues.SharedString:
                var stringTable = workbook.GetPartsOfType<SharedStringTablePart>().First();
                return stringTable.SharedStringTable.ElementAt(int.Parse(text)).InnerText;
            case CellValues.InlineString:
                return text;
            default:
                throw new Exception();
            
        }
    }

    private DocumentFormat.OpenXml.Spreadsheet.Sheet GetSheet(WorkbookPart workbook, WorksheetPart worksheet)
    {
        if (workbook.Workbook.Sheets == null)
        {
            throw new Exception();
        }

        var id = workbook.GetIdOfPart(worksheet);
        return workbook.Workbook.Sheets.Elements<DocumentFormat.OpenXml.Spreadsheet.Sheet>().First(s => s.Id != null && s.Id.HasValue && s.Id.Value!.Equals(id));
    }
}
