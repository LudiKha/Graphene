using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UIElements;

namespace Graphene
{
  using Elements;

  [ExecuteInEditMode]
  [RequireComponent(typeof(UIDocument))]
  [DisallowMultipleComponent]
  public class Graphene : MonoBehaviour
  {
    [SerializeField, Tooltip("Disable this if you want to manually initialize Graphene")] bool initializeOnStart = true;
    [SerializeField, Tooltip("Disable this if you want to manually initialize Graphene")] bool runInEditMode = true;
    [SerializeField] string addClasses;
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.InfoBox("$bindingsInfo")]
#elif NAUGHTY_ATTRIBUTES    
    [NaughtyAttributes.InfoBox("bindingsInfo")]
#endif
    [SerializeField] float bindingRefreshRate = 0.2f;
    
    #region ReadOnlyAttribute
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.ReadOnly]
#elif NAUGHTY_ATTRIBUTES
    [NaughtyAttributes.ReadOnly]
#endif
    #endregion
    [SerializeField] float lastRefreshTime;

    [SerializeField] List<Plate> plates = new List<Plate>();

    List<IGrapheneDependent> dependents = new List<IGrapheneDependent>();
    public event System.Action<ICollection<IGrapheneDependent>> onPreInitialize;
    public event System.Action<ICollection<IGrapheneDependent>> onPostInitialize;

    /// <summary>
    /// Root Graphene element controller
    /// </summary>
    GrapheneRoot grapheneRoot;

    /// <summary>
    /// The UI Document
    /// </summary>
    [SerializeField] UIDocument doc;
    /// <summary>
    /// The router
    /// </summary>
    [SerializeField] Router router; public Router Router => router;

    protected void Start()
    {
      if (!Application.isPlaying)
        return;

      if (enabled && initializeOnStart)
        Initialize();
    }

    public bool Initialized { get; private set; }
    public void Initialize()
    {
      GetLocalReferences();

      RunInstallation();

      doc.enabled = false;
      doc.enabled = true;
      Initialized = true;

      CloneOrReattach();
    }

    protected void GetLocalReferences()
    {
      doc ??= GetComponent<UIDocument>();
      router ??= GetComponent<Router>();
    }

    protected void RunInstallation()
    {
      var sw = new Stopwatch();
      sw.Start();

      dependents = GetComponentsInChildren<IGrapheneDependent>().ToList();
      plates = dependents.Where(x => x is Plate).Select(x => x as Plate).ToList();

      onPreInitialize?.Invoke(dependents);
      // First initialize
      foreach (var item in dependents.Where(x => x is IInitializable).Select(x => x as IInitializable))
        item.Initialize();

      // Construct the visual tree hierarchy 
      ConstructVisualTree(plates);

      // Second initialize
      foreach (var item in dependents.Where(x => x is ILateInitializable).Select(x => x as ILateInitializable))
        item.LateInitialize();

      onPostInitialize?.Invoke(dependents);

      sw.Stop();
      UnityEngine.Debug.Log($"Graphene initialization time: {sw.ElapsedMilliseconds}ms");
    }


    void ConstructVisualTree(List<Plate> plates)
    {
      var sw = new Stopwatch();
      sw.Start();

      // Create the root controller
      CreateRootElement();

      // Clone the visual tree for each plate
      foreach (Plate plate in plates)
      {
#if UNITY_EDITOR
        try
        {
          plate.ConstructVisualTree();
        }
        catch (System.Exception e)
        {
          UnityEngine.Debug.LogError(e, plate);
        }
#else
        plate.ConstructVisualTree();
#endif
      }

      // Refresh hierarchy -> render & compose children
      foreach (Plate plate in plates)
      {
        if (plate.IsRootPlate)
        {
          grapheneRoot.Add(plate.Root);
          plate.Root.AddToClassList("unity-ui-document__child");
        }

        plate.Root.name = $"{plate.gameObject.name}-container";
        plate.RenderAndComposeChildren();

        plate.ReevaluateState();
      }

      FinalizeInitialzation();
      sw.Stop();
      //UnityEngine.Debug.Log($"Graphene ConstructVisualTree: {sw.ElapsedMilliseconds}ms");
    }

    #region Build VisualElement

    void CreateRootElement()
    {
      // Create the root controller
      grapheneRoot = new GrapheneRoot(router);
      grapheneRoot.AddMultipleToClassList(addClasses);

      doc.rootVisualElement.Add(grapheneRoot);
    }

    void RebuildRootElement()
    {
      var oldRoot = grapheneRoot;

      CreateRootElement();

      if (oldRoot != null)
      {
        // Add root plates
        if (oldRoot.childCount > 0)
        {
          var children = oldRoot.Children().ToList();
          foreach (var child in children)
            grapheneRoot.Add(child);
        }

        doc.rootVisualElement.Remove(oldRoot);
      }
    }
    #endregion

    #region BuildHierarchy
    #endregion

    void FinalizeInitialzation()
    {
      /// Needs to go here because UIDocuments may initialize late
      grapheneRoot.BringToFront();
      lastRefreshTime = Time.time;

    }

    void Update()
    {
      if (!Application.isPlaying && !runInEditMode)
        return;

      if (Time.time - lastRefreshTime < bindingRefreshRate)
        return;

      if (grapheneRoot == null)
        Rebuild();

      Profiler.BeginSample($"Update Graphene bindings ({BindingManager.bindingsCount} bindings)", this);
      BindingManager.OnUpdate();
      lastRefreshTime = Time.time;
      Profiler.EndSample();
    }

    public void RebuildBranch(Plate plate)
    {

    }

    // Needs to be in because UIDocument destroys the root
    private void OnEnable()
    {
      // Live reload
      Rebuild();
      return;
      if (!Application.isPlaying)
        Initialize();
      else if (!Initialized)
        Initialize();
    }


    private void CloneOrReattach()
    {
      if (grapheneRoot != null)
      {
        doc.rootVisualElement.Add(grapheneRoot);
        FinalizeInitialzation();
      }
      else
        ConstructVisualTree(plates);
    }

    private void OnDisable()
    {
    }


    #region ButtonAttribute
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.ResponsiveButtonGroup]
#elif NAUGHTY_ATTRIBUTES
    [NaughtyAttributes.Button]
#endif
    #endregion
    void Rebuild()
    {
      if (!runInEditMode)
        return;

      grapheneRoot?.Clear();
      grapheneRoot = null;
      Initialize();
    }

#if UNITY_EDITOR
    public string bindingsInfo => $"{BindingManager.bindingsCount} bindings";
#endif

    private void OnValidate()
    {
      if (Application.isPlaying || doc.rootVisualElement == null)
        return;

      RebuildRootElement();
    }
  }
}