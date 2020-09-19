using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace Graphene.Demo
{
  // Atomic "Über" object for the view
  [System.Serializable, Draw(ControlType.ListItem)]
  public class BindableObject : IRoute, ICustomControlType
  {
    [SerializeField] ControlType controlType; public ControlType ControlType => controlType;
    [Bind("Label", BindingMode.OneWay)]
    public string Name;

    [Bind("Value")]
    public string Value;

    [Route]
    public string route;

    [BindTooltip("Tooltip")]
    public string Description;

    #region FoldoutAttribute
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.FoldoutGroup("Additionals")]
#elif NAUGHTY_ATTRIBUTES
    [NaughtyAttributes.Foldout("Additionals")]
#endif
    #endregion
    public string addClass;

    #region FoldoutAttribute
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.FoldoutGroup("Additionals")]
#elif NAUGHTY_ATTRIBUTES
    [NaughtyAttributes.Foldout("Additionals")]
#endif
    #endregion
    [Bind("OnClick")]
    public UnityEvent OnClick;
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
    }
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
    public virtual float normalizedValue { get => throw new System.NotImplementedException(); }

    public TValueType min;
    public TValueType max;
  }

  [System.Serializable, Draw(ControlType.Slider)]
  public class BindableFloat : RangeBaseField<float>
  {
    public override float normalizedValue => value / (max - min);
  }

  [System.Serializable, Draw(ControlType.SliderInt)]
  public class BindableInt : RangeBaseField<int>
  {
    public override float normalizedValue => (float)value / ((float)max - (float)min);
  }

  [System.Serializable, Draw(ControlType.SelectField)]
  public class BindableNamedInt : BindableBaseField<int>
  {
    [Bind("Items")]
    public List<string> items = new List<string>();
    [Bind("Value")]
    public override int Value { get => base.Value; set => base.Value = value; }
    public float normalizedValue => (float)value / items.Count;

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

}
