using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Graphene.Editor
{
  [CustomPropertyDrawer(typeof(SerializableDictionary<ControlType, Template>))]
  [CustomPropertyDrawer(typeof(ControlTemplateMapping))]
  public class AnySerializableDictionaryStoragePropertyDrawer : SerializableDictionaryPropertyDrawer { }
}