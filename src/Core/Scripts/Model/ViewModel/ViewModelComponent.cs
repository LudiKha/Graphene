using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Graphene.ViewModel
{
  [RequireComponent(typeof(Renderer))]
  public abstract class ViewModelComponent : GrapheneComponent, IModel
  {
	[SerializeField, HideInInspector] protected new Renderer renderer; public Renderer Renderer => renderer;
	[SerializeField, HideInInspector] protected Plate plate; public Plate Plate => plate;

	[Bind("Title", BindingMode.OneWay)]
    [field: SerializeField] public virtual string Title { get; set; }

	[Bind("HasContent")]
	public virtual bool HasContent => Render;

	[field: SerializeField] public bool Render { get; set; } = true;
    public Action onModelChange { get; set; }

	public bool PlateIsActive => plate && plate.IsActive;

	protected override void Awake()
	{
	  base.Awake();
	  if (!this.plate)
		this.plate = GetComponent<Plate>();
	  if (!this.renderer)
		renderer = GetComponent<Renderer>();
	}

	public override void Inject(Graphene graphene)
	{
	  base.Inject(graphene);
	  if(!this.plate)
		this.plate = GetComponent<Plate>();
	  if(!this.renderer)
		renderer = GetComponent<Renderer>();

	  plate.onShow.AddListener(OnShow);
	  plate.onHide.AddListener(OnHide);
	}

	public abstract void Initialize(VisualElement container, Plate plate);

    // For enabled
    void Start()
    {
    }

	protected virtual void OnShow() { }
	protected virtual void OnHide() { }

#if ODIN_INSPECTOR
	[Sirenix.OdinInspector.ResponsiveButtonGroup]
#endif
	public virtual void ModelChange()
	{
	  onModelChange?.Invoke();
	}
#if ODIN_INSPECTOR
	[Sirenix.OdinInspector.ResponsiveButtonGroup]
#endif
	public virtual void Refresh(VisualElement container)
	{
	}
  }
}