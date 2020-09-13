using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using UnityEngine.UIElements;

namespace Graphene
{
  public class SelectField : BaseField<int>, IDisposable
  {
    public const string itemsPath = "Items";
    public const int defaultItemHeight = 24;

    [SerializeField]
    private List<string> m_Items = new List<string>();

    public List<string> items { get => m_Items;
    set
      {
        m_Items = value.ToList();
        m_ListView.itemsSource = m_Items;
      }
    }

    /// <summary>
    /// Instantiates a <see cref="SelectField"/> using the data read from a UXML file.
    /// </summary>
    public new class UxmlFactory : UxmlFactory<SelectField, UxmlTraits> { }

    /// <summary>
    /// Defines <see cref="UxmlTraits"/> for the <see cref="SelectField"/>.
    /// </summary>
    public new class UxmlTraits : BaseFieldTraits<int, UxmlIntAttributeDescription>
    {
      UxmlIntAttributeDescription m_ItemHeight = new UxmlIntAttributeDescription { name = "itemHeight" };
      UxmlStringAttributeDescription m_Text = new UxmlStringAttributeDescription { name = "text" };
      UxmlStringAttributeDescription m_Items = new UxmlStringAttributeDescription { name = "items" };

      /// <summary>
      /// Initialize <see cref="SelectField"/> properties using values from the attribute bag.
      /// </summary>
      /// <param name="ve">The object to initialize.</param>
      /// <param name="bag">The attribute bag.</param>
      /// <param name="cc">The creation context; unused.</param>
      public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
      {
        base.Init(ve, bag, cc);

        int itemHeight = m_ItemHeight.GetValueFromBag(bag, cc);
        if (itemHeight <= 0)
          itemHeight = defaultItemHeight;

        ((SelectField)ve).m_ListView.itemHeight = itemHeight;

        ((SelectField)ve).text = m_Text.GetValueFromBag(bag, cc);
        ((SelectField)ve).items = m_Items.GetValueFromBag(bag, cc).Split(';').Where(x => !string.IsNullOrEmpty(x)).ToList();
      }
    }

    /// <summary>
    /// USS class name of elements of this type.
    /// </summary>
    public new static readonly string ussClassName = "unity-select-field";
    /// <summary>
    /// USS class name of labels in elements of this type.
    /// </summary>
    public new static readonly string labelUssClassName = ussClassName + "__label";
    /// <summary>
    /// USS class name of input elements in elements of this type.
    /// </summary>
    public new static readonly string inputUssClassName = ussClassName + "__input";
    /// <summary>
    /// USS class name of elements of this type, when there is no text.
    /// </summary>
    public static readonly string noTextVariantUssClassName = ussClassName + "--no-text";
    /// <summary>
    /// USS class name of elements of this type.
    /// </summary>
    public static readonly string listContainerUssClassName = ussClassName + "__list-container";
    public static readonly string listViewUssClassName = ussClassName + "__list-view";
    /// <summary>
    /// USS class name of text elements in elements of this type.
    /// </summary>
    public static readonly string textUssClassName = ussClassName + "__text";

    public static readonly string hiddenClassName = "hidden";

    private Label m_Label;
    private VisualElement visualInput;
    VisualElement m_ListContainer;
    private ListView m_ListView;

    Toggle m_Toggle;
    

    public SelectField()
        : this(null) {


    }

    public SelectField(string label)
        : base(label, null)
    {

      // Hax
      var children = hierarchy.Children().ToList();
      visualInput = hierarchy.Children().ToList().Find(x => x.ClassListContains("unity-base-field__input"));

      AddToClassList(ussClassName);
      AddToClassList(noTextVariantUssClassName);

      visualInput.AddToClassList(inputUssClassName);
      labelElement.AddToClassList(labelUssClassName);

      // The picking mode needs to be Position in order to have the Pseudostate Hover applied...
      visualInput.pickingMode = PickingMode.Position;

      // Set-up the label and text...
      text = null;
      this.AddManipulator(new Clickable(OnClickEvent));


      m_Toggle = new Toggle();
      m_Toggle.text = text;
      m_Toggle.RegisterValueChangedCallback((evt) =>
      {
        SetToggleState(m_Toggle.value);
        evt.StopPropagation();
      });
      visualInput.Add(m_Toggle);

      m_ListContainer = new VisualElement();
      m_ListContainer.AddToClassList(listContainerUssClassName);
      m_ListContainer.AddManipulator(new Clickable(OnClickBackground));

      hierarchy.Add(m_ListContainer);

      m_ListView = new ListView(items, 24, MakeItem, BindItem);
      m_ListView.AddToClassList(listViewUssClassName);
      m_ListView.AddToClassList("h2");
      m_ListView.bindingPath = "Items";
      m_ListView.focusable = true;

      m_ListView.onSelectionChange += M_ListView_onSelectionChange;
      m_ListView.onItemsChosen += M_ListView_onItemsChosen;

      Func<VisualElement> makeItem = () => new Label("ListViewOption");
      Action<VisualElement, int> bindItem = (e, i) => (e as Label).text = (e as Label).text + " " + i;
      bindItem = (e, i) => (e as Label).text = items[i];

      m_ListView.makeItem = makeItem;
      m_ListView.bindItem = bindItem;
      //m_ListView.reorderable = true;

      m_ListContainer.Add(m_ListView);
      SetToggleState(false);
      m_ListView.itemsSource = items;

    }

