using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Graphene
{

  ///<summary>
  /// <para>A `Theme` is a data asset that can be used to author high-level styling configurations for (parts of) the VisualTree.</para> 
  /// <para><see href="https://github.com/LudiKha/Graphene#theme">Read more in the online documentation</see></para>
  ///</summary>
  [CreateAssetMenu(menuName ="Graphene/Theming/Theme")]
  public class Theme : ScriptableObject
  {
    [SerializeField] Theme parent; public Theme Parent => parent;

    [SerializeField] List<StyleSheet> styleSheets; public IReadOnlyCollection<StyleSheet> StyleSheets => styleSheets;

    /// <summary>
    /// Applies all StyleSheets of the Theme tree to a visual element tree.
    /// </summary>
    /// <param name="el"></param>
    public void ApplyStyles(VisualElement el)
    {
      el.AddStyles(GetStyleSheets());
    }

    /// <summary>
    /// Returns all style sheets of the Theme tree.
    /// </summary>
    /// <returns></returns>
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