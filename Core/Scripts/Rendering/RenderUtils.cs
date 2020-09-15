using Kinstrife.Core.ReflectionHelpers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Graphene
{
  public static class RenderUtils
  {
    public static void Draw(Plate plate, VisualElement container, in object context, ComponentTemplates templates)
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

      foreach (var member in drawableMembers)
      {
        if (member.Value.GetType().IsPrimitive || member.Value is string)
          DrawFromPrimitiveContext(plate, container, in context, templates, member, bindableMembers);
        else if (member.Value is IEnumerable enumerable)
          DrawFromEnumerableContext(plate, container, in enumerable, templates, member);
        else
          DrawFromObjectContext(plate, container, in context, templates, member);
      }
    }

    internal static void DrawFromObjectContext(Plate panel, VisualElement container, in object context, ComponentTemplates templates, ValueWithAttribute<DrawAttribute> member)
    {
      var template = templates.TryGetTemplate(member.Value, member.Attribute);
      // Clone & bind the control
      VisualElement clone = Binder.Instantiate(in member.Value, template, panel);

      if (!string.IsNullOrEmpty(template.AddClass))
        clone.AddToClassList(template.AddClass);

      // Add the control to the container
      container.Add(clone);
    }

    internal static void DrawFromPrimitiveContext(Plate panel, VisualElement container, in object context, ComponentTemplates templates, ValueWithAttribute<DrawAttribute> member, List<ValueWithAttribute<BindAttribute>> bindableMembers)
    {
      var bind = bindableMembers.Find(x => x.MemberInfo.Equals(member.MemberInfo));// (BindAttribute)Attribute.GetCustomAttribute(member.MemberInfo, typeof(BindAttribute));

      var template = templates.TryGetTemplate(member.Value, member.Attribute);
      // Clone & bind the control
      VisualElement clone = Binder.InstantiatePrimitive(in context, ref bind, template, panel);

      // Add the control to the container
      container.Add(clone);
    }

    internal static void DrawFromEnumerableContext(Plate panel, VisualElement container, in IEnumerable context, ComponentTemplates templates, ValueWithAttribute<DrawAttribute> member)
    {
      // Don't support primitives or string
      //if (typeof(T).IsPrimitive)
      //  return;

      foreach (var item in context)
      {
        // Fugly, but works (for now)
        if (item.GetType().IsPrimitive || item is string)
        {
          var bind = new ValueWithAttribute<BindAttribute>(item, new BindAttribute("Label", BindingMode.OneTime), member.MemberInfo);
          DrawFromPrimitiveContext(panel, container, in item, templates, new ValueWithAttribute<DrawAttribute>(item, member.Attribute, member.MemberInfo), new List<ValueWithAttribute<BindAttribute>> { bind });
        }
        else
        {
          var template = templates.TryGetTemplate(item, member.Attribute);
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