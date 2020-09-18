using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace Graphene
{
  [RequireComponent(typeof(Plate))]
  public class Renderer : MonoBehaviour, IInitializable
  {
    [SerializeField] Plate plate;
    [SerializeField, Bind("Model")] protected Object model; public Object Model => model;    
    [SerializeField] protected ComponentTemplates templates; public ComponentTemplates Templates => templates;

    /// <summary>
    /// Overriding this will target a non-default content container (as defined in Plate)
    /// </summary>
    [SerializeField] protected string[] contentSelector = new string[] {  };

    public void Initialize()
    {
      if (plate || (plate = GetComponent<Plate>()))
      {
        plate.onRefreshStatic += Plate_onRefreshStatic;
        plate.onRefreshDynamic += Plate_onRefreshDynamic;
      }
    }

    private void Plate_onRefreshStatic()
    {
      // Bind the static template to the renderer
      Binder.BindRecursive(plate.Root, this, null, plate, true);
    }

    private void Plate_onRefreshDynamic()
    {

      RenderToContainer(GetDrawContainer());
    }

    internal void RenderToContainer(VisualElement container)
    {
      // Initialize & render the form
      if (!model)
        return;

      if (model is IModel iModel)
      {
        if (!iModel.Render)
          return;

        iModel.Initialize(container, plate);
      }


      // Render & bind the dynamic items
      RenderUtils.DrawDataContainer(plate, container, model, templates);
    }

    #region Public API
    public void Draw()
    {
      // Render & bind the dynamic items
      RenderUtils.DrawDataContainer(plate, GetDrawContainer(), model, templates);
    }

    public void Draw(object model)
    {
      RenderUtils.DrawDataContainer(plate, GetDrawContainer(), in model, templates);
    }
    #endregion

    #region Helper Methods
    internal VisualElement GetDrawContainer()
    {
      // Default use the plate's default container
      VisualElement container = plate.ContentContainer;

      if (contentSelector.Length > 0)
        container = plate.GetVisualElement(contentSelector);

      return container;
    }
    #endregion
  }
}