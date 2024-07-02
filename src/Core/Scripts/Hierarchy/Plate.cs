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

  public enum PositionMode
  {
    None,
    Relative,
    Absolute
  }

  public enum BindingRefreshMode
  {
    None,
    Continuous,
    ModelChange
  }

  public enum Mode
  {
    Prebuilt,
    OnDemand
  }

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

	public const string plateClassName = "plate";
	public const string contentViewSelector = "Content";
	public const string childViewSelector = "Children";
	#endregion

	#region Inspector/Authoring
	[SerializeField, OnValueChanged(nameof(OnChangeDocument))] VisualTreeAsset visualAsset; public VisualTreeAsset VisualTreeAsset { get =>  visualAsset;  set => SetVisualTreeAsset(value); }
    [SerializeField, HideInInspector] VisualTreeAsset cachedAsset;


	[SerializeField] PickingMode pickingMode = PickingMode.Position;

    [SerializeField] public BindingRefreshMode bindingRefreshMode = BindingRefreshMode.ModelChange;

    [SerializeField, FoldoutGroup("Styles Overrides")] InlineStyleOverrides styleOverrides = new InlineStyleOverrides() { positionMode = PositionMode.Relative };
    [SerializeField, FoldoutGroup("Styles Overrides"), ListDrawerSettings(HideAddButton = true, HideRemoveButton = true, ListElementLabelName = "Id")] List<SerializedView> viewStyleOverrides = new List<SerializedView>();
	#endregion

	#region State
	internal bool wasChangedThisFrame;
	bool isActive = true; public bool IsActive => isActive && enabled && gameObject.activeInHierarchy;
	#endregion

	#region Properties
	public bool IsRootPlate => !parent;

    public bool RequiresViews => children.Count > 0 || renderer != null;

    public bool RequiresRebuild => cachedAsset != visualAsset || viewIds.Count != viewStyleOverrides.Count;
	#endregion

    #region Component Reference
    [ShowInInspector] Plate parent; public Plate Parent => parent;
    [ShowInInspector] List<Plate> children = new List<Plate>(); public IReadOnlyList<Plate> Children => children;

    [SerializeField, ReadOnly] public List<string> viewIds = new List<string>();
    [SerializeField] public ViewRef defaultViewRef = new ViewRef(childViewSelector);
    [SerializeField] public ViewRef contentViewRef = new ViewRef(contentViewSelector);
    [SerializeField] public ViewRef attachToParentView = new ViewRef("");

    [SerializeField] protected Router router; public Router Router => router;
    [SerializeField] protected StateHandle stateHandle; public StateHandle StateHandle => stateHandle;
    [SerializeField] new protected Renderer renderer; public Renderer Renderer => renderer;
    #endregion

    #region VisualElements Reference
    private TemplateContainer clone;
    public VisualElement Root { get; private set; }
    //public VisualElement RootPlateEl { get; private set; }
    /// <summary>
    /// Main container for attached renderer's output of (repeat) elements.
    /// </summary>
    public VisualElement ContentContainer => contentViewRef.view;

    /// <summary>
    /// The default view. This controller's children will be added to this by default.
    /// </summary>
    View defaultView => defaultViewRef.view;
    /// <summary>
    /// List of views in the template.
    /// </summary>
    Dictionary<string, View> views = new Dictionary<string, View>();
    Dictionary<View, List<Plate>> childAttachments = new Dictionary<View, List<Plate>>();

    #endregion

    #region (Unity) Events
    public event System.Action onEvaluateState;
    public event System.Action onRefreshStatic;
    public event System.Action onRefreshDynamic;
    public event System.Action onRefreshVisualTree;

    public UnityEvent onShow = new UnityEvent();
    public UnityEvent onHide = new UnityEvent();
    #endregion

    public bool Initialized { get; set; }
    bool registeredToParent;

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
          ContentContainer?.Focus();
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