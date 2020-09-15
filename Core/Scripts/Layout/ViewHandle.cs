using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Graphene
{
  [DisallowMultipleComponent]
  public class ViewHandle : MonoBehaviour
  {
    [SerializeField] protected string id; public string Id => id;
  }
}