using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace Graphene
{
  using Elements;

  [RequireComponent(typeof(Plate))]
  public class Renderer : MonoBehaviour, IGrapheneInitializable
  {
    [SerializeField] Plate plate; public Plate Plate => plate;
    [field: SerializeField]/*[Bind("Model")]*/ public Object Model { get; set; }
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
        plate.onRefreshStatic += Plate_onRefreshStatic;
        plate.onRefreshDynamic += Plate_onRefreshDynamic;

        if ((Model && Model is IModel || (Model = GetComponent<IModel>() as Object)))
        {
          if (Model is IModel iModel)
            SetModel(iModel);
        }
      }
    }

    void SetModel(IModel newViewModel)
    {
      // Unsubscribe to old
      if(viewModel != null)
        viewModel.onModelChange -= Model_onModelChange;

      // Subscribe to new
      viewModel = newViewModel;
      viewModel.onModelChange = Model_onModelChange;
    }

    internal void Plate_onRefreshStatic()
    {     
      // Render the template components
      plate.Root.Query<TemplateRef>().ForEach(t => {
        t.Inject(null, plate, this);
        t.Render();
      }
      );

      // Initialize the ViewModel
      if (viewModel != null)
      {
        try
        {
          viewModel.Initialize(GetDrawContainer(), plate);
        }
        catch(System.Exception e)
        {
          Debug.LogException(e, this);
        }

        if (!viewModel.Render)
          return;
      }

      // Bind the static template to the viewmodel
      BindStatic(viewModel);
    }

    internal void BindStatic(IModel viewModelContext)
    {
      if (viewModelContext is ICustomBindContext customBindContext)
        Binder.BindRecursive(plate.Root, customBindContext.GetCustomBindContext, null, plate, true);
      else if (viewModelContext != null)
        Binder.BindRecursive(plate.Root, viewModelContext, null, plate, true);
      // Bind empty model -> routes (NOTE: Not that great of an option)
      else
      {
        Binder.BindRecursive(plate.Root, this, null, plate, true);
      }
    }

    // Callbacks
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
      if (viewModel == null)
        return;

      if(viewModel is ICustomDrawContext customDrawContext)
        RenderUtils.DrawDataContainer(plate, container, customDrawContext.GetCustomDrawContext, templates);
      else
        RenderUtils.DrawDataContainer(plate, container, viewModel, templates);

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
    
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.ResponsiveButtonGroup("Actions")]
#elif NAUGHTY_ATTRIBUTES
    [NaughtyAttributes.Button]
#endif
    public void HardRefresh()
    {
      ClearContent();
      RenderToContainer(GetDrawContainer());
    }


#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.ResponsiveButtonGroup("Actions")]
#elif NAUGHTY_ATTRIBUTES
    [NaughtyAttributes.Button]
#endif
    public void ClearContent()
    {
      GetDrawContainer()?.Clear();
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
