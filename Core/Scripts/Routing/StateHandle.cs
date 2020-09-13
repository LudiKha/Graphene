using UnityEngine;

namespace Graphene
{
  public abstract class StateHandle : MonoBehaviour, IInjectable, IInitializable, ILateInitializable
  {
    public abstract Router Router { get; }

    [SerializeField] protected Plate plate;
    /// <summary>
    /// This will enable the plate when the parent is activated.
    /// </summary>
    [SerializeField] protected bool enableWithParent;

    public bool Initialized { get; private set; }
    public virtual void Initialize()
    {
      if (Initialized)
        return;
      Initialized = true;

      if (!plate)
        plate = GetComponent<Plate>();
    }

    public bool LateInitialized { get; private set; }
    public void LateInitialize()
    {
      if (LateInitialized)
        return;
      LateInitialized = true;
    }
  }

  public class StateHandle<T> : StateHandle
  {
    [SerializeField] T stateID; public T StateID => stateID;
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
      if (!router)
        router = GetComponentInParent<Router<T>>();

      // Get parent state
      StateHandle<T> parentStateHandle = transform.parent.GetComponentInParent<StateHandle<T>>();
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

      // Check if our state is active
      if (router.StateIsActive(stateID))
        plate.Show();
      else if(enableWithParent && router.StateIsActive(parentStateID) && router.LeafStateFromAddress(address).Equals(parentStateID))
        plate.Show();
      else
        plate.Hide();
    }
  }
}