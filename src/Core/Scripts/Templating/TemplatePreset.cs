
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

using Kinstrife.Core.ReflectionHelpers;

namespace Graphene
{
  public enum ControlType
  {
    None,
    Label,
    Button,
    Slider,
    SliderInt,
    Toggle,
    Foldout,
    ListView,
    ListItem,
    SelectField,
    CycleField,
    TextField,
    Title,
    SubTitle,
    Body,
    Border,
    DropdownField,
    Card,
    ButtonGroup,
    SubContext,
    MinMaxSlider,
    ProgressBar
  }

  public interface ICustomControlType
  {
    ControlType ControlType { get; }
  }

  public interface ICustomAddClasses
  {
    string ClassesToAdd { get; }
  }

  public interface ICustomName
  {
    string CustomName { get; }
  }

  public interface ISubContext
  {

  }

  [Serializable] public class ControlVisualTreeAssetMapping : SerializableDictionary<ControlType, VisualTreeAsset> { }


  [CreateAssetMenu(menuName = "Graphene/Templating/ComponentTemplates")]
  public class TemplatePreset : ScriptableObject
  {
    [SerializeField] TemplatePreset parent; public TemplatePreset Parent => parent;

    [SerializeField] ControlVisualTreeAssetMapping data = new ControlVisualTreeAssetMapping();

	public static ControlType ResolveControlType(object data, bool isPrimitiveContext, DrawAttribute drawAttribute = null)
	{
	  ControlType controlType = ControlType.None;
	  // Try get from attributes
	  // No member draw attribute -> try get ControlType from class attribute
	  if (!isPrimitiveContext && (drawAttribute == null || drawAttribute.controlType == ControlType.None))
	  {
		var info = TypeInfoCache.GetExtendedTypeInfo(data.GetType());
		if (info.HasTypeAttribute<DrawAttribute>())
		  drawAttribute = info.GetTypeAttribute<DrawAttribute>();
	  }
	  // Set ControlType from attribute (if present)
	  if (drawAttribute != null && drawAttribute.controlType != ControlType.None)
		controlType = drawAttribute.controlType;
	  // Didn't find in attribute
	  else
		controlType = GetControlTypeFromData(data, isPrimitiveContext);

      return controlType;
	}

	public static ControlType GetControlTypeFromData(object data, bool isPrimitiveContext)
    {
      if (data is bool)
        return ControlType.Toggle;
      else if (data is float)
        return ControlType.Slider;
      else if (data is int)
        return ControlType.SliderInt;
      else if (data is string)
        return ControlType.Label;
      else if (data is System.Action || data is UnityEvent)
        return ControlType.Button;
	  else if (data is Vector2)
		return ControlType.MinMaxSlider;
	  else if (data is IList)
        return ControlType.ListView;
      else if (data is Enum)
        return ControlType.DropdownField;
      else if (!isPrimitiveContext) // Use nested scope
        return ControlType.SubContext;
      return ControlType.None;
    }

    public bool TryGetTemplateAsset(ControlType controlType, out VisualTreeAsset visualTreeAsset)
    {
      if (data.TryGetValue(controlType, out visualTreeAsset))
        return true;
      else if (parent)
        return parent.TryGetTemplateAsset(controlType, out visualTreeAsset);
      else
        Debug.LogError($"Didn't find template for control {controlType}", this);

      return false;
    }
  }
}