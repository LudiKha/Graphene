﻿using System.Collections;
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
        plate.onRefreshStatic += RebindStatic;
        plate.onRefreshDynamic += HardRefresh;

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

    public void RebindStatic()
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
      if (viewModelContext is ICustomBindContext customBindContext && customBindContext != null)
        Binder.BindRecursive(plate.Root, customBindContext.GetCustomBindContext, null, plate, true);
      if (viewModelContext != null)
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

      if (!templates)
		templates = GetTemplatesFromParentsRecursive(this);

      //else
        RenderUtils.DrawDataContainer(plate, container, viewModel, templates);
      // if (viewModel is ICustomDrawContext customDrawContext)
      //RenderUtils.DrawDataContainer(plate, container, customDrawContext.GetCustomDrawContext, templates);

      viewModel.Refresh(container);
      viewModel.onModelChange?.Invoke();
    }

    TemplatePreset GetTemplatesFromParentsRecursive(Renderer current)
    {
      if(current.templates)
        return current.templates;
      else if(current.plate?.Parent?.Renderer)
        return GetTemplatesFromParentsRecursive(current.plate.Parent.Renderer);
      else
        return null;
    }

    #region Public API
        
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.ResponsiveButtonGroup("Actions")]
#elif NAUGHTY_ATTRIBUTES
    [NaughtyAttributes.Button]
#endif
    public void Refresh()
    {
      viewModel?.Refresh(GetDrawContainer());
    }

	bool isRefreshing;
#if ODIN_INSPECTOR
	[Sirenix.OdinInspector.ResponsiveButtonGroup("Actions")]
#elif NAUGHTY_ATTRIBUTES
    [NaughtyAttributes.Button]
#endif
	public void HardRefresh()
	{
      if (isRefreshing)
      {
        Debug.LogError($"Recursing refresh {name}", this);
        return;
      }
      isRefreshing = true;
	  ClearContent();
	  RenderToContainer(GetDrawContainer());
      isRefreshing = false;
	}

#if ODIN_INSPECTOR
	[Sirenix.OdinInspector.ResponsiveButtonGroup("Actions")]
#elif NAUGHTY_ATTRIBUTES
    [NaughtyAttributes.Button]
#endif
    public void ClearContent()
    {
	  plate.RefreshContentContainer();
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
