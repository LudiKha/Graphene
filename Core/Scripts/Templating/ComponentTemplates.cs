
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

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


  [CreateAssetMenu(menuName = "Graphene/Template/ComponentTemplates")]
  public class ComponentTemplates : SerializedScriptableObject
  {
    [SerializeField] ComponentTemplates parent; public ComponentTemplates Parent => parent;

    [SerializeField] Dictionary<ControlType, Template> mapping = new Dictionary<ControlType, Template>();
    public IReadOnlyDictionary<ControlType, Template> Mapping => mapping;

    // From generic object
    public Template TryGetTemplate(object data, ControlType controlType)
    {
      return GetTemplateRecursive(controlType);
    }

    public Template TryGetTemplate(object data, DrawAttribute drawAttribute = null)
    {
      ControlType controlType = ControlType.None;

      // Get control type from class attribute
      if (drawAttribute == null || drawAttribute.controlType == ControlType.None)
        drawAttribute = Attribute.GetCustomAttribute(data.GetType(), typeof(DrawAttribute)) as DrawAttribute;
      if (drawAttribute != null)
        controlType = drawAttribute.controlType;

      return GetTemplateRecursive(controlType);
    }


    public Template TryGetTemplateForField<T>(T data)
    {
      ControlType controlType = ControlType.None;
      if (data is float)
        controlType = ControlType.Slider;
      else if (data is bool)
        controlType = ControlType.Toggle;
      else if (data is System.Action || data is UnityEngine.Events.UnityEvent)
        controlType = ControlType.Button;
      else if (data is string)
        controlType = ControlType.Label;


      return GetTemplateRecursive(controlType);
    }

    internal Template GetTemplateRecursive(ControlType controlType)
    {
      if (mapping.TryGetValue(controlType, out var result))
        return result;
      else if (parent)
        return parent.GetTemplateRecursive(controlType);
      else
        Debug.LogError($"Didn't find template for control {controlType}", this);

      return null;
    }
  }
}