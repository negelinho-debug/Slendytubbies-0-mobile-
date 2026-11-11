using HostGame;
using UnityEngine;

public class ExcelElement : ScriptableObject
{
	public ExcelAsset excel;

	public Spreadsheet sheet;

	public string id;

	public int start;

	public int length;

	public bool isDummy;

	public int FindColumnIndex(string columnName)
	{
		return sheet.FindColumnIndex(columnName);
	}

	public string Read(string columnPrimary, string columnSecondary, int row)
	{
		if (isDummy)
		{
			return id;
		}
		string text = null;
		int num = FindColumnIndex(columnPrimary);
		if (num >= 0)
		{
			text = Read(num, row);
		}
		if (num < 0 || string.IsNullOrEmpty(text))
		{
			num = FindColumnIndex(columnSecondary);
			text = Read(num, row);
		}
		return text;
	}

	public string Read(string column, int row)
	{
		if (isDummy)
		{
			return id;
		}
		int num = FindColumnIndex(column);
		if (num < 0)
		{
			return "";
		}
		return Read(num, row);
	}

	public string Read(int column, int row)
	{
		if (isDummy)
		{
			return id;
		}
		BoundCheck(column, row);
		row += start;
		return sheet[column, row];
	}

	private void BoundCheck(int column, int row)
	{
	}

	public static ExcelElement MakeFallback(ExcelElement el, string fallback)
	{
		if (!el)
		{
			return Make(fallback);
		}
		return el;
	}

	public static ExcelElement Make(string content)
	{
		ExcelElement excelElement = ScriptableObject.CreateInstance<ExcelElement>();
		excelElement.id = content;
		excelElement.isDummy = true;
		return excelElement;
	}
}
