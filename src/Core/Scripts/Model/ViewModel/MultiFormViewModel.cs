using JetBrains.Annotations;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Lumin;
using UnityEngine.UIElements;

namespace Graphene.ViewModel
{

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
	  if (buttonsViewModel && buttonsViewModel.Items.Count > 1)
	  {
		buttonsViewModel.Items[0].SetEnabled(dirty);
		buttonsViewModel.Items[1].SetEnabled(dirty);
	  }
	}
  }
}