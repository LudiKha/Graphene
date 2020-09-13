using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace Graphene {
  public class BindingsSystem : MonoBehaviour
  {
    [SerializeField] float interval = 0.02f;
    [SerializeField, ReadOnly] float lastUpdateTime;
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