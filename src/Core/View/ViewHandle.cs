using Graphene.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Graphene
{
  /// <summary>
  /// Represents a reference to a <see cref="View"/> within a <see cref="Plate"/>.
  /// Used to resolve and access views by ID, and to manage their association with plates.
  /// </summary>
  [Serializable]
  public class ViewRef
  {
	/// <summary>
	/// The default selector used if no ID is specified.
	/// </summary>
	public readonly string defaultSelector;

	[SerializeField, HideInInspector]
	Plate plate;

	#region ValueDropdownAttribute
#if ODIN_INSPECTOR
	[Sirenix.OdinInspector.ValueDropdown("GetViewsFromVisualTreeAsset")]
#endif
	#endregion
	/// <summary>
	/// The ID of the view to reference.
	/// </summary>
	[SerializeField]
	protected string id;
	/// <summary>
	/// Gets the ID of the referenced view.
	/// </summary>
	public string Id => id;

	/// <summary>
	/// The resolved <see cref="View"/> instance.
	/// </summary>
	public View view;

	#region ShowInInspectorAttribute
#if ODIN_INSPECTOR
	[Sirenix.OdinInspector.ShowInInspector]
#endif
	#endregion
	/// <summary>
	/// Indicates whether the view has been successfully resolved.
	/// </summary>
	public bool initialized => view != null;

	/// <summary>
	/// Returns a list of available view IDs from the associated plate's visual tree asset.
	/// </summary>
	public IEnumerable<string> GetViewsFromVisualTreeAsset()
	{
	  if (!plate)
		return Enumerable.Empty<string>();
	  else if (plate.viewIds != null && plate.viewIds.Count > 0)
		return plate.viewIds;
	  else
		return new List<string> { "" };
	}

	/// <summary>
	/// Constructs a new <see cref="ViewRef"/> with the specified default ID.
	/// </summary>
	/// <param name="defaultId">The default view selector.</param>
	public ViewRef(string defaultId)
	{
	  this.defaultSelector = defaultId;
	}

	/// <summary>
	/// Implicitly converts a <see cref="ViewRef"/> to a boolean indicating if it is valid and has a non-empty ID.
	/// </summary>
	public static implicit operator bool(ViewRef viewRef) => viewRef != null && !string.IsNullOrWhiteSpace(viewRef.id);

	/// <summary>
	/// Resolves the view reference using the specified plate, updating the <see cref="view"/> field.
	/// </summary>
	/// <param name="plate">The plate to resolve the view from.</param>
	public void ResolveView(Plate plate)
	{
	  this.plate = plate;

	  // Use default selector if ID is not set
	  if (string.IsNullOrWhiteSpace(id))
		id = defaultSelector;

	  view = plate.GetViewById(id);
	}

	/// <summary>
	/// Sets the plate associated with this view reference.
	/// </summary>
	/// <param name="plate">The plate to associate.</param>
	public void SetPlate(Plate plate)
	{
	  this.plate = plate;
	}

	/// <summary>
	/// Clears the parent plate and view reference.
	/// </summary>
	public void NoParent()
	{
	  this.view = null;
	  this.plate = null;
	}
  }

  [System.Obsolete("Use ViewRef instead")]

  /// <summary>
  /// Component for referencing a view by ID within a plate.
  /// Used to select and interact with views in the UI hierarchy.
  /// </summary>
  [DisallowMultipleComponent]
  public class ViewHandle : MonoBehaviour
  {
	#region ButtonAttribute
#if ODIN_INSPECTOR
	[Sirenix.OdinInspector.ValueDropdown("GetViewsFromVisualTreeAsset")]
#endif
	#endregion
	/// <summary>
	/// The ID of the view to reference.
	/// </summary>
	[SerializeField]
	protected string id;
	/// <summary>
	/// Gets the ID of the referenced view.
	/// </summary>
	public string Id => id;

	/// <summary>
	/// Returns a list of available view IDs from the parent plate's visual tree asset.
	/// </summary>
	public IEnumerable<string> GetViewsFromVisualTreeAsset()
	{
	  Plate plate = GetComponent<Plate>();
	  var root = plate.transform.parent.GetComponent<Plate>().VisualTreeAsset.CloneTree();
	  // Get views
	  return root?.Query<View>().ToList().Select(v => v.id);
	}
  }
}