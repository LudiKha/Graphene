
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Graphene.Elements
{
  public class GrButton : TemplateRef, IBindableElement<ControlType>, IGrapheneElement
  {
    /// <summary>
    /// Instantiates a <see cref="id"/> using the data read from a UXML file.
    /// </summary>
    public new class UxmlFactory : UxmlFactory<GrButton, UxmlTraits> { }

    /// <summary>
    /// USS class name of elements of this type.
    /// </summary>
    /// <remarks>
    /// Graphene adds this USS class to every instance of the Template element. Any styling applied to
    /// this class affects every button located beside, or below the stylesheet in the visual tree.
    /// </remarks>
    public static readonly string ussClassName = "gr-button-ref";

    /// <summary>
    /// Constructs an Template.
    /// </summary>
    public GrButton() : this(null)
    {
      m_Type = ControlType.Button;
    }

    public GrButton(Renderer renderer) : base(renderer)
    {
      m_Type = ControlType.Button;
      AddToClassList(ussClassName);
    }
  }
}