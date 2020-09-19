
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Graphene.Elements
{
  public class ButtonGroup : BindableElement, IBindableElement<int>
  {
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

      /// <summary>
      /// Initialize <see cref="id"/> properties using values from the attribute bag.
      /// </summary>
      /// <param name="ve">The object to initialize.</param>
      /// <param name="bag">The attribute bag.</param>
      /// <param name="cc">The creation context; unused.</param>
      public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
      {
        base.Init(ve, bag, cc);

        ((ButtonGroup)ve).activeIndex = m_ActiveIndex.GetValueFromBag(bag, cc);
      }
    }

    [SerializeField]
    private int m_ActiveIndex = 0;
    public virtual int activeIndex
    {
      get { return m_ActiveIndex; }
      set
      {
        m_ActiveIndex = Mathf.Clamp(value, 0, childCount);
        SetButtonActive();
      }
    }

    /// <summary>
    /// USS class name of elements of this type.
    /// </summary>
    /// <remarks>
    /// Unity adds this USS class to every instance of the TabGroup element. Any styling applied to
    /// this class affects every button located beside, or below the stylesheet in the visual tree.
    /// </remarks>
    public static readonly string ussClassName = "gr-button-group";
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
        if (i == activeIndex)
        {
          child.AddToClassList(ussActiveClassName);
        }
        else
          child.RemoveFromClassList(ussActiveClassName);
        i++;
      }
    }
  }
}