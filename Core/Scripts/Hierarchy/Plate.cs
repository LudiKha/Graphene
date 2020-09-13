using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using Sirenix.OdinInspector;

namespace Graphene
{
  public class Plate : MonoBehaviour, IInitializable, ILateInitializable
  {
    [AssetList(AssetNamePrefix = "_")] public List<StyleSheet> styleSheets;
    protected UIDocument doc; public UIDocument Doc => doc;

    [SerializeField] protected string[] contentContainerSelector = new string[] { "GR__Content" };

    #region VisualElements Reference
    VisualElement root;
    // Main container for repeat elements
    protected VisualElement contentContainer; public VisualElement ContentContainer => contentContainer;
    protected VisualElement childContainer; public VisualElement ChildContainer => childContainer;
    #endregion

    [SerializeField] ViewHandle customView; public ViewHandle CustomView => customView;
    [SerializeField] protected StateHandle stateHandle; public StateHandle StateHandle => stateHandle;

    [SerializeField] bool isActive = true; public bool IsActive => isActive && enabled && gameObject.activeInHierarchy;

    View defaultView;
    List<View> views = new List<View>();

    [SerializeField] Plate parent;
    [SerializeField] List<Plate> children = new List<Plate>();

    #region (Unity) Events
    public event System.Action onRefresh;

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

      styleSheets.ForEach(x => doc.rootVisualElement.styleSheets.Add(x));
    }

    public virtual void LateInitialize()
    {
      Refresh();
    }

    protected virtual void GetLocalReferences()
    {
      if (!doc)
        doc = GetComponent<UIDocument>();

      root = doc.rootVisualElement;

      if (!stateHandle)
        stateHandle = GetComponent<StateHandle>();

      // Get nearest parent
      if (parent || (parent = transform.parent.GetComponentInParent<Plate>()))
      {
        parent.RegisterChild(this);
        parent.onRefresh += Refresh;
      }
      if (!customView)
        customView = GetComponent<ViewHandle>();

      contentContainer = GetVisualElement(contentContainerSelector);

      // Get views
      views = root.Query<View>().ToList();
      defaultView = views.Find(x => x.isDefault) ?? views.FirstOrDefault();
    }

    protected void RegisterChild(Plate child)
    {
      if (!children.Contains(child))
        children.Add(child);
    }

    [Button]
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

    [Button]
    protected virtual void Refresh()
    {
      Clear();

      DetachChildren();

      // Bind the static plate to its scope
      Binder.BindRecursive(doc.rootVisualElement, this, null, this, true);

      AttachChildren();

      onRefresh?.Invoke();
    }

    //private void OnEnable()
    //{
    //  Show();
    //}

    //private void OnDisable()
    //{
    //  Hide();
    //}

    public void Show()
    {
      if (!Initialized)
        return;

      // Enable
      root.style.display = DisplayStyle.Flex;
      contentContainer.Focus();

      SetActive(true);
    }
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
      //root = doc.visualTreeAsset.CloneTree();
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
    #endregion
  }
}
