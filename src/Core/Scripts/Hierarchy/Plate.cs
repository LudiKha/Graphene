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
  using UnityEngine.Assertions;
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
  //[RequireComponent(typeof(UIDocument))]
  [DisallowMultipleComponent]
  public class Plate : GrapheneComponent, IGrapheneInitializable, IDisposable
  {
    [SerializeField] VisualTreeAsset visualAsset; public VisualTreeAsset VisualTreeAsset => visualAsset;

#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.ReadOnly, ShowInInspector]
#endif
    bool isActive = true; public bool IsActive => isActive && enabled && gameObject.activeInHierarchy;

    //[SerializeField] protected string[] contentContainerSelector = new string[] { contentViewSelector };

    [Tooltip("Adds any number of classes to the root element. Separated by space")]
    [SerializeField] protected string addClasses;

    [SerializeField] Mode drawMode = Mode.OnDemand; public Mode Drawmode => Mode.OnDemand;
    
    [SerializeField] PositionMode positionMode = PositionMode.Relative;
    [SerializeField] ShowHideMode showHideMode = ShowHideMode.Immediate;
    [SerializeField] JustifyOverride justifyContent =  new JustifyOverride();
    //[SerializeField] AlignContentOverride alignContent = new AlignContentOverride();
    [SerializeField] AlignItemsOverride alignItemsOverride = new AlignItemsOverride();
    [SerializeField] FlexDirectionOverride flexDirectionOverride = new FlexDirectionOverride();
    [SerializeField] WrapOverride wrapOverride = new WrapOverride();

    [SerializeField] public BindingRefreshMode bindingRefreshMode = BindingRefreshMode.ModelChange;
    internal bool wasChangedThisFrame;

    public bool IsRootPlate => !parent;

    #region Constants

    public const string plateClassName = "plate";
    public const string contentViewSelector = "GR__Content";
    public const string childViewSelector = "GR__Children";
    #endregion

    #region Component Reference
    [ShowInInspector] Plate parent; public Plate Parent => parent;
    [ShowInInspector] List<Plate> children = new List<Plate>(); public IReadOnlyList<Plate> Children => children;

    [SerializeField] public ViewRef defaultViewRef = new ViewRef(childViewSelector);
    [SerializeField] public ViewRef contentViewRef = new ViewRef(contentViewSelector);
    [SerializeField] public ViewRef attachToParentView = new ViewRef("");

    [SerializeField] protected Router router; public Router Router => router;
    [SerializeField] protected StateHandle stateHandle; public StateHandle StateHandle => stateHandle;
    [SerializeField] new protected Renderer renderer; public Renderer Renderer => renderer;
    #endregion

    #region VisualElements Reference
    public VisualElement Root { get; private set; }
    public VisualElement RootPlateEl { get; private set; }
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
    List<View> views = new List<View>(); public IReadOnlyList<View> Views => views;

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

    private void Awake()
    {
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
      Root = visualAsset.CloneTree();
      RootPlateEl = Root.Children().First();

#if UNITY_ASSERTIONS
      Assert.IsNotNull(Root);
      Assert.IsNotNull(RootPlateEl);
#endif

      Root.pickingMode = Graphene.defaultPickingMode;
      RootPlateEl.pickingMode = Graphene.defaultPickingMode;

      // Get views
      InitViews();

      RefreshClassesAndStyles();

      Root.AddToClassList(plateClassName);
      if (!string.IsNullOrWhiteSpace(addClasses))
      {
        Root.AddMultipleToClassList(addClasses);
        RootPlateEl.AddMultipleToClassList(addClasses);
      }

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

      // Fadeout events
      Root.RegisterCallback<TransitionStartEvent>(Root_StartTransition);
      Root.RegisterCallback<TransitionEndEvent>(Root_EndTransition);

      Profiler.EndSample();
    }

    void Root_StartTransition(TransitionStartEvent evt)
    {
    }
    void Root_EndTransition(TransitionEndEvent evt)
    {
      ApplyActiveState();
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
    [Sirenix.OdinInspector.Button]
#elif NAUGHTY_ATTRIBUTES
    [NaughtyAttributes.Button]
#endif
    #endregion
    protected virtual void Clear()
    {
      // Clear the dynamic content
      ContentContainer?.Clear();
    }

    protected virtual void DetachChildPlates()
    {
      foreach (var child in children)
      {
        if(child)
          child.Detach();
      }
    }

    #region ButtonAttribute
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Button]
#elif NAUGHTY_ATTRIBUTES
    [NaughtyAttributes.Button]
#endif
    #endregion
    internal virtual void RenderAndComposeChildren()
    {
      Profiler.BeginSample("Graphene Plate RenderAndComposeChildren");
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

    void InitViews()
    {
      if (Root == null)
        return;

      views = Root.Query<View>().ToList();

      defaultViewRef.OnValidate(this);
      if (!defaultViewRef.initialized)
        defaultViewRef.view = views.Find(v => v.isDefault) ?? views.FirstOrDefault();

      contentViewRef.OnValidate(this);

      if (parent != null)
        attachToParentView.OnValidate(parent);
      else
        attachToParentView.NoParent();
    }

    #region ButtonAttribute
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Button, Sirenix.OdinInspector.HorizontalGroup("ShowHide")]
#elif NAUGHTY_ATTRIBUTES
    [NaughtyAttributes.Button]
#endif
    #endregion
    public void Show()
    {
      if (!Initialized)
        return;

      if (isActive) return;

      SetActive(true);
      ApplyActiveState(); // Immediately activate GO

      // Enable
      if (showHideMode == ShowHideMode.Transition)
        Root.FadeIn();
    }

    #region ButtonAttribute
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Button, Sirenix.OdinInspector.HorizontalGroup("ShowHide")]
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

      if (showHideMode == ShowHideMode.Immediate)
        ApplyActiveState();
      else
        Root.FadeOut();
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
          onShow.Invoke();
          ChangeEvent();
        }
        else
        {
          Root.Hide();
          onHide.Invoke();
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

    void Detach()
    {
      VisualElement temp = new VisualElement();
      temp.Add(Root);
    }

    /// <summary>
    /// Attaches child plates into designated view(s)
    /// </summary>
    void AttachChildPlates()
    {
      foreach (var child in children)
      {
        // Child can have optional view override
        if (child.attachToParentView)
        {
          var customView = GetViewById(child.attachToParentView.Id);
          if (customView != null && customView == child.attachToParentView.view)
          {
            customView.Add(child.Root);
            continue;
          }
        }

        // By default we attach children to default view
        defaultView.Add(child.Root);
      }
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
      View view = views.Find(x => x.id.Equals(id));
      if (view == null)
        view = views.Find(x => x.isDefault);
      return view;
    }

    private void OnDestroy()
    {
      Dispose();
    }
    public void Dispose()
    {
      BindingManager.DisposePlate(this);
    }

    void OnValidate()
    {
      GetLocalReferences();

      if (Root == null)
        return;

      InitViews();
      RefreshClassesAndStyles();
    }

    const string positionModeRelativeClassNames = "flex-grow";
    const string positionModeAbsoluteClassNames = "absolute fill";
    const string showHideModeTransitionClassNames = "fade";

    internal void RefreshClassesAndStyles()
    {
      if (positionMode == PositionMode.Relative)
      {
        Root.RemoveFromClassList(positionModeAbsoluteClassNames);
        Root.AddToClassList(positionModeRelativeClassNames);
        RootPlateEl.RemoveFromClassList(positionModeAbsoluteClassNames);
        RootPlateEl.AddToClassList(positionModeRelativeClassNames);
      }
      else if (positionMode == PositionMode.Absolute)
      {
        Root.RemoveFromClassList(positionModeRelativeClassNames);
        Root.AddMultipleToClassList(positionModeAbsoluteClassNames);
        RootPlateEl.RemoveFromClassList(positionModeRelativeClassNames);
        RootPlateEl.AddMultipleToClassList(positionModeAbsoluteClassNames);
      }

      if(showHideMode == ShowHideMode.Immediate)
      {
        Root.RemoveFromClassList(showHideModeTransitionClassNames);
      }
      else if(showHideMode == ShowHideMode.Transition)
      {
        Root.AddToClassList(showHideModeTransitionClassNames);
        // When transitioning, we can only position absolutely, as the fadeout process will interfere with routing 
        Root.AddMultipleToClassList(positionModeAbsoluteClassNames);
      }

      justifyContent.TryApply(Root);
      //alignContent.TryApply(Root);
      alignItemsOverride.TryApply(Root);
      flexDirectionOverride.TryApply(Root);
      wrapOverride.TryApply(Root);
    }
    #endregion
  }

