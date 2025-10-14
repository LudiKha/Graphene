using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Graphene.ViewModel
{
  public abstract class ScriptableObjectModel<T> : ScriptableObjectModel
  {
    [Draw(ControlType.Button)]
    public List<T> model = new List<T>();
  }

  /// <summary>
  /// A generic model asset that can be used to author a list of BindableObject.
  /// </summary>
  [CreateAssetMenu(menuName = "Graphene/Model/GenericModelAsset")]
  public class GenericModelAsset : ScriptableObjectModel<BindableObject>
  {
    public override void Initialize(VisualElement container, Plate plate)
    {
    }
  }
}
