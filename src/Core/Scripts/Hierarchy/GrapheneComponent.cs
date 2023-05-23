using UnityEngine;

namespace Graphene
{
  public class GrapheneComponent : MonoBehaviour, IGrapheneDependent
  {
    [SerializeField, HideInInspector] protected Graphene graphene; public Graphene Graphene => graphene;

    public virtual void Inject(Graphene graphene) => this.graphene = graphene;
  }
}