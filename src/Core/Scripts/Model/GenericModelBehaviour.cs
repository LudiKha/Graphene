using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Graphene.ViewModel
{
  public abstract class GenericModelBehaviour<T> : ViewModelComponent
  {
    [Draw(ControlType.Button)]
    [SerializeField]
    protected List<T> items = new List<T>();

    public abstract List<T> Items { get; }
  }

  public class GenericModelBehaviour : GenericModelBehaviour<BindableObject>
  {
    public override List<BindableObject> Items => items;

    public override void Initialize(VisualElement container, Plate plate)
    {
    }
  }
}
