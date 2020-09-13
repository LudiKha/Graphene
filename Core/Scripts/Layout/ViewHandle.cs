using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Graphene
{
  public class ViewHandle : MonoBehaviour
  {
    [SerializeField] protected string id; public string Id => id;
    /// <summary>
    /// Overriding this will
    /// </summary>
    [SerializeField] public string[] containerSelector = new string[] { };
  }
}