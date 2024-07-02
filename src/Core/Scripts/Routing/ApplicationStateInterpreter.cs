using Graphene;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace Graphene
{
  using UnityEngine.Events;

  public enum RouterCommand
  {
    None,
    Back,
    Previous,
    Exit,
    Root,
    Menu,
    ToggleUI
  }

  [System.Flags]
  public enum NavigationInput
  {
    None = 0,
    NavigationMove = 1 << 0,
    NavigationSubmit = 1 << 1,
    NavigationCancel = 1 << 2
  }

#if ODIN_INSPECTOR
  [Toggle("enabled", CollapseOthersOnExpand = false)]
#endif
  [System.Serializable]
  public class InputOverride
  {
	public string name => input.ToString();
	public bool enabled;
	public NavigationInput input;
    public TrickleDown trickleDown = TrickleDown.TrickleDown;
	[BoxGroup("Output")] public RouterCommand routerCommand;
	[BoxGroup("Output")] public UnityEvent OnInput;
	[BoxGroup("Output")] public bool preventDefault = true;
  }

  //[RequireComponent(typeof(Plate))]
  public class ApplicationStateInterpreter : StateInterpreter<string>, IGrapheneInjectable, IGrapheneInitializable
  {
#if ODIN_INSPECTOR
    [Toggle("enabled", CollapseOthersOnExpand = false)]
#endif
    [System.Serializable]
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

#if ODIN_INSPECTOR
	[ListDrawerSettings(ListElementLabelName = nameof(StateCommandHandle.name))]
#endif
    public StateCommandHandle[] commands = new StateCommandHandle[0];
    public InputOverride[] inputs = new InputOverride[0];

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

      if (plate || TryGetComponent<Plate>(out plate))
      {
        plate.onShow.AddListener(Plate_OnShow);
        plate.onHide.AddListener(Plate_OnHide);
		RegisterInput();
		plate.onRefreshVisualTree += RegisterInput;
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
        if (!command.enabled || command.stateCommand != state)
          continue;
		if (command.OnStateEnter != null || command.routerCommand != RouterCommand.None || command.hasCustomState)
		{
          if (command.hasCustomState)
          {
            if (command.customState.IndexOf("http") == 0)
			  Application.OpenURL(command.customState);
            else
			  router.TryChangeState(command.customState);
          }
          else
            HandleRouterCommand(command.routerCommand);
		  command.OnStateEnter?.Invoke();
		  return true;
		}
	  }

      return false;
  }
    void HandleRouterCommand(RouterCommand routerCommand)
    {
	  switch (routerCommand)
	  {
		case RouterCommand.None:
		  break;
		case RouterCommand.Back:
		  router.TryGoUpOneState();
		  break;
		case RouterCommand.Previous:
		  router.TryGoToPreviousState();
		  break;
		case RouterCommand.Exit:
		  TryExit();
		  break;
		case RouterCommand.Root:
		  router.ResetState();
		  break;
		default:
		  break;
	  }
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

    bool inputRegistered;
    void RegisterInput()
    {
      if (inputRegistered || !plate || plate.Root == null)
        return;
      inputRegistered = true;
      foreach (var item in inputs)
	  {
		Debug.Log($"Registering Input {item.routerCommand}");

		if ((item.input & NavigationInput.NavigationCancel) != 0)
        {
		  plate.Root.RegisterCallback<NavigationCancelEvent>(ctx => OnNavigationCancel(item, ctx), item.trickleDown);
		  Debug.Log($"Registering Input Event {typeof(NavigationCancelEvent).Name} {item.routerCommand}");
		}

		if ((item.input & NavigationInput.NavigationSubmit) != 0)
		  plate.Root.RegisterCallback<NavigationSubmitEvent>(ctx => OnNavigationSubmit(item, ctx), item.trickleDown);

		if ((item.input & NavigationInput.NavigationMove) != 0)
		  plate.Root.RegisterCallback<NavigationMoveEvent>(ctx => OnNavigationMove(item, ctx), item.trickleDown);
	  }
    }

	void OnNavigationSubmit(InputOverride item, NavigationSubmitEvent evt)
	{
	  Debug.Log($"isActiveAndEnabled {item.routerCommand}");
	  if (!isActiveAndEnabled) return;
	  Handle(item, evt);
	}
	void OnNavigationCancel(InputOverride item, NavigationCancelEvent evt)
	{
	  Debug.Log($"isActiveAndEnabled {item.routerCommand}");
	  if (!isActiveAndEnabled) return;
	  Handle(item, evt);
	}
	void OnNavigationMove(InputOverride item, NavigationMoveEvent evt)
	{
	  Debug.Log($"isActiveAndEnabled {item.routerCommand}");
	  if (!isActiveAndEnabled) return;
      Handle(item, evt);
	}

    void Handle(InputOverride item, EventBase evt)
	{
      Debug.Log($"Handling {item.routerCommand}");
	  if (item.routerCommand != RouterCommand.None || item.OnInput != null)
	  {
		Debug.Log($"Valid Handling {item.routerCommand}");

		item.OnInput?.Invoke();
		HandleRouterCommand(item.routerCommand);
		evt.PreventDefault();
	  }
	}
  }
}