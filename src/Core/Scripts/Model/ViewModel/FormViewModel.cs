using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UIElements;

namespace Graphene.ViewModel
{
  public interface IFormViewModel : IModel
  {
	bool IsModelDirty { get; }
	bool BlockRoutingOnDirty { get; set; }
	bool HideButtons { get; set; }

	bool PlateIsActive { get; }

	void Submit();
	void Cancel();
	void Reset();

	void PromptReset();

	public event System.Action onSubmit;
	public event System.Action onCancel;

	void UpdateFormButtonsState(bool enabled, bool active);
  }

  public abstract class FormViewModel : ViewModelComponent, IFormViewModel, IStateInterpreter<string>, IGrapheneInitializable
  {
	#region Bindables
	//[Bind("Title")] public override string Title => originalCached?.Title;

	public event System.Action onSubmit;
	public event System.Action onCancel;
	public event System.Action onReset;

	[Bind("Submit")]
	public BindableObject submitBinding =  new BindableObject();
	[Bind("Cancel")]
	public BindableObject cancelBinding =  new BindableObject();
	[Bind("Reset")]
	public BindableObject resetBinding = new BindableObject();

	bool initialized;

#if ODIN_INSPECTOR
	[field: ShowInInspector]
#endif
	public bool IsModelDirty { get; set; }
	[field: SerializeField] public bool BlockRoutingOnDirty { get; set; } = true;

	[field: SerializeField] public bool HideButtons { get; set; } = false;
	#endregion

	#region VisualElement
	Button submitButton;
	Button cancelButton;
	Button resetButton;
	#endregion

	Router<string> router;
	#region LifeCycle
	protected override void Awake()
	{
	  base.Awake();
	  //submitBinding = Submit;
	  //cancelBinding = Cancel;
	  submitBinding.OnClick.AddListener(Submit);
	  cancelBinding.OnClick.AddListener(Cancel);
	  resetBinding.OnClick.AddListener(Reset);
	  MarkDirty(false);
	}

	public virtual void Initialize()
	{
	  router = graphene.Router as Router<string>;
	  router.RegisterInterpreter(this);
	  initialized = true;
	}

	void OnEnable()
	{
	  if (!initialized)
		return;
	  router.RegisterInterpreter(this);
	  Subscribe();
	}

	void OnDisable()
	{
	  UnSubscribe();
	  graphene?.Router.UnregisterInterpreter(this);
	}
	bool subscribed;

	void Subscribe()
	{
	  if (subscribed)
		return;
	  subscribed = true;
	}
	void UnSubscribe()
	{
	  subscribed = false;
	}
	#endregion

	public bool TrySubmit()
	{
	  if (!enabled)
		return false;

	  Submit();
	  return true;
	}

	public bool TryCancel()
	{
	  Cancel();
	  return true;
	}

	public bool TryReset()
	{
	  Reset();
	  return true;
	}
	bool IStateInterpreter.CanCatch(object state) => TryCatch((string)state);
	public bool CanCatch(string state)
	{
	  if (!enabled || !gameObject.activeInHierarchy)
		return false;

	  if (state == "submit")
		return true;
	  else if (state == "cancel")
		return true;
	  else if (state == "reset")
		return true;
	  return false;
	}
	bool IStateInterpreter.TryCatch(object state) => TryCatch((string)state);

	public bool TryCatch(string state)
	{
	  if (!enabled || !gameObject.activeInHierarchy)
		return false;


	  if (state == "submit")
	  {
		TrySubmit();
		return true;
	  }
	  else if (state == "cancel")
	  {
		TryCancel();
		return true;
	  }
	  else if (state == "reset")
	  {
		TryReset();
		return true;
	  }

	  // Show confirmation dialog here
	  if (IsModelDirty)
		return true;
	  
	  return false;
	}


	[ResponsiveButtonGroup]
	public abstract void Cancel();
	[ResponsiveButtonGroup]
	public abstract void Submit();

	public abstract void Reset();
	public abstract void PromptReset();

	protected void MarkDirty(bool dirty)
	{
	  IsModelDirty = dirty;
	  SetButtonsDirty(dirty);

	  // Block router
	  if (BlockRoutingOnDirty && graphene?.Router)
	  {
		if (dirty)
		  graphene.Router.TryBlock(this);
		else
		  graphene.Router.TryUnblock(this);
	  }
	  ModelChange();
	}

	[Button]
	protected virtual void SetButtonsDirty(bool dirty)
	{
	  submitButton?.SetEnabled(dirty && CanSubmit());
	  cancelButton?.SetEnabled(dirty && CanCancel());
	  submitBinding?.SetEnabled(dirty && CanSubmit());
	  cancelBinding?.SetEnabled(dirty && CanCancel());


	  resetButton?.SetEnabled(CanReset());
	  resetBinding?.SetEnabled(CanReset());
	}
	public void UpdateFormButtonsState(bool enabled, bool active)
	{
	  SetButtonsDirty(enabled);
	  submitButton?.SetActive(active);
	  cancelButton?.SetActive(active);
	  submitBinding?.SetActive(active);
	  cancelBinding?.SetActive(active);


	  resetButton?.SetActive(active);
	  resetBinding?.SetActive(active);
	}

	public virtual bool CanSubmit() => true;
	public virtual bool CanCancel() => true;
	public virtual bool CanReset() => true;
  }

  public abstract class FormViewModel<T> : FormViewModel, ICustomDrawContext
  {
	[ShowInInspector] T originalCached;
	[System.NonSerialized, ShowInInspector, InlineEditor] public T viewModelCopy;
	object ICustomDrawContext.GetCustomDrawContext { get => viewModelCopy; }

	public abstract void UpdateSourceData();
  }
}