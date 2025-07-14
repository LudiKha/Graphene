using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Graphene.ViewModel
{
  public class NavViewModel : GenericModelBehaviour
  {
	[Bind("HasContent")]
	public override bool HasContent => Routes != null && Routes.Count > 0 || base.HasContent;


	[field: SerializeField]
	public bool TitleFromRoutes { get; private set; }

	public enum RenderMode
	{
	  Manual,
	  Siblings,
	  SiblingsWithState
	}

	[field: SerializeField]
	public RenderMode renderMode { get; private set; } = NavViewModel.RenderMode.SiblingsWithState;

	[field: SerializeField] public Plate OverridePlate { get; private set; }

	[Bind("Routes")]
	public List<string> Routes = new List<string>();

	public override void Initialize(VisualElement container, Plate plate)
	{
	  switch (renderMode)
	  {
		case RenderMode.SiblingsWithState:
		  CreateBindableObjectsFromSiblingsWithState(OverridePlate ?? plate);
		  break;
	  }
	}

	public override void Inject(Graphene graphene)
	{
	  base.Inject(graphene);
	  if(router)
		router.onStateChange += Router_onStateChange;
	}
	private void Router_onStateChange(string newState)
	{
	  if (TitleFromRoutes)
	  {
		var index = Routes.IndexOf(newState);
		if (index >= 0)
		  Title = Routes[index].ToUpper();
		ModelChange();
	  }
	}
	void OnDestroy()
	{
	  if (router)
		router.onStateChange -= Router_onStateChange;
	}

	void CreateBindableObjectsFromSiblingsWithState(Plate plate)
	{
	  this.Routes.Clear();

	  IReadOnlyList<Plate> children = plate.Parent ? plate.Parent.Children : null;

	  if (children == null || children.Count == 0)
		return;

	  foreach (var sibling in children)
	  {
		if (!sibling || !(sibling.StateHandle is StringStateHandle stringStateHandle))
		  continue;

		if (stringStateHandle)
		{
		  Routes.Add(stringStateHandle.StateID);
		}
	  }
	}
  }
}