using System;

namespace Graphene
{
  using UnityEngine;

  [CreateAssetMenu(menuName = "Graphene/Route/StateID")]
  public class UIState : ScriptableObject, IEquatable<string>
  {
    [SerializeField] string _Name; public string Name => _Name;

    [SerializeField] UIState _Parent; public UIState Parent => _Parent;

    public bool Equals(string other)
    {
      return _Name == other || name == other;
    }
  }
}