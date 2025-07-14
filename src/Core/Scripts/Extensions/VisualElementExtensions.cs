using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Graphene
{
  public static class VisualElementExtensions
  {
    internal const string documentRootUssClassName = "unity-ui-document__root";
    internal const string documentChildUssClassName = "unity-ui-document__child";
    internal const string hiddenUssClassName = "hidden";
    internal const string invisibleUssClassName = "invisible";
	internal const string fadeoutUssClassName = "fadeout";
    internal const string transitioningUssClassName = "transitioning";
    //internal const string fadeinUssClassName = "fadein";

    internal const string activeUssClassName = "active";
    internal const string selectedUssClassName = "selected";

    public static bool IsHidden(this VisualElement el)
    { 
      return el.ClassListContains(hiddenUssClassName);
    }

    public static void Show(this VisualElement el)
    {
      el.RemoveFromClassList(hiddenUssClassName);
    }
    public static void Hide(this VisualElement el)
    {
      el.AddToClassList(hiddenUssClassName);
    }

    public static void SetShowHide(this VisualElement el, bool value)
    {
      if (value)
        el.Show();
      else
        el.Hide();
    }

	public static void SetVisibility(this VisualElement el, bool value)
	{
	  if (value)
		el.RemoveFromClassList(invisibleUssClassName);
	  else
		el.AddToClassList(invisibleUssClassName);
	}
	public static void SetActive(this VisualElement el, bool value)
    {
      if(value)
        el.AddToClassList(activeUssClassName);
      else
        el.RemoveFromClassList(activeUssClassName);
	}

	public static void ToggleClass(this VisualElement el, string className, bool value)
	{
	  if (value)
		el.AddToClassList(className);
	  else
		el.RemoveFromClassList(className);
	}


	public static void FadeIn(this VisualElement el)
    {
      el.RemoveFromClassList(fadeoutUssClassName);
    }

    public static void FadeOut(this VisualElement el)
    {
      el.AddToClassList(fadeoutUssClassName);
    }
	public static void StartTransition(this VisualElement el)
	{
	  el.AddToClassList(transitioningUssClassName);
	}
	public static void StopTransition(this VisualElement el)
	{
	  el.RemoveFromClassList(transitioningUssClassName);
	}

	public static bool IsFadingOut(this VisualElement el)
	{
	  return el.ClassListContains(fadeoutUssClassName);
	}

	public static bool IsTransitioning(this VisualElement el)
	{
	  return el.ClassListContains(transitioningUssClassName);
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
        var sheet = styleSheets[i];
        if (sheet != null)
          el.styleSheets.Add(sheet);
#if UNITY_EDITOR
        else
          UnityEngine.Debug.LogError("Trying to add null stylesheet");
#endif
      }
    }

    public static void AddStyles(this VisualElement el, IEnumerable<StyleSheet> styleSheets)
    {
      foreach (var styleSheet in styleSheets)
      {
        if(styleSheet)
          el.styleSheets.Add(styleSheet);
#if UNITY_EDITOR
        else
          UnityEngine.Debug.LogError("Trying to add null stylesheet");
#endif
      }
    }

    /// <summary>
    /// Adds multiple classes to VisualElement. ClassNames separated by space ' '.
    /// </summary>
    /// <param name="el"></param>
    /// <param name="classes">Separated by space</param>
    public static void AddMultipleToClassList(this VisualElement el, string classes)
    {
      if (string.IsNullOrWhiteSpace(classes))
        return;
        AddMultipleToClassList(el, Parse(classes));
    }

    public static void AddMultipleToClassList(this VisualElement el, IEnumerable<string> classes)
    {
      foreach (var className in classes)
        el.AddToClassList(className);
    }


	/// <summary>
	/// Removes multiple classes from VisualElement. ClassNames separated by space ' '.
	/// </summary>
	/// <param name="el"></param>
	/// <param name="classes">Separated by space</param>
	public static void RemoveMultipleFromClassList(this VisualElement el, string classes)
	{
	  if (string.IsNullOrWhiteSpace(classes))
		return;

	  RemoveMultipleFromClassList(el, Parse(classes));
	}
	public static void RemoveMultipleFromClassList(this VisualElement el, IEnumerable<string> classes)
	{
	  foreach (var className in classes)
		el.RemoveFromClassList(className);
	}

    public static IEnumerable<string> Parse(string classes)
    {
      return classes?.Split(new string[] { " " }, System.StringSplitOptions.RemoveEmptyEntries);

	}
  }
}