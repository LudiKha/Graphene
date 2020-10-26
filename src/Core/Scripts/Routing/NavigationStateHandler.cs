using Graphene;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Graphene
{
  using Elements;

  [RequireComponent(typeof(Plate))]
  public class NavigationStateHandler : StateInterpreter<string>, IInjectable, IInitializable
  {
    [SerializeField] string backCommand = "back";
    [SerializeField] string previousCommand = "previous";
    [SerializeField] string nextCommand = "next";
    [SerializeField] string exitCommand = "exit";

    [SerializeField] Router<string> router;
    [SerializeField] Plate plate;

    ButtonGroup navigationButtonGroup;

    [SerializeField] List<string> states = new List<string>();

    public void Inject(Router<string> router)
    {
      this.router = router;      
    }

    public bool Initialized { get; private set; }
    public void Initialize()
    {
      if (Initialized) return;
      Initialized = true;

      if (!router)
        router = GetComponentInParent<Router<string>>();
      router.RegisterInterpreter(this);
      router.onStateChange += Router_onStateChange;

      if (!plate)
      {
        plate = GetComponent<Plate>();
        plate.onShow.AddListener(Plate_OnShow);
        plate.onHide.AddListener(Plate_OnHide);
      }

      plate.onRefreshVisualTree += Plate_onRefreshHierarchy;
    }


    bool HasElements()
    {
      if (navigationButtonGroup != null)
        return true;
      else
        navigationButtonGroup = plate.Root.Q<ButtonGroup>();

      if (navigationButtonGroup == null)
      {
        Debug.LogError($"{GetType().Name} requires a ButtonGroup VisualElement in its static template. Select a template that contains a ButtonGroup element.", this);
        return false;
      }

      return true;
    }

    private void Plate_onRefreshHierarchy()
    {
      if (!HasElements())
        return;
      else
        navigationButtonGroup.onChangeIndex += NavigationButtonGroup_onChangeIndex;
    }

    private void Router_onStateChange(string newState)
    {
      if (navigationButtonGroup == null)
        return;

      int i = 0;
      
      foreach (var state in states)
      {
        if(router.StateIsActive(state))
        {
          navigationButtonGroup.SetValueWithoutNotify(i);
          return;
        }
        i++;
      }
    }

    private void NavigationButtonGroup_onChangeIndex(int index)
    {
      index = Mathf.Clamp(index, 0, states.Count - 1);

      router.TryChangeState(states[index]);
    }

    public override bool TryCatch(object state)
    {
      return TryCatch((string)state);
    }

    public override bool TryCatch(string state)
    {
      if (!enabled || !gameObject.activeInHierarchy)
        return false;

      if (state == backCommand)
        router.TryGoToPreviousState();
      else if (state == previousCommand)
        navigationButtonGroup.value -= 1;
      else if (state == nextCommand)
        navigationButtonGroup.value += 1;
      else if (state == exitCommand)
      {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_WEBPLAYER
      Application.OpenURL("https://github.com/LudiKha/Graphene");
#else
        Application.Quit();
#endif
      }
      else
        return false;

      return true;
  }

    internal void Plate_OnShow()
    {
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