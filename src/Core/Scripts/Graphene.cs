using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UIElements;

namespace Graphene
{
  [DisallowMultipleComponent]
  public class Graphene : MonoBehaviour
  {
    [SerializeField] Theme globalTheme;
    [SerializeField] float interval = 0.02f;
    [SerializeField] float lastUpdateTime;

    [SerializeField] List<Plate> plates = new List<Plate>();

    List<IGrapheneDependent> dependents = new List<IGrapheneDependent>();
    public event System.Action<ICollection<IGrapheneDependent>> onPreInitialize;
    public event System.Action<ICollection<IGrapheneDependent>> onPostInitialize;

    protected void Awake()
    {
      RunInstallation();
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
      UnityEngine.Debug.Log($"Time: {sw.ElapsedMilliseconds}ms");
    }

    private void Start()
    {
      UIDocument doc;
      if (globalTheme && (doc = GetComponent<UIDocument>()))
        globalTheme.ApplyStyles(doc.rootVisualElement);
    }

    void ConstructHierarchy(List<Plate> plates)
    {
      //foreach (var plate in plates)
      //{
      //  plate.onShow.AddListener(OnShow_Plate)
      //} 
    }

    void Update()
    {
      if (Time.time - lastUpdateTime < interval)
        return;

      Profiler.BeginSample("Update Bindings", this);
      BindingManager.OnUpdate();
      lastUpdateTime = Time.time;
      Profiler.EndSample();
    }
  }
}