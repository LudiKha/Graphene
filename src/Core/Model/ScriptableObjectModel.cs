
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Graphene
{
  /// <summary>
  /// An IModel that is also a ScriptableObject, allowing it to be saved as an asset in Unity.
  /// </summary>
  public abstract class ScriptableObjectModel : ScriptableObject, IModel
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