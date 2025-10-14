using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

namespace Graphene
{
  [DisallowMultipleComponent]
  public class Injector : MonoBehaviour
  {
    [SerializeField] Graphene graphene; 
    private void Awake()
    {
      if (graphene || (graphene = GetComponent<Graphene>()))
        graphene.onPreInitialize += Graphene_onPreInitialize;
    }

    private void Graphene_onPreInitialize(ICollection<IGrapheneDependent> dependents)
    {
      foreach (var dependent in dependents)
      {
        if (dependent is Router router)
          router.InjectIntoHierarchy();
      }
    }
  }
}