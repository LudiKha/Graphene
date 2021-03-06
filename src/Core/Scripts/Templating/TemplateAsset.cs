﻿using UnityEngine;
using UnityEngine.UIElements;

namespace Graphene
{


  ///<summary>
  /// <para>A `Template` is a static asset that represents a chunk of UXML of varying granularity and complexity, which are used as building blocks to build and render the application.</para> 
  /// <para><see href="https://github.com/LudiKha/Graphene#template">Read more in the online documentation</see></para>
  ///</summary>
  [CreateAssetMenu(menuName = "Graphene/Templating/TemplateAsset")]
  public class TemplateAsset : ScriptableObject
  {
    internal const string templateAddClassName = "gr-template";
    [SerializeField] VisualTreeAsset _VisualTreeAsset; public VisualTreeAsset VisualTreeAsset => _VisualTreeAsset;

    [SerializeField] string _RootElementName; public string RootElementName => _RootElementName;

    [SerializeField] string _AddClass; public string AddClass => _AddClass;
    [SerializeField] string _AddClassToChildren; public string AddClassToChildren => _AddClassToChildren;

    [SerializeField] float _forceHeight = -1; public float ForceHeight => _forceHeight;

    public virtual VisualElement Instantiate()
    {
      TemplateContainer clone = VisualTreeAsset.CloneTree();

      clone.AddMultipleToClassList(templateAddClassName);
      if (AddClass != null)
        clone.AddMultipleToClassList(AddClass);

      return clone;
    }
  }
}