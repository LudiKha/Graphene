using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace Graphene
{
  using Elements;
  using System;

  public enum PositionMode
  {
    None,
    Relative,
    Absolute
  }

  ///<summary>
  /// <para>A `Plate` represents a view controller in the VisualTree, and is used when by Graphene to the hierarchy, its states and views.</para> 
  /// <para><see href="https://github.com/LudiKha/Graphene#plates">Read more in the online documentation</see></para>
  ///</summary>
  [RequireComponent(typeof(UIDocument))]
  [DisallowMultipleComponent]
  public class Plate : MonoBehaviour, IInitializable, ILateInitializable, IDisposable
  {
    /// <summary>
    /// The theme that will be applied to the root element of this plate
    /// </summary>
    [SerializeField] Theme theme;
    protected UIDocument doc;

    [SerializeField] protected string[] contentContainerSelector = new string[] { "GR__Content" };

    [SerializeField] bool isActive = true; public bool IsActive => isActive && enabled && gameObject.activeInHierarchy;

    [SerializeField] PositionMode positionMode;

    #region Component Reference
    [SerializeField] Plate parent;
    [SerializeField] List<Plate> children = new List<Plate>();
    [SerializeField] ViewHandle customView; public ViewHandle CustomView => customView;
    [SerializeField] protected Router router; public Router Router => router;
    #endregion

    #region VisualElements Reference
    protected VisualElement root; public VisualElement Root => root;
    // Main container for repeat elements
    protected VisualElement contentContainer; public VisualElement ContentContainer => contentContainer;
    protected VisualElement childContainer; public VisualElement ChildContainer => childContainer;


    View defaultView;
    List<View> views = new List<View>();

    #endregion

    #region (Unity) Events
    public event System.Action onRefreshHierarchy;
    public event System.Action onRefreshStatic;
    public event System.Action onRefreshDynamic;

    public UnityEvent onShow = new UnityEvent();
    public UnityEvent onHide = new UnityEvent();
    #endregion

    public bool Initialized { get; set; }
    public virtual void Initialize()
    {
      if (Initialized)
        return;
      Initialized = true;

      GetLocalReferences();
      SetupVisualTree();
    }

    public virtual void LateInitialize()
    {
      RefreshHierarchy();
    }

    protected virtual void GetLocalReferences()
    {
      if (!doc)
        doc = GetComponent<UIDocument>();

      if (!router)
        router = GetComponentInParent<Router>();

      // Get nearest parent
      if (parent || (parent = transform.parent.GetComponentInParent<Plate>()))
      {
        parent.RegisterChild(this);
        parent.onRefreshHierarchy += RefreshHierarchy;
      }
      if (!customView)
        customView = GetComponent<ViewHandle>();

    }

    protected void SetupVisualTree()
    {
      root = doc.rootVisualElement;

      contentContainer = GetVisualElement(contentContainerSelector);

      // Get views
      views = root.Query<View>().ToList();
      defaultView = views.Find(x => x.isDefault) ?? views.FirstOrDefault();
      if (theme)
        theme.ApplyStyles(root);

      if (positionMode == PositionMode.Relative)
        root.AddToClassList("flex-grow");
      else if (positionMode == PositionMode.Absolute)
        root.AddMultipleToClassList("absolute fill");
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
      contentContainer.Clear();
    }

    protected virtual void DetachChildren()
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
    protected virtual void RefreshHierarchy()
    {
      Clear();

      DetachChildren();

      // Bind the static plate to its scope
      Binder.BindRecursive(root, this, null, this, true);

      AttachChildren();

      onRefreshStatic?.Invoke();
      onRefreshDynamic?.Invoke();
    }


    private void OnEnable()
    {
      if (!Initialized)
        return;

      SetupVisualTree();
    }

    //private void OnDisable()
    //{
    //  Hide();
    //}

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
      root.style.display = DisplayStyle.Flex;
      contentContainer.Focus();

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

      root.style.display = DisplayStyle.None;

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

    void Detach()
    {
      VisualElement temp = new VisualElement();
      temp.Add(root);
    }


    void AttachChildren()
    {
      foreach (var child in children)
      {
        // Child can have optional override
        if (child.CustomView)
        {
          var layoutContainer = GetViewById(child.customView.Id);
          if (layoutContainer != null)
          {
            layoutContainer.Add(child.root);
            continue;
          }
        }

        // By default we attach children to default view
        defaultView.Add(child.root);
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
      var root = doc.rootVisualElement;
      VisualElement target = root;

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