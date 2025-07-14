using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Graphene
{
  public class UIAttribute : System.Attribute
  {

  }

  public enum BindingMode
  {
    /// <summary>
    /// For immutable data. Use this for things like menu (buttons/labels) and static text.
    /// </summary>
    OneTime,
    /// <summary>
    /// For dynamic data. Use this for mutable data that isn't changable via UI.
    /// </summary>
    OneWay,
    /// <summary>
    /// For dynamic data which is changable through the UI.
    /// </summary>
    TwoWay
  }

  [System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Property)]
  public class BindAttribute : UIAttribute
  {
    string path = ""; public string Path => path;
    /// <summary>
    /// The binding mode for this binding. When left null, the system will pick a binding mode based on the control type (recommended).
    /// </summary>
    public BindingMode? bindingMode = null; // By default don't override binding mode

    public bool hideIfEmpty;

    public BindAttribute()
    {
    }

    public BindAttribute(string path)
    {
      this.path = path;
    }
    public BindAttribute(BindingMode bindingMode)
    {
      this.bindingMode = bindingMode;
    }
    public BindAttribute(string path, BindingMode bindingMode)
    {
      this.path = path;
      this.bindingMode = bindingMode;
	}
	public BindAttribute(string path, BindingMode bindingMode, bool hideIfEmpty)
	{
	  this.path = path;
	  this.bindingMode = bindingMode;
      this.hideIfEmpty = hideIfEmpty;
	}
  }

  [System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Property)]
  public class BindBaseFieldAttribute : BindAttribute
  {
    public string label;
    public bool showInputField;

    public BindBaseFieldAttribute(string path, string label = null, bool showInput = false) : base(path)
    {
      this.label = label;
      this.showInputField = showInput;
    }

    public BindBaseFieldAttribute(string path, BindingMode bindingMode, string label = null, bool showInput = false) : base(path, bindingMode)
    {
      this.label = label;
      this.showInputField = showInput;
    }
  }

  // For fields & properties
  [System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Property)]
  public class BindFloatAttribute : BindBaseFieldAttribute
  {
    public float startingValue;
    public float lowValue;
    public float highValue;

    public BindFloatAttribute(string path, float startingValue, float min, float max, string label = null, bool showInput = false) : base(path, label, showInput)
    {
      this.startingValue = startingValue;
      this.lowValue = min;
      this.highValue = max;
    }
    public BindFloatAttribute(string path, BindingMode bindingMode, float startingValue, float min, float max, string label = null, bool showInput = false) : base(path, bindingMode, label, showInput)
    {
      this.startingValue = startingValue;
      this.lowValue = min;
      this.highValue = max;
    }
  }


  // For fields & properties
  [System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Property)]
  public class BindIntAttribute : BindBaseFieldAttribute
  {
    public int startingValue;
    public int lowValue;
    public int highValue;

    public BindIntAttribute(string path, int startingValue, int min, int max, string label = null, bool showInput = false) : base(path, label, showInput)
    {
      this.startingValue = startingValue;
      this.lowValue = min;
      this.highValue = max;
    }
    public BindIntAttribute(string path, BindingMode bindingMode, int startingValue, int min, int max, string label = null, bool showInput = false) : base(path, bindingMode, label, showInput)
    {
      this.startingValue = startingValue;
      this.lowValue = min;
      this.highValue = max;
    }
  }
  // For fields & properties
  [System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Property)]
  public class BindRangeAttribute : BindBaseFieldAttribute
  {
	public Vector2 startingValue;
	public float lowLimit;
	public float highLimit;

	public BindRangeAttribute(string path, Vector2 startingValue, float min, float max, string label = null) : base(path, label, showInput:false)
	{
	  this.startingValue = startingValue;
	  this.lowLimit = min;
	  this.highLimit = max;
	}
	public BindRangeAttribute(string path, BindingMode bindingMode, Vector2 startingValue, float min, float max, string label = null) : base(path, bindingMode, label, showInput: false)
	{
	  this.startingValue = startingValue;
	  this.lowLimit = min;
	  this.highLimit = max;
	}
  }

  // For fields & properties
  [System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Property)]
  public class BindStringAttribute : BindBaseFieldAttribute
  {
    public readonly string startingValue;
    public readonly int maxLength;
    public readonly bool readOnly;
    public readonly bool multiLine;
    public readonly bool password;

    public BindStringAttribute(string path, string startingValue, int maxLength = -1, bool readOnly = false, bool multiLine = false, bool password = false, string label = null, bool showInput = false) : base(path, label, showInput)
    {
      this.startingValue = startingValue;
      this.maxLength = maxLength;
      this.readOnly = readOnly;
      this.multiLine = multiLine;
    }
    public BindStringAttribute(string path, BindingMode bindingMode, string startingValue, int maxLength = -1, bool readOnly = false, bool multiLine = false, bool password = false, string label = null, bool showInput = false) : base(path, bindingMode, label, showInput)
    {
      this.startingValue = startingValue;
      this.maxLength = maxLength;
      this.readOnly = readOnly;
      this.multiLine = multiLine;
    }
  }

  [System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Property)]
  public class BindTooltip : BindAttribute
  {
    public BindTooltip(string path) : base(path)
    {
    }
  }

  [System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Property)]
  public class BindValueChangeCallbackAttribute : BindAttribute
  {
    public BindValueChangeCallbackAttribute(string path) : base(path)
    {
    }
  }


  /// <summary>
  /// Marks a field or property to be drawn when an entire type is marked for rendering
  /// </summary>
  [System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Property | System.AttributeTargets.Class)]
  public class DrawAttribute : System.Attribute
  {
    public ControlType controlType = ControlType.None;

    public int order;

    //public DrawAttribute(ControlType controlType)
    //{
    //  this.controlType = controlType;
    //}

    public DrawAttribute([CallerLineNumber]int order = 0)
    {
      this.order = order;
    }

    public DrawAttribute(ControlType controlType, [CallerLineNumber] int order = 0)
    {
      this.controlType = controlType;
      this.order = order;
    }
  }

  public enum Typography
  {
    None,
    h1,
    h2,
    h3,
    h4,
    h5,
    h6,
    subtitle1,
    subtitle2,
    body1,
    body2,
    caption,
    overline
  }

  /// <summary>
  /// Marks a field or property to be drawn when an entire type is marked for rendering
  /// </summary>
  [System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Property | System.AttributeTargets.Class)]
  public class DrawTextAttribute : DrawAttribute
  {
    public Typography typography = Typography.None;

    public DrawTextAttribute([CallerLineNumber] int order = 0) : base(order)
    {
      this.order = order;
    }

    public DrawTextAttribute(Typography typography, [CallerLineNumber] int order = 0) : base(ControlType.Label, order)
    {
      this.typography = typography;
    }
  }

  [System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Property)]
  public class RouteAttribute : UIAttribute
  {
    public RouteAttribute()
    {
    }
  }
}