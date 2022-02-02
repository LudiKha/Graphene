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

    [Bind("Routes")]
    public List<string> Routes =  new List<string>();

    public void Initialize(VisualElement container, Plate plate)
    {
      switch (renderMode)
      {
        case RenderMode.SiblingsWithState:
          CreateBindableObjectsFromSiblingsWithState(plate);
          break;
      }
    }

    void CreateBindableObjectsFromSiblingsWithState(Plate plate)
    {
      this.Routes.Clear();
      foreach (var sibling in plate.Parent.Children)
      {
        if (!sibling || !(sibling.stateHandle is StringStateHandle stringStateHandle))
          continue;

        if (stringStateHandle)
        {
          Routes.Add(stringStateHandle.StateID);
        }
      }
    }
  }
}