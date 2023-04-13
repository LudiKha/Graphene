using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

namespace Graphene.ViewModel
{
	[System.Serializable]
  public abstract class BindableBaseField : BindableObjectBase
  {
  }

  [System.Serializable][DataContract]
  public abstract class BindableBaseField<T> : BindableBaseField, INotifyValueChanged<T>, INotifyPropertyChanged
  {
    [SerializeField]
    protected T m_Value;

    public BindableBaseField([CallerMemberName] string label = "Label")
    {
      this.Label = label;
    }

    [BindBaseField("Value")][DataMember(Name = "Value")]
    public virtual T value { get => m_Value; set {
        SetValueWithoutNotify(value);
        ValueChangeCallback(m_Value);
      } 
    }

    [field: SerializeField]
    [Bind(nameof(Label), BindingMode.OneWay)][IgnoreDataMember]
    public virtual string Label { get; set; }

    /*[BindValueChangeCallback(nameof(ValueChange))]*/[IgnoreDataMember]
    public EventCallback<ChangeEvent<T>> ValueChange => (changeEvent) => { ValueChangeCallback(changeEvent.newValue); };

    public event System.EventHandler<T> OnValueChange;
    public event PropertyChangedEventHandler PropertyChanged;

    public virtual void SetValueWithoutNotify(T newValue)
    {
      m_Value = newValue;
    }

    protected virtual void ValueChangeCallback(T value)
    {
      OnValueChange?.Invoke(this, value);
      PropertyChanged?.Invoke(null, null);
    }

    public override void ResetCallbacks()
    {
      base.ResetCallbacks();
      OnValueChange = null;
      PropertyChanged = null;
    }
  }



  [System.Serializable, Draw(ControlType.Toggle), DataContract]
  public class BindableBool : BindableBaseField<bool>
  {
  }



  [System.Serializable, Draw(ControlType.Slider)]
  public abstract class RangeBaseField<TValueType> : BindableBaseField<TValueType>
  {
    public abstract float normalizedValue { get; }

    [BindBaseField("Value")][DataMember(Name = "Value")]
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.ShowInInspector, Sirenix.OdinInspector.PropertyRange(0, 1, MinGetter = nameof(min), MaxGetter = nameof(max))]
#endif
    public override TValueType value { get => base.value; set => base.value = value; }

    [Bind("Min"), DataMember(Name = "Min")]
    public TValueType min;
    [Bind("Max"), DataMember(Name = "Max")]
    public TValueType max;

    public RangeBaseField([CallerMemberName] string label = null) : base(label) { }

	protected float Normalize(float value, float min, float max)
	{
	  // Clamp the input value between the min and max values
	  value = Mathf.Clamp(value, min, max);
	  // Normalize the input value based on the range
	  return (value - min) / (max - min);
	}

  }

  [System.Serializable, Draw(ControlType.Slider), DataContract]
  public class BindableFloat : RangeBaseField<float>
  {
    public override float normalizedValue => Normalize(m_Value, min, max);

    public BindableFloat()
    {
      min = 0;
      max = 1;
      m_Value = 0.5f;
    }

    public override void SetValueWithoutNotify(float newValue)
    {
      m_Value = Mathf.Clamp(newValue, min, max);
    }
  }

  [System.Serializable, Draw(ControlType.SliderInt), DataContract]
  public class BindableInt : RangeBaseField<int>
  {
	public override float normalizedValue => Normalize(m_Value, min, max);

	public BindableInt()
    {
      min = 0;
      max = 10;
      //m_Value = 5;
    }

    public override void SetValueWithoutNotify(int newValue)
    {
      m_Value = Mathf.Clamp(newValue, min, max);
    }
  }

  [System.Serializable, Draw(ControlType.SelectField), DataContract]
  public class BindableNamedInt : BindableBaseField<int>
  {
    [field: SerializeField]
    [Bind("Items"), IgnoreDataMember]
    public List<string> items { get; set; } = new List<string>();

    public float normalizedValue => (float)m_Value / items.Count;

    public void InitFromEnum<T>(bool splitUppercase = true)
    {
      this.items.Clear();

      foreach (string s in System.Enum.GetNames(typeof(T)).ToList())
      {
        string item = s;
        if (splitUppercase)
          item = StringUtility.InsertSpaceBeforeUpperCase(s);

        this.items.Add(item);
      }
    }
    public void InitFromList(IEnumerable<string> list)
    {
      this.items.Clear();
      this.items = list.ToList();
    }
  }

  [System.Serializable, Draw(ControlType.DropdownField), DataContract]
  public class BindableStringSelect : BindableBaseField<string>
  {
    [field: SerializeField]
    [Bind("Items"), IgnoreDataMember]
    public List<string> items { get; set; } = new List<string>();

    public float normalizedValue => (float)Index / items.Count;

    public int Index => items.IndexOf(value);

    public void InitFromEnum<T>(bool splitUppercase = true)
    {
      this.items.Clear();

      foreach (string s in System.Enum.GetNames(typeof(T)).ToList())
      {
        string item = s;
        if (splitUppercase)
          item = StringUtility.InsertSpaceBeforeUpperCase(s);

        this.items.Add(item);
      }
    }
    public void InitFromList(IEnumerable<string> list)
    {
      this.items.Clear();
      this.items = list.ToList();
    }
  }


  [System.Serializable, Draw(ControlType.TextField), DataContract]
  public class BindableInput : BindableBaseField<string>
  {
  }


  public static class StringUtility
  {
    public static string InsertSpaceBeforeUpperCase(this string str)
    {
      var sb = new StringBuilder();

      char previousChar = char.MinValue; // Unicode '\0'

      foreach (char c in str)
      {
        if (char.IsUpper(c))
        {
          // If not the first character and previous character is not a space, insert a space before uppercase

          if (sb.Length != 0 && previousChar != ' ')
          {
            sb.Append(' ');
          }
        }

        sb.Append(c);

        previousChar = c;
      }

      return sb.ToString();
    }
  }
}
