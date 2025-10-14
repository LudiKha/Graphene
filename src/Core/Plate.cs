using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace Graphene
{
  using Elements;
  using UnityEngine.Profiling;

  /// <summary>
  /// Determines the <see cref="Position"> mode for a plate when the hierarchy is built
  /// </summary>
  public enum PositionMode
  {
	/// <summary>
	/// Don't override the <see cref="Position"/> mode
	/// </summary>
	None,
	/// <summary>
	/// Sets the plate to <see cref="Position.Relative"/>
	/// </summary>
	Relative,
	/// <summary>
	/// Sets the plate to <see cref="Position.Absolute"/>
	/// </summary>
	Absolute
  }

  /// <summary>
  /// Determines how the binding system refreshes bindings on this plate
  /// </summary>
  public enum BindingRefreshMode
  {
	None,
	/// <summary>
	/// Bindings are refreshed every update of the BindingsManager.
	/// </summary>
	Continuous,
	/// <summary>
	/// Bindings only update when the model triggers its ModelChanged callback.
	/// </summary>
	ModelChange
  }

  /// <summary>
  /// Determines whether the plate shows/hides immediately or with a transition.
  /// </summary>
  public enum ShowHideMode
  {
	Immediate,
	Transition
  }
  ///<summary>
  /// <para>A `Plate` represents a view controller in the VisualTree, and is used when by Graphene to the hierarchy, its states and views.</para> 
  /// <para><see href="https://github.com/LudiKha/Graphene#plates">Read more in the online documentation</see></para>
  ///</summary>
  [DisallowMultipleComponent]
  public class Plate : GrapheneComponent, IGrapheneInitializable, IDisposable
  {
    #region Constants

    /// <summary>
    /// The default class name for a plate root element.
    /// </summary>
    public const string plateClassName = "plate";
    /// <summary>
    /// Selector for the content view.
    /// </summary>
    public const string contentViewSelector = "Content";
    /// <summary>
    /// Selector for the children view.
    /// </summary>
    public const string childViewSelector = "Children";
    #endregion

    #region Inspector/Authoring
    /// <summary>
    /// The VisualTreeAsset used to instantiate the plate's UI.
    /// </summary>
    [SerializeField, OnValueChanged(nameof(OnChangeDocument))]
    VisualTreeAsset visualAsset;
    /// <summary>
    /// Gets or sets the VisualTreeAsset for this plate.
    /// </summary>
    public VisualTreeAsset VisualTreeAsset { get => visualAsset; set => SetVisualTreeAsset(value); }

    [SerializeField, HideInInspector]
    VisualTreeAsset cachedAsset;

    /// <summary>
    /// Determines how picking is handled for the root element.
    /// </summary>
    [SerializeField]
    PickingMode pickingMode = PickingMode.Position;

    /// <summary>
    /// Controls how bindings are refreshed.
    /// </summary>
    [SerializeField]
    public BindingRefreshMode bindingRefreshMode = BindingRefreshMode.ModelChange;

    /// <summary>
    /// Inline style overrides for the plate.
    /// </summary>
    [SerializeField, FoldoutGroup("Styles Overrides")]
    InlineStyleOverrides styleOverrides = new InlineStyleOverrides() { positionMode = PositionMode.Relative };

    /// <summary>
    /// List of style overrides for individual views.
    /// </summary>
    [SerializeField, FoldoutGroup("Styles Overrides"), ListDrawerSettings(HideAddButton = true, HideRemoveButton = true, ListElementLabelName = "Id")]
    List<SerializedView> viewStyleOverrides = new List<SerializedView>();
    #endregion

    #region State
    /// <summary>
    /// Indicates if the plate was changed this frame.
    /// </summary>
    internal bool wasChangedThisFrame;

    bool isActive = true;
    /// <summary>
    /// Gets whether the plate is currently active and enabled in the hierarchy.
    /// </summary>
    public bool IsActive => isActive && enabled && gameObject.activeInHierarchy;
    #endregion

    #region Properties
    /// <summary>
    /// Returns true if this plate is the root plate (has no parent).
    /// </summary>
    public bool IsRootPlate => !parent;

    /// <summary>
    /// Returns true if this plate requires views to be rendered.
    /// </summary>
    public bool RequiresViews => children.Count > 0 || renderer != null;

    /// <summary>
    /// Returns true if the plate requires a rebuild of its views.
    /// </summary>
    public bool RequiresRebuild => cachedAsset != visualAsset || viewIds.Count != viewStyleOverrides.Count;
    #endregion

    #region Component Reference
    [ShowInInspector]
    Plate parent;
    /// <summary>
    /// Gets the parent plate, if any.
    /// </summary>
    public Plate Parent => parent;

    [ShowInInspector]
    List<Plate> children = new List<Plate>();
    /// <summary>
    /// Gets the child plates of this plate.
    /// </summary>
    public IReadOnlyList<Plate> Children => children;

    /// <summary>
    /// List of view IDs in the template.
    /// </summary>
    [SerializeField, ReadOnly]
    public List<string> viewIds = new List<string>();

    /// <summary>
    /// Reference to the default view.
    /// </summary>
    [SerializeField]
    public ViewRef defaultViewRef = new ViewRef(childViewSelector);

    /// <summary>
    /// Reference to the content view.
    /// </summary>
    [SerializeField]
    public ViewRef contentViewRef = new ViewRef(contentViewSelector);

    /// <summary>
    /// Reference to the view to attach to in the parent plate.
    /// </summary>
    [SerializeField]
    public ViewRef attachToParentView = new ViewRef("");

    /// <summary>
    /// The router associated with this plate.
    /// </summary>
    [SerializeField]
    protected Router router;
    public Router Router => router;

    /// <summary>
    /// The state handle associated with this plate.
    /// </summary>
    [SerializeField]
    protected StateHandle stateHandle;
    public StateHandle StateHandle => stateHandle;

    /// <summary>
    /// The renderer associated with this plate.
    /// </summary>
    [SerializeField]
    new protected Renderer renderer;
    public Renderer Renderer => renderer;
    #endregion

    #region VisualElements Reference
    private TemplateContainer clone;
    /// <summary>
    /// The root visual element of this plate.
    /// </summary>
    public VisualElement Root { get; private set; }

    /// <summary>
    /// Main container for attached renderer's output of (repeat) elements.
    /// </summary>
    public VisualElement ContentContainer => contentViewRef.view;

    /// <summary>
    /// The default view. This controller's children will be added to this by default.
    /// </summary>
    View defaultView => defaultViewRef.view;

    /// <summary>
    /// Dictionary of views in the template, keyed by ID.
    /// </summary>
    Dictionary<string, View> views = new Dictionary<string, View>();

    /// <summary>
    /// Tracks which child plates are attached to which views.
    /// </summary>
    Dictionary<View, List<Plate>> childAttachments = new Dictionary<View, List<Plate>>();
    #endregion

    #region (Unity) Events
    /// <summary>
    /// Invoked when the plate's state is evaluated.
    /// </summary>
    public event System.Action onEvaluateState;
    /// <summary>
    /// Invoked when static content is refreshed.
    /// </summary>
    public event System.Action onRefreshStatic;
    /// <summary>
    /// Invoked when dynamic content is refreshed.
    /// </summary>
    public event System.Action onRefreshDynamic;
    /// <summary>
    /// Invoked when the visual tree is refreshed.
    /// </summary>
    public event System.Action onRefreshVisualTree;

    /// <summary>
    /// Unity event invoked when the plate is shown.
    /// </summary>
    public UnityEvent onShow = new UnityEvent();
    /// <summary>
    /// Unity event invoked when the plate is hidden.
    /// </summary>
    public UnityEvent onHide = new UnityEvent();
    #endregion

    /// <summary>
    /// Gets or sets whether the plate has been initialized.
    /// </summary>
    public bool Initialized { get; set; }
    bool registeredToParent;

    /// <summary>
    /// Initializes the plate, setting up references and parent-child relationships.
    /// </summary>
    public virtual void Initialize()
    {
      GetLocalReferences();

      if (parent && !registeredToParent)
      {
        parent.onShow.AddListener(Parent_OnShow);
        parent.onHide.AddListener(Parent_OnHide);
        registeredToParent = true;
      }
    }

    /// <summary>
    /// Called when the component awakens. Clears events and child list.
    /// </summary>
    protected override void Awake()
    {
      base.Awake();
      // Clear events
      onRefreshDynamic = null;
      onRefreshStatic = null;
      onRefreshVisualTree = null;
      onShow.RemoveAllListeners();
      onHide.RemoveAllListeners();
      children.Clear();
    }

    /// <summary>
    /// Gets local component references and registers with parent plate if applicable.
    /// </summary>
    protected virtual void GetLocalReferences()
    {
      if (graphene)
      {
        router ??= graphene.Router;
      }
      //customView ??= GetComponent<ViewHandle>();

      stateHandle ??= GetComponent<StateHandle>();
      renderer ??= GetComponent<Renderer>();

      // Get nearest parent
      if ((Application.isPlaying && parent) || (parent = transform.parent?.GetComponentInParent<Plate>(true))) 
          parent.RegisterChild(this);
    }

    /// <summary>
    /// Constructs the visual tree for this plate, instantiating views and applying styles.
    /// </summary>
    internal void ConstructVisualTree()
    {
      Profiler.BeginSample("Graphene Plate Construct VisualTree", this);
      Root?.Clear();

      clone = visualAsset.CloneTree();
	  Root = clone.Children().First();

#if UNITY_ASSERTIONS
      Debug.Assert(clone != null, this);
      Debug.Assert(Root != null, this);
      Debug.Assert(clone.childCount == 1, $"{nameof(Plate)} {nameof(TemplateContainer)} must have exactly 1 child {nameof(VisualElement)}", this);
      //Assert.IsNotNull(RootPlateEl);
#endif

      Root.pickingMode = pickingMode;

      if(pickingMode == PickingMode.Ignore)
        Root.Query().ForEach(t => t.pickingMode = PickingMode.Ignore);

	  if (RequiresRebuild)
		EditModeCacheViewIds();

	  // Get views
	  InitViewsRuntime();

      RefreshClassesAndStyles();

      Root.AddToClassList(plateClassName);

      Initialized = true;
      onRefreshVisualTree?.Invoke();

      Root.RegisterCallback<MouseOverEvent>((evt) => ChangeEvent());
      Root.RegisterCallback<PointerCaptureEvent>((evt) => ChangeEvent());
      Root.RegisterCallback<PointerMoveEvent>((evt) => ChangeEvent());
      Root.RegisterCallback<PointerDownEvent>((evt) => ChangeEvent());
      Root.RegisterCallback<PointerUpEvent>((evt) => ChangeEvent());

      if (IsRootPlate)
      {
        graphene.GrapheneRoot.Add(Root);
        Root.AddToClassList("unity-ui-document__child");
      }

	  // Hide on start
	  if (styleOverrides.showHideMode == ShowHideMode.Transition)
		Root.FadeOut();

	  // Fadeout events
	  //Root.RegisterCallback<TransitionStartEvent>(Root_StartTransition);

	  Profiler.EndSample();
    }

    void Root_StartTransition(TransitionStartEvent evt)
    {
      //Debug.Log($"Start transition {evt.target}");
    }
    void Root_EndTransition(TransitionEndEvent evt)
	{
	  //Debug.Log($"End transition {evt.target}");
	  if (evt.target != Root)
        return;

      ApplyActiveState();
	  Root.UnregisterCallback<TransitionEndEvent>(Root_EndTransition);

	  //if (!isActive || Root.ClassListContains(VisualElementExtensions.fadeoutUssClassName))
	  //{

	  //}
	  //else
	  //  Show();
	}

	void ChangeEvent()
    {
      wasChangedThisFrame = true;
    }

    protected void RegisterChild(Plate child)
    {
      if (!children.Contains(child))
        children.Add(child);
    }

    #region ButtonAttribute
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.ResponsiveButtonGroup("ShowHide/Actions"), FoldoutGroup("ShowHide")]
#elif NAUGHTY_ATTRIBUTES
    [NaughtyAttributes.Button]
#endif
    #endregion
    protected virtual void Clear()
    {
      // Clear the dynamic content
      ContentContainer?.Clear();
    }

    public void RefreshContentContainer()
    {
      DetachChildPlates(contentViewRef.view);
      Clear();
      ReattachChildPlates(contentViewRef.view);
    }

	#region ButtonAttribute
#if ODIN_INSPECTOR
	[Sirenix.OdinInspector.ResponsiveButtonGroup("ShowHide/Actions"), FoldoutGroup("ShowHide")]
#elif NAUGHTY_ATTRIBUTES
    [NaughtyAttributes.Button]
#endif
    #endregion
    internal virtual void RenderAndComposeChildren()
    {
      Profiler.BeginSample("RenderAndComposeChildren", this);
      // Detach the children so they don't get bound to the scope
      DetachChildPlates();
      Clear();

      onRefreshStatic?.Invoke();

      // (Re)attach & compose the tree
      AttachChildPlates();

      onRefreshDynamic?.Invoke();
      Profiler.EndSample();
    }

    // UIDocument removes the root OnDisable, so we only need OnEnable
    private void OnEnable()
    {
      if (!Initialized)
        return;
      Show();
      //ConstructVisualTree();
      //RenderAndComposeChildren();
      //ReevaluateState();
    }

    private void OnDisable()
    {
      if (!Initialized)
        return;
      Hide();
    }

    void InitViewsRuntime()
    {
      if (Root == null)
        return;

      var views = Root.Query<View>().ToList();
      this.views.Clear();
      View resolvedDefaultView = null;
      foreach (var view in views)
      {
        this.views.Add(view.id, view);
        if (view.isDefault || (resolvedDefaultView == null))
		  resolvedDefaultView = view;
      }

      defaultViewRef.ResolveView(this);

      if (resolvedDefaultView == null)
		resolvedDefaultView = views.FirstOrDefault();

      if (!defaultViewRef.initialized)
        defaultViewRef.view = resolvedDefaultView;

      if (!defaultViewRef.initialized)
      {
#if UNITY_ASSERTIONS
        if(children.Count > 0 || renderer != null)
          Debug.LogWarning($"No default view {defaultViewRef.Id}", this);
#endif
      }

	  contentViewRef.ResolveView(this);

      if (parent != null)
        attachToParentView.ResolveView(parent);
    }

    void EditModeCacheViewIds()
    {
      // Playing & already initialized
      if (Root != null)
        return;

      var el = Root != null ? Root : this.visualAsset.CloneTree();
      viewIds.Clear();
      el.Query<View>().ForEach(v => viewIds.Add(v.id));

      // Sync Ids
      if (this.viewStyleOverrides.Count != viewIds.Count)
      {
        this.viewStyleOverrides = new List<SerializedView>();
        foreach (var view in viewIds)
          this.viewStyleOverrides.Add(new SerializedView(view));
	  }
    }

    void SetVisualTreeAsset(VisualTreeAsset asset)
    {
      if(this.visualAsset == asset) 
        return;
      this.visualAsset = asset;
      OnChangeDocument();
	}

    void OnChangeDocument()
    {
	  UpdateViewPlates();
      RefreshClassesAndStyles();
    }

    void UpdateViewPlates()
    {
	  EditModeCacheViewIds();

	  defaultViewRef.SetPlate(this);
      contentViewRef.SetPlate(this);

	  if (parent != null)
		attachToParentView.SetPlate(parent);
	  else
		attachToParentView.NoParent();

	}

    #region ButtonAttribute
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.ResponsiveButtonGroup("ShowHide/Actions"), FoldoutGroup("ShowHide")]
#elif NAUGHTY_ATTRIBUTES
    [NaughtyAttributes.Button]