    private void M_ListView_onItemsChosen(IEnumerable<object> obj)
    {
      value = m_ListView.selectedIndex;
    }

    private void M_ListView_onSelectionChange(IEnumerable<object> obj)
    {
      value = m_ListView.selectedIndex;
    }

    public override void SetValueWithoutNotify(int newValue)
    {
      base.SetValueWithoutNotify(newValue);

      string newText = "";
      if (newValue >= 0 && newValue < m_ListView.itemsSource.Count)
        newText = (string)m_ListView.itemsSource[newValue];

      text = newText;
      m_Toggle.text = newText;
    }

    /// <summary>
    /// Optional text after the toggle.
    /// </summary>
    public string text
    {
      get { return m_Label?.text; }
      set
      {
        if (!string.IsNullOrEmpty(value))
        {
          // Lazy allocation of label if needed...
          if (m_Label == null)
          {
            m_Label = new Label
            {
              pickingMode = PickingMode.Ignore
            };
            m_Label.AddToClassList(textUssClassName);
            RemoveFromClassList(noTextVariantUssClassName);
            visualInput.Add(m_Label);
          }

          m_Label.text = value;
        }
        else if (m_Label != null)
        {
          m_Label.RemoveFromHierarchy();
          AddToClassList(noTextVariantUssClassName);
          m_Label = null;
        }
      }
    }


    bool ProcessClick(EventBase evt)
    {
      if (evt.eventTypeId == MouseUpEvent.TypeId())
      {
        var ce = (IMouseEvent)evt;
        if (ce.button == (int)MouseButton.LeftMouse)
        {
          return true; 
        }
      }
      else if (evt.eventTypeId == PointerUpEvent.TypeId() || evt.eventTypeId == ClickEvent.TypeId())
      {
        var ce = (IPointerEvent)evt;
        if (ce.button == (int)MouseButton.LeftMouse)
        {
          return true;

        }
      }
      return false;
    }

    void OnClickEvent(EventBase evt)
    {
      if(ProcessClick(evt))
        OnClick();
    }

    void OnClickBackground(EventBase evt)
    {
      if (ProcessClick(evt))
      {
        m_Toggle.value = false;
        this.Focus();
      }
    }


    void OnClick()
    {
      m_Toggle.value = !m_Toggle.value;
    }

    void SetToggleState(bool value)
    {
      m_ListContainer.SetEnabled(value);

      if (value)
      {
        var root = this.GetRootRecursively();
        var screen = root.Q(null, "screen");

        if(screen != null)
          root.Add(m_ListContainer);
        else
          root.Add(m_ListContainer);


        m_ListView.RemoveFromClassList(hiddenClassName);
        m_ListContainer.RemoveFromClassList(hiddenClassName);
        m_ListContainer.Add(m_ListView);
        m_ListView.Focus();
        m_ListContainer.BringToFront();
      }
      else
      {
        m_ListView.AddToClassList(hiddenClassName);
        m_ListContainer.AddToClassList(hiddenClassName);
        visualInput.Add(m_ListView);
      }
    }
    VisualElement MakeItem()
    {
      return new Button();
    }

    void BindItem(VisualElement el, int index)
    {
      (el as TextElement).text = items[index];
      Debug.Log($"Created item at element {index}");
    }

    public void Dispose()
    {
      this.m_ListContainer.parent.Remove(this.m_ListContainer);
    }
  }
}