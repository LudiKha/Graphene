using JetBrains.Annotations;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Lumin;
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

  [RequireComponent(typeof(Renderer))]
  public abstract class ViewModelComponent : GrapheneComponent, IModel
  {
	[SerializeField, HideInInspector] protected Plate plate;

	[Bind("Title")]
    [field: SerializeField] public virtual string Title { get; set; }
    [field: SerializeField] public bool Render { get; set; } = true;
    public Action onModelChange { get; set; }

	public override void Inject(Graphene graphene)
	{
	  base.Inject(graphene);
	  if(!this.plate)
		this.plate = GetComponent<Plate>();
	}

	public abstract void Initialize(VisualElement container, Plate plate);

    // For enabled
    void Start()
    {
    }

	void Awake()
	{
	  if(!plate)
		plate = GetComponent<Plate>();
	}
  }

  public abstract class FormViewModel : ViewModelComponent, IFormViewModel, IStateInterpreter
  {
	#region Bindables
	//[Bind("Title")] public override string Title => originalCached?.Title;

	public event System.Action onSubmit;
	public event System.Action onCancel;

	[Bind("Submit")]
	public System.Action OnSubmit => onSubmit;
	[Bind("Cancel")]
	public System.Action OnCancel => onCancel;

#if ODIN_INSPECTOR
	[field: ShowInInspector]
#endif
	[Bind("Dirty")]
	public bool IsModelDirty { get; set; }
	[field: SerializeField] public bool BlockRoutingOnDirty { get; set; } = true;

	[field: SerializeField] public bool HideButtons { get; set; } = false;
	#endregion

	#region VisualElement
	Button submitButton;
	Button cancelButton;
	#endregion

	#region State
	bool initialized;
	#endregion

	#region LifeCycle
	void OnEnable()
	{
	  if (!initialized)
		return;

	  Subscribe();
	  graphene.Router.RegisterInterpreter(this);
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

	  plate ??= GetComponent<Plate>();
	  //plate.onShow.AddListener(OnShow);
	  //plate.onHide.AddListener(OnHide);
	  onSubmit += Submit;
	  onCancel += Cancel;
	}
	void UnSubscribe()
	{
	  subscribed = false;

	  plate ??= GetComponent<Plate>();
	  //plate.onShow.RemoveListener(OnShow);
	  //plate.onHide.RemoveListener(OnHide);
	  onSubmit = null;
	  onCancel = null;
	}
	#endregion

	public bool TrySubmit()
	{
	  Submit();
	  return true;
	}

	public bool TryCancel()
	{
	  Cancel();
	  return true;
	}

	bool IStateInterpreter.TryCatch(object state)
	{
	  if(state is string str)
	  {
		if (str == "submit")
		  return TrySubmit();
		else if (str == "cancel")
		  return TryCancel();
	  }

	  // Show confirmation dialog here
	  if (IsModelDirty)
		return true;
	  else
		return false;
	}

	[ResponsiveButtonGroup]
	public abstract void Cancel();
	[ResponsiveButtonGroup]
	public abstract void Submit();

	void ModelChange()
	{
	  MarkDirty(true);
	  onModelChange?.Invoke();
	}

	void MarkDirty(bool dirty)
	{
	  IsModelDirty = dirty;
	  SetButtonsDirty(dirty);

	  // Block router
	  if (BlockRoutingOnDirty && graphene.Router)
	  {
		if (dirty)
		  graphene.Router.TryBlock(this);
		else
		  graphene.Router.TryUnblock(this);
	  }
	}

	[Button]
	protected virtual void SetButtonsDirty(bool dirty)
	{
	  submitButton?.SetEnabled(dirty);
	  cancelButton?.SetEnabled(dirty);
	}
	public void UpdateFormButtonsState(bool enabled, bool active)
	{
	  SetButtonsDirty(enabled);
	  submitButton?.SetActive(active);
	  cancelButton?.SetActive(active);
	}
  }

  public abstract class FormViewModel<T> : FormViewModel, ICustomDrawContext
  {
	[ShowInInspector] T originalCached;
	[System.NonSerialized, ShowInInspector, InlineEditor] public T viewModelCopy;
	object ICustomDrawContext.GetCustomDrawContext => viewModelCopy;

	public abstract void UpdateSourceData();
  }

  public class MultiFormViewModel : FormViewModel, IStateInterpreter, IFormViewModel
  {
	[SerializeField] GenericModelBehaviour buttonsViewModel;

#if ODIN_INSPECTOR
	[ShowInInspector]
#endif
	IEnumerable<IFormViewModel> childForms;
	bool formsInitialized;

	#region LifeCycle
	public override void Initialize(VisualElement container, Plate plate)
	{
	  if (!formsInitialized)
	  {
		var forms = transform.GetComponentsInChildren<IFormViewModel>().ToList();
		forms.Remove(this);
		childForms = forms;
		foreach (var item in childForms)
		{
		  item.BlockRoutingOnDirty = false;
		  item.HideButtons = true;
		  item.UpdateFormButtonsState(false, false);
		}
		formsInitialized = true;
	  }

	  if (buttonsViewModel)
	  {
		buttonsViewModel.Items.Clear();
		buttonsViewModel.Items.Add(new BindableObject
		{
		  Name = "SUBMIT",
		  customName = "SubmitButton",
		  addClass = "submit",
		  route = "index"
		});
		buttonsViewModel.Items.Add(new BindableObject
		{
		  Name = "CANCEL",
		  customName = "CancelButton",
		  addClass = "cancel",
		  route = "index"
		});

		buttonsViewModel.Items[0].OnClick.AddListener(Submit);
		buttonsViewModel.Items[1].OnClick.AddListener(Cancel);
	  }
	}
	#endregion

	public override void Cancel()
	{
	  foreach (var form in childForms)
	  {
		if (form.IsModelDirty)
		  form.Cancel();
	  }
	}

	public override void Submit()
	{
	  foreach (var form in childForms)
	  {
		if (form.IsModelDirty)
		  form.Submit();
	  }
	}

	protected override void SetButtonsDirty(bool dirty)
	{
	  if (buttonsViewModel)
	  {
		buttonsViewModel.Items[0].SetEnabled(dirty);
		buttonsViewModel.Items[1].SetEnabled(dirty);
	  }
	}
  }
}