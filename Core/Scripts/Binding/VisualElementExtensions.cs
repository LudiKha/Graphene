using UnityEngine.UIElements;

namespace Graphene
{
  public static class VisualElementExtensions
  {
    public static VisualElement GetRootRecursively(this VisualElement el)
    {
      if (el.parent == null)
      {
        foreach (var child in el.Children())
        {
          if (child.style.display != DisplayStyle.None && child.ClassListContains("unity-ui-document__root"))
            return child;
        }
        return null;// el.Q(null, "unity-ui-document__root");
      }

      return el.parent.GetRootRecursively();
    }
  }
}