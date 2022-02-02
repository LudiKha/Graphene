﻿using Graphene;
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

  [RequireComponent(typeof(Plate))]
  public class ApplicationStateInterpreter : StateInterpreter<string>, IGrapheneInjectable, IGrapheneInitializable
  {

    [System.Serializable, Toggle("enabled")]
    public struct StateCommandHandle
    {
      public bool enabled;
      [SerializeField] public string stateCommand;
      public RouterCommand routerCommand;
      public UnityEvent OnStateEnter;
    }

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

      if (!plate)
      {
        plate = GetComponent<Plate>();
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
          if(command.OnStateEnter != null || command.routerCommand != RouterCommand.None)
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
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_WEBPLAYER
      Application.OpenURL("https://github.com/LudiKha/Graphene");
#else
        Application.Quit();
#endif
                break;
              default:
                break;
            }
            command.OnStateEnter?.Invoke();
            return true;
          }
        }
      }

      return false;
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