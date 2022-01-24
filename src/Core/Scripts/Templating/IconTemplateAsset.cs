using UnityEngine;
using UnityEngine.UIElements;

namespace Graphene
{


  ///<summary>
  /// <para>A `Template` is a static asset that represents a chunk of UXML of varying granularity and complexity, which are used as building blocks to build and render the application.</para> 
  /// <para><see href="https://github.com/LudiKha/Graphene#template">Read more in the online documentation</see></para>
  ///</summary>
  [CreateAssetMenu(menuName = "Graphene/Templating/IconTemplateAsset")]
  public class IconTemplateAsset : TemplateAsset
  {
    internal const string iconAddClassName = "gr-icon";

    [SerializeField] Texture Texture;
    [SerializeField] Sprite Sprite;

    public override VisualElement Instantiate()
    {
      Image clone = new Image();
      if (Texture)
        clone.image = Texture;
      if (Sprite)
        clone.sprite = Sprite;

      clone.AddMultipleToClassList(templateAddClassName);
      clone.AddMultipleToClassList(iconAddClassName);
      if (AddClass != null)
        clone.AddMultipleToClassList(AddClass);

      return clone;
    }
  }
}