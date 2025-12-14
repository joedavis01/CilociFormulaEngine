using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using ciloci.FormulaEngine;

namespace ConsoleTester;

internal static class Program
{
    private static FormulaEngine _engine = null!;

    private static void Main()
    {
        var sheet1 = new DumbSheet("Sheet1");
        sheet1.SetCellValue(1, 1, 14.56);
        sheet1.SetCellValue(2, 1, "");
        sheet1.SetCellValue(3, 1, 156);
        sheet1.SetCellValue(1, 2, 100);
        sheet1.SetCellValue(2, 2, 200);
        sheet1.SetCellValue(3, 2, 300);

        var sheet2 = new DumbSheet("Sheet2");

        _engine = new FormulaEngine();

        object result = _engine.Evaluate("Substitute(\"bbb\", \"\", \"!!\")");

        IExternalReference reference = _engine.ReferenceFactory.External();
        _engine.AddFormula("cos(45)", reference);

        var memento = new Memento
        {
            MyRef = reference,
            MyEngine = _engine
        };

        using var ms = new MemoryStream();
#pragma warning disable SYSLIB0011
        var formatter = new BinaryFormatter();
        formatter.Serialize(ms, memento);

        ms.Seek(0, SeekOrigin.Begin);
        var restored = (Memento)formatter.Deserialize(ms);
#pragma warning restore SYSLIB0011
    }

    [Serializable]
    private class Memento
    {
        public IExternalReference MyRef { get; set; } = null!;
        public FormulaEngine MyEngine { get; set; } = null!;
    }

    private static Formula CreateFormula(string expression, string reference)
    {
        ISheetReference gridRef = _engine.ReferenceFactory.Parse(reference);
        Formula formula = _engine.AddFormula(expression, gridRef);
        return formula;
    }
}

[Serializable]
internal class DumbSheet : ISheet
{
    private const int RowCountConst = 16;
    private const int ColCountConst = 10;

    private readonly object?[,] _cells;
    private readonly string _name;

    public DumbSheet(string name)
    {
        _cells = new object[RowCountConst, ColCountConst];
        _name = name;
    }

    public void SetCellValue(int row, int col, object value)
    {
        _cells[row - 1, col - 1] = value;
    }

    public object GetCellValue(int row, int column)
    {
        return _cells[row - 1, column - 1]!;
    }

    public void SetFormulaResult(object result, int row, int column)
    {
    }

    public string Name => _name;

    int ISheet.RowCount => RowCountConst;

    int ISheet.ColumnCount => ColCountConst;
}
