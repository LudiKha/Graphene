using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Graphene
{
  [CreateAssetMenu(menuName ="Graphene/Theming/Theme")]
  public class Theme : ScriptableObject
  {
    [SerializeField] Theme parent; public Theme Parent => parent;

    [SerializeField] List<StyleSheet> styleSheets; public IReadOnlyCollection<StyleSheet> StyleSheets => styleSheets;

    public void ApplyStyles(VisualElement el)
    {
      el.AddStyles(GetStyleSheets());
    }

    public List<StyleSheet> GetStyleSheets()
    {
      List<StyleSheet> results = new List<StyleSheet>();
      GetStyleSheetsRecursive(this, results);
      return results;
    }

    internal void GetStyleSheetsRecursive(Theme current, List<StyleSheet> results)
    {
      // Insert to front
      results.InsertRange(0, current.styleSheets);

      if (!current.parent)
        return;

      GetStyleSheetsRecursive(current.parent, results);
    }
  }
}