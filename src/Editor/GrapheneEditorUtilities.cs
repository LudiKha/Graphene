using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.EditorCoroutines.Editor;

namespace Graphene
{
  internal static class GrapheneEditorUtilities
  {
    public const string uuid = "com.graphene.core";
    public const string gitUrlCore = "https://github.com/LudiKha/Graphene.git?path=/src";
    public const string gitUrlComponents = "https://github.com/LudiKha/Graphene-Components.git?path=/src";
    public const string gitUrlDemo = "https://github.com/LudiKha/Graphene-Demo.git?path=/src";

    public class PackageRequest
    {
    }

    [MenuItem("Window/Graphene/Check for updates/Graphene Core")]
    static void CheckForUpdates()
    {
      var owner = new PackageRequest();
      EditorCoroutineUtility.StartCoroutine(MonitorPackageUpdate(owner, gitUrlComponents, "Graphene Core"), owner);
    }
    [MenuItem("Window/Graphene/Check for updates/Graphene Components")]
    static void CheckForUpdatesComponents()
    {
      var owner = new PackageRequest();
      EditorCoroutineUtility.StartCoroutine(MonitorPackageUpdate(owner, gitUrlCore, "Graphene Components"), owner);
    }
    [MenuItem("Window/Graphene/Check for updates/Graphene Demo")]
    static void CheckForUpdatesDemo()
    {
      var owner = new PackageRequest();
      EditorCoroutineUtility.StartCoroutine(MonitorPackageUpdate(owner, gitUrlDemo, "Graphene Demo"), owner);
    }


    static IEnumerator MonitorPackageUpdate(PackageRequest owner, string gitUrl, string packageName)
    {
      Debug.Log($"Checking for updates for {packageName}...");

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