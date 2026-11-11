using UnityEngine;

[CreateAssetMenu(menuName = "Report/Commands")]
public class TerminalCommands : ScriptableObject
{
	public ExcelElement[] _commandNames;

	public ExcelElement _introText;

	public ExcelElement _errorText;

	public bool _showAvailable;
}
