using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;
using Kinstrife.Core.ReflectionHelpers;

namespace Graphene
{
  internal static class RenderUtils
  {
    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    internal static bool IsPrimitiveContext(in object context) => context is string || context.GetType().IsPrimitive;

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
        else if (IsPrimitiveContext(member.Value))
          DrawFromPrimitiveContext(plate, container, in context, templates, member, bindableMembers);
        else if (member.Value is IEnumerable enumerable)
          DrawFromEnumerableContext(plate, container, in context, templates, member, bindableMembers);
        else
          DrawFromObjectContext(plate, container, in context, templates, member);
      }
    }

    internal static void DrawFromObjectContext(Plate panel, VisualElement container, in object context, TemplatePreset templates, ValueWithAttribute<DrawAttribute> drawMember)
    {
      ControlType? controlType = null;
      if (drawMember.Value is ICustomControlType customControl)
        controlType = customControl.ControlType;

      var template = templates.TryGetTemplateAsset(drawMember.Value, drawMember.Attribute, controlType);
      // Clone & bind the control
      VisualElement clone = Binder.Instantiate(in drawMember.Value, template, panel);

      // Add the control to the container
      container.Add(clone);
    }

    internal static void DrawFromPrimitiveContext(Plate panel, VisualElement container, in object context, TemplatePreset templates, ValueWithAttribute<DrawAttribute> drawMember, List<ValueWithAttribute<BindAttribute>> bindableMembers)
    {
      var bind = bindableMembers.Find(x => x.MemberInfo.Equals(drawMember.MemberInfo));

      var template = templates.TryGetTemplateAsset(drawMember.Value, drawMember.Attribute);
      // Clone & bind the control
      VisualElement clone = Binder.InstantiatePrimitive(in context, ref bind, template, panel);

      // Add the control to the container
      container.Add(clone);
    }

    internal static void DrawFromEnumerableContext(Plate plate, VisualElement container, in object context, TemplatePreset templates, ValueWithAttribute<DrawAttribute> drawMember, List<ValueWithAttribute<BindAttribute>> bindableMembers)
    {
      // Don't support primitives or string
      //if (typeof(T).IsPrimitive)
      //  return;

      // If element is listview -> use native functionality
      if(container is ListView listView && drawMember.Value is IList listContext)
      {
        var bind = bindableMembers.Find(x => x.MemberInfo.Equals(drawMember.MemberInfo));
        DrawListView(plate, listView, listContext, templates, in drawMember, bind);
        return;
      }

      foreach (var item in drawMember.Value as IEnumerable)
      {
        if (IsPrimitiveContext(in item))
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