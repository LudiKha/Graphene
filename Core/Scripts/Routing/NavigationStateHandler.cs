using Graphene;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Graphene
{

  public class NavigationStateHandler : StateInterpreter<string>, IInjectable, IInitializable
  {
    [SerializeField] string backCommand = "back";
    [SerializeField] string previousCommand = "previous";
    [SerializeField] string nextCommand = "next";
    [SerializeField] string exitCommand = "exit";

    [SerializeField] Router<string> router;
    [SerializeField] Plate plate;

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

      if (!router)
        router = GetComponentInParent<Router<string>>();
      router.RegisterInterpreter(this);

      if (!plate)
      {
        plate = GetComponent<Plate>();
        plate.onShow.AddListener(Plate_OnShow);
        plate.onHide.AddListener(Plate_OnHide);
      }

      navigationButtonGroup = plate.Root.Q<ButtonGroup>();
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
        navigationButtonGroup.activeIndex -= 1;
      else if (state == nextCommand)
        navigationButtonGroup.activeIndex += 1;
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