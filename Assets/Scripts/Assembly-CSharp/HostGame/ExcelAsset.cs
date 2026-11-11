using System;
using System.Collections.Generic;
using UnityEngine;

namespace HostGame;

public class ExcelAsset : ScriptableObject
{
	private Dictionary<string, ExcelElement> elementMap = new Dictionary<string, ExcelElement>(2048, StringComparer.OrdinalIgnoreCase);

	[SerializeField]
	private List<string> m_SpreadsheetNames = new List<string>(1);

	[SerializeField]
	private List<Spreadsheet> m_Spreadsheets = new List<Spreadsheet>(1);

	[SerializeField]
	private ExcelElement[] m_Elements;

	public static Action<ExcelAsset> OnExcelAssetImported
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	public int SpreadsheetCount => m_Spreadsheets.Count;

	public Spreadsheet this[int index] => m_Spreadsheets[index];

	public Spreadsheet this[string name]
	{
		get
		{
			int num = m_SpreadsheetNames.IndexOf(name);
			if (num < 0)
			{
				return null;
			}
			return m_Spreadsheets[num];
		}
	}

	public DateTime TimeImported => default(DateTime);

	public static event Action OnAnyExcelAssetImported;

	public string[] GetAllNames()
	{
		return m_SpreadsheetNames.ToArray();
	}

	public string GetSpreadsheetName(int index)
	{
		return m_SpreadsheetNames[index];
	}

	public int GetSpreadsheetIndex(string name)
	{
		return m_SpreadsheetNames.IndexOf(name);
	}

	public bool Contains(string spreadsheetName)
	{
		return m_SpreadsheetNames.IndexOf(spreadsheetName) >= 0;
	}

	public void PrepareMap()
	{
		elementMap.Clear();
		for (int i = 0; i < m_Elements.Length; i++)
		{
			ExcelElement excelElement = m_Elements[i];
			elementMap[excelElement.id] = excelElement;
		}
	}

	public ExcelElement FindElement(string id)
	{
		return elementMap.GetValueOrDefault(id, null);
	}
}
