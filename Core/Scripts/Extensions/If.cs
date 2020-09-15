
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Graphene
{
  public interface IBindableElement<TValue>
  {
    void OnModelChange(TValue newValue);
  }

  public class If : BindableElement, IBindableElement<object>
  {
    /// <summary>
    /// Instantiates a <see cref="id"/> using the data read from a UXML file.
    /// </summary>
    public new class UxmlFactory : UxmlFactory<If, UxmlTraits> { }

    /// <summary>
    /// Defines <see cref="UxmlTraits"/> for the <see cref="id"/>.
    /// </summary>
    public new class UxmlTraits : BindableElement.UxmlTraits
    {
      UxmlBoolAttributeDescription m_Value = new UxmlBoolAttributeDescription { name = "value" };

      /// <summary>
      /// Initialize <see cref="id"/> properties using values from the attribute bag.
      /// </summary>
      /// <param name="ve">The object to initialize.</param>
      /// <param name="bag">The attribute bag.</param>
      /// <param name="cc">The creation context; unused.</param>
      public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
      {
        base.Init(ve, bag, cc);

        ((If)ve).value = m_Value.GetValueFromBag(bag, cc); 
      }
    }

    [SerializeField]
    private bool m_Value = false;
    public virtual bool value
    {
      get { return m_Value; }
      set
      {
        m_Value = value;
        if (m_Value)
          this.Show();
        else
          this.Hide();
      }
    }

    /// <summary>
    /// USS class name of elements of this type.
    /// </summary>
    /// <remarks>
    /// Unity adds this USS class to every instance of the If element. Any styling applied to
    /// this class affects every button located beside, or below the stylesheet in the visual tree.
    /// </remarks>
    public static readonly string ussClassName = "gr-if";

    /// <summary>
    /// Constructs an If.
    /// </summary>
    public If() 
    {
      AddToClassList(ussClassName);
    }

    public void OnModelChange(object newValue)
    {
      if (newValue is bool b)
        value = b;
      else if (newValue is null || newValue.Equals(false))
        value = false;
      else
        value = true;
    }
  }
}