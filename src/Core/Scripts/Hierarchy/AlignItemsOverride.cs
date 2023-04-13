using UnityEngine.UIElements;

#if ODIN_INSPECTOR
#endif

namespace Graphene
{
  [System.Serializable]
  public sealed class AlignItemsOverride : StyleOverride<Align>
  {
    public override void TryApply(VisualElement visualElement)
    {
      if (enabled)
        visualElement.style.alignItems = value;
      else
	    visualElement.style.alignItems = base.Null;
	}
  }
}