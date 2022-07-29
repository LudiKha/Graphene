using Graphene.ViewModel;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Graphene.ViewModel
{
  public class NavViewModel : MonoBehaviour, IModel
  {
    [Bind("Title")]
    [field: SerializeField] public string Title { get; private set; }


    [Bind("HasContent")] public bool HasContent => Routes != null && Routes.Count > 0;

    public enum RenderMode
    {
      None,
      Siblings,
      SiblingsWithState
    }

    [field: SerializeField]
    public RenderMode renderMode { get; private set; } = NavViewModel.RenderMode.SiblingsWithState;

    public bool Render => true;

    public Action onModelChange { get; set; }


    [field: SerializeField] public Plate OverridePlate { get; private set; }

    [Bind("Routes")]
    public List<string> Routes =  new List<string>();

    public void Initialize(VisualElement container, Plate plate)
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
      foreach (var sibling in plate.Parent.Children)
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