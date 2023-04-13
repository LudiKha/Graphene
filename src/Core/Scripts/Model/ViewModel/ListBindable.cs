using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UIElements;

namespace Graphene.ViewModel
{
  public interface IListViewBindable
  {
	IList ItemsSource { get; }

	ControlType ItemControlType { get; }

	CollectionVirtualizationMethod CollectionVirtualizationMethod { get; }
	SelectionType SelectionType { get; }
	bool ShowBorder { get; }
	string HeaderTitle { get; }
	bool ShowFoldoutHeader { get; }
	bool ShowAddRemoveFooter { get; }
	AlternatingRowBackground AlternatingRowBackground { get; }
	bool Reorderable { get; }
	ListViewReorderMode ReorderMode { get; }
	bool ShowCollectionSize { get; }

	System.Action onRefresh { get; set; }
	System.Action onRebuild { get; set; }

	void Apply(ListView el);
  }

  public abstract class ListBindable : BindableObjectBase
  {
	[SerializeField] private ControlType controlType = ControlType.ListItem; public ControlType ItemControlType => controlType;

	public System.Action onRefresh { get; set; }
	public System.Action onRebuild { get; set; }

	public CollectionVirtualizationMethod collectionVirtualizationMethod; public CollectionVirtualizationMethod CollectionVirtualizationMethod => collectionVirtualizationMethod;
	public SelectionType selectionType; public SelectionType SelectionType => selectionType;
	public bool showBorder; public bool ShowBorder => showBorder;
	public string headerTitle; public string HeaderTitle => headerTitle;
	public bool showFoldoutHeader; public bool ShowFoldoutHeader => showFoldoutHeader;
	public bool showAddRemoveFooter; public bool ShowAddRemoveFooter => showAddRemoveFooter;
	public AlternatingRowBackground alternatingRowBackground; public AlternatingRowBackground AlternatingRowBackground => alternatingRowBackground;
	public bool reorderable; public bool Reorderable => reorderable;
	public ListViewReorderMode reorderMode; public ListViewReorderMode ReorderMode => reorderMode;
	public bool showCollectionSize; public bool ShowCollectionSize => showCollectionSize;

	[ResponsiveButtonGroup]	void Refresh() => onRefresh?.Invoke();
	[ResponsiveButtonGroup] void Rebuild() => onRebuild?.Invoke();

  }

  [System.Serializable]
  [Draw(controlType = ControlType.ListView)]
  public class ListBindable<TObjectType> : ListBindable, IListViewBindable
  {
	[Bind("Items")] public List<TObjectType> SourceItems;

	public IList ItemsSource => SourceItems;

	public void Apply(ListView el)
	{
	  el.virtualizationMethod= collectionVirtualizationMethod;
	  el.selectionType= selectionType;
	  el.showBorder= showBorder;
	  el.headerTitle = headerTitle;
	  el.showFoldoutHeader = showFoldoutHeader;
	  el.showAddRemoveFooter= showAddRemoveFooter;
	  el.showAlternatingRowBackgrounds = alternatingRowBackground;
	  el.reorderable = reorderable;
	  el.reorderMode = reorderMode;
	  el.showBoundCollectionSize = showCollectionSize;
	}
  }
}
