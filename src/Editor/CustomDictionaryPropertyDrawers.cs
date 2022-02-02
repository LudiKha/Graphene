using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Graphene.Editor
{
  [CustomPropertyDrawer(typeof(SerializableDictionary<ControlType, VisualTreeAsset>))]
  [CustomPropertyDrawer(typeof(ControlVisualTreeAssetMapping))]
  public class AnySerializableDictionaryStoragePropertyDrawer : SerializableDictionaryPropertyDrawer { }
}