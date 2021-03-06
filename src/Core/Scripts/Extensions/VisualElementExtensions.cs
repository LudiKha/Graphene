﻿using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Graphene
{
  public static class VisualElementExtensions
  {
    internal const string documentRootUssClassName = "unity-ui-document__root";
    internal const string documentChildUssClassName = "unity-ui-document__child";
    internal const string hiddenUssClassName = "hidden";

    public static void Show(this VisualElement el)
    {
      el.RemoveFromClassList(hiddenUssClassName);

      //el.style.display = DisplayStyle.Flex;
    }
    public static void Hide(this VisualElement el)
    {
      el.AddToClassList(hiddenUssClassName);

      //el.style.display = DisplayStyle.None;
    }
    public static VisualElement GetRootRecursively(this VisualElement el)
    {
      if (el.parent == null)
      {
        return el;
        foreach (var child in el.Children())
        {
          if (child.style.display != DisplayStyle.None && child.ClassListContains(documentRootUssClassName))
            return child;
        }
        return null;// el.Q(null, "unity-ui-document__root");
      }

      return el.parent.GetRootRecursively();
    }


    public static VisualElement TopRoot(this VisualElement el)
    {
      return el.panel?.visualTree.Q(null, documentRootUssClassName);
    }
    public static VisualElement TopRoot(this IPanel panel)
    {
      return panel?.visualTree.Query(null, documentRootUssClassName).Last();
    }

    public static void AddStyles(this VisualElement el, VisualElementStyleSheetSet styleSheets)
    {
      for (int i = 0; i < styleSheets.count; i++)
      {
        el.styleSheets.Add(styleSheets[i]);
      }
    }

    public static void AddStyles(this VisualElement el, IEnumerable<StyleSheet> styleSheets)
    {
      foreach (var styleSheet in styleSheets)
      {
        el.styleSheets.Add(styleSheet);
      }
    }

    /// <summary>
    /// Adds multiple classes to VisualElement. ClassNames separated by space ' '.
    /// </summary>
    /// <param name="el"></param>
    /// <param name="classes"></param>
    public static void AddMultipleToClassList(this VisualElement el, string classes)
    {
      AddMultipleToClassList(el, classes.Split(new string[] { " " }, System.StringSplitOptions.RemoveEmptyEntries));
    }

    public static void AddMultipleToClassList(this VisualElement el, IEnumerable<string> classes)
    {
      foreach (var className in classes)
        el.AddToClassList(className);
    }
  }
}