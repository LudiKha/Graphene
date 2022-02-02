using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

namespace Graphene.ViewModel
{
  [System.Serializable][DataContract]
  public class BindableBaseField<T>: INotifyValueChanged<T>
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
    [Bind("Label", BindingMode.OneWay)][IgnoreDataMember]
    public virtual string Label { get; set; }

    [BindValueChangeCallback("ValueChange")][IgnoreDataMember]
    public EventCallback<ChangeEvent<T>> ValueChange => (changeEvent) => { ValueChangeCallback(changeEvent.newValue); };

    public event System.EventHandler<T> OnValueChange;

    public virtual void SetValueWithoutNotify(T newValue)
    {
      m_Value = newValue;
    }

    protected virtual void ValueChangeCallback(T value)
    {
      OnValueChange?.Invoke(this, value);
    }

    public void ResetCallbacks() => OnValueChange = null;
  }



  [System.Serializable, Draw(ControlType.Toggle), DataContract]
  public class BindableBool : BindableBaseField<bool>
  {
  }



  [System.Serializable, Draw(ControlType.Slider)]
  public abstract class RangeBaseField<TValueType> : BindableBaseField<TValueType>
  {
    public abstract float normalizedValue { get; }

    [Bind("Min"), DataMember(Name = "Min")]
    public TValueType min;
    [Bind("Max"), DataMember(Name = "Max")]
    public TValueType max;
  }

  [System.Serializable, Draw(ControlType.Slider), DataContract]
  public class BindableFloat : RangeBaseField<float>
  {
    public override float normalizedValue => m_Value / (max - min);

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
    public override float normalizedValue => (float)m_Value / ((float)max - (float)min);

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

  static class StringUtility
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
