
//using System.Collections.Generic;

//namespace Graphene
//{
//  public class StateIDRouter : Router<UIState>
//  {
//    protected override UIState[] GetActiveStateHierarchy(UIState state)
//    {
//      return GetUIStatesRecursive(state, new List<UIState>()).ToArray();
//    }

//    public override bool ValidState(UIState route)
//    {
//      return route != null;
//    }

//    List<UIState> GetUIStatesRecursive(UIState state, List<UIState> hierarchy)
//    {
//      hierarchy.Insert(0, state);

//      if (!state.Parent)
//        return hierarchy;
//      else
//        return GetUIStatesRecursive(state.Parent, hierarchy);

//    }
//  }
//}