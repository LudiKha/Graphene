using Graphene.ViewModel;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Graphene.ViewModel
{
  public class NavViewModel : ViewModelComponent
  {
    [Bind("HasContent")] public bool HasContent => Routes != null && Routes.Count > 0;

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
    public List<string> Routes =  new List<string>();

    public override void Initialize(VisualElement container, Plate plate)
    {
      switch (renderMode)
	  {
		case RenderMode.SiblingsWithState:
          CreateBindableObjectsFromSiblingsWithState(OverridePlate ?? plate);
          break;
      }
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