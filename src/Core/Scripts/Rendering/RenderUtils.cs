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

	/// <summary>
	/// Draws controls for all members of a context object
	/// </summary>
	/// <param name="plate"></param>
	/// <param name="container"></param>
	/// <param name="context"></param>
	/// <param name="templates"></param>
	internal static void DrawDataContainer(Plate plate, VisualElement container, in object context, TemplatePreset templates)
	{
	  if (!templates)
	  {
		UnityEngine.Debug.LogError($"Assign templates to Renderer for plate for {plate}", plate);
		return;
	  }
	  else if (container == null)
	  {
#if UNITY_ASSERTIONS
		UnityEngine.Debug.LogError($"Trying to draw to null VisualElement container {plate.name}", plate);
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

	internal static void DrawFromObjectContext(Plate panel, VisualElement container, in object context, TemplatePreset templates, ValueWithAttribute<DrawAttribute> drawMember)
	{
	  VisualTreeAsset template;

	  if (drawMember.Value is IHasCustomVisualTreeAsset customVisualTreeAsset && customVisualTreeAsset.VisualTreeAsset != null)
		template = customVisualTreeAsset.VisualTreeAsset;
	  else
	  {
		ControlType? controlType = null;
		if (drawMember.Value is ICustomControlType customControl)
		  controlType = customControl.ControlType;

		template = templates.TryGetTemplateAsset(drawMember.Value, drawMember.Attribute, controlType);

		if (!template)
		{
		  Debug.LogError($"Failed to instantiate template {controlType} for field {drawMember.MemberInfo.Name}", panel);
		  return;
		}
	  }

	  // Clone & bind the control
	  VisualElement clone = Binder.Instantiate(in drawMember.Value, template, panel);

	  // Needs optimization      
	  //foreach (var child in clone.Children())
	  //   {
	  //     if (drawMember.Value is ICustomAddClasses customAddClasses)
	  //       child.AddMultipleToClassList(customAddClasses.ClassesToAdd);
	  //     if (drawMember.Value is ICustomName customName && !string.IsNullOrWhiteSpace(customName.CustomName))
	  //       child.name = customName.CustomName;
	  //   }

	  // Add the control to the container
	  container.Add(clone);
	}

	internal static void DrawFromPrimitiveContext(Plate panel, VisualElement container, in object context, TemplatePreset templates, ValueWithAttribute<DrawAttribute> drawMember, List<ValueWithAttribute<BindAttribute>> bindableMembers)
	{
	  var bind = bindableMembers.Find(x => x.MemberInfo.Equals(drawMember.MemberInfo));
	  if (bind.MemberInfo == null)
		bind = new ValueWithAttribute<BindAttribute>(drawMember.Value, new BindAttribute("Label", BindingMode.OneTime), drawMember.MemberInfo);

	  // Get template, clone & bind the control
	  var template = templates.TryGetTemplateAsset(drawMember.Value, drawMember.Attribute);
	  VisualElement clone = Binder.InstantiatePrimitive(in context, ref bind, template, panel);

	  // Add any custom typography
	  if (drawMember.Attribute is DrawTextAttribute text && text.typography != Typography.None)
		clone.AddToClassList(Enum.GetName(typeof(Typography), text.typography));

	  // Add the control to the container
	  container.Add(clone);
	}

	internal static void DrawFromEnumerableContext(Plate plate, VisualElement container, in object context, TemplatePreset templates, ValueWithAttribute<DrawAttribute> drawMember, List<ValueWithAttribute<BindAttribute>> bindableMembers)
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

	internal static void DrawListView(Plate plate, ListView listView, in object context, TemplatePreset templates, in ValueWithAttribute<DrawAttribute> drawMember, in ValueWithAttribute<BindAttribute> bindMember)
	{
	  var template = templates.TryGetTemplateAsset(drawMember.Value, drawMember.Attribute);

	  Binder.BindListView(listView, in context, plate, template, in bindMember);
	}
  }
}