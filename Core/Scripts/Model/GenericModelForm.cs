using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Graphene
{
  public abstract class GenericModelForm<T> : Form
  {
    [Draw(ControlType.Button)]
    public List<T> model = new List<T>();
  }

  [CreateAssetMenu(menuName = "UI/Forms/GenericModelForm")]
  public class GenericModelForm : GenericModelForm<BindableObject>
  {
    public override void Initialize(VisualElement container, Plate plate)
    {
    }

    //public override void Render(VisualElement container, UIControlsTemplates templates)
    //{
    //  // Presuming these are controls
    //  //foreach (var item in model)
    //  //{
    //  //  var template = templates.TryGetTemplate(item, ControlType.Button);
    //  //  var inst = UI.BindingExtensions.Instantiate(item, template);

    //  //  content.Add(inst);

    //  //  if (item is BindableObject bindable)
    //  //  {
    //  //    inst.AddToClassList(bindable.addClass);
    //  //  }
    //  //}
    //}
  }
}