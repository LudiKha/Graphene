
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Graphene
{
  public interface IModel
  {
    void Initialize(VisualElement container, Plate plate);
    bool Render { get; }

    System.Action onModelChange { get; set; }

    void Refresh(VisualElement container);
  }

  /// <summary>
  /// Instructs the <see cref="Renderer"/> to use a custom <see cref="BindAttribute"/> context
  /// </summary>
  public interface ICustomBindContext
  {
    public object GetCustomBindContext { get; }
  }

  /// <summary>
  /// Instructs the <see cref="Renderer"/> to use a custom <see cref="DrawAttribute"/> context
  /// </summary>
  public interface ICustomDrawContext
  {
    public object GetCustomDrawContext { get; }
  }

  public interface IForm
  {
    [Bind("Title")]
    string Title { get; }
    void OnSubmit();
    void OnCancel();
  }

  public abstract class Form : ScriptableObject, IModel
  {
    [field: SerializeField][Bind("Title")] public string Title { get; set; } = "Title";
    [field: SerializeField][Bind("Render")] public bool Render { get; set; } = true;
    public Action onModelChange { get; set; }

    public event System.Action Redraw;

    //public abstract List<object> GetDrawableObjects() { }

    public abstract void Initialize(VisualElement container, Plate plate);

	public void Refresh(VisualElement container)
	{
	}


	#region ButtonAttribute
#if ODIN_INSPECTOR
	[Sirenix.OdinInspector.Button]
#elif NAUGHTY_ATTRIBUTES
    [NaughtyAttributes.Button]
#endif
    #endregion
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