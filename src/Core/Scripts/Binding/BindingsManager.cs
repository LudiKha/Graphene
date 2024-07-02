

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.UIElements;

namespace Graphene
{
  using global::Graphene.Elements;
  using Kinstrife.Core.ReflectionHelpers;
  using UnityEngine;
  using UnityEngine.Profiling;

  public class BindingsManager : GrapheneComponent
  {
	#region ShowInInspectorAttribute
#if ODIN_INSPECTOR
	[Sirenix.OdinInspector.ShowInInspector]
#elif NAUGHTY_ATTRIBUTES
    [NaughtyAttributes.ShowInInspector]
#endif
	#endregion
	/// <summary>
	/// Mapping of all current bindings, keyed by panels
	/// </summary>
	Dictionary<Plate, List<Binding>> bindings = new Dictionary<Plate, List<Binding>>();

	#region ShowInInspectorAttribute
#if ODIN_INSPECTOR
	[Sirenix.OdinInspector.ShowInInspector]
#elif NAUGHTY_ATTRIBUTES
    [NaughtyAttributes.ShowInInspector]
#endif
	#endregion
	Dictionary<Plate, List<Binding>> disposePostUpdate = new Dictionary<Plate, List<Binding>>();
    Dictionary<Plate, List<Binding>> createPostUpdate = new Dictionary<Plate, List<Binding>>();

    internal uint bindingsCount;


#if ODIN_INSPECTOR
	[Sirenix.OdinInspector.InfoBox("$bindingsInfo")]
#elif NAUGHTY_ATTRIBUTES
    [NaughtyAttributes.InfoBox("bindingsInfo")]
#endif
	[SerializeField] float bindingRefreshRate = 0.2f;


#if UNITY_EDITOR
	public string bindingsInfo => $"{bindingsCount} bindings";
#endif

	#region ReadOnlyAttribute
#if ODIN_INSPECTOR
	[Sirenix.OdinInspector.ReadOnly, Sirenix.OdinInspector.ShowInInspector]
#elif NAUGHTY_ATTRIBUTES
    [NaughtyAttributes.ReadOnly, NaughtyAttributes.ShowInInspector]
#endif
	#endregion
	float lastRefreshTime;

	//#if UNITY_EDITOR
	//    [UnityEditor.InitializeOnEnterPlayMode]
	//    public static void InitializeOnEnterPlayMode()
	//    {
	//      bindings = new Dictionary<Plate, List<Binding>>();
	//      disposePostUpdate = new Dictionary<Plate, List<Binding>>();
	//      createPostUpdate = new Dictionary<Plate, List<Binding>>();
	//    }
	//#endif


	void LateUpdate()
	{
#if UNITY_EDITOR
      if (!Application.isPlaying && !runInEditMode)
		return;
#endif

      if (Time.unscaledTime - lastRefreshTime < bindingRefreshRate)
		return;

	  if (!graphene || !graphene.IsActiveAndVisible)
		return;

	  OnUpdate();
	  lastRefreshTime = Time.unscaledTime;
	}
	public void OnUpdate()
	{
      bindingsCount = 0;
#if UNITY_ASSERTIONS
	  Profiler.BeginSample("Update Bindings", this);
#endif
	  // Update the bindings for active/visible panels
	  foreach (var kvp in bindings)
     {
		var plate = kvp.Key;
        // Was disposed
        if (!plate)
          continue;
		// The panel is invisible, or inactive
		if (!plate.IsActive || !plate.Graphene.IsActiveAndVisible)
          continue;

        if (plate.bindingRefreshMode == BindingRefreshMode.None || (plate.bindingRefreshMode == BindingRefreshMode.ModelChange && !plate.wasChangedThisFrame))
          continue;
        plate.wasChangedThisFrame = false;

#if UNITY_ASSERTIONS
		Profiler.BeginSample(plate.DebugName, plate);
#endif
		foreach (var binding in kvp.Value)
        {
          // Needs to be disposed
          if (binding.scheduleDispose)
          {
            ScheduleDispose(kvp.Key, binding);
          }
          // Update the binding
          else
          {
            binding.Update();
            bindingsCount++;
          }
        }
#if UNITY_ASSERTIONS
        Profiler.EndSample();
#endif
	  }
#if UNITY_ASSERTIONS
	  Profiler.EndSample();
#endif

#if UNITY_ASSERTIONS
	  Profiler.BeginSample("CreateDispose");
#endif
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

#if UNITY_ASSERTIONS
	  Profiler.EndSample();
#endif
	}

	List<Binding> GetList(Plate panel, Dictionary<Plate, List<Binding>> bindings)
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
    public void TryCreate(TextElement el, ref object context, in ValueWithAttribute<BindAttribute> member, Plate panel)
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
    public void TryCreate<TValueType>(BaseField<TValueType> el, in object context, in ValueWithAttribute<BindAttribute> member, Plate panel)
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
    public void TryCreate<TValueType>(BindableElement el, in object context, in ValueWithAttribute<BindAttribute> member, Plate panel)
    {
      // Specifically set to one-time -> cancel binding
      if (member.Attribute.bindingMode.HasValue && member.Attribute.bindingMode.Value == BindingMode.OneTime)
        return;

      CreateBinding<TValueType>(el, in context, in member, panel);
    }

    internal void CreateBinding<TValueType>(BindableElement el, in object context, in ValueWithAttribute<BindAttribute> member, Plate panel)
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

    void ScheduleDispose(Plate panel, Binding binding)
    {
      GetList(panel, disposePostUpdate).Add(binding);
    }

    void Destroy(Plate panel, Binding binding)
    {
      GetList(panel, bindings).Remove(binding);
      binding.Dispose();
      binding = null;
    }

    internal void DisposePlate(Plate plate, bool isDestroyed)
    {
      if(bindings.TryGetValue(plate, out var list))
      {
        if (isDestroyed)
          bindings.Remove(plate);
        else
          list.Clear();
	  }
      if (disposePostUpdate.ContainsKey(plate))
		disposePostUpdate.Remove(plate);
    }
  }

}