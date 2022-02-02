﻿
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
    DropdownField
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

  [Serializable] public class ControlVisualTreeAssetMapping : SerializableDictionary<ControlType, VisualTreeAsset> { }


  [CreateAssetMenu(menuName = "Graphene/Templating/ComponentTemplates")]
  public class TemplatePreset : ScriptableObject
  {
    [SerializeField] TemplatePreset parent; public TemplatePreset Parent => parent;

    [SerializeField] ControlVisualTreeAssetMapping data = new ControlVisualTreeAssetMapping();


    public VisualTreeAsset TryGetTemplateAsset(object data, DrawAttribute drawAttribute = null, ControlType? overrideControlType = null)
    {
      ControlType controlType = ControlType.None;

      // Try get control type from method param
      if (overrideControlType.HasValue && overrideControlType != ControlType.None)
        controlType = overrideControlType.Value;

      // Try get from attributes
      else
      {
        // No member draw attribute -> try get ControlType from class attribute
        if ((drawAttribute == null || drawAttribute.controlType == ControlType.None) && !data.GetType().IsPrimitive && !(data is string))
        {
          var info = TypeInfoCache.GetExtendedTypeInfo(data.GetType());
          info.HasTypeAttribute<DrawAttribute>();
          drawAttribute = info.GetTypeAttribute<DrawAttribute>();
        }
        // Set ControlType from attribute (if present)
        if (drawAttribute != null)
          controlType = drawAttribute.controlType;
      }
      // Didn't find in attribute
      if (controlType == ControlType.None)
        controlType = GetControlTypeFromData(data);

      return TryGetTemplateAsset(controlType);
    }

    public static ControlType GetControlTypeFromData(object data)
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
      else if (data is IList<string>)
        return ControlType.ListView;
      return ControlType.None;
    }

    public VisualTreeAsset TryGetTemplateAsset(ControlType controlType)
    {
      if (data.TryGetValue(controlType, out var result))
        return result;
      else if (parent)
        return parent.TryGetTemplateAsset(controlType);
      else
        Debug.LogError($"Didn't find template for control {controlType}", this);

      return null;
    }
  }
}