using OpenDataService.DataSources;
using OpenDataService.DataSources.Dynamic;
using OpenDataService.DataSources.Extensions;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Collections;

namespace OpenDataService.DataSources.Excel;

public class ExcelDataSource : IDataSource
{
    private Dictionary<string, IEntitySet> entitySets = new Dictionary<string, IEntitySet>();
    public ExcelDataSource(Stream stream)
    {

        Sheets = Read(stream);

        var entitySetBuilder = new EntitySetBuilder("Excel");
        var builtEntitySets = entitySetBuilder.Build("DS", Sheets);
        foreach (var entitySet in builtEntitySets)
        {
            entitySets.Add(entitySet.Name, entitySet);
        }
    }
    public IEnumerable<Sheet> Sheets { get; private set;}

    public IEntitySet GetEntitySet(string name)
    {
        return entitySets[name];
    }

    public IEnumerator<IEntitySet> GetEnumerator()
    {
        return entitySets.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return entitySets.Values.GetEnumerator();
    }
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
                var stride = columnRow.Elements<Cell>().Count();
                var dataRows = sheetData.Elements<Row>().Skip(1).ToArray();
                var dataRaw = new object[dataRows.Length * stride];

                foreach (var (row, i) in dataRows.WithIndex())
                {
                    var offset = i * stride;
                    foreach (var cell in row.Elements<Cell>())
                    {
                        var columnIndex = GetColumnIndex(cell);
                        dataRaw[offset + columnIndex] = GetCellValue(cell, workbook);
                    }
                }

                sheets.Add(new Sheet(name, GetColumns(columnRow, workbook, dataRaw), dataRaw));
                var worksheet = sheetPart.Worksheet;
            }
        }

        return sheets;
    }

    private int GetColumnIndex(Cell cell)
    {
        int index = 0;
        var reference = cell.CellReference?.ToString()?.ToUpper();

        if (reference == null)
        {
            throw new Exception();
        }
       
        foreach (char ch in reference)
        {
            if (char.IsLetter(ch))
            {
                int value = ch - 'A';
                index = (index * 26) + value;
            }
            else
            {
                break;
            }
        }
        return index;
    }

    private List<ColumnDefinition> GetColumns(Row row, WorkbookPart workbook, object[] data)
    {
        var columns = new List<ColumnDefinition>();
        var columnCells = row.Elements<Cell>().ToArray();
        var stride = columnCells.Length;

        foreach (var (columnCell, i) in columnCells.WithIndex())
        {
            var type = FindLeastComplexTypeInColumn(data, stride, i);
            string value = GetCellValue(columnCell, workbook).ToString() ?? string.Empty;
            columns.Add(new ColumnDefinition(i, value, type.Type, data, stride));
        }

        
        return columns;
    }
    private class TypeCounter
    {
        public Type Type = typeof(object);
        public int Count;
        public bool Nullable;
    }
    private TypeCounter FindLeastComplexTypeInColumn(object[] data, int columnCount, int columnIndex)
    {
        int rowCount = data.Length / columnCount;
        var cellIndices = new int[rowCount];
        for (int i = 0; i < rowCount; i++)
        {
            cellIndices[i] = (i * columnCount) + columnIndex;
        }
        var typesOrderedByComplexity = new []
        {
            new TypeCounter { Type = typeof(string), Count = cellIndices.Count(idx => data[idx]?.GetType() == typeof(string)) },
            new TypeCounter { Type = typeof(bool), Count = cellIndices.Count(idx => data[idx]?.GetType() == typeof(bool)) },
            new TypeCounter { Type = typeof(float), Count = cellIndices.Count(idx => data[idx]?.GetType() == typeof(float)) },
            new TypeCounter { Type = typeof(int), Count = cellIndices.Count(idx => data[idx]?.GetType() == typeof(int)) },
        };
        
        var nulls = cellIndices.Count(idx => data[idx] == null);

        var leastComplexType = typesOrderedByComplexity.FirstOrDefault(t => t.Count > 0);

        if (leastComplexType != null)
        {
            if (nulls > 0)
            {
                leastComplexType.Nullable = true;
                if (leastComplexType.Type == typeof(int))
                {
                    leastComplexType.Type = typeof(int?);
                }
            }
            return leastComplexType;
        }
        throw new Exception();
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
            case null:
                return ParseAny(text);
            default:
                throw new Exception();
            
        }
    }

    private object ParseAny(string text)
    {
        if (bool.TryParse(text, out var boolValue))
        {
            return boolValue;
        }
        else if (int.TryParse(text, out var intValue))
        {
            return intValue;
        }
        else if (float.TryParse(text, out var floatValue))
        {
            return floatValue;
        }
        return text;
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
