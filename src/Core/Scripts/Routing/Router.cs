
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Graphene
{
  using Elements;
  using Kinstrife.Core.ReflectionHelpers;

  [DisallowMultipleComponent]
  public abstract class Router : MonoBehaviour, IGrapheneDependent, IInitializable
  {
    /// <summary>
    /// List of interpreters in the hierarchy that can intercept a state change request
    /// </summary>
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.ShowInInspector]
#endif
    protected List<IStateInterpreter> interpreters = new List<IStateInterpreter>();

    public abstract void InjectIntoHierarchy();
    public abstract void Initialize();
    public abstract void BindRouteToContext(BindableElement el, object data);
    public abstract void BindRoute(Route el, object data);
    public abstract void TryGoToPreviousState();
    public abstract void TryGoToNextState();

    public void RegisterInterpreter(IStateInterpreter stateInterpreter)
    {
      if (!interpreters.Contains(stateInterpreter))
        interpreters.Add(stateInterpreter);
    }
    public void UnregisterInterpreter(IStateInterpreter stateInterpreter)
    {
      if (interpreters.Contains(stateInterpreter))
        interpreters.Remove(stateInterpreter);
    }

    protected virtual void Awake()
    {
      interpreters.Clear();
    }
  }

  public abstract class Router<T> : Router, IInitializable, ILateInitializable
  {
    // state & parent
    protected SortedDictionary<T, T> states = new SortedDictionary<T, T>();
    public SortedDictionary<T, T> States => states;

    [SerializeField] protected T startingState; public T StartingState => startingState;

    public T CurrentState => activeStates.LastOrDefault();

#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.ShowInInspector]
#endif
    List<T> activeStates = new List<T>();
    public IReadOnlyList<T> ActiveStates => activeStates;

#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.ShowInInspector]
#endif
    List<T> traversedStates = new List<T>();
    public IReadOnlyList<T> TraversedStates => traversedStates;

    // Events
    public event System.Action<T> onStateChange;

#if UNITY_EDITOR && ODIN_INSPECTOR
    [Sirenix.OdinInspector.ShowInInspector, Sirenix.OdinInspector.ValueDropdown("StateKeys"), Sirenix.OdinInspector.OnValueChanged("TryChangeState")]
    T changeState;

    internal IEnumerable<T> StateKeys => states.Keys;
#endif

    #region Initialization

    // Injects the router into
    public override void InjectIntoHierarchy()
    {
      foreach (var stateHandle in GetComponentsInChildren<StateHandle<T>>())
      {
        stateHandle.Inject(this);
      }
    }

    public bool Initialized { get; private set; }
    public override void Initialize()
    {
      if (Application.isPlaying && Initialized)
        return;
      Initialized = true;

      traversedStates.Clear();
      states.Clear();
      onStateChange = null;
      RegisterState(startingState, default);
    }

    public bool LateInitialized { get; private set; }
    public void LateInitialize()
    {
      if (Application.isPlaying && LateInitialized)
        return;

      var targetState = StartingState;

      if (!ValidState(targetState) || !states.ContainsKey(targetState))
      {
        foreach (var kvp in states)
        {
          targetState = kvp.Key;
          break;
        }
      }

      if (targetState != null)
        TryChangeState(targetState);
    }
    #endregion

    #region Binding
    public override void BindRouteToContext(BindableElement el, object context)
    {
      if (el is Button btn)
      {
        // Get members
        List<ValueWithAttribute<RouteAttribute>> members = new List<ValueWithAttribute<RouteAttribute>>();
        TypeInfoCache.GetMemberValuesWithAttribute(context, members);

        foreach (var item in members)
        {
          T targetState = (T)item.Value;
          if (!ValidState(targetState))
            continue;

          btn.clicked += delegate { TryChangeState(targetState); };
        }
      }
      else if (el is Route routeEl)
      {

      }
    }

    public override void BindRoute(Route el, object context)
    {
      T targetState = default;
      if (el.route is T stateFromEl)
        targetState = stateFromEl;

      // Does the element have a route defined
      if (ValidState(targetState))
        Bind(targetState);
      // Route is not defined in element -> look in members with Route attributes
      else
      {
        // Get members
        List<ValueWithAttribute<RouteAttribute>> members = new List<ValueWithAttribute<RouteAttribute>>();
        TypeInfoCache.GetMemberValuesWithAttribute(context, members);

        foreach (var item in members)
        {
          if (ValidState((T)item.Value))
          {
            Bind((T)item.Value);
            break;
          }
        }
      }

      void Bind(T target)
      {
        if (target is string str)
          el.route = str;
        else
        {
          el.clicked += delegate
          {
            TryChangeState(target);
          };
        }
      }
    }
    #endregion

    #region Public API
    public void RegisterState(T state, T parentState)
    {
      if (!ValidState(state))
      {
        Debug.LogError($"Trying to register invalid route: {state}", this);
        return;
      }

      //state = ProcessRouteRequest(state);

      if (states.ContainsKey(state))
        return;
      else
        states.Add(state, parentState);
    }

    public void RegisterInterpreter(IStateInterpreter<T> stateInterpreter)
    {
      if(!interpreters.Contains(stateInterpreter))
        interpreters.Add(stateInterpreter);
    }
    public void UnregisterInterpreter(IStateInterpreter<T> stateInterpreter)
    {
      if (interpreters.Contains(stateInterpreter))
        interpreters.Remove(stateInterpreter);
    }


    public virtual bool TryChangeState(T state)
    {
      // See if the router
      foreach (var interpreter in interpreters)
      {
        if (interpreter != null && interpreter.TryCatch(state))
          return true;
      }

      // Only contained keys allowed
      if (!AddressExists(state))
      {
        Debug.LogError($"Trying to change state {state}, which isn't registered", this);
        return false;
      }

      this.activeStates = GetActiveStateHierarchy(state).ToList();
      UpdateTraversedStates(this.activeStates.Last());

      onStateChange?.Invoke(state);
      return true;
    }

    public override void TryGoToPreviousState()
    {
      if (traversedStates.Count <= 1)
        return;

      T previousState = traversedStates[traversedStates.IndexOf(CurrentState) - 1];
      TryChangeState(previousState);
    }

    public override void TryGoToNextState()
    {
    }

    public bool StateIsActive(T state)
    {
      return activeStates.IndexOf(state) != -1;
    }

    #endregion

    #region Helper Methods
    public abstract bool ValidState(T state);

    public abstract bool AddressExists(T address);
    public abstract T[] GetStatesFromAddress(T address);

    public abstract T LeafStateFromAddress(T address);

    protected abstract T[] GetActiveStateHierarchy(T state);

    protected void UpdateTraversedStates(T newState)
    {
      int curIndex = traversedStates.IndexOf(newState);

      if (curIndex >= 0) // Already visited this state -> trim the tree
      {
        int excessStates = traversedStates.Count - (curIndex + 1);
        traversedStates.RemoveRange(curIndex + 1, excessStates);
      }
      else
        traversedStates.Add(newState);
    }

    #endregion
  }
}