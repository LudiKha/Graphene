
using UnityEngine;
using UnityEngine.UIElements;

namespace Graphene
{
  [CreateAssetMenu(menuName = "UI/Forms/AudioSettingsForm")]
  public class AudioSettingsForm : Form
  {
    [Draw, Bind]
    public string VolumeHeader = "Volume";

    [Draw, BindFloat("", 0.5f, 0,1, "Master Volume")] 
    public float MasterVolume = 0.5f;

    [Draw, BindFloat("", 0.5f, 0, 1, "SFX Volume")]
    public float SFXVolume;
    [Draw, BindFloat("", 0.5f, 0, 1, "Music Volume")]
    public float MusicVolume;

    [Draw(ControlType.Label), Bind("Info")]
    public string info = "Other";

    [Draw(ControlType.TextField)]
    public BindableBaseField<string> infoBindable = new BindableBaseField<string> { Value = "Information", Label = "Informaçione" };

    [Draw(ControlType.CycleField), BindBaseField("", "Named Int Path", true)]
    public BindableNamedInt Mode;

    [Draw(ControlType.TextField), BindString("Value", "Input 'ere", label: "Dat label tho")]
    public string twoWayText = "TwoWayText";


    public override void Initialize(VisualElement container, Plate plate)
    {
      Mode.InitFromEnum<StereoTargetEyeMask>();
    }

  }
}