using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Graphene/Template/Template")]
public class Template : ScriptableObject
{
  [SerializeField] UnityEngine.UIElements.VisualTreeAsset _VisualTreeAsset; public UnityEngine.UIElements.VisualTreeAsset VisualTreeAsset => _VisualTreeAsset;

  [SerializeField] string _RootElementName; public string RootElementName => _RootElementName;

  [SerializeField] string _AddClass; public string AddClass => _AddClass;
}
