

using System;
using UnityEngine;
using UnityEngine.UIElements;
using System.ComponentModel;

namespace Graphene
{
    using Elements;
    using Kinstrife.Core.ReflectionHelpers;
  using UnityEditor;

  /// <summary>
  /// Non-generic base class
  /// </summary>
  public abstract class Binding : IDisposable, IBinding
  {
    public bool scheduleDispose;
    public virtual void Dispose()
    {
    }

    public abstract void PreUpdate();

    public abstract void Release();

    public abstract void Update();
  }
  public abstract class Binding<T> : Binding
  {
    protected object context;
    [SerializeField] protected T lastValue;
    [SerializeField] protected T newValue;

    protected BindableElement element;
    [SerializeField] BindAttribute attribute;

    // The target field
    protected string memberName;
    protected ExtendedTypeInfo extendedTypeInfo;

    public Binding(BindableElement el, in object context, in ValueWithAttribute<BindAttribute> member)
    {
      this.element = el;
      this.context = context;
      this.extendedTypeInfo = TypeInfoCache.GetExtendedTypeInfo(context.GetType()); // K: 28-10-2020 -> Could be optimized with member.MemberInfo.DeclaringType;

      this.attribute = member.Attribute;
      this.memberName = member.MemberInfo.Name;

      //el.binding = this;

      DetermineBindingMode();
      RegisterEvents();
    }

    void DetermineBindingMode()
    {
      if (context is INotifyPropertyChanged notifyPropertyChanged)
      {
        notifyPropertyChanged.PropertyChanged += Model_PropertyChanged;
      }
      // Specifically set to not have two-way binding
      if (attribute.bindingMode.HasValue)
      {
        if (attribute.bindingMode == BindingMode.TwoWay)
          RegisterTwoWayValueChangeCallback();
      }
      // No value set - Determine based on control type
      else
      {
        // Can't two-way bind a label
        if (this.element is Label || element is If)
          return;
        else if (this.element is INotifyValueChanged<T>)
          RegisterTwoWayValueChangeCallback();
      }
    }

    void RegisterEvents()
    {      
      if (context is IHasTooltip hasTooltip)
      {
		element.tooltip = hasTooltip.Tooltip;
		if (element is BaseField<T> baseField)
		  baseField.labelElement.tooltip = hasTooltip.Tooltip;

		if (context is IBindableToVisualElement bindable)
		{
		  bindable.onSetEnabled += element.SetEnabled;
		  bindable.onShowHide += element.SetShowHide;
		  bindable.onSetActive += element.SetActive;

		  element.SetEnabled(bindable.isEnabled);          
		  element.SetActive(bindable.isActive2);
          bindable.SetBinding(element);
		  element.RegisterCallback<DetachFromPanelEvent>(OnDetach);
		}
	  }
    }

    void OnDetach(DetachFromPanelEvent evt) => UnregisterEvents();

    void UnregisterEvents()
    {
      if (context is IBindableToVisualElement bindable)
      {
        bindable.onSetEnabled -= element.SetEnabled;
        bindable.onShowHide -= element.SetShowHide;
        bindable.onSetActive -= element.SetActive;
      }
    }

    void SyncVisualElementToModel()
    {

    }

    protected virtual void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
    }

    public override void Dispose()
    {
      base.Dispose();
      UnregisterEvents();
    }
    public override void PreUpdate()
    {
      throw new NotImplementedException();
    }

    public override void Release()
    {
      throw new NotImplementedException();
    }

    public override void Update()
    {
      // Needs to be disposed because the context ceased to exist
      if (context == null || !IsValidBinding())
      {
        scheduleDispose = true;
        return;
      }

      newValue = GetValueFromMemberInfo();

      UpdateFromModel(in newValue);
    }

    protected virtual void UpdateFromModel(in T newValue)
    {
      // Model changed -> Update view
      if (this.lastValue != null && !this.lastValue.Equals(newValue))
      {
        if (newValue is T && element is INotifyValueChanged<T> notifyValueChanged)
          notifyValueChanged.SetValueWithoutNotify(newValue);
        else if (newValue is string text && element is TextElement textEl)
          textEl.text = text;
        else if (newValue is string foldoutText && element is Foldout foldout)
          foldout.text = foldoutText;
        else if (element is IBindableElement<object> bindableEl)
          bindableEl.OnModelChange(newValue);
        else
          Debug.LogError($"No binding found for {element?.bindingPath} {context}", context as UnityEngine.Object);
      }

      lastValue = newValue;
    }

    void RegisterTwoWayValueChangeCallback()
    {
      if (element is INotifyValueChanged<T> notifyValChangedEl)
      {
        notifyValChangedEl.RegisterValueChangedCallback((evt) =>
        {
          SetValueFromMemberInfo(evt.newValue);
        });
      }
      return;

      //if (context is INotifyValueChanged<T> notifyChangeContext && element is INotifyValueChanged<T> notifyChangeEl)
      //{
      //  notifyChangeEl.RegisterValueChangedCallback((evt) => { notifyChangeContext.SetValueWithoutNotify(evt.newValue); });

      //  notifyChangeContext.RegisterValueChangedCallback((evt) => { notifyChangeEl.SetValueWithoutNotify(evt.newValue); });
      //}
      //else if(element is INotifyValueChanged<T> notifyValChangedEl)
      //{
      //  notifyValChangedEl.RegisterValueChangedCallback((evt) =>
      //  {
      //    SetValueFromMemberInfo(evt.newValue);
      //  });
      //}
    }

    protected abstract bool IsValidBinding();
    protected abstract T GetValueFromMemberInfo();
    protected abstract void SetValueFromMemberInfo(T value);

  }
}