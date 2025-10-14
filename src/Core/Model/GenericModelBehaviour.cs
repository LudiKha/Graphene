using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Graphene.ViewModel
{
  public abstract class GenericModelBehaviour<T> : ViewModelComponent
  {
	public override bool HasContent => items?.Count > 0; 

	[Draw(ControlType.Button)]
    [SerializeField]
    protected List<T> items = new List<T>();

    public abstract List<T> Items { get; }
  }

  public class GenericModelBehaviour : GenericModelBehaviour<BindableObject>
  {
	[SerializeField] bool validateRouteAdresses = true;
	[SerializeField] bool hideInvalidAdress;
	protected StringRouter router;

	public override List<BindableObject> Items => items;

	public override void Inject(Graphene graphene)
	{
	  base.Inject(graphene);
	  if (graphene.Router is StringRouter stringRouter)
		router = stringRouter;
	}

	protected override void OnShow()
	{
	  base.OnShow();
	  ValidateAddresses();
	}

	void ValidateAddresses()
	{
	  if (router && validateRouteAdresses)
	  {
		foreach (var item in items)
		{
		  if (!string.IsNullOrEmpty(item.route))
		  {
			bool exists = router.ValidateAddress(item.route);
			item.SetEnabled(exists);			
			//item.isEnabled = exists;
			if (hideInvalidAdress)
			  item.SetShow(exists);
		  }
		}
	  }
	}

	//public override void Refresh(VisualElement container)
	//{
	//  if (router && validateRouteAdresses)
	//  {
	//	foreach (var item in items)
	//	{
	//	  if (!string.IsNullOrEmpty(item.route))
	//	  {
	//		bool exists = router.ValidateAddress(item.route);
	//		item.isEnabled = exists;
	//		if (hideInvalidAdress)
	//		  item.isShown = exists;
	//	  }
	//	}
	//  }

	//  base.Refresh(container);
	//}

	public override void Initialize(VisualElement container, Plate plate)
    {
	  ValidateAddresses();
    }
  }
}
