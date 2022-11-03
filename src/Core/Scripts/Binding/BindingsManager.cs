

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.UIElements;

namespace Graphene
{
  using Kinstrife.Core.ReflectionHelpers;

  public static class BindingManager
  {
    /// <summary>
    /// Mapping of all current bindings, keyed by panels
    /// </summary>
    static Dictionary<Plate, List<Binding>> bindings = new Dictionary<Plate, List<Binding>>();

    static Dictionary<Plate, List<Binding>> disposePostUpdate = new Dictionary<Plate, List<Binding>>();
    static Dictionary<Plate, List<Binding>> createPostUpdate = new Dictionary<Plate, List<Binding>>();

    internal static uint bindingsCount;

#if UNITY_EDITOR
    [UnityEditor.InitializeOnEnterPlayMode]
    public static void InitializeOnEnterPlayMode()
    {
      bindings = new Dictionary<Plate, List<Binding>>();
      disposePostUpdate = new Dictionary<Plate, List<Binding>>();
      createPostUpdate = new Dictionary<Plate, List<Binding>>();
      bindingsCount = 0;
    }
#endif

    public static void OnUpdate()
    {
      bindingsCount = 0;
      // Update the bindings for active/visible panels
      foreach (var kvp in bindings)
      {
        var plate = kvp.Key;
        // Was disposed
        if (!plate)
          continue;

        // The panel is invisible, or inactive
        if (!plate.IsActive)
          continue;

        if (plate.bindingRefreshMode == BindingRefreshMode.None || (plate.bindingRefreshMode == BindingRefreshMode.ModelChange && !plate.wasChangedThisFrame))
          continue;
        plate.wasChangedThisFrame = false;

        foreach (var binding in kvp.Value)
        {
          // Needs to be disposed
          if (binding.scheduleDispose)
          {
            ScheduleDispose(kvp.Key, binding);
            continue;
          }
          // Update the binding
          else
          {
            binding.Update();
            bindingsCount++;
          }
        }
      }

      // Create bindings
      foreach (var kvp in createPostUpdate)
      {
        var list = GetList(kvp.Key, bindings);
        foreach (var binding in kvp.Value)
          list.Add(binding);
      }

      // Dispose unused bindings
      foreach (var kvp in disposePostUpdate)
        foreach (var binding in kvp.Value)
          Destroy(kvp.Key, binding);


      createPostUpdate.Clear();
      disposePostUpdate.Clear();
    }

    internal static List<Binding> GetList(Plate panel, Dictionary<Plate, List<Binding>> bindings)
    {
      if (bindings.ContainsKey(panel))
        return bindings[panel];

      List<Binding> list = new List<Binding>();
      bindings.Add(panel, list);
      return list;
    }

    /// <summary>
    /// Creates a continuous binding between a TextElement and a member variable on a context (scope) and panel
    /// </summary>
    /// <param name="el"></param>
    /// <param name="context"></param>
    /// <param name="bindingPath"></param>
    /// <param name="panel"></param>
    public static void TryCreate(TextElement el, ref object context, in ValueWithAttribute<BindAttribute> member, Plate panel)
    {
      // Specifically set to one-time -> cancel binding
      if (member.Attribute.bindingMode.HasValue && member.Attribute.bindingMode.Value == BindingMode.OneTime)
        return;

      CreateBinding<string>(el, in context, in member, panel);
    }

    /// <summary>
    /// Creates a continuous binding between a BaseField<typeparamref name="TValueType"/> and a member variable on a context (scope) and panel
    /// </summary>
    /// <typeparam name="TValueType"></typeparam>
    /// <param name="el"></param>
    /// <param name="context"></param>
    /// <param name="member"></param>
    /// <param name="panel"></param>
    public static void TryCreate<TValueType>(BaseField<TValueType> el, in object context, in ValueWithAttribute<BindAttribute> member, Plate panel)
    {
      // Specifically set to one-time
      if (member.Attribute.bindingMode.HasValue && member.Attribute.bindingMode.Value != BindingMode.OneTime)
        return;

      CreateBinding<TValueType>(el, in context, in member, panel);
    }


    /// <summary>
    /// Creates a continuous binding between a TextElement and a member variable on a context (scope) and panel
    /// </summary>
    /// <param name="el"></param>
    /// <param name="context"></param>
    /// <param name="bindingPath"></param>
    /// <param name="panel"></param>
    public static void TryCreate<TValueType>(BindableElement el, in object context, in ValueWithAttribute<BindAttribute> member, Plate panel)
    {
      // Specifically set to one-time -> cancel binding
      if (member.Attribute.bindingMode.HasValue && member.Attribute.bindingMode.Value == BindingMode.OneTime)
        return;

      CreateBinding<TValueType>(el, in context, in member, panel);
    }

    internal static void CreateBinding<TValueType>(BindableElement el, in object context, in ValueWithAttribute<BindAttribute> member, Plate panel)
    {
      Binding binding = null;
      // Collection binding
      if (el is ListView && typeof(TValueType).IsAssignableFrom(typeof(ICollection)))
        binding = new CollectionBinding(el, in context, in member);
      // Single binding
      else
        binding = new MemberBinding<TValueType>(el, in context, in member);

      if (binding != null)
        GetList(panel, createPostUpdate).Add(binding);
    }

    public static void ScheduleDispose(Plate panel, Binding binding)
    {
      GetList(panel, disposePostUpdate).Add(binding);
    }

    internal static void Destroy(Plate panel, Binding binding)
    {
      GetList(panel, bindings).Remove(binding);
      binding.Dispose();
      binding = null;
    }

    public static void DisposePlate(Plate plate)
    {
      if (bindings.ContainsKey(plate))
        bindings.Remove(plate);
      if (disposePostUpdate.ContainsKey(plate))
        bindings.Remove(plate);
    }
  }

}