using UnityEngine.UIElements;
using static UnityEngine.UI.CanvasScaler;


#if ODIN_INSPECTOR
#endif

namespace Graphene
{
  [System.Serializable]
  public sealed class JustifyOverride : StyleOverride<Justify>
  {
    public override void TryApply(VisualElement visualElement)
    {
      if (enabled)
        visualElement.style.justifyContent = value;
      else
        visualElement.style.justifyContent = base.Null;
	}
  }

  public abstract class StyleLengthOverride : StyleOverride<float>
  {
	public LengthUnit unit = LengthUnit.Percent;
  }

  [System.Serializable]
  public sealed class WidthOverride : StyleLengthOverride
  {
	public override void TryApply(VisualElement visualElement)
	{
	  if (enabled)
		visualElement.style.width = new Length(value, unit);
	  else
		visualElement.style.width = new StyleLength(StyleKeyword.Null);// base.Null;
	}
  }

  [System.Serializable]
  public sealed class HeightOverride : StyleLengthOverride
  {
	public override void TryApply(VisualElement visualElement)
	{
	  if (enabled)
		visualElement.style.height = new Length(value, unit);
	  else
		visualElement.style.height = new StyleLength(StyleKeyword.Null);// base.Null;
	}
  }
}