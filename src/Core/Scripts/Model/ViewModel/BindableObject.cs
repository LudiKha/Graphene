
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace Graphene.ViewModel
{
  [System.Serializable]
  public abstract class BindableObjectBase : IBindableToVisualElement
  {
#if ODIN_INSPECTOR
	[field: InlineButton(nameof(ToggleEnable))]
#endif
	[field: SerializeField, IgnoreDataMember] public bool isEnabled { get; set; } = true;
	public Action<bool> onSetEnabled { get; set; }

#if ODIN_INSPECTOR
	[field: InlineButton(nameof(ToggleShow))]
#endif
	[field: SerializeField, IgnoreDataMember] public bool isShown { get; set; } = true;
	public Action<bool> onShowHide { get; set; }

#if ODIN_INSPECTOR
	[field: InlineButton(nameof(ToggleActive))]
#endif
	[field: SerializeField, IgnoreDataMember] public bool isActive2 { get; set; } = false;
	public Action<bool> onSetActive { get; set; }

	public VisualElement boundToElement { get; set; }
	public System.Action<VisualElement> onBindToElement;// { get; set; }

	public void SetBinding(VisualElement el)
	{
	  boundToElement = el;
	  onBindToElement?.Invoke(el);
	}

	[field: SerializeField] public string Tooltip { get; set; }

	void ToggleEnable() => SetEnabled(!isEnabled);
	void ToggleShow() => SetShow(!isShown);
	void ToggleActive() => SetActive(!isActive2);

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
	public void SetActive(bool active)
	{
	  isActive2 = active;
	  onSetActive?.Invoke(active);
	}

	public virtual void ResetCallbacks()
	{
	  onSetEnabled = null;
	  onShowHide = null;
	}
  }

  // Atomic "Über" object for the view
  [System.Serializable, Draw(ControlType.Button)]
  public class BindableObject : BindableObjectBase, IRoute, ICustomControlType, ICustomAddClasses, ICustomName, IHasCustomVisualTreeAsset
  {
	[field: SerializeField]
	public ControlType ControlType { get; set; }

	[field: SerializeField]
	public VisualTreeAsset VisualTreeAsset { get; set; }

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

	[field: SerializeField]
	[Bind("Image")]
	public Texture Image { get; set; }

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

	#region Util
	public override string ToString()
	{
	  return $"{this.GetType().Name} - [{this.Name}]";
	}
	#endregion

	public BindableObject()
	{
	}

	public BindableObject(UnityAction callback) : this()
	{
	  OnClick.AddListener(callback);
	}
  }

  // Atomic "Über" object for the view
  [System.Serializable, Draw(ControlType.Button)]
  public class ContextBindableObject : BindableObject
  {
	[Bind("Content")]
	[field: SerializeField] public List<ContextBindableObject> Content { get; private set; }

	[Bind("HasContent")][field: SerializeField] public bool HasContent { get; private set; }

	[Bind("Actions")]
	[field: SerializeField]
	public List<ActionButton> Actions { get; set; }

	[Bind("HasActions")]
	public bool HasActions => Actions != null && Actions.Count > 0;


	public void AddAction(string name, Action callback, string tooltip = null)
	{
	  if (Actions == null)
		Actions = new List<ActionButton>();
	  var action = new ActionButton
	  {
		Label = name,
		Tooltip = tooltip,
		OnClick = callback
	  };
	  Actions.Add(action);
	}
  }

  [Draw(ControlType.Button)]
  [System.Serializable]
  public class ActionButton : IHasTooltip
  {
	[field: SerializeField]
	[Bind("Label")]
	public string Label { get; set; }

	[field: SerializeField]
	[Bind]
	public System.Action OnClick { get; set; }

	[field: SerializeField]	public string Tooltip { get; set; }

	[Bind("Enabled")]
	public bool Enabled => OnClick != null;
  }
}
