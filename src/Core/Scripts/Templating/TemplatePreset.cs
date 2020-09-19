
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
    TextField  
  }

  public interface ICustomControlType
  {
    ControlType ControlType { get; }
  }

  [Serializable]
  public class ControlTemplateMapping : SerializableDictionary<ControlType, TemplateAsset> { }


  [CreateAssetMenu(menuName = "Graphene/Templating/ComponentTemplates")]
  public class TemplatePreset : ScriptableObject
  {
    [SerializeField] TemplatePreset parent; public TemplatePreset Parent => parent;

    [SerializeField] ControlTemplateMapping mapping = new ControlTemplateMapping();


    public TemplateAsset TryGetTemplateAsset(object data, DrawAttribute drawAttribute = null, ControlType? overrideControlType = null)
    {
      ControlType controlType = ControlType.None;

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

    public TemplateAsset TryGetTemplateAsset(ControlType controlType)
    {
      if (mapping.TryGetValue(controlType, out var result))
        return result;
      else if (parent)
        return parent.TryGetTemplateAsset(controlType);
      else
        Debug.LogError($"Didn't find template for control {controlType}", this);

      return null;
    }
  }
}