using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace Graphene
{
  using Elements;

  [RequireComponent(typeof(Plate))]
  public class Renderer : MonoBehaviour, IInitializable
  {
    [SerializeField] Plate plate;
    [field: SerializeField][Bind("Model")] public Object Model { get; set; }
    [SerializeField] protected TemplatePreset templates; public TemplatePreset Templates => templates;

    /// <summary>
    /// Overriding this will target a non-default content container (as defined in Plate)
    /// </summary>
    [SerializeField] protected string[] contentSelector = new string[] { };

    /// <summary>
    /// The ViewModel attached to this Renderer
    /// </summary>
    IModel viewModel;

    public void Initialize()
    {
      if (plate || (plate = GetComponent<Plate>()))
      {
        plate.renderer = this;
        plate.onRefreshStatic += Plate_onRefreshStatic;
        plate.onRefreshDynamic += Plate_onRefreshDynamic;

        if ((Model || (Model = GetComponent<IModel>() as Object)) && Model is IModel iModel)
        {
          viewModel = iModel;
          viewModel.onModelChange += Model_onModelChange;
        }
      }
    }

    internal void Plate_onRefreshStatic()
    {
      // Render the template components
      plate.Root.Query<TemplateRef>().ForEach(t => {
        t.Inject(null, plate, this);
        t.Render();
      }
      );

      // Bind the static template to the renderer
      Binder.BindRecursive(plate.Root, this, null, plate, true);
    }

    internal void Plate_onRefreshDynamic()
    {
      HardRefresh();
    }

    internal void Model_onModelChange()
    {
      plate.wasChangedThisFrame = true;
    }


    internal void RenderToContainer(VisualElement container)
    {
      // Initialize & render the form
      if (!Model)
        return;

      if (viewModel != null)
      {
        if (!viewModel.Render)
          return;

        viewModel.Initialize(container, plate);
      }

      // Render & bind the dynamic items
      RenderUtils.DrawDataContainer(plate, container, Model, templates);

      if (viewModel != null)
        viewModel.onModelChange?.Invoke();
    }

    #region Public API
    public void Draw()
    {
      // Render & bind the dynamic items
      RenderUtils.DrawDataContainer(plate, GetDrawContainer(), Model, templates);

      if (viewModel != null)
        viewModel.onModelChange?.Invoke();
    }

    public void Draw(object model)
    {
      RenderUtils.DrawDataContainer(plate, GetDrawContainer(), in model, templates);

      if (model is IModel iModel)
        iModel.onModelChange?.Invoke();
    }

    [Sirenix.OdinInspector.Button]
    public void HardRefresh()
    {
      RenderToContainer(GetDrawContainer());
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
