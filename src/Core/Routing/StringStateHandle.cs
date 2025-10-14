using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Graphene
{
  /// <summary>
  /// Component that marks a Plate as having a string-based state ID. Adding this to a plate indicates that this Gameobject is capable of being routed to using a string address.
  /// </summary>
  public class StringStateHandle : StateHandle<string>
  {
    [field: SerializeField] public bool StateFromGameObjectName { get; private set; } = true;
    public override string StateID => Application.isPlaying ? stateID : StateFromGameObjectName ? GameObjectNameAsStateId() : base.StateID;

    protected override void Awake()
    {
      base.Awake();
      if (StateFromGameObjectName)
        this.stateID = GameObjectNameAsStateId();
    }

    string GameObjectNameAsStateId () => gameObject.name.ToLower();
  }
}