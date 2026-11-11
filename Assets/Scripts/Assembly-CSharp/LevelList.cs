using System;
using UnityEngine;

[CreateAssetMenu]
public class LevelList : ScriptableObject
{
	[Serializable]
	public struct LevelInfo
	{
		public string archiveName;

		public string sceneName;

		public GameCompletionFlags completionFlag;

		public bool displayByDefault;
	}

	public LevelInfo[] _levelInfos;
}