#if ODIN_INSPECTOR
  [Sirenix.OdinInspector.Toggle("enabled")]
#endif
  [System.Serializable]
  public abstract class StyleOverride<T>
  {
    public bool enabled;
    public T value;

    public abstract void TryApply(VisualElement visualElement);

    public static implicit operator bool (StyleOverride<T> styleOverride) => styleOverride != null && styleOverride.enabled;
  }



  [System.Serializable]
  public sealed class JustifyOverride : StyleOverride<Justify>
  {
    public override void TryApply(VisualElement visualElement)
    {
      if(enabled)
        visualElement.style.justifyContent = value;
    }
  }

  [System.Serializable]
  public sealed class AlignItemsOverride : StyleOverride<Align>
  {
    public override void TryApply(VisualElement visualElement)
    {
      if (enabled)
        visualElement.style.alignItems = value;
    }
  }

  [System.Serializable]
  public sealed class FlexDirectionOverride : StyleOverride<FlexDirection>
  {
    public override void TryApply(VisualElement visualElement)
    {
      if (enabled)
        visualElement.style.flexDirection = value;
    }
  }
  [System.Serializable]
  public sealed class WrapOverride : StyleOverride<Wrap>
  {
    public override void TryApply(VisualElement visualElement)
    {
      if (enabled)
        visualElement.style.flexWrap = value;
    }
  }
}