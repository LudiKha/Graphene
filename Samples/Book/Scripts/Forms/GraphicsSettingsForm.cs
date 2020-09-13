using UnityEngine;
using System.Collections;


namespace Graphene
{
  [CreateAssetMenu(menuName = "UI/Forms/GraphicsSettingsForm")]
  public class GraphicsSettingsForm : Form
  {
    [Draw(ControlType.CycleField), BindBaseField("Value", "quality level")]
    public BindableNamedInt QualityLevel;

    [Draw(ControlType.SliderInt), BindInt("Value", 1, 0, 4, "anti-aliasing")]
    public BindableInt AntiAliasing;


    public override void InitModel()
    {
      QualityLevel.Value = QualitySettings.GetQualityLevel();
      AntiAliasing.Value = QualitySettings.antiAliasing;

      QualityLevel.OnValueChange += QualityLevel_OnValueChange;
      AntiAliasing.OnValueChange += AntiAliasing_OnValueChange;
    }

    private void AntiAliasing_OnValueChange(object sender, int e)
    {
      QualitySettings.antiAliasing = e;
    }

    private void QualityLevel_OnValueChange(object sender, int e)
    {
      QualitySettings.SetQualityLevel(e);
    }
  }
}