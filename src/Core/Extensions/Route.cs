
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Graphene.Elements
{
    public class Route : BindableElement
  {
    /// <summary>
    /// Instantiates a <see cref="Route"/> using the data read from a UXML file.
    /// </summary>
    public new class UxmlFactory : UxmlFactory<Route, UxmlTraits> { }

    /// <summary>
    /// Defines <see cref="UxmlTraits"/> for the <see cref="Route"/>.
    /// </summary>
    public new class UxmlTraits : BindableElement.UxmlTraits
    {
      UxmlStringAttributeDescription m_Route = new UxmlStringAttributeDescription { name = "route" };

      /// <summary>
      /// Initialize <see cref="View"/> properties using values from the attribute bag.
      /// </summary>
      /// <param name="ve">The object to initialize.</param>
      /// <param name="bag">The attribute bag.</param>
      /// <param name="cc">The creation context; unused.</param>
      public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
      {
        base.Init(ve, bag, cc);

        ((Route)ve).route = m_Route.GetValueFromBag(bag, cc);
      }
    }


    public Router<string> router;
    [SerializeField]
    private string m_Route = String.Empty;
    public virtual string route
    {
      get { return m_Route; }
      set
      {
        m_Route = value?.ToLower();
      }
    }

    /// <summary>
    /// USS class name of elements of this type.
    /// </summary>
    /// <remarks>
    /// Unity adds this USS class to every instance of the Route element. Any styling applied to
    /// this class affects every button located beside, or below the stylesheet in the visual tree.
    /// </remarks>
    public static readonly string ussClassName = "unity-route";
  
    /// <summary>
    /// Constructs a Route.
    /// </summary>
    public Route() : this(null)
    {
    }

    /// <summary>
    /// Constructs a route with an Action that is triggered when the button is clicked.
    /// </summary>
    /// <param name="clickEvent">The action triggered when the button is clicked.</param>
    /// <remarks>
    /// By default, a single left mouse click triggers the Action. To change the activator, modify <see cref="clickable"/>.
    /// </remarks>
    public Route(string route)
    {
      AddToClassList(ussClassName);
      this.route = route;

      // Add click to itself
      this.AddManipulator(new Clickable(OnClickEvent));

      clicked += Clicked;
    }

    public Action clicked;

    public void Clicked()
    {
      router.TryChangeState(route);
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
      if (ProcessClick(evt)) {
        clicked.Invoke();
      }
    }

    internal void SetRouter(Router r)
    {
      this.router = r as Router<string>;
      r.onRoutingBlocked += OnRoutingBlocked;
      r.onRoutingUnblocked += OnRoutingUnblocked;
    }

    private void OnRoutingUnblocked()
    {
      SetEnabled(true);
    }

    private void OnRoutingBlocked()
    {
      SetEnabled(false);
    }
  }
}