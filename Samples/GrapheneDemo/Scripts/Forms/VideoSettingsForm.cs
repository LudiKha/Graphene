
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Graphene
{
  [CreateAssetMenu(menuName = "UI/Forms/VideoSettingsForm")]
  public class VideoSettingsForm : Form
  {
    [Draw, BindBaseField("Value", "Fullscreen")]
    public BindableBool FullScreen = new BindableBool();

    [Draw(ControlType.Toggle)]
    public BindableBool AdaptiveResolution = new BindableBool();

    [Draw(ControlType.SliderInt)]
    public BindableInt VSync = new BindableInt();

    [Draw(ControlType.SelectField)]
    public BindableNamedInt Resolution = new BindableNamedInt();

    [Draw(ControlType.CycleField)]
    public BindableNamedInt UIScale = new BindableNamedInt { items = new List<string> { "Tiny", "Small", "Medium", "Large", "Extra Large" } };

    [SerializeField] PanelSettings settingsToScale;

    public override void Initialize(VisualElement container, Plate plate)
    {
      // Init controller
      VSync.Value = QualitySettings.vSyncCount;

      FullScreen.Value = IsFullScreen;

      Resolution.InitFromList(Screen.resolutions.Select(x => $"{x.width}x{x.height}@{x.refreshRate}"));

      // Subscriber
      FullScreen.OnValueChange += FullScreen_OnValueChange;
      VSync.OnValueChange += VSync_OnValueChange;
      Resolution.OnValueChange += Resolution_OnValueChange;
      UIScale.OnValueChange += UIScale_OnValueChange;
    }

    private void UIScale_OnValueChange(object sender, int e)
    {
      //float normalized = UIScale.normalizedValue * 2;
      settingsToScale.scale = 0.25f * (e + 1);
    }

    private void Resolution_OnValueChange(object sender, int e)
    {
      Resolution.Value = e;
      var resolutions = Screen.resolutions;
      Resolution selected = resolutions[e];
      Screen.SetResolution(selected.width, selected.height, FullScreen.Value);
    }

    private void VSync_OnValueChange(object sender, int e)
    {
      VSync.Value = e;
      QualitySettings.vSyncCount = e;
    }

#if UNITY_EDITOR
    // Assume the game view is focused.
    bool IsFullScreen { get { return UnityEditor.EditorWindow.focusedWindow != null ? UnityEditor.EditorWindow.focusedWindow.maximized : false; } }
#else
    bool IsFullScreen => Screen.fullScreen;
#endif

  private void FullScreen_OnValueChange(object sender, bool e)
    {
      FullScreen.Value = e;

#if UNITY_EDITOR
      UnityEditor.EditorWindow window = UnityEditor.EditorWindow.focusedWindow;
      // Assume the game view is focused.
      window.maximized = e;
#else
      Screen.fullScreen = e;
#endif
    }

    //[Sirenix.OdinInspector.Button, Sirenix.OdinInspector.HorizontalGroup]
    //public override void OnSubmit()
    //{
    //  throw new System.NotImplementedException();
    //}
    ////[Sirenix.OdinInspector.Button, Sirenix.OdinInspector.HorizontalGroup]
    //public override void OnCancel()
    //{
    //  throw new System.NotImplementedException();
    //}
  }
}