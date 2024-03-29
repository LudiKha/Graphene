﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Graphene.Elements;

namespace Graphene
{
  [System.Serializable]
  public class ViewRef
  {
    public readonly string defaultSelector;
    [SerializeField, HideInInspector] Plate plate;
    #region ValueDropdownAttribute
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.ValueDropdown("GetViewsFromVisualTreeAsset")]
#endif
    #endregion
    [SerializeField] protected string id; public string Id => id;

    public View view;

    #region ShowInInspectorAttribute
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.ShowInInspector]
#endif
    #endregion
    public bool initialized => view != null;

    public IEnumerable<string> GetViewsFromVisualTreeAsset()
    {
      if (!plate)
        return Enumerable.Empty<string>();
      else if (plate.viewIds != null && plate.viewIds.Count > 0)
        return plate.viewIds;
      else
        return new List<string> { "" };
    }

    public ViewRef(string defaultId)
    {
      this.defaultSelector = defaultId;
    }

    public static implicit operator bool(ViewRef viewRef) => viewRef != null && !string.IsNullOrWhiteSpace(viewRef.id);

    public void ResolveView(Plate plate)
    {
      this.plate = plate;

      // Invalid?
      if (string.IsNullOrWhiteSpace(id))
        id = defaultSelector;

      view = plate.GetViewById(id);
      //if(view == null)
      //{
      //  Debug.LogError($"No view found for Id {id} {plate}", plate);
      //}
    }

    public void SetPlate(Plate plate)
    {
      this.plate = plate;
    }

    public void NoParent()
    {
      this.view = null;
      this.plate = null;
    }
  }

  [DisallowMultipleComponent]
  public class ViewHandle : MonoBehaviour
  {
    #region ButtonAttribute
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.ValueDropdown("GetViewsFromVisualTreeAsset")]
#endif
    #endregion
    [SerializeField] protected string id; public string Id => id;

    public IEnumerable<string> GetViewsFromVisualTreeAsset()
    {
      Plate plate = GetComponent<Plate>();
      //if (plate.IsRootPlate)
      //  return new List<string>();

      var root = plate.transform.parent.GetComponent<Plate>().VisualTreeAsset.CloneTree();
      // Get views
      return root?.Query<View>().ToList().Select(v => v.id);
    }
  }
}