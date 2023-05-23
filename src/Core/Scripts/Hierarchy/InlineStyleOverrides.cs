using UnityEngine;
using UnityEngine.UIElements;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace Graphene
{
#if !ODIN_INSPECTOR
  public class HideLabelAttribute : Attribute { }
  public class ResponsiveButtonGroup : Attribute { }
  public class Button : Attribute { }
  public class EnumToggleButtonsAttribute : Attribute { }

  public class FoldoutGroupAttribute : Attribute
  {
    public FoldoutGroupAttribute(string name) { }
  }
#endif

  [System.Serializable]
  public class InlineStyleOverrides
  {
	const string positionModeRelativeClassNames = "flex-grow";
	const string positionModeAbsoluteClassNames = "absolute fill";
	const string showHideModeTransitionClassNames = "fade";

	[Tooltip("Adds any number of classes to the root element. Separated by space")]
	[SerializeField] protected string addClasses;

	[SerializeField, EnumToggleButtons, HideLabel] internal PickingMode pickingMode = PickingMode.Position;
	[SerializeField, EnumToggleButtons, HideLabel] internal PositionMode positionMode = PositionMode.None;
	[SerializeField, EnumToggleButtons, HideLabel] internal ShowHideMode showHideMode = ShowHideMode.Immediate;
	[SerializeField, FoldoutGroup("Detail")] FlexGrowOverride flexGrowOverride = new FlexGrowOverride();
	[SerializeField, FoldoutGroup("Detail")] JustifyOverride justifyContent = new JustifyOverride();
	[SerializeField, FoldoutGroup("Detail")] AlignItemsOverride alignItemsOverride = new AlignItemsOverride();
	[SerializeField, FoldoutGroup("Detail")] FlexDirectionOverride flexDirectionOverride = new FlexDirectionOverride();
	[SerializeField, FoldoutGroup("Detail")] WrapOverride wrapOverride = new WrapOverride();
	[SerializeField, FoldoutGroup("Detail")] WidthOverride widthOverride = new WidthOverride();
	[SerializeField, FoldoutGroup("Detail")] HeightOverride heightOverride = new HeightOverride();

	internal void Apply(VisualElement el)
	{
	  if (el == null)
		return;

	  el.AddMultipleToClassList(addClasses);

	  if (positionMode == PositionMode.Relative)
	  {
		el.RemoveMultipleFromClassList(positionModeAbsoluteClassNames);
		el.AddToClassList(positionModeRelativeClassNames);
	  }
	  else if (positionMode == PositionMode.Absolute)
	  {
		el.RemoveFromClassList(positionModeRelativeClassNames);
		el.AddMultipleToClassList(positionModeAbsoluteClassNames);
	  }

	  if (showHideMode == ShowHideMode.Immediate)
	  {
		el.RemoveFromClassList(showHideModeTransitionClassNames);
	  }
	  else if (showHideMode == ShowHideMode.Transition)
	  {
		el.AddToClassList(showHideModeTransitionClassNames);
		// When transitioning, we can only position absolutely, as the fadeout process will interfere with routing 
		el.AddMultipleToClassList(positionModeAbsoluteClassNames);
	  }

	  flexGrowOverride.TryApply(el);
	  justifyContent.TryApply(el);
	  //alignContent.TryApply(Root);
	  alignItemsOverride.TryApply(el);
	  flexDirectionOverride.TryApply(el);
	  wrapOverride.TryApply(el);
	  widthOverride.TryApply(el);
	  heightOverride.TryApply(el);
	}
  }
}