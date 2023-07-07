using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using UnityEngine.UIElements;

namespace Graphene.Elements
{
  public class CycleField : BaseField<int>
  {
    public const string itemsPath = "Items";

    [SerializeField]
    private List<string> m_Items = new List<string>();

    public List<string> items { get => m_Items;
    set
      {
        if (value != null)
          m_Items = value;
        else
          m_Items = new List<string>();
      }
    }

    /// <summary>
    /// Instantiates a <see cref="CycleField"/> using the data read from a UXML file.
    /// </summary>
    public new class UxmlFactory : UxmlFactory<CycleField, UxmlTraits> { }

    /// <summary>
    /// Defines <see cref="UxmlTraits"/> for the <see cref="CycleField"/>.
    /// </summary>
    public new class UxmlTraits : BaseFieldTraits<int, UxmlIntAttributeDescription>
    {
      UxmlStringAttributeDescription m_Text = new UxmlStringAttributeDescription { name = "text" };
      UxmlStringAttributeDescription m_Items = new UxmlStringAttributeDescription { name = "items" };
      UxmlStringAttributeDescription m_Plus = new UxmlStringAttributeDescription { name = "plusSymbol" };
      UxmlStringAttributeDescription m_Minus = new UxmlStringAttributeDescription { name = "minusSymbol" };

      /// <summary>
      /// Initialize <see cref="CycleField"/> properties using values from the attribute bag.
      /// </summary>
      /// <param name="ve">The object to initialize.</param>
      /// <param name="bag">The attribute bag.</param>
      /// <param name="cc">The creation context; unused.</param>
      public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
      {
        base.Init(ve, bag, cc);
        var cycleField = (CycleField)ve;
		cycleField.text = m_Text.GetValueFromBag(bag, cc);
		cycleField.items = m_Items.GetValueFromBag(bag, cc).Split(';').Where(x => !string.IsNullOrEmpty(x)).ToList();
        cycleField.plusSymbol = m_Plus.GetValueFromBag(bag, cc);
        cycleField.minusSymbol = m_Minus.GetValueFromBag(bag, cc);
	  }
	}

    /// <summary>
    /// USS class name of elements of this type.
    /// </summary>
    public new static readonly string ussClassName = "gr-cycle-field";
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


    public static readonly string previousButtonUssClassName = ussClassName + "__button-previous";
    public static readonly string nextButtonUssClassName = ussClassName + "__button-next";

    /// <summary>
    /// USS class name of text elements in elements of this type.
    /// </summary>
    public static readonly string textUssClassName = ussClassName + "__text";

    public static readonly string hiddenClassName = "hidden";


    private Label m_Label;
    private VisualElement visualInput;

    Button m_Next;
    Button m_Previous;

	public string plusSymbol = "›";   // › ▶〉→
	public string minusSymbol = "‹";  // ‹ ◀〈 ←

	public CycleField()
        : this(null) {
    }

    public CycleField(string label)
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

      // Allocate and add the buttons to the hierarchy
      m_Previous = new Button();
      m_Next = new Button();

      m_Previous.text = minusSymbol;
      m_Next.text = plusSymbol;
      //m_Next.AddToClassList("bold");
      //m_Previous.AddToClassList("bold");

      m_Previous.focusable = true;
      m_Next.focusable = true;

      m_Previous.AddToClassList(previousButtonUssClassName);
      m_Next.AddToClassList(nextButtonUssClassName);
	  m_Previous.AddToClassList("nomargin");
	  m_Next.AddToClassList("nomargin");

	  visualInput.Add(m_Previous);

      m_Label = new Label
      {
        pickingMode = PickingMode.Ignore
      };
      m_Label.text = "Select an option";
      visualInput.Add(m_Label);
      visualInput.Add(m_Next);

      m_Previous.clicked += M_Previous_clicked;
      m_Next.clicked += M_Next_clicked;

      // Set-up the label and text...
      text = null;
    }

    private void M_Previous_clicked()
    {
      int newValue = value - 1;
      if (newValue < 0)
        newValue = items.Count - 1;

      value = newValue;
    }

    private void M_Next_clicked()
    {
      int newValue = value + 1;
      if (newValue >= items.Count)
        newValue = 0;

      value = newValue;
    }

    public override void SetValueWithoutNotify(int newValue)
    {
      base.SetValueWithoutNotify(newValue);
      string newText = "";

      if (newValue >= 0 && newValue < items.Count)
        newText = items[newValue];

      text = newText;
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
      }
    }

  }
}