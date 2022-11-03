
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.Events;

namespace Graphene.ViewModel
{
  [System.Serializable]
  public abstract class BindableObjectBase : IBindableToVisualElement
  {
#if ODIN_INSPECTOR
    [field: InlineButton(nameof(ToggleEnable))]
#endif
    [field: SerializeField, IgnoreDataMember] public bool isEnabled { get; private set; } = true;
    public Action<bool> onSetEnabled { get; set; }

#if ODIN_INSPECTOR
    [field: InlineButton(nameof(ToggleShow))]
#endif
    [field: SerializeField, IgnoreDataMember] public bool isShown { get; private set; } = true;
    public Action<bool> onShowHide { get; set; }

    void ToggleEnable() => SetEnabled(!isEnabled);
    void ToggleShow() => SetShow(!isShown);

    public void SetEnabled(bool enabled)
    {
      isEnabled = enabled;
      onSetEnabled?.Invoke(enabled);
    }

    public void SetShow(bool show)
    {
      isShown = show;
      onShowHide?.Invoke(show);
    }

    public virtual void ResetCallbacks()
    {
      onSetEnabled = null;
      onShowHide = null;
    }
  }

  // Atomic "Über" object for the view
  [System.Serializable, Draw(ControlType.Button)]
  public class BindableObject : BindableObjectBase, IRoute, ICustomControlType, ICustomAddClasses, ICustomName
  {
    [field: SerializeField]
    public ControlType ControlType { get; set; }

    [field: SerializeField]
    [Bind("Label", BindingMode.OneWay)]
    public string Name { get; set; }

    [field: SerializeField]
    [Bind("Value")]
    public string Value { get; set; }

    [field: SerializeField]
    [Route]
    public string route;

    [field: SerializeField]
    [BindTooltip("Description")]
    public string Description { get; set; }
        
    [field: SerializeField][Bind("Image")]
    public Texture Image { get; private set; }

    #region FoldoutAttribute
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.FoldoutGroup("Additionals")]
#elif NAUGHTY_ATTRIBUTES
    [NaughtyAttributes.Foldout("Additionals")]
#endif
    #endregion
    public string addClass; public string ClassesToAdd => addClass;

    #region FoldoutAttribute
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.FoldoutGroup("Additionals")]
#elif NAUGHTY_ATTRIBUTES
    [NaughtyAttributes.Foldout("Additionals")]
#endif
    #endregion
    public string customName; public string CustomName => customName;

    #region FoldoutAttribute
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.FoldoutGroup("Additionals")]
#elif NAUGHTY_ATTRIBUTES
    [NaughtyAttributes.Foldout("Additionals")]
#endif
    #endregion
    [Bind("")]
    public UnityEvent OnClick = new UnityEvent();
  }


  // Atomic "Über" object for the view
  [System.Serializable, Draw(ControlType.Button)]
  public class ContextBindableObject : BindableObject
  {
    [Bind("Content")]
    [field: SerializeField] public List<ContextBindableObject> Content { get; private set; }

    [Bind("HasContent")][field: SerializeField] public bool HasContent { get; private set; }

    [Draw]
    [field: SerializeField]
    public List<ActionButton> Actions { get; set; }

    [Bind("HasActions")]
    public bool HasActions => Actions != null && Actions.Count > 0;
  }

  [Draw(ControlType.Button)]
  [System.Serializable]
  public class ActionButton
  {
    [field: SerializeField]
    [Bind]
    public string Label { get; set; }

    [field: SerializeField]
    [Bind]
    public System.Action OnClick { get; set; }
  }
}