#endif
    #endregion
    public void Show()
    {
      if (!Initialized)
        return;

      if (isActive) return;

      if (canShow != null && !canShow.Invoke())
        return;

      // Cannot show
      if (transform.parent?.gameObject.activeInHierarchy == false)
        return;

      SetActive(true);
      ApplyActiveState(); // Immediately activate GO

      // Enable
      if (styleOverrides.showHideMode == ShowHideMode.Transition)
        Root.FadeIn();
    }

    #region ButtonAttribute
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.ResponsiveButtonGroup("ShowHide/Actions"), FoldoutGroup("ShowHide")]
#elif NAUGHTY_ATTRIBUTES
    [NaughtyAttributes.Button]
#endif
    #endregion
    public void Hide()
    {
      if (!Initialized)
        return;

      if (!isActive)
        return;

      SetActive(false);

      if (styleOverrides.showHideMode == ShowHideMode.Immediate)
        ApplyActiveState();
      else
      {
		Root.RegisterCallback<TransitionEndEvent>(Root_EndTransition);
		Root.FadeOut();
      }
    }

    internal void HideImmediately()
    {
      if (!Initialized)
        return;
      SetActive(false);
      ApplyActiveState();
    }

    void SetActive(bool active)
    {
      // Not changed
      if (this.isActive == active)
        return;

      this.isActive = active;
    }

    void ApplyActiveState()
    {
      gameObject.SetActive(isActive);
      RefreshClassesAndStyles();

      if (Root != null)
      {
        if (isActive)
        {
          Root.Show();
          //ContentContainer?.Focus();
          onShow?.Invoke();
          ChangeEvent();
        }
        else
        {
          Root.Hide();
          onHide?.Invoke();
        }
      }

#if UNITY_EDITOR
      if (Application.isPlaying)
      {
        //gameObject.SetActive(IsActive);

        gameObject.name = gameObject.name.Trim('*');
        if (isActive)
          gameObject.name = gameObject.name + "*";
      }
#endif
    }

    internal void ReevaluateState()
    {
      if (!Initialized)
        return;

      if (!stateHandle)
      {
        if(!parent || parent.isActive)
          Show();
      }

      onEvaluateState?.Invoke();
    }

    public Func<bool> canShow;
    void Parent_OnShow()
    {
      if (!stateHandle)
        Show();
    }

    void Parent_OnHide()
    {
      if (!stateHandle)
        Hide();
    }

    VisualElement temp;
	internal void Detach()
    {
      if(temp == null)
        temp = new VisualElement();
      temp.Add(Root);
    }

	protected virtual void DetachChildPlates()
	{
	  foreach (var child in children)
	  {
		if (child)
		  child.Detach();
	  }
	}
	protected virtual void DetachChildPlates(View view)
	{
      if(view == null)
        return;

	  if (childAttachments.TryGetValue(view, out var children))
	  {
        foreach (var child in children)
		  child.Detach();
	  }
	}

	protected virtual void ReattachChildPlates(View view)
	{
	  if (view == null)
		return;
	  if (childAttachments.TryGetValue(view, out var children))
	  {
		foreach (var child in children)
          InternalAttach(view, child);
	  }
	}

	/// <summary>
	/// Attaches child plates into designated view(s)
	/// </summary>
	void AttachChildPlates()
    {
      // Prolly unnecessary and will prevent dynamic child attackments
      if (childAttachments.Count > 0)
        ReattachChildren();

      // Rebuild from afresh
      childAttachments.Clear();

	  foreach (var child in children)
      {
        // Child can have optional view override
        if (child.attachToParentView)
        {
          var customView = GetViewById(child.attachToParentView.Id);
		  AttachChild(customView, child);

		  //customView.Add(child.Root);
        }
        else
        {
		  // By default we attach children to default view
		  AttachChild(defaultView, child);
		  //defaultView.Add(child.Root);
        }
      }
    }

    void ReattachChildren()
    {
      foreach (var kvp in childAttachments)
      {
        var view = kvp.Key;
        var children = kvp.Value;
        foreach (var child in children)
        {
          InternalAttach(view, child);
        }
      }
    }

    void AttachChild(View view, Plate child)
    {
      if(childAttachments.TryGetValue(view, out var children))
      {
        children.Add(child);
      }
      else
      {
        var list = new List<Plate>();
        childAttachments.Add(view, list);
        list.Add(child);
      }
      InternalAttach(view, child);
	}

    void InternalAttach(View view, Plate child)
    {
      view.Add(child.Root);
    }

	#region Helper  Methods
	/// <summary>
	/// Gets a visual element for a collection of selectors by name
	/// </summary>
	/// <param name="names"></param>
	/// <returns></returns>
	public VisualElement GetVisualElement(ICollection<string> names)
    {
      VisualElement target = Root;

      foreach (var name in names)
      {
        // Drill down
        var newTarget = target.Q(name);

        if (newTarget == null)
        {
          Debug.LogError($"VisualElement with name {name} not found as child of {target}", this);
          break;
        }

        target = newTarget;
      }

      return target;
    }

    public View GetViewById(string id)
    {
      if (views.TryGetValue(id, out View view))
        return view;
      else
        return defaultView;
    }

    private void OnDestroy()
    {
      Dispose();
    }
    public void Dispose()
    {
      if(BindingsManager)
        BindingsManager.DisposePlate(this, false);
    }

    void OnValidate()
    {
      if (isPrefab && !RequiresRebuild)
        return;

      GetLocalReferences();
	  UpdateViewPlates();
      RefreshClassesAndStyles();
      this.cachedAsset = visualAsset;
    }

    bool isPrefab => !gameObject.scene.isLoaded;

    const string positionModeRelativeClassNames = "flex-grow";
    const string positionModeAbsoluteClassNames = "absolute fill";
    const string showHideModeTransitionClassNames = "fade";

    internal void RefreshClassesAndStyles()
    {
      if (Root == null)
        return;

      this.styleOverrides.Apply(Root);
            
      foreach (var viewStyleOverride in viewStyleOverrides)
      {
        if (!viewStyleOverride.Enabled)
          continue;

        if (views.TryGetValue(viewStyleOverride.Id, out View view))
        {
          viewStyleOverride.Apply(view);
        }        
      }
    }
    #endregion
  }
}