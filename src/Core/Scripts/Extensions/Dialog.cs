using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using UnityEngine.UIElements;

namespace Graphene.Elements
{
  public class Dialog : VisualElement, IDisposable
  {
    /// <summary>
    /// Instantiates a <see cref="Dialog"/> using the data read from a UXML file.
    /// </summary>
    public new class UxmlFactory : UxmlFactory<Dialog, UxmlTraits> { }

    /// <summary>
    /// Defines <see cref="UxmlTraits"/> for the <see cref="Dialog"/>.
    /// </summary>
    public new class UxmlTraits : BindableElement.UxmlTraits
    {
      /// <summary>
      /// Initialize <see cref="Dialog"/> properties using values from the attribute bag.
      /// </summary>
      /// <param name="ve">The object to initialize.</param>
      /// <param name="bag">The attribute bag.</param>
      /// <param name="cc">The creation context; unused.</param>
      public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
      {
        base.Init(ve, bag, cc);
      }
    }

    /// <summary>
    /// USS class name of elements of this type.
    /// </summary>
    public static readonly string ussClassName = "gr-dialog";
    /// <summary>
    /// USS class name of elements of this type.
    /// </summary>
    public static readonly string dialogBackgroundUssClassName = ussClassName + "__background";
    public static readonly string dialogPanelUssClassName = ussClassName + "__panel";

    public static readonly string hiddenClassName = "hidden";

    VisualElement m_Background;
    private View m_PanelView;

    public event System.Action onClose;

    public Dialog()
        : this(null, null) {
    }

    public Dialog(IPanel panel, VisualElement content)
    {
      AddToClassList(ussClassName);

      // Set-up the label and text...
      //this.AddManipulator(new Clickable(OnClickEvent));

      m_Background = new VisualElement();
      m_Background.AddToClassList(dialogBackgroundUssClassName);
      m_Background.AddToClassList("unity-ui-document__child");
      m_Background.AddManipulator(new Clickable(OnClickBackground));

      // Add background to root
      panel.TopRoot().Add(m_Background);
      //panel.visualTree.Add(m_Background);
      
      // Add dialog to background
      m_Background.Add(this);
      //hierarchy.Add(m_Background);

      //Set up view
      m_PanelView = new View("GR__Content");
      this.Add(m_PanelView);

      m_PanelView.isDefault = true;

      m_PanelView.AddToClassList(dialogPanelUssClassName);
      m_PanelView.Focus();

      if (content != null)
      {
        m_PanelView.Add(content);
        content.Focus();
      }

      m_Background.BringToFront();
      this.BringToFront();

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

    void OnClickBackground(EventBase evt)
    {
      if (ProcessClick(evt))
      {
        onClose?.Invoke();
        Dispose();
      }
    }

    public void Dispose()
    {
      if(this.m_Background != null && this.m_Background.parent != null)
        this.m_Background.parent.Remove(this.m_Background);
    }

    public Dialog WithStyles(VisualElementStyleSheetSet styleSheets)
    {
      for (int i = 0; i < styleSheets.count; i++)
      {
        m_Background.styleSheets.Add(styleSheets[i]);
      }
      return this;
    }
  }
}