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
	void Submit();
	void Cancel();

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

	[Bind("Submit")]
	public BindableObject submitBinding =  new BindableObject();
	[Bind("Cancel")]
	public BindableObject cancelBinding =  new BindableObject();

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
	#endregion

	Router<string> router;
	#region LifeCycle
	void Awake()
	{
	  //submitBinding = Submit;
	  //cancelBinding = Cancel;
	  submitBinding.OnClick.AddListener(Submit);
	  cancelBinding.OnClick.AddListener(Cancel);
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

	  // Show confirmation dialog here
	  if (IsModelDirty)
		return true;
	  
	  return false;
	}

	bool IStateInterpreter.TryCatch(object state) => TryCatch((string)state);

	[ResponsiveButtonGroup]
	public abstract void Cancel();
	[ResponsiveButtonGroup]
	public abstract void Submit();

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
	  //OnSubmit?.SetEnabled(dirty);
	  //OnCancel?.SetEnabled(dirty);
	}
	public void UpdateFormButtonsState(bool enabled, bool active)
	{
	  SetButtonsDirty(enabled);
	  submitButton?.SetActive(active);
	  cancelButton?.SetActive(active);
	  submitBinding?.SetActive(active);
	  cancelBinding?.SetActive(active);
	  //OnSubmit?.SetActive(active);
	  //OnCancel?.SetActive(active);
	}

	public virtual bool CanSubmit() => true;
	public virtual bool CanCancel() => true;
  }
  public abstract class FormViewModel<T> : FormViewModel, ICustomDrawContext
  {
	[ShowInInspector] T originalCached;
	[System.NonSerialized, ShowInInspector, InlineEditor] public T viewModelCopy;
	object ICustomDrawContext.GetCustomDrawContext => viewModelCopy;

	public abstract void UpdateSourceData();
  }
}