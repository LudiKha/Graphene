
using Graphene.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Graphene.Elements
{
  public class ButtonGroup : GroupBox, IBindableElement<int>, INotifyValueChanged<int>
  {
    public const string itemsPath = "Items";

    [SerializeField]
    private List<string> m_Items = new List<string>();

    public List<string> items
    {
      get => m_Items;
      set
      {
        SetItems(value);
      }
    }

    /// <summary>
    /// Instantiates a <see cref="id"/> using the data read from a UXML file.
    /// </summary>
    public new class UxmlFactory : UxmlFactory<ButtonGroup, UxmlTraits> { }

    /// <summary>
    /// Defines <see cref="UxmlTraits"/> for the <see cref="id"/>.
    /// </summary>
    public new class UxmlTraits : BindableElement.UxmlTraits
    {
      UxmlIntAttributeDescription m_ActiveIndex = new UxmlIntAttributeDescription { name = "activeIndex" };
      UxmlStringAttributeDescription m_Items = new UxmlStringAttributeDescription { name = "items" };

      /// <summary>
      /// Initialize <see cref="id"/> properties using values from the attribute bag.
      /// </summary>
      /// <param name="ve">The object to initialize.</param>
      /// <param name="bag">The attribute bag.</param>
      /// <param name="cc">The creation context; unused.</param>
      public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
      {
        base.Init(ve, bag, cc);

		ButtonGroup buttonGroup = (ButtonGroup)ve;

		buttonGroup.value = m_ActiveIndex.GetValueFromBag(bag, cc);
		buttonGroup.items = SelectField.ParseChoiceList(m_Items.GetValueFromBag(bag, cc));
      }
    }

    [SerializeField]
    private int m_ActiveIndex = 0;
    public virtual int value
    {
      get { return m_ActiveIndex; }
      set
      {
        value = Mathf.Clamp(value, 0, childCount - 1);
        if (m_ActiveIndex == value)
        {
          SetValueWithoutNotify(value);
          return;
        }

        // in order for the serialization binding to update it's expecting you
        // to dispatch the event
        using (ChangeEvent<int> valueChangeEvent = ChangeEvent<int>.GetPooled(m_ActiveIndex, value))
        {
          valueChangeEvent.target = this; // very umportant
          SetValueWithoutNotify(value); // actually set the value and do any init with the value
          SendEvent(valueChangeEvent);
        }
      }
    }

    public event System.Action<int, string> clicked;

    public void SetValueWithoutNotify(int value)
    {
      m_ActiveIndex = Mathf.Clamp(value, 0, childCount - 1);
      SetButtonActive();
    }

    /// <summary>
    /// USS class name of elements of this type.
    /// </summary>
    /// <remarks>
    /// Unity adds this USS class to every instance of the TabGroup element. Any styling applied to
    /// this class affects every button located beside, or below the stylesheet in the visual tree.
    /// </remarks>
    public static new readonly string ussClassName = "gr-button-group ";
    public static readonly string ussActiveClassName = "active";

    /// <summary>
    /// Constructs an TabGroup.
    /// </summary>
    public ButtonGroup()
    {
      AddToClassList(ussClassName);
    }

    /// <summary>
    /// Callback from two-way binding system that model changed
    /// </summary>
    /// <param name="newValue"></param>
    public void OnModelChange(int newValue)
    {
      tabIndex = newValue;
    }

    internal void SetButtonActive()
    {
      int i = 0;
      foreach (var child in Children())
      {
        if (i == value)
        {
          child.AddToClassList(ussActiveClassName);
        }
        else
          child.RemoveFromClassList(ussActiveClassName);
        i++;
      }
    }

    public void SetItems(List<string> items)
    {
      m_Items = items ?? new List<string>();
      RefreshButtons();
    }

    public IList<ActionButton> SourceData = new List<ActionButton>();

    public void RefreshButtons()
	{
	  Clear();

      foreach (var item in items)
      {
		InternalAddItem(item);
	  }
	}

    internal void ButtonClicked(int i)
    {
      value = i;
      clicked?.Invoke(i, items[i]);

      if (SourceData != null && i < SourceData.Count)
        SourceData[i].OnClick?.Invoke();
    }

    public void ClearItems()
    {
      m_Items.Clear();
      Clear();
    }

    public void ClearCallback()
    {
      clicked = null;
    }

    public void AddItem(string text, string tooltip = null)
    {
      items.Add(text);
      InternalAddItem(text, tooltip);
	}

    void InternalAddItem(string text, string tooltip = null)
	{
	  int buttonIndex = childCount;
	  var btn = new Button(() => ButtonClicked(buttonIndex));
	  btn.text = text.ToUpper();
	  btn.tooltip = tooltip;
	  btn.AddToClassList("gr-button");
	  Add(btn);
	}
  }
}