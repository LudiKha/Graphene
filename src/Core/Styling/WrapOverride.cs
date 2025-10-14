using UnityEngine.UIElements;

#if ODIN_INSPECTOR
#endif

namespace Graphene
{
  [System.Serializable]
  public sealed class WrapOverride : StyleOverride<Wrap>
  {
    public override void TryApply(VisualElement visualElement)
    {
      if (enabled)
        visualElement.style.flexWrap = value;
	  else
		visualElement.style.flexWrap = base.Null;
    }
  }
}