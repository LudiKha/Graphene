using System;
using UnityEngine.UIElements;

#if ODIN_INSPECTOR
#endif

namespace Graphene
{
#if ODIN_INSPECTOR
  [Sirenix.OdinInspector.Toggle("enabled")]
#endif
  [System.Serializable]
  public abstract class StyleOverride<T>
    where T : struct, IConvertible
  {
    public bool enabled;
    public T value;

    public abstract void TryApply(VisualElement visualElement);

    public static implicit operator bool (StyleOverride<T> styleOverride) => styleOverride != null && styleOverride.enabled;
	protected StyleEnum<T> Null => new StyleEnum<T>(StyleKeyword.Null);

    public StyleOverride()
    {
    }

    public StyleOverride(T value)
    {
      this.value = value;
    }
  }
}