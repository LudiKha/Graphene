using Sirenix.OdinInspector;
using Sirenix.Utilities;
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

	void Apply(ListView el);
  }

  public abstract class ListBindable : BindableObjectBase
  {
	[SerializeField] private ControlType controlType = ControlType.ListItem; public ControlType ItemControlType => controlType;
	public CollectionVirtualizationMethod collectionVirtualizationMethod = CollectionVirtualizationMethod.DynamicHeight; public CollectionVirtualizationMethod CollectionVirtualizationMethod => collectionVirtualizationMethod;

	[Range(0, 100)] public int height = 30;
	public SelectionType selectionType = SelectionType.Single; public SelectionType SelectionType => selectionType;
	public bool showBorder; public bool ShowBorder => showBorder;
	public string headerTitle; public string HeaderTitle => headerTitle;
	public bool showFoldoutHeader; public bool ShowFoldoutHeader => showFoldoutHeader;
	public bool showAddRemoveFooter; public bool ShowAddRemoveFooter => showAddRemoveFooter;
	public AlternatingRowBackground alternatingRowBackground; public AlternatingRowBackground AlternatingRowBackground => alternatingRowBackground;
	public bool reorderable; public bool Reorderable => reorderable;
	public ListViewReorderMode reorderMode; public ListViewReorderMode ReorderMode => reorderMode;
	public bool showCollectionSize; public bool ShowCollectionSize => showCollectionSize;

	// Bound element
	public ListView listView;

	[ResponsiveButtonGroup] public void Rebuild()
	{
	  if (listView == null)
		return;

	  Apply(listView);
	  listView.Rebuild();
	}

	public void Apply(ListView el)
	{
	  this.listView = el;
	  el.virtualizationMethod = collectionVirtualizationMethod;
	  el.fixedItemHeight = this.height;
	  el.selectionType = selectionType;
	  el.showBorder = showBorder;
	  el.headerTitle = headerTitle;
	  el.showFoldoutHeader = showFoldoutHeader;
	  el.showAddRemoveFooter = showAddRemoveFooter;
	  el.showAlternatingRowBackgrounds = alternatingRowBackground;
	  el.reorderable = reorderable;
	  el.reorderMode = reorderMode;
	  el.showBoundCollectionSize = showCollectionSize;
	}
  }

  [System.Serializable]
  [Draw(controlType = ControlType.ListView)]
  public class ListBindable<TObjectType> : ListBindable, IListViewBindable//, IList<TObjectType>
  {
	[Bind("Items")] public List<TObjectType> SourceItems = new List<TObjectType>();

	public IList ItemsSource => SourceItems;
	public void Apply(ListView el)
	{
	  this.listView = el;
	  el.virtualizationMethod = collectionVirtualizationMethod;
	  el.fixedItemHeight = this.height;
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

	#region IList<T>
	public TObjectType this[int index] { get => SourceItems[index]; set => SourceItems[index] = value; }

	public int Count => SourceItems.Count;

	public bool IsReadOnly => false;

	public void Add(TObjectType item) => SourceItems.Add(item);

	public void Clear() => SourceItems.Clear();

	public bool Contains(TObjectType item) => SourceItems.Contains(item);

	public void CopyTo(TObjectType[] array, int arrayIndex) => SourceItems.CopyTo(array, arrayIndex);

	public IEnumerator<TObjectType> GetEnumerator() => SourceItems.GetEnumerator();

	public int IndexOf(TObjectType item) => SourceItems.IndexOf(item);

	public void Insert(int index, TObjectType item) => SourceItems.Insert(index, item);

	public bool Remove(TObjectType item) => SourceItems.Remove(item);

	public void RemoveAt(int index) => SourceItems.RemoveAt(index);

	//IEnumerator IEnumerable.GetEnumerator() => SourceItems.GetEnumerator();
	#endregion
  }
}
