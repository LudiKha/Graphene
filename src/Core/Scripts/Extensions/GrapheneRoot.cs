
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Graphene.Elements
{
  /// <summary>
  /// Root Graphene class that contains injec
  /// </summary>
  public class GrapheneRoot : BindableElement
  {
    ///// <summary>
    ///// Instantiates a <see cref="GrapheneRoot"/> using the data read from a UXML file.
    ///// </summary>
    //public new class UxmlFactory : UxmlFactory<GrapheneRoot, UxmlTraits> { }

    [SerializeField]
    private Router m_Router;
    public virtual Router router
    {
      get { return router; }
      set
      {
        m_Router = value;
      }
    }

    /// <summary>
    /// USS class name of elements of this type.
    /// </summary>
    /// <remarks>
    /// Unity adds this USS class to every instance of the GrapheneRoot element. Any styling applied to
    /// this class affects every button located beside, or below the stylesheet in the visual tree.
    /// </remarks>
    public static readonly string ussClassName = "gr-root";

    /// <summary>
    /// Constructs a GrapheneRoot.
    /// </summary>
    public GrapheneRoot() : this(null)
    {
    }

    /// <summary>
    /// Constructs a GrapheneRoot.
    /// </summary>
    public GrapheneRoot(Router router)
    {
      AddToClassList(ussClassName);
      this.router = router;
    }
  }
}