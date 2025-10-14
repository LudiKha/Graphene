using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Graphene
{
  public interface IStateInterpreter 
  {
    bool CanCatch(object state);
    bool TryCatch(object state);
  }
  public interface IStateInterpreter<TStateType> : IStateInterpreter 
  {
    bool CanCatch(TStateType state);

	bool TryCatch(TStateType state);
  }

  /// <summary>
  /// A StateInterpreter is a component that can intercept a state change commands  at a Router, consume it, prevent it from propagating further, and optionally trigger other logic.
  /// </summary>
  public abstract class StateInterpreter : GrapheneComponent, IStateInterpreter
  {
    public abstract bool CanCatch(object state);
    public abstract bool TryCatch(object state);
  }

  public abstract class StateInterpreter<TStateType> : StateInterpreter, IStateInterpreter<TStateType>
  {
    public abstract bool CanCatch(TStateType state);
    public abstract bool TryCatch(TStateType state);
  }
}