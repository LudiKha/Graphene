using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace Graphene.ViewModel
{
  [System.Serializable]
  [DataContract]
  public abstract class BindableBaseField : BindableObjectBase
  {
	[field: SerializeField]
	[Bind(nameof(Label), BindingMode.OneWay)]
	[IgnoreDataMember]
	public virtual string Label { get; set; }
  }

  [System.Serializable]
  [DataContract]
  public abstract class BindableBaseField<T> : BindableBaseField, INotifyValueChanged<T>, INotifyPropertyChanged
  {
	[SerializeField]
	protected T m_Value;

	public BindableBaseField() { }

	public BindableBaseField([CallerMemberName] string label = "Label") : this()
	{
	  this.Label = label;
	}

	[BindBaseField("Value")]
	[DataMember(Name = "Value")]
	public virtual T value
	{
	  get => m_Value; set
	  {
		SetValueWithoutNotify(value);
		ValueChangeCallback(m_Value);
	  }
	}


	/*[BindValueChangeCallback(nameof(ValueChange))]*/
	[IgnoreDataMember]
	public EventCallback<ChangeEvent<T>> ValueChange => (changeEvent) => { ValueChangeCallback(changeEvent.newValue); };

	public event System.EventHandler<T> OnValueChange;
	public event PropertyChangedEventHandler PropertyChanged;

	public virtual void SetValueWithoutNotify(T newValue)
	{
	  m_Value = newValue;
	}

#if UNITY_EDITOR
	PropertyChangedEventArgs propertyChangedArgs;
#endif

	protected virtual void ValueChangeCallback(T value)
	{
	  OnValueChange?.Invoke(this, value);
#if UNITY_EDITOR
	  if (propertyChangedArgs == null)
		propertyChangedArgs = new PropertyChangedEventArgs(Label);
	  PropertyChanged?.Invoke(this, propertyChangedArgs);
#else
	  PropertyChanged?.Invoke(this, null);
#endif
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
	public BindableBool() : base() { }
  }



  [System.Serializable, Draw(ControlType.Slider), DataContract]
  public abstract class RangeBaseField<TValueType> : BindableBaseField<TValueType>
  {
	public abstract float normalizedValue { get; }

	// These come first, so the ranges are deserialized _before_ the value
#if ODIN_INSPECTOR
	[PropertyOrder(1)]
#endif
	[Bind("Min"), DataMember(Name = "Min")]
	public TValueType min;
#if ODIN_INSPECTOR
	[PropertyOrder(1)]
#endif
	[Bind("Max"), DataMember(Name = "Max")]
	public TValueType max;

	[BindBaseField("Value")]
	[DataMember(Name = "Value")]
#if ODIN_INSPECTOR
	[Sirenix.OdinInspector.ShowInInspector, Sirenix.OdinInspector.PropertyRange(0, 1, MinGetter = nameof(min), MaxGetter = nameof(max))]
#endif
	public override TValueType value { get => base.value; set => base.value = value; }

	public TValueType Min { get => min; set => min = value; }
	public TValueType Max { get => max; set => max = value; }

	public RangeBaseField() : base() { }

	protected float Normalize(float value, float min, float max)
	{
	  return Mathf.InverseLerp(min, max, value);
	}
  }

  public interface INumericBindable
  {
	float normalizedValue { get; }
	float value { get; set; }

	float Min { get; set; }
	float Max { get; set; }

	void SetValueWithoutNotify(float value);
  }

  [System.Serializable, Draw(ControlType.Slider), DataContract]
  public class BindableFloat : RangeBaseField<float>, INumericBindable
  {
#if ODIN_INSPECTOR
	[Sirenix.OdinInspector.ShowInInspector, Sirenix.OdinInspector.PropertyRange(0, 1)]
#endif
	public override float normalizedValue => Normalize(m_Value, min, max);

	public BindableFloat() : base()
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
  public class BindableInt : RangeBaseField<int>, INumericBindable
  {
#if ODIN_INSPECTOR
	[Sirenix.OdinInspector.ShowInInspector, Sirenix.OdinInspector.PropertyRange(0, 1)]
#endif
	public override float normalizedValue => Normalize(m_Value, min, max);

	float INumericBindable.value { get => value; set => base.value = (int)value; }
	float INumericBindable.Min { get => min; set => min = (int)value; }
	float INumericBindable.Max { get => max; set => max = (int)value; }

	public BindableInt() : base()
	{
	  min = 0;
	  max = 100;
	  m_Value = 50;
	  //m_Value = 5;
	}

	public override void SetValueWithoutNotify(int newValue)
	{
	  m_Value = Mathf.Clamp(newValue, min, max);
	}

	void INumericBindable.SetValueWithoutNotify(float value) => SetValueWithoutNotify((int)value);
  }

  [System.Serializable, Draw(ControlType.CycleField), DataContract]
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
		item = item.Replace("_", " ").Trim();

		this.items.Add(item);
	  }
	}
	public void InitFromList(IEnumerable<string> list)
	{
	  this.items.Clear();
	  this.items = list.ToList();
	}

	public BindableNamedInt() : base() { }

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

	public BindableStringSelect() : base() { }
  }


  [System.Serializable, Draw(ControlType.TextField), DataContract]
  public class BindableInput : BindableBaseField<string>
  {
	public BindableInput() : base() { }
  }


  [System.Serializable, Draw(ControlType.MinMaxSlider), DataContract]
  public class BindableRange : BindableBaseField<Vector2>
  {
	[BindBaseField("Value")]
	[DataMember(Name = "Value")]
#if ODIN_INSPECTOR
	[Sirenix.OdinInspector.ShowInInspector, Sirenix.OdinInspector.MinMaxSlider(0, 1, MinValueGetter = nameof(min), MaxValueGetter = nameof(max))]
#endif
	public override Vector2 value { get => base.value; set => base.value = value; }

	[Bind("Min"), DataMember(Name = "Min")]
	public float min;
	[Bind("Max"), DataMember(Name = "Max")]
	public float max;

	public float Min { get => min; set => min = value; }
	public float Max { get => max; set => max = value; }

	public BindableRange() : base()
	{
	  min = 0;
	  max = 1;
	  m_Value = new Vector2(0, 1);
	}

	protected float Normalize(float value, float min, float max)
	{
	  return Mathf.InverseLerp(min, max, value);
	}
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
