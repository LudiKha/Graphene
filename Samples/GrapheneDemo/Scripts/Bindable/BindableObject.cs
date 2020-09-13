using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using Sirenix.OdinInspector;


namespace Graphene
{
  // Atomic object for the view
  [System.Serializable, Draw(ControlType.Button)]
  public class BindableObject : IRoute
  {
    [Bind("Label", BindingMode.OneWay), HorizontalGroup, HideLabel]
    public string Name;

    [Bind("Value"), HorizontalGroup, HideLabel]
    public string Value;

    [Route]
    public string route;

    [BindTooltip("Tooltip")]
    public string Description;

    [FoldoutGroup("Additionals")]
    public string addClass;

    [Bind("OnClick"), FoldoutGroup("Additionals")]
    public UnityEvent OnClick;
  }

  [System.Serializable, Draw(ControlType.Slider)]
  public class BindableFloat : BindableBaseField<float>
  {
    [BindFloat("Value", 0.75f, 0, 1, null, true)]
    public override float Value { get => value; set { this.value = value; } }

    public Vector2 minMax;
  }

  [System.Serializable, Draw(ControlType.Toggle)]
  public class BindableBool : BindableBaseField<bool>
  {
    [BindBaseFieldAttribute("Value", null, true)]
    public override bool Value { get => value; set { this.value = value; } }
  }


  [System.Serializable, Draw(ControlType.Slider)]
  public class RangeBaseField<TValueType> : BindableBaseField<TValueType>
  {
    [BindBaseFieldAttribute("Value")]
    public override TValueType Value { get => value; set { this.value = value; } }

    public TValueType min;
    public TValueType max;
  }

  [System.Serializable, Draw(ControlType.SliderInt)]
  public class BindableInt : RangeBaseField<int>
  {
  }

  [System.Serializable, Draw(ControlType.SelectField)]
  public class BindableNamedInt : BindableBaseField<int>
  {
    [Bind("Items")]
    public List<string> items = new List<string>();
    [Bind("Value")]
    public override int Value { get => base.Value; set => base.Value = value; }

    public void InitFromEnum<T>()
    {
      this.items.Clear();

      foreach (string s in System.Enum.GetNames(typeof(T)).ToList())
        this.items.Add(s);
    }
    public void InitFromList(IEnumerable<string> list)
    {
      this.items.Clear();
      this.items = list.ToList();
    }
  }

  [System.Serializable]
  public class BindableBaseField<T>
  {
    [SerializeField] protected T value;
    public virtual T Value { get => value; set { this.value = value; } }

    [Bind("Label", BindingMode.OneWay)]
    public string Label;

    [BindValueChangeCallback("ValueChange")]
    public EventCallback<ChangeEvent<T>> ValueChange => (changeEvent) => { ValueChangeCallback(changeEvent.newValue); };

    public event System.EventHandler<T> OnValueChange;

    protected virtual void ValueChangeCallback(T value)
    {
      OnValueChange?.Invoke(this, value);
      UnityEngine.Debug.Log($"{this.GetType().Name}'s value changing to {value}");
    }

  }

}