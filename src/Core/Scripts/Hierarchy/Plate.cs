using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace Graphene
{
  using Elements;

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

  ///<summary>
  /// <para>A `Plate` represents a view controller in the VisualTree, and is used when by Graphene to the hierarchy, its states and views.</para> 
  /// <para><see href="https://github.com/LudiKha/Graphene#plates">Read more in the online documentation</see></para>
  ///</summary>
  //[RequireComponent(typeof(UIDocument))]
  [DisallowMultipleComponent]
  public class Plate : MonoBehaviour, IInitializable, IDisposable
  {
    [SerializeField] VisualTreeAsset visualAsset;

    /// <summary>
    /// The theme that will be applied to the root element of this plate
    /// </summary>
    [SerializeField] Theme theme;
    [SerializeField] bool isActive = true; public bool IsActive => isActive && enabled && gameObject.activeInHierarchy;

    [SerializeField] protected string[] contentContainerSelector = new string[] { "GR__Content" };

    [Tooltip("Adds any number of classes to the root element. Separated by space")]
    [SerializeField] protected string addClasses;

    [SerializeField] PositionMode positionMode;
    [SerializeField] public BindingRefreshMode bindingRefreshMode = BindingRefreshMode.ModelChange;
    internal bool wasChangedThisFrame;

    public bool IsRootPlate => !parent;

    #region Component Reference
    [SerializeField] Plate parent;
    [SerializeField] List<Plate> children = new List<Plate>();
    [SerializeField] ViewHandle customView; public ViewHandle CustomView => customView;
    [SerializeField] protected Router router; public Router Router => router;
    [SerializeField] public StateHandle stateHandle { get; internal set; }
    [SerializeField] new public Renderer renderer { get; internal set; }
    #endregion

    #region VisualElements Reference
    public VisualElement Root { get; private set; }
    /// <summary>
    /// Main container for attached renderer's output of (repeat) elements.
    /// </summary>
    public VisualElement ContentContainer { get; private set; }
    public VisualElement ChildContainer { get; private set; }

    /// <summary>
    /// The default view. This controller's children will be added to this by default.
    /// </summary>
    View defaultView;
    /// <summary>
    /// List of views in the template.
    /// </summary>
    List<View> views = new List<View>();

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
    public virtual void Initialize()
    {
      if (Initialized)
        return;
      GetLocalReferences();
    }

    protected virtual void GetLocalReferences()
    {
      if (!router)
        router = GetComponentInParent<Router>();

      // Get nearest parent
      if (parent || (parent = transform.parent.GetComponentInParent<Plate>()))
        parent.RegisterChild(this);

      if (!customView)
        customView = GetComponent<ViewHandle>();
    }

    internal void ConstructVisualTree()
    {
      Root = visualAsset.CloneTree();

      ContentContainer = GetVisualElement(contentContainerSelector);

      // Get views
      views = Root.Query<View>().ToList();
      defaultView = views.Find(x => x.isDefault) ?? views.FirstOrDefault();
      if (theme)
        theme.ApplyStyles(Root);

      if (positionMode == PositionMode.Relative)
        Root.AddToClassList("flex-grow");
      else if (positionMode == PositionMode.Absolute)
        Root.AddMultipleToClassList("absolute fill");

      if (!string.IsNullOrWhiteSpace(addClasses))
        Root.AddMultipleToClassList(addClasses);

      Initialized = true;
      onRefreshVisualTree?.Invoke();

      Root.RegisterCallback<PointerMoveEvent>((evt) => ChangeEvent());
      Root.RegisterCallback<PointerDownEvent>((evt) => ChangeEvent());
      Root.RegisterCallback<PointerUpEvent>((evt) => ChangeEvent());
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
      ContentContainer.Clear();
    }

    protected virtual void DetachChildPlates()
    {
      foreach (var child in children)
      {
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
      Clear();

      // Detach the children so they don't get bound to the scope
      DetachChildPlates();

      //if (renderer)
      //{
      //  renderer.Plate_onRefreshStatic();
      //  Binder.BindRecursive(Root, renderer, null, this, true);
      //  //Binder.BindRecursive(Root, renderer, null, this, true);
      //}
      onRefreshStatic?.Invoke();

      // Bind the static template to the renderer
      //Binder.BindRecursive(Root, this, null, this, true);

      // (Re)attach & compose the tree
      AttachChildPlates();

      onRefreshDynamic?.Invoke();
    }

    // UIDocument removes the root OnDisable, so we only need OnEnable
    private void OnEnable()
    {
      if (!Initialized)
        return;

      ReevaluateState();
    }

    private void OnDisable()
    {
      if (!Initialized)
        return;
      Hide();
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

      // Enable
      Root.Show();
      ContentContainer.Focus();

      SetActive(true);
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

      Root.Hide();

      SetActive(false);
    }

    public void SetActive(bool active)
    {
      // Not changed
      if (this.isActive == active)
        return;

      this.isActive = active;

      if (this.isActive)
        onShow.Invoke();
      else
        onHide.Invoke();
    }

    internal void ReevaluateState()
    {
      if (!Initialized)
        return;

      if (!stateHandle)
        Show();

      onEvaluateState?.Invoke();
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
        if (child.CustomView)
        {
          var layoutContainer = GetViewById(child.customView.Id);
          if (layoutContainer != null)
          {
            layoutContainer.Add(child.Root);
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
    #endregion
  }
}