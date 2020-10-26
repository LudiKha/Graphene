using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UIElements;

namespace Graphene
{
  using Elements;

  [RequireComponent(typeof(UIDocument))]
  [DisallowMultipleComponent]
  public class Graphene : MonoBehaviour
  {
    [SerializeField, Tooltip("Disable this if you want to manually initialize Graphene")] bool initializeOnAwake = true;
    [SerializeField] Theme globalTheme;

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
    UIDocument doc;
    /// <summary>
    /// The router
    /// </summary>
    [SerializeField] Router router;

    protected void Awake()
    {
      if (initializeOnAwake)
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
      if (!doc)
        doc = GetComponent<UIDocument>();

      if (!router)
        router = GetComponent<Router>();
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
      //ConstructVisualTree(plates);

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
      grapheneRoot = new GrapheneRoot(router);
      doc.rootVisualElement.Add(grapheneRoot);

      // Clone the visual tree for each plate
      foreach (Plate plate in plates)
        plate.ConstructVisualTree();

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
      UnityEngine.Debug.Log($"Graphene ConstructVisualTree: {sw.ElapsedMilliseconds}ms");
    }

    void FinalizeInitialzation()
    {
      /// Needs to go here because UIDocuments may initialize late
      if (globalTheme)
        globalTheme.ApplyStyles(doc.rootVisualElement);

      grapheneRoot.BringToFront();
      lastRefreshTime = Time.time;

    }

    void Update()
    {
      if (Time.time - lastRefreshTime < bindingRefreshRate)
        return;

      Profiler.BeginSample($"Update Graphene bindings ({BindingManager.bindingsCount} bindings)", this);
      BindingManager.OnUpdate();
      lastRefreshTime = Time.time;
      Profiler.EndSample();
    }

    public void RebuildBranch (Plate plate)
    {

    }

    // Needs to be in because UIDocument destroys the root
    private void OnEnable()
    {
      if (!Initialized)
        return;
      wasDisabled = false;
      CloneOrReattach();
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

    private bool wasDisabled;
    private void OnDisable()
    {
      wasDisabled = true;
    }
  }
}