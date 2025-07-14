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