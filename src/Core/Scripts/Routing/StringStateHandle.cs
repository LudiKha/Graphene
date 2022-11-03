using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Graphene
{
  public class StringStateHandle : StateHandle<string>
  {
    [field: SerializeField] public bool StateFromGameObjectName { get; private set; } = true;
    public override string StateID => Application.isPlaying ? stateID : StateFromGameObjectName ? GameObjectNameAsStateId() : base.StateID;

    private void Awake()
    {
      if (StateFromGameObjectName)
        this.stateID = GameObjectNameAsStateId();
    }

    string GameObjectNameAsStateId () => gameObject.name.ToLower();
  }
}