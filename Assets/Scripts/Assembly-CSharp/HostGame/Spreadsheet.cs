using System;
using System.Collections.Generic;
using UnityEngine;

namespace HostGame;

[Serializable]
public class Spreadsheet : ScriptableObject
{
	[SerializeField]
	private int XSize;

	[SerializeField]
	private int YSize;

	[SerializeField]
	private string[] TextData;

	[SerializeField]
	private int[] StyleIdData;

	[SerializeField]
	private SpreadsheetCellStyle[] Styles;

	[SerializeField]
	public List<ExcelElement> Elements = new List<ExcelElement>();

	public int Columns => XSize;

	public int Rows => YSize;

	public string this[int column, int row]
	{
		get
		{
			CheckBounds(row, column);
			int num = column + row * XSize;
			return TextData[num];
		}
	}

	public void SetData(CellData[,] array, SpreadsheetCellStyle[] styles)
	{
		XSize = array.GetLength(0);
		YSize = array.GetLength(1);
		int num = 0;
		TextData = new string[XSize * YSize];
		StyleIdData = new int[XSize * YSize];
		for (int i = 0; i < array.GetLength(1); i++)
		{
			for (int j = 0; j < array.GetLength(0); j++)
			{
				TextData[num] = array[j, i].Text;
				StyleIdData[num] = array[j, i].StyleID;
				num++;
			}
		}
		Styles = styles;
	}

	public int FindColumnIndex(string filter, int row = 0)
	{
		for (int i = 0; i < Columns; i++)
		{
			if (string.Equals(this[i, row], filter, StringComparison.OrdinalIgnoreCase))
			{
				return i;
			}
		}
		return -1;
	}

	public int FindColumnIndex(string startsWith, string filter, int row = 0)
	{
		for (int i = 0; i < Columns; i++)
		{
			string text = this[i, row];
			if (text.StartsWith(startsWith))
			{
				ReadOnlySpan<char> readOnlySpan = MemoryExtensions.AsSpan(text);
				int length = startsWith.Length;
				if (MemoryExtensions.Equals(readOnlySpan.Slice(length, readOnlySpan.Length - length), MemoryExtensions.AsSpan(filter), StringComparison.Ordinal))
				{
					return i;
				}
			}
		}
		return -1;
	}

	public int FindRowIndex(string filter, int column = 0)
	{
		for (int i = 0; i < Rows; i++)
		{
			if (this[column, i] == filter)
			{
				return i;
			}
		}
		return -1;
	}

	public IEnumerable<(string value, int row)> FilteredElementsInColumn(int index, string ignoreChar = "#")
	{
		for (int i = 0; i < Rows; i++)
		{
			string text = this[index, i];
			if (!string.IsNullOrEmpty(text) && !text.StartsWith(ignoreChar))
			{
				yield return (value: text, row: i);
			}
		}
	}

	public SpreadsheetCellStyle GetSyle(int column, int row)
	{
		CheckBounds(row, column);
		int num = column + row * XSize;
		return Styles[StyleIdData[num]];
	}

	private void CheckBounds(int row, int column)
	{
	}
}
