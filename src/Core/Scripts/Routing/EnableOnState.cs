using System.Collections.Generic;
using UnityEngine;

namespace Graphene
{
  [RequireComponent(typeof(Plate))]
  [DisallowMultipleComponent]
  public abstract class EnableOnState<T> : MonoBehaviour, IGrapheneInjectable, IGrapheneInitializable
  {
    public enum ActivationMode
    {
      EnableOnStates,
      DisableOnStates
    }
    [SerializeField] ActivationMode mode;

    [SerializeField] protected Plate plate;
    [SerializeField] List<T> states = new List<T>();
    protected Router<T> router;

    public bool Initialized { get; private set; }
    public virtual void Initialize()
    {
      if (Initialized)
        return;
      Initialized = true;

      if (plate || (plate = GetComponent<Plate>()))
        plate.onEvaluateState += Plate_onEvaluateState;

      // Get the router in case we didn't inject
      if (!router)
        router = GetComponentInParent<Router<T>>();

      // Subscribe to router state changes
      router.onStateChange += Router_onStateChange;
    }

    /// <summary>
    /// Dependency injection handle
    /// </summary>
    /// <param name="router"></param>
    public void Inject(Router<T> router)
    {
      this.router = router;
    }


    private void Router_onStateChange(T address)
    {
      bool match = states.IndexOf(router.LeafStateFromAddress(address)) >= 0;
      bool show = false;
      switch (mode)
      {
        case ActivationMode.EnableOnStates:
          show = match;
          break;
        case ActivationMode.DisableOnStates:
          show = !match;
          break;
        default:
          break;
      }

      if (show)
        plate.Show();
      else
        plate.Hide();
    }

    protected void Plate_onEvaluateState()
    {
      Router_onStateChange(router.CurrentState);
    }
  }

}