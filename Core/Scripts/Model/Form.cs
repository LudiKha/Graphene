
using NaughtyAttributes;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Graphene
{
  public interface IForm
  {
    [Bind("Title")]
    string Title { get; }
    void OnSubmit();
    void OnCancel();
  }

  public abstract class Form : ScriptableObject, IModel
  {
    [SerializeField, Bind("Title")] protected string title; public string Title => title;    
    [SerializeField, Bind("Render")] protected bool render = true; public bool Render => render;

    //public abstract List<object> GetDrawableObjects() { }

    public abstract void Initialize(VisualElement container, Plate plate);

    public event System.Action Redraw;

    [Button]
    internal void ForceRedraw()
    {
      Redraw?.Invoke();
    }

    //public abstract void Render(UnityEngine.UIElements.VisualElement container, UIControlsTemplates templates);

    //[Button]
    //public abstract void OnSubmit();
    //[Button]
    //public abstract void OnCancel();
  }
}