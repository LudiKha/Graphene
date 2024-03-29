﻿using System.Collections.Generic;
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
    [SerializeField, Tooltip("Disable this if you want to manually initialize Graphene")] new bool runInEditMode = false;
    [SerializeField] string addClasses;
    [SerializeField] public PickingMode defaultPickingMode;

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
    [SerializeField] List<Plate> plates = new List<Plate>(); public IReadOnlyList<Plate> Plates => plates;

    List<IGrapheneDependent> dependents = new List<IGrapheneDependent>();
    public event System.Action<ICollection<IGrapheneDependent>> onPreInitialize;
    public event System.Action<ICollection<IGrapheneDependent>> onPostInitialize;
    public event System.Action<BindableElement, object> onBindElement;

    /// <summary>
    /// Root Graphene element controller
    /// </summary>
    GrapheneRoot grapheneRoot; public GrapheneRoot GrapheneRoot => grapheneRoot;

    /// <summary>
    /// The UI Document
    /// </summary>
    [SerializeField] UIDocument doc; public UIDocument Doc => doc;
    /// <summary>
    /// The router
    /// </summary>
    [SerializeField] Router router; public Router Router => router;

    public bool IsInitialized => grapheneRoot != null;

    public bool IsActiveAndInitialized => isActiveAndEnabled && IsInitialized;

    public bool IsActiveAndVisible => IsActiveAndInitialized && !grapheneRoot.ClassListContains("hidden");

    #region Events
    public event System.Action<Plate> plateOnShow;
    public event System.Action<Plate> plateOnHide;
    #endregion

    protected void Start()
    {
      GetLocalReferences();
      if (!Application.isPlaying)
      {
        GetChildPlates();
        return;
      }

      if (enabled && initializeOnStart)
        Initialize();
    }

    #region Attributes
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.ShowInInspector]
#elif NAUGHTY_ATTRIBUTES
    [NaughtyAttributes.ShowInInspector]
#endif
    #endregion
    public bool Initialized { get; private set; }

    public bool IsValid => doc && doc.panelSettings && doc.visualTreeAsset;

    #region ButtonAttribute
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.ResponsiveButtonGroup]
#elif NAUGHTY_ATTRIBUTES
    [NaughtyAttributes.Button]
#endif
    #endregion
    public void Initialize()
    {
      if (Initialized)
        return;
      GetLocalReferences();

      if (!IsValid)
      {
        UnityEngine.Debug.LogError($"Graphene missing requirements. Please make sure UIDocument is present, has PanelSettings and a VisualTreeAsset", this);
        return;
      }

      RunInstallation();

      //doc.enabled = false;
      //doc.enabled = true;
      Initialized = true;

      FinalizeInitialzation();
    }

    protected void GetLocalReferences()
    {
      doc ??= GetComponent<UIDocument>();
      router ??= GetComponent<Router>();
    }

