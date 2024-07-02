using UnityEngine;

namespace Graphene
{
  public class GrapheneComponent : MonoBehaviour, IGrapheneDependent
  {
	internal string debugNameCached; public string DebugName => debugNameCached;
#if ODIN_INSPECTOR
	[Sirenix.OdinInspector.HideIf(nameof(graphene))]
#endif
	[SerializeField] protected Graphene graphene; public Graphene Graphene => graphene;
	public BindingsManager BindingsManager => graphene?.Binder;

	public virtual void Inject(Graphene graphene)
	{
	  this.graphene = graphene;

	  if (string.IsNullOrEmpty(debugNameCached))
		debugNameCached = name;
	}

	protected virtual void Awake()
	{
	  if (string.IsNullOrEmpty(debugNameCached))
		debugNameCached = name;
	}
  }
}