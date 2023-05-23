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

	[Bind("Title")]
    [field: SerializeField] public virtual string Title { get; set; }

	[Bind("HasContent")]
	public virtual bool HasContent => Render;

	[field: SerializeField] public bool Render { get; set; } = true;
    public Action onModelChange { get; set; }

	public override void Inject(Graphene graphene)
	{
	  base.Inject(graphene);
	  if(!this.plate)
		this.plate = GetComponent<Plate>();
	  if(!this.renderer)
		renderer = GetComponent<Renderer>();
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
	  if (!this.renderer)
		renderer = GetComponent<Renderer>();
	}

#if ODIN_INSPECTOR
	[Sirenix.OdinInspector.ResponsiveButtonGroup]
#endif
	public void ModelChange()
	{
	  onModelChange?.Invoke();
	}
  }
}