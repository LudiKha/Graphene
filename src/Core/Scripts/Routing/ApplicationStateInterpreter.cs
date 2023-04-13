using Graphene;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Graphene
{
  using Elements;
  using Sirenix.OdinInspector;
  using UnityEngine.Events;

  public enum RouterCommand
  {
    None,
    Back,
    Previous,
    Exit
  }

  //[RequireComponent(typeof(Plate))]
  public class ApplicationStateInterpreter : StateInterpreter<string>, IGrapheneInjectable, IGrapheneInitializable
  {

    [System.Serializable, Toggle("enabled", CollapseOthersOnExpand = false)]
    public struct StateCommandHandle
    {
      public string name => stateCommand;
      public bool enabled;
      [SerializeField] public string stateCommand;

#if ODIN_INSPECTOR
      [ValidateInput(nameof(ValidateCustomState), "Custom state reroute should be different from input state command")]
#endif
      [BoxGroup("Output")] public string customState;
      [BoxGroup("Output"), DisableIf(nameof(hasCustomState))] public RouterCommand routerCommand;
      [BoxGroup("Output")] public UnityEvent OnStateEnter;

      internal bool hasCustomState => !System.String.IsNullOrWhiteSpace(customState) && customState != stateCommand;
#if ODIN_INSPECTOR
      bool ValidateCustomState(string customState)
      {
        return customState != stateCommand;
      }
#endif
    }

    [ListDrawerSettings(ListElementLabelName = nameof(StateCommandHandle.name))]
    public StateCommandHandle[] commands = new StateCommandHandle[0];

    Router<string> router;
    Plate plate;

    public void Inject(Router<string> router)
    {
      this.router = router;      
    }

    public bool Initialized { get; private set; }
    public void Initialize()
    {
      if (Initialized) return;
      Initialized = true;

      router = graphene.Router as Router<string>;
      router.RegisterInterpreter(this);

      if (plate ??= GetComponent<Plate>())
      {
        plate.onShow.AddListener(Plate_OnShow);
        plate.onHide.AddListener(Plate_OnHide);
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

      foreach (var command in commands)
      {
        if(command.stateCommand == state)
        {
          if(command.OnStateEnter != null || command.routerCommand != RouterCommand.None || command.hasCustomState)
          {
            if (command.hasCustomState)
              router.TryChangeState(command.customState);
            else
            {
              switch (command.routerCommand)
              {
                case RouterCommand.None:
                  break;
                case RouterCommand.Back:
                  router.TryGoToPreviousState();
                  break;
                case RouterCommand.Previous:
                  break;
                case RouterCommand.Exit:
                  TryExit();
				  break;
                default:
                  break;
              }
            }

            command.OnStateEnter?.Invoke();
            return true;
          }
        }
      }

      return false;
  }

    public virtual void TryExit()
    {
#if UNITY_EDITOR
	  UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_WEBPLAYER
      Application.OpenURL("https://github.com/LudiKha/Graphene");
#else
        Application.Quit();
#endif
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