using UnityEngine.UIElements;

#if ODIN_INSPECTOR
#endif

namespace Graphene
{
  [System.Serializable]
  public sealed class FlexDirectionOverride : StyleOverride<FlexDirection>
  {
	public override void TryApply(VisualElement visualElement)
	{
	  if (enabled)
		visualElement.style.flexDirection = value;
	  else
		visualElement.style.flexDirection = base.Null;
	}
  }

  [System.Serializable]
  public sealed class FlexGrowOverride : StyleOverride<float>
  {
	public override void TryApply(VisualElement visualElement)
	{
	  if (enabled)
		visualElement.style.flexGrow = value;
	  else
		visualElement.style.flexGrow = new StyleFloat(StyleKeyword.Null);
	}
  }
}