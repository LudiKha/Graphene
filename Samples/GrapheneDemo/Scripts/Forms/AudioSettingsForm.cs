using Kinstrife.Core.ReflectionHelpers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Graphene
{

  [System.Serializable]
  [CreateAssetMenu(menuName = "UI/Forms/AudioSettingsForm")]
  public class AudioSettingsForm : Form
  {
    [Draw(ControlType.Label)]
    public string header = "AudioMixer";

    [Draw] 
    public BindableFloat MasterVolume;
    [Draw]
    public BindableFloat SFXVolume;
    [Draw]
    public BindableFloat MusicVolume;

    [Draw(ControlType.Label), Bind("Info")]
    public string info = "Other";

    [Draw(ControlType.Label)]
    public BindableBaseField<string> infoBindable = new BindableBaseField<string> { Value = "Information", Label = "Informaçione" };

    [Draw, BindFloat("Value", 1, 0, 1, "StereoBind", true)]
    public BindableFloat Stereo;

    [Draw(controlType = ControlType.Slider), BindFloat("Value", 1, 0, 3, "Volume1234", true)]
    public float GeneralVolume = 5;


    [Draw(ControlType.CycleField), BindBaseField("Value", "Named Int Path", true)]
    public BindableNamedInt Mode;

    [Draw(ControlType.TextField), BindString("Value", "Input 'ere", label: "Dat label tho")]
    public string twoWayText = "TwoWayText";


    [Draw(ControlType.Toggle), BindBaseField("Value", "Da label")]
    public bool toggle = true;

    public override void InitModel()
    {
      Mode.InitFromEnum<StereoTargetEyeMask>();
    }

  }
}