//using Sirenix.OdinInspector;
//using System;
//using System.Linq;
//using UnityEngine.UIElements;

//namespace Graphene
//{
//  public class UIPage : Plate
//  {    
//    public string titleContainerName = "Title";
//    VisualElement titleContainer;

//    [Bind("Title")]
//    public string titleText = "MyAwesome Title";

//    public string decoClassNames = "font-deco-2 title red ink nomargin";


//    protected override void GetLocalReferences()
//    {
//      base.GetLocalReferences();

//      if (!string.IsNullOrEmpty(containerParentName))
//        titleContainer = doc.rootVisualElement.Q(containerParentName).Q(titleContainerName);
//      else
//        titleContainer = doc.rootVisualElement.Q(titleContainerName);
//    }

//    public override void Initialize()
//    {
//      base.Initialize();

//      if (StateHandle)
//      {
//        var previous = doc.rootVisualElement.Q("PageLeft")?.Q<Button>("Route");
//        var next = doc.rootVisualElement.Q("PageRight")?.Q<Button>("Route");
//        if (previous != null)
//          previous.clicked += stateHandle.Router.TryGoToPreviousState;
//        if (next != null)
//          next.clicked += stateHandle.Router.TryGoToNextState;
//      }
//    }

//    protected override void Refresh()
//    {
//      base.Refresh();

//      SetTitle();
//    }

//    protected override void Clear()
//    {
//      base.Clear();

//      titleContainer.Clear();
//    }

//    [Button]
//    void SetTitle()
//    {
//      titleContainer.Clear();

//      string titleText = !string.IsNullOrWhiteSpace(this.titleText) ? this.titleText : this.Form && !string.IsNullOrWhiteSpace(this.Form.Title) ? Form.Title : null;
//      if (titleText == null)
//        return;

//      var elements = titleText.Split(' ');
//      int i = 0;
//      foreach (var el in elements)
//      {
//        char first = el.First();

//        //Split up label
//        if (Char.IsUpper(first))
//        {
//          string firstString = first.ToString();
//          if (i > 0)
//            firstString += " ";

//          Label decoLabel = new Label(firstString);
//          foreach (string className in this.decoClassNames.Split(' '))
//            decoLabel.AddToClassList(className);

//          titleContainer.Add(decoLabel);
//          titleContainer.Add(new Label(el.Substring(1)));
//        }
//        else
//        {
//          string text = el;
//          if (i > 0)
//            text += " ";
//          titleContainer.Add(new Label(text));
//        }
//        i++;
//      }
//    }
//  }
//}