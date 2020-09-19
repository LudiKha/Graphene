using Kinstrife.Core.ReflectionHelpers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;

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
          DrawFromEnumerableContext(plate, container, in enumerable, templates, member);
        else
          DrawFromObjectContext(plate, container, in context, templates, member);
      }
    }

    internal static void DrawFromObjectContext(Plate panel, VisualElement container, in object context, TemplatePreset templates, ValueWithAttribute<DrawAttribute> drawMember)
    {


      var template = templates.TryGetTemplateAsset(drawMember.Value, drawMember.Attribute);
      // Clone & bind the control
      VisualElement clone = Binder.Instantiate(in drawMember.Value, template, panel);

      if (!string.IsNullOrEmpty(template.AddClass))
        clone.AddToClassList(template.AddClass);

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

    internal static void DrawFromEnumerableContext(Plate panel, VisualElement container, in IEnumerable context, TemplatePreset templates, ValueWithAttribute<DrawAttribute> drawMember)
    {
      // Don't support primitives or string
      //if (typeof(T).IsPrimitive)
      //  return;

      foreach (var item in context)
      {
        if (IsPrimitiveContext(in item))
        {
          // Fugly, but works (for now)
          var bind = new ValueWithAttribute<BindAttribute>(item, new BindAttribute("Label", BindingMode.OneTime), drawMember.MemberInfo);
          DrawFromPrimitiveContext(panel, container, in item, templates, new ValueWithAttribute<DrawAttribute>(item, drawMember.Attribute, drawMember.MemberInfo), new List<ValueWithAttribute<BindAttribute>> { bind });
        }
        else
        {
          var template = templates.TryGetTemplateAsset(item, drawMember.Attribute);
          // Clone & bind the control
          VisualElement clone = Binder.Instantiate(in item, template, panel);
          // Add the control to the container
          container.Add(clone);
        }
        //DrawFromObjectContext(panel, container, in item, templates, member);
      }
    }
  }
}