using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.EditorCoroutines.Editor;

namespace Graphene
{
  internal static class GrapheneEditorUtilities
  {
    public const string uuid = "com.cupbearer.graphene";
    public const string gitUrl = "https://github.com/LudiKha/Graphene.git?path=/src";

    public class PackageRequest
    {
    }

    [MenuItem("Window/Graphene/Check for updates")]
    static void CheckForUpdates()
    {
      Debug.Log($"Checking for updates");
      var owner = new PackageRequest();
      EditorCoroutineUtility.StartCoroutine(MonitorPackageUpdate(owner), owner);
    }

    static IEnumerator MonitorPackageUpdate(PackageRequest owner)
    {
      var request = UnityEditor.PackageManager.Client.Add(gitUrl);

      while (!request.IsCompleted)
      {
        yield return null;
      }

      if(request.Error!= null)
        Debug.LogError($"Error code {request.Error.message}: {request.Error.message}");

      Debug.Log($"Latest version: {request.Result.version}");
      yield break;
    }
  }
}