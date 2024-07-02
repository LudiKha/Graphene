using Kinstrife.Core.ReflectionHelpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Graphene
{
  internal static class RenderUtils
  {
	internal static TemplatePreset templatesDefault; // Hax

	internal readonly static System.Type stringType = typeof(string);

	/// <summary>
	/// 
	/// </summary>
	/// <param name="context"></param>
	/// <returns></returns>
	internal static bool IsPrimitiveContext(this System.Type type) => type.IsPrimitive || type.IsEnum || type == stringType;

	static int recursiveCheck = 0;
	/// <summary>
	/// Draws controls for all members of a context object
	/// </summary>
	/// <param name="plate"></param>
	/// <param name="container"></param>
	/// <param name="context"></param>
	/// <param name="templates"></param>
	internal static void DrawDataContainer(Plate plate, VisualElement container, in object context, TemplatePreset templates)
	{
	  if (recursiveCheck > 5)
	  {
		Debug.LogError($"Recursive error {plate}", plate);
		return;
	  }
	  if (context is ICustomDrawContext customDrawContext)
	  {
		recursiveCheck++;
		DrawDataContainer(plate, container, customDrawContext.GetCustomDrawContext, templates);
		recursiveCheck--;
		return;
	  }

	  if (!templates)
	  {
		UnityEngine.Debug.LogError($"Assign templates to Renderer for plate for {plate}", plate);
		return;
	  }
	  else if (container == null)
	  {
#if UNITY_ASSERTIONS
		UnityEngine.Debug.LogWarning($"Trying to draw to null VisualElement container {plate.name}", plate);
#endif
		return;
	  }
	  else if (context == null)
	  {
#if UNITY_ASSERTIONS && false
        UnityEngine.Debug.LogError("Trying to draw null context", plate);
#endif
		return;
	  }
	  templatesDefault = templates;

	  // Get members
	  List<ValueWithAttribute<DrawAttribute>> drawableMembers = new List<ValueWithAttribute<DrawAttribute>>();
	  TypeInfoCache.GetMemberValuesWithAttribute(context, drawableMembers);

	  // Sort draw order
	  drawableMembers = drawableMembers.OrderBy(x => x.Attribute.order).ToList();// as List<ValueWithAttribute<DrawAttribute>>;

	  List<ValueWithAttribute<BindAttribute>> bindableMembers = new List<ValueWithAttribute<BindAttribute>>();
	  TypeInfoCache.GetMemberValuesWithAttribute(context, bindableMembers);

	  // Check how each member should be drawn
	  foreach (var member in drawableMembers)
	  {
		if (member.Value is null)
		  continue;
		else if (member.Type.IsPrimitiveContext())
		  DrawFromPrimitiveContext(plate, container, in context, templates, member, bindableMembers);
		else if (member.Value is IEnumerable enumerable)
		  DrawFromEnumerableContext(plate, container, in context, templates, member, bindableMembers);
		else
		  DrawFromObjectContext(plate, container, in context, templates, member);
	  }
	}

	static void DrawFromObjectContext(Plate panel, VisualElement container, in object context, TemplatePreset templates, ValueWithAttribute<DrawAttribute> drawMember)
	{
	  VisualTreeAsset template;

	  if (drawMember.Value is IHasCustomVisualTreeAsset customVisualTreeAsset && customVisualTreeAsset.VisualTreeAsset != null)
		template = customVisualTreeAsset.VisualTreeAsset;
	  else
	  {
		ControlType controlType = ControlType.None;

		// Override control for class
		if (drawMember.Value is ICustomControlType customControl && customControl.ControlType != ControlType.None)
		  controlType = customControl.ControlType;
		else
		  controlType = TemplatePreset.ResolveControlType(drawMember.Value, isPrimitiveContext: false, drawMember.Attribute);


		// Drill down to subcontext
		if (controlType == ControlType.None || controlType == ControlType.SubContext)
		{
		  //Debug.Log($"{controlType} {panel.name} {drawMember} {context}", panel);
		  recursiveCheck++;
		  DrawDataContainer(panel, container, drawMember.Value, templates);
		  recursiveCheck--;
		  return;
		}

		if (!templates.TryGetTemplateAsset(controlType, out template))
		{
		  Debug.LogError($"Failed to instantiate template {controlType} for field {drawMember.MemberInfo.Name}", panel);
		  return;
		}
	  }

	  // Clone & bind the control
	  VisualElement clone = Binder.Instantiate(in drawMember.Value, template, panel);

	  // Add the control to the container
	  container.Add(clone);
	}

	static void DrawFromPrimitiveContext(Plate panel, VisualElement container, in object context, TemplatePreset templates, ValueWithAttribute<DrawAttribute> drawMember, List<ValueWithAttribute<BindAttribute>> bindableMembers)
	{
	  var bind = bindableMembers.Find(x => x.MemberInfo.Equals(drawMember.MemberInfo));
	  if (bind.MemberInfo == null)
		bind = new ValueWithAttribute<BindAttribute>(drawMember.Value, new BindAttribute("Label", BindingMode.OneTime), drawMember.MemberInfo);

	  // Get template, clone & bind the control
	  ControlType controlType = TemplatePreset.ResolveControlType(drawMember.Value, isPrimitiveContext: true, drawMember.Attribute);
	  templates.TryGetTemplateAsset(controlType, out VisualTreeAsset template);
	  VisualElement clone = Binder.InstantiatePrimitive(in context, ref bind, template, panel);

	  // Add any custom typography
	  if (drawMember.Attribute is DrawTextAttribute text && text.typography != Typography.None)
		clone.AddToClassList(Enum.GetName(typeof(Typography), text.typography));

	  // Add the control to the container
	  container.Add(clone);
	}

	static void DrawFromEnumerableContext(Plate plate, VisualElement container, in object context, TemplatePreset templates, ValueWithAttribute<DrawAttribute> drawMember, List<ValueWithAttribute<BindAttribute>> bindableMembers)
	{
	  // Don't support primitives or string
	  //if (typeof(T).IsPrimitive)
	  //  return;

	  // If element is listview -> use native functionality
	  if (container is ListView listView && drawMember.Value is IList listContext)
	  {
		var bind = bindableMembers.Find(x => x.MemberInfo.Equals(drawMember.MemberInfo));
		DrawListView(plate, listView, listContext, templates, in drawMember, bind);
		return;
	  }

	  foreach (var item in drawMember.Value as IEnumerable)
	  {
		if (IsPrimitiveContext(item.GetType()))
		{
		  // Fugly, but works (for now)
		  var bind = new ValueWithAttribute<BindAttribute>(item, new BindAttribute("Label", BindingMode.OneTime), drawMember.MemberInfo);
		  DrawFromPrimitiveContext(plate, container, in item, templates, new ValueWithAttribute<DrawAttribute>(item, drawMember.Attribute, drawMember.MemberInfo), new List<ValueWithAttribute<BindAttribute>> { bind });
		}
		else
		{
		  var draw = new ValueWithAttribute<DrawAttribute>(item, drawMember.Attribute, drawMember.MemberInfo);
		  DrawFromObjectContext(plate, container, in item, templates, draw);

		  //var template = templates.TryGetTemplateAsset(item, drawMember.Attribute);
		  //// Clone & bind the control
		  //VisualElement clone = Binder.Instantiate(in item, template, panel);
		  //// Add the control to the container
		  //container.Add(clone);
		}
	  }
	}

	static void DrawListView(Plate plate, ListView listView, in object context, TemplatePreset templates, in ValueWithAttribute<DrawAttribute> drawMember, in ValueWithAttribute<BindAttribute> bindMember)
	{
	  ControlType controlType = TemplatePreset.ResolveControlType(drawMember.Value, isPrimitiveContext: false, drawMember.Attribute);
	  templates.TryGetTemplateAsset(controlType, out VisualTreeAsset template);
	  Binder.BindListView(listView, in context, plate, template, in bindMember);
	}
  }
}