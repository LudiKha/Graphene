using UnityEngine.UIElements;

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
}