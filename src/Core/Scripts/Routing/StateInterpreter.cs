using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Graphene
{
  public abstract class StateInterpreter : MonoBehaviour
  {
    public abstract bool TryCatch(object state);
  }

  public abstract class StateInterpreter<TStateType> : StateInterpreter
  {
    public abstract bool TryCatch(TStateType state);
  }
}