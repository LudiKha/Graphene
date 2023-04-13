
using System.Collections.Generic;
using System.Linq;

namespace Graphene
{
  public class StringRouter : Router<string> 
  {
    const char separator = '/';
    static char[] separatorArray = new char[] { separator };

    public StringRouter()
    {
      startingState = "app";
    }

    public override string[] GetStatesFromAddress(string address)
    {
      if (string.IsNullOrWhiteSpace(address))
        return new string[0];

      var states = address.Split(separatorArray, System.StringSplitOptions.RemoveEmptyEntries);
      return states;
    }

    protected override string[] GetActiveStateHierarchy(string routeRequest)
    {
      // Trim starting & trailing whitespace
      routeRequest = routeRequest.Trim();

      // Check if the address is relative, absolute or a leaf
      int index = routeRequest.IndexOf(separator);
      // Starting with '/', check for relative address
      if (index == 0)
        return AddressFromRelativeState(routeRequest);

      return AddressFromRelativeState(routeRequest);
    }


    public string[] AddressFromRelativeState(string relativeAddress)
    {
      string[] states = GetStatesFromAddress(relativeAddress);

      // Starting at the leaf, finding n parent states
      //string current = default;
      //for (int i = states.Length - 1; i >= 0; i++)
      //{
      //  current = states[i];
      //  if (this.states.ContainsKey(current))
      //    continue;
      //  else
      //    return false;
      //}
      // Get parent states
      var parentStates = GetParentStatesRecursive(states.First(), states.ToList());
      // Add relative states
      parentStates.AddRange(states);
      return parentStates.ToArray();
    }

    List<string> GetParentStatesRecursive(string state, List<string> list)
    {
      // Travel upwards
      if(states.TryGetValue(state, out string parent))
      {
        if (!ValidState(parent))
          return list;

        list.Insert(0, parent);
        return GetParentStatesRecursive(parent, list);
      }
      // Out of parents
      return list;
    }

#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Button]
#endif
    public bool ChangeState(string path) => base.TryChangeState(path);

    #region Helper Methods

    public override bool ValidState(string state)
    {
      return !string.IsNullOrWhiteSpace(state);
    }

    // Should improve this check
    public override bool AddressExists(string address)
    {
      foreach (var state in GetStatesFromAddress(address))
      {
        if (!states.ContainsKey(state))
          return false;
      }
      return true;
    }

    public override string LeafStateFromAddress(string address)
    {
      return GetStatesFromAddress(address).LastOrDefault();
    }
    #endregion
  }
}