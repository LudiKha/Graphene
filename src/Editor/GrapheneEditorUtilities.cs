using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Graphene
{
  public static class GrapheneEditorUtilities
  {
    public const string uuid = "com.cupbearer.graphene";

    [MenuItem("Window/Graphene/Update to latest version")]
    static void UpdateToLatest()
    {
      UnityEditor.PackageManager.Client.Add(uuid);
    }
  }
}