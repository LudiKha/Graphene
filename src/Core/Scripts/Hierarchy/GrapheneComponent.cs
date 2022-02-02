using UnityEngine;

namespace Graphene
{
  public class GrapheneComponent : MonoBehaviour, IGrapheneDependent
  {
    protected Graphene graphene;

    public virtual void Inject(Graphene graphene) => this.graphene = graphene;
  }
}