#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.ResponsiveButtonGroup]
#endif
    public void GetChildPlates()
    {
      plates = GetComponentsInChildren<Plate>(true).ToList();
	}

    protected void RunInstallation()
    {
      var sw = new Stopwatch();
      sw.Start();

      dependents = GetComponentsInChildren<IGrapheneDependent>(true).ToList();
      GetChildPlates();

      // Inject Graphene into
      foreach (var c in dependents)
      {
        if (c is GrapheneComponent gc)
          gc.Inject(this);
      }

      Profiler.BeginSample("Graphene Initialize", this);
      onPreInitialize?.Invoke(dependents);
      // First initialize
      foreach (var item in dependents.Where(x => x is IGrapheneInitializable).Select(x => x as IGrapheneInitializable))
        item.Initialize();
      Profiler.EndSample();

      Profiler.BeginSample("Graphene Construct VisualTree", this);
      // Construct the visual tree hierarchy 
      ConstructVisualTree(plates);
      Profiler.EndSample();

      Profiler.BeginSample("Graphene Late Initialize", this);
      // Second initialize
      foreach (var item in dependents.Where(x => x is IGrapheneLateInitializable).Select(x => x as IGrapheneLateInitializable))
        item.LateInitialize();
      onPostInitialize?.Invoke(dependents);
      Profiler.EndSample();

      sw.Stop();
      UnityEngine.Debug.Log($"Graphene ({gameObject.scene.name}/{gameObject.name}) initialization: {sw.ElapsedMilliseconds}ms", this);
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
        if (!plate.VisualTreeAsset)
        {
          UnityEngine.Debug.LogError($"Missing Plate VisualTreeAsset {plate}", plate);
          continue;
        }

        try
        {
          plate.ConstructVisualTree();
        }
        catch (System.Exception e)
        {
          UnityEngine.Debug.LogError(e, plate);
        }
      }

      // Refresh hierarchy -> render & compose children
      foreach (Plate plate in plates)
      {
        if (!plate.VisualTreeAsset)
          continue;

        RegisterPlate(plate);
      }

      sw.Stop();
      //UnityEngine.Debug.Log($"Graphene ConstructVisualTree: {sw.ElapsedMilliseconds}ms");
    }

    public void AddPlate(Plate plate)
    {
      if (!plates.Contains(plate))
        plates.Add(plate);
    }

    public void RegisterPlate(Plate plate)
    {
      if (plate.IsRootPlate)
      {
        grapheneRoot.Add(plate.Root);
        plate.Root.AddToClassList("unity-ui-document__child");
      }

      plate.Root.name = $"{plate.gameObject.name}-container";
      plate.RenderAndComposeChildren();
      plate.HideImmediately(); // Hide immediately by default

      plate.onShow.AddListener(() => { plateOnShow?.Invoke(plate); });
      plate.onHide.AddListener(() => { plateOnHide?.Invoke(plate); });

      plate.ReevaluateState();
    }

    #region Build VisualElement

    void CreateRootElement()
    {
      // Create the root controller
      grapheneRoot = new GrapheneRoot(router);
      grapheneRoot.AddMultipleToClassList(addClasses);
      grapheneRoot.pickingMode = defaultPickingMode;

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
      lastRefreshTime = Time.unscaledTime;
    }

    void Update()
    {
      if (!Application.isPlaying && !runInEditMode)
        return;

      if (Time.unscaledTime - lastRefreshTime < bindingRefreshRate)
        return;

      if (grapheneRoot == null || !grapheneRoot.visible || grapheneRoot.IsHidden())
        return;

      Profiler.BeginSample($"Update Graphene bindings ({BindingManager.bindingsCount} bindings)", this);
      BindingManager.OnUpdate();
      lastRefreshTime = Time.unscaledTime;
      Profiler.EndSample();
    }

    public void RebuildBranch(Plate plate)
    {

    }

    // Needs to be in because UIDocument destroys the root
    private void OnEnable()
    {
#if UNITY_EDITOR
      if (UnityEditor.EditorApplication.isCompiling || UnityEditor.BuildPipeline.isBuildingPlayer)
        return;
#endif

      if (grapheneRoot != null && initializeOnStart)
        // Live reload
        Rebuild();
      return;
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
#if UNITY_EDITOR
      if (UnityEditor.EditorApplication.isCompiling || UnityEditor.BuildPipeline.isBuildingPlayer)
        return;
#endif
    }

    bool canRebuild => Initialized && doc.enabled && isActiveAndEnabled;

    public event System.Action onRebuild;
    #region ButtonAttribute
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.ResponsiveButtonGroup]
#elif NAUGHTY_ATTRIBUTES
    [NaughtyAttributes.Button]
#endif
    #endregion
    public void Rebuild()
    {
      if (!Application.isPlaying && !runInEditMode)
        return;

      if (!canRebuild)
        return;

      if (grapheneRoot != null)
      {
        grapheneRoot.parent?.Remove(grapheneRoot);
        grapheneRoot.Clear();
        grapheneRoot = null;
      }

      ConstructVisualTree(plates);
      FinalizeInitialzation();
      onRebuild?.Invoke();
    }

#if UNITY_EDITOR
    public string bindingsInfo => $"{BindingManager.bindingsCount} bindings";
#endif

    //private void OnValidate()
    //{

    //  LiveLink
    //  if (Application.isPlaying || doc.rootVisualElement == null)
    //    return;

    //  RebuildRootElement();
    //}

    public void BroadcastBindCallback(BindableElement el, object context, Plate plate) => onBindElement?.Invoke(el, context);
  }
}