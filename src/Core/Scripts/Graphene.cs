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

    [SerializeField] float bindingRefreshRate = 0.02f;
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

    public void Initialize()
    {
      GetLocalReferences();

      RunInstallation();

      doc.enabled = false;
      doc.enabled = true;
      /// Needs to go here because UIDocuments may initialize late
      if (globalTheme)
        globalTheme.ApplyStyles(doc.rootVisualElement);

      doc.rootVisualElement.Add(grapheneRoot);
      grapheneRoot.SendToBack();

      lastRefreshTime = Time.time;
    }

    protected void GetLocalReferences()
    {
      if (!doc)
        doc = GetComponent<UIDocument>();

      if (!router)
        router = GetComponent<Router>();

      grapheneRoot = new GrapheneRoot(router);
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

      // Construct the hierarchy 
      ConstructHierarchy(plates);

      // Second initialize
      foreach (var item in dependents.Where(x => x is ILateInitializable).Select(x => x as ILateInitializable))
        item.LateInitialize();

      onPostInitialize?.Invoke(dependents);

      sw.Stop();
      UnityEngine.Debug.Log($"Graphene initialization time: {sw.ElapsedMilliseconds}ms");
    }


    void ConstructHierarchy(List<Plate> plates)
    {
    }

    void Update()
    {
      if (Time.time - lastRefreshTime < bindingRefreshRate)
        return;

      Profiler.BeginSample("Update Graphene bindings", this);
      BindingManager.OnUpdate();
      lastRefreshTime = Time.time;
      Profiler.EndSample();
    }

  }
}