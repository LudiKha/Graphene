using UnityEngine;

namespace Graphene
{
  public enum ChildActivationMode
  {
    None,
    ShowWithParent,
    DefaultState
  }

  [RequireComponent(typeof(Plate))]
  [DisallowMultipleComponent]
  public abstract class StateHandle : GrapheneComponent, IGrapheneInjectable, IGrapheneInitializable
  {
    public abstract Router Router { get; }

    [SerializeField] protected Plate plate;

    /// <summary>
    /// This will Show the plate when the parent is activated.
    /// </summary>
    [SerializeField] protected ChildActivationMode activationMode = ChildActivationMode.None;

    public bool Initialized { get; private set; }
    public virtual void Initialize()
    {
      if (Initialized)
        return;
      Initialized = true;

      if (plate || (plate = GetComponent<Plate>()))
      {
        plate.onEvaluateState += Plate_onEvaluateState;
      }
    }

    protected abstract void Plate_onEvaluateState();
  }

  public class StateHandle<T> : StateHandle
  {
    [SerializeField] protected T stateID; public virtual T StateID => stateID;
    [SerializeField] T parentStateID;
    protected Router<T> router; public override Router Router => router as Router;
    /// <summary>
    /// Dependency injection handle
    /// </summary>
    /// <param name="router"></param>
    public void Inject(Router<T> router)
    {
      this.router = router;
    }

    public override void Initialize()
    {
      base.Initialize();

      // Get the router in case we didn't inject
      router ??= graphene.Router as Router<T>;

      // Get parent state
      StateHandle<T> parentStateHandle = transform.parent.GetComponentInParent<StateHandle<T>>(true);
      parentStateID = parentStateHandle ? parentStateHandle.StateID : default;

      // Register the state at the router
      router.RegisterState(stateID, parentStateID);
      // Subscribe to router state changes
      router.onStateChange += Router_onStateChange;
    }

    private void Router_onStateChange(T address)
    {
      if (!router.ValidState(stateID))
        return;

      bool parentWasTarget = router.StateIsActive(parentStateID) && router.LeafStateFromAddress(address).Equals(parentStateID);

      // Try Change state to this
      if (activationMode == ChildActivationMode.DefaultState && parentWasTarget) {
        if(router.TryChangeState(stateID))
          return;
      }

      // Check if our state is active
        if (router.StateIsActive(stateID))
        plate.Show();
      else if(activationMode == ChildActivationMode.ShowWithParent && parentWasTarget)
        plate.Show();
      else
        plate.Hide();
    }

    protected override void Plate_onEvaluateState()
    {
      Router_onStateChange(router.CurrentState);
    }
  }
}