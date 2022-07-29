using UnityEngine;
using UnityEngine.UIElements;

namespace Graphene
{
  using Elements;

  // Comment K: This should be split up into (ButtonGroup->StateButtonGroup) & (NavigationStateInterpreter)
  [RequireComponent(typeof(Plate))]
  public class NavigationStateHandler : StateInterpreter<string>, IGrapheneInjectable, IGrapheneInitializable
  {
    [SerializeField] string previousCommand = "previous";
    [SerializeField] string nextCommand = "next";

    Router<string> router;
    Plate plate;

    ButtonGroup navigationButtonGroup;

    public void Inject(Router<string> router)
    {
      this.router = router;
    }

    public bool Initialized { get; private set; }
    public void Initialize()
    {
      if (Initialized) return;
      Initialized = true;

      router ??= graphene.Router as Router<string>;
      router.RegisterInterpreter(this);
      router.onStateChange += Router_onStateChange;

      plate ??= GetComponent<Plate>();
      plate.onShow.AddListener(Plate_OnShow);
      plate.onHide.AddListener(Plate_OnHide);

      Plate_OnHide();
    }


    bool HasElements()
    {
      if (navigationButtonGroup != null)
        return true;

      navigationButtonGroup = plate?.Root?.Q<ButtonGroup>();

      if (navigationButtonGroup == null)
      {
        Debug.LogError($"{GetType().Name} requires a ButtonGroup VisualElement in its static template. Select a template that contains a ButtonGroup element.", this);
        return false;
      }

      //navigationButtonGroup.RegisterValueChangedCallback(evt =>
      //{
      //  router.TryChangeState(navigationButtonGroup.items[navigationButtonGroup.value]);
      //});

      return true;
    }

    private void Router_onStateChange(string newState)
    {
      if (!HasElements())
        return;

      int i = 0;

      foreach (var state in navigationButtonGroup.items)
      {
        if (router.StateIsActive(state))
        {
          navigationButtonGroup.SetValueWithoutNotify(i);
          return;
        }
        i++;
      }
    }

    public override bool TryCatch(object state)
    {
      return TryCatch((string)state);
    }

    public override bool TryCatch(string state)
    {
      if (!enabled || !gameObject.activeInHierarchy)
        return false;

      else if (state == previousCommand)
      {
        navigationButtonGroup.value -= 1;
        router.TryChangeState(navigationButtonGroup.items[navigationButtonGroup.value]);
      }
      else if (state == nextCommand)
        navigationButtonGroup.value += 1;
      else
        return false;

      return true;
    }

    internal void Plate_OnShow()
    {
      if (!HasElements())
        return;

      navigationButtonGroup.SetValueWithoutNotify(0);

      enabled = true;
    }

    internal void Plate_OnHide()
    {
      enabled = false;
    }

    private void OnEnable()
    {
      if (!Initialized)
        return;

      router.RegisterInterpreter(this);
    }

    private void OnDisable()
    {
      if (!Initialized)
        return;

      router.UnregisterInterpreter(this);
    }
  }
}