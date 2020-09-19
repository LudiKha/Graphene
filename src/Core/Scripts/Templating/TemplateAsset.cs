using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Graphene {


  ///<summary>
  /// <para>A `Template` is a static asset that represents a chunk of UXML of varying granularity and complexity, which are used as building blocks to build and render the application.</para> 
  /// <para><see href="https://github.com/LudiKha/Graphene#template">Read more in the online documentation</see></para>
  ///</summary>
  [CreateAssetMenu(menuName = "Graphene/Templating/TemplateAsset")]
  public class TemplateAsset : ScriptableObject
  {
    [SerializeField] VisualTreeAsset _VisualTreeAsset; public VisualTreeAsset VisualTreeAsset => _VisualTreeAsset;

    internal const string templateAddClassName = "gr-template";

    [SerializeField] string _RootElementName; public string RootElementName => _RootElementName;

    [SerializeField] string _AddClass; public string AddClass => _AddClass;
    [SerializeField] string _AddClassToChildren; public string AddClassToChildren => _AddClassToChildren;

    public TemplateContainer Instantiate()
    {
      TemplateContainer clone = VisualTreeAsset.CloneTree();

      clone.AddMultipleToClassList(templateAddClassName);
      clone.AddMultipleToClassList(AddClass);

      return clone;
    }
  }
}