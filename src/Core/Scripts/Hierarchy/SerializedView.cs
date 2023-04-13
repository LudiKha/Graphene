using UnityEngine;
using UnityEngine.UIElements;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace Graphene
{
  [System.Serializable, Toggle(nameof(Enabled))]
  public struct SerializedView
  {
    [SerializeField] public bool Enabled;
    [ReadOnly, SerializeField] public string Id;
    [SerializeField] InlineStyleOverrides StyleOverrides;

    public SerializedView(string id)
    {
      Enabled = false;
      this.Id = id;
	  StyleOverrides = new InlineStyleOverrides();
	}

	public void Apply(VisualElement el)
    {
      StyleOverrides.Apply(el);
	}
  }
}