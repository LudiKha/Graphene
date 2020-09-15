
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Graphene
{
  public class If : BindableElement
  {
    /// <summary>
    /// Instantiates a <see cref="id"/> using the data read from a UXML file.
    /// </summary>
    public new class UxmlFactory : UxmlFactory<View, UxmlTraits> { }

    /// <summary>
    /// Defines <see cref="UxmlTraits"/> for the <see cref="id"/>.
    /// </summary>
    public new class UxmlTraits : BindableElement.UxmlTraits
    {
      UxmlStringAttributeDescription m_Id = new UxmlStringAttributeDescription { name = "id" };
      UxmlBoolAttributeDescription m_Value = new UxmlBoolAttributeDescription { name = "default" };

      /// <summary>
      /// Initialize <see cref="id"/> properties using values from the attribute bag.
      /// </summary>
      /// <param name="ve">The object to initialize.</param>
      /// <param name="bag">The attribute bag.</param>
      /// <param name="cc">The creation context; unused.</param>
      public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
      {
        base.Init(ve, bag, cc);

        ((View)ve).id = m_Id.GetValueFromBag(bag, cc);
        ((View)ve).isDefault = m_Value.GetValueFromBag(bag, cc);
      }
    }

    [SerializeField]
    private string m_Id = String.Empty;
    public virtual string id
    {
      get { return m_Id; }
      set
      {
        m_Id = value;
      }
    }

    [SerializeField]
    private bool m_Default = false;
    public virtual bool isDefault
    {
      get { return m_Default; }
      set
      {
        m_Default = value;
      }
    }

    /// <summary>
    /// USS class name of elements of this type.
    /// </summary>
    /// <remarks>
    /// Unity adds this USS class to every instance of the View element. Any styling applied to
    /// this class affects every button located beside, or below the stylesheet in the visual tree.
    /// </remarks>
    public static readonly string ussClassName = "unity-view";

    /// <summary>
    /// Constructs a View.
    /// </summary>
    public If() : this(null)
    {
    }

    /// <summary>
    /// Constructs a View with an Action that is triggered when the button is clicked.
    /// </summary>
    /// <param name="clickEvent">The action triggered when the button is clicked.</param>
    /// <remarks>
    /// By default, a single left mouse click triggers the Action. To change the activator, modify <see cref="clickable"/>.
    /// </remarks>
    public If(string id) : base ()
    {
      AddToClassList(ussClassName);
      this.id = id;
    }
  }
}