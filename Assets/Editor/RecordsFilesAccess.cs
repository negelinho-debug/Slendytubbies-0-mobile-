#if UNITY_EDITOR && UNITY_IOS
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

public static class RecordsFilesAccess
{
    [PostProcessBuild(100)]
    public static void EnableFiles(BuildTarget target, string output)
    {
        if (target != BuildTarget.iOS) return;
        var path = Path.Combine(output, "Info.plist");
        var plist = new PlistDocument();
        plist.ReadFromFile(path);
        plist.root.SetBoolean("UIFileSharingEnabled", true);
        plist.root.SetBoolean("LSSupportsOpeningDocumentsInPlace", true);
        plist.WriteToFile(path);
    }
}
#endif
