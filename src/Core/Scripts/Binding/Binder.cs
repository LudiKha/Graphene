using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Graphene
{
  using Elements;
  using Kinstrife.Core.ReflectionHelpers;

  public interface IRoute
  {
  }

  public static class Binder
  {
    public static event System.Action<BindableElement> OnBindElement;

#if UNITY_EDITOR
    [UnityEditor.InitializeOnEnterPlayMode]
    static void InitializeOnEnterPlayMode()
    {
      OnBindElement = null;
    }
#endif

    /// <summary>
    /// Binds the tree recursively
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="element"></param>
    /// <param name="context"></param>
    public static VisualElement Instantiate(in object context, TemplateAsset template, Plate panel)
    {
      var clone = template.Instantiate();

      if (context.GetType().IsPrimitive)
      {
      }
      // Bind class with its own context
      else
      {
        // Get members
        List<ValueWithAttribute<BindAttribute>> members = new List<ValueWithAttribute<BindAttribute>>();
        TypeInfoCache.GetMemberValuesWithAttribute<BindAttribute>(context, members);
        Binder.BindRecursive(clone, context, members, panel, false);
      }
      return clone;
    }

    /// <summary>
    /// Binds the tree recursively
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="element"></param>
    /// <param name="fieldValue"></param>
    public static VisualElement InstantiatePrimitive(in object context, ref ValueWithAttribute<BindAttribute> bindableMember, TemplateAsset template, Plate panel)
    {
      var clone = template.Instantiate();

      if (bindableMember.Attribute == null)
      {
        Debug.LogError($"Drawing {template.name} for primitive on {context} without Bind Attribute", template);
        return clone;
      }

      // Get members
      List<ValueWithAttribute<BindAttribute>> members = new List<ValueWithAttribute<BindAttribute>>();
      members.Add(bindableMember);

      // Bind without scope drilldown
      Binder.BindRecursive(clone, context, members, panel, false);

      return clone;
    }

    /// <summary>
    /// Binds the tree recursively
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="element"></param>
    /// <param name="context"></param>
    public static void BindRecursive(VisualElement element, object context, List<ValueWithAttribute<BindAttribute>> members, Plate panel, bool notFullyDrilledDown)
    {
      if (members == null)
      {
        // Get members
        members = new List<ValueWithAttribute<BindAttribute>>();
        TypeInfoCache.GetMemberValuesWithAttribute<BindAttribute>(context, members);
      }

      // Is bindable with binding-path in uxml
      if (element is BindableElement el && !string.IsNullOrWhiteSpace(el.bindingPath))
      {
        // Should drill down to a child's scope (based on binding-path '.', and scope ovveride '~')
        bool branched = notFullyDrilledDown && TryBranch(el, context, panel);
        if (branched) // Started branch via drilled down scope branch
          return;

        BindElementValues(el, ref context, members, panel);

        // Context potentially has routing binding (TODO remove interface check)
        if (context is IRoute && panel.Router)
          panel.Router.BindRouteToContext(el, context);
      }
      // Rout el special case
      else if (element is Route route)
      {
        BindRoute(route, ref context, panel);
      }

      BindChildren(element, context, members, panel, notFullyDrilledDown);
    }

    static void BindChildren(VisualElement element, object context, List<ValueWithAttribute<BindAttribute>> members, Plate panel, bool scopeDrillDown)
    {
      //element.BindValues(data);
      if (element.childCount == 0)
      {
        return;
      }

      // Loop through children and bind data to them
      foreach (var child in element.Children())
      {
        BindRecursive(child, context, members, panel, scopeDrillDown);
      }
    }

    /// <summary>
    /// Binds values of a particular VisualElement to an IBindable
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="el"></param>
    /// <param name="data"></param>
    private static void BindElementValues<V>(V el, ref object context, List<ValueWithAttribute<BindAttribute>> members, Plate panel) where V : BindableElement
    {
      // Pass in list of properties for possible custom logic spanning multiple properties
      if (el is Label)
        BindLabel(el as Label, ref context, members, panel);
      else if (el is Button)
        BindButton(el as Button, ref context, members, panel);
      else if (el is If)
        BindIf(el as If, ref context, members, panel);
      else if (el is CycleField)
        BindCycleField(el as CycleField, ref context, members, panel);
      else if (el is ListView)
        BindListView(el as ListView, ref context, members, panel);
      else if (el is SelectField)
        BindSelectField(el as SelectField, ref context, members, panel);
      else if (el is Toggle)
        BindBaseField<bool>(el as Toggle, ref context, members, panel);
      else if (el is Slider)
        BindSlider(el as Slider, ref context, members, panel);
      else if (el is SliderInt)
        BindSlider(el as SliderInt, ref context, members, panel);
      else if (el is TextField)
        BindTextField(el as TextField, ref context, members, panel);
      else if (el is TextElement)
        BindTextElement(el as TextElement, ref context, members, panel);
    }

    private static void BindTextElement(TextElement el, ref object context, List<ValueWithAttribute<BindAttribute>> members, Plate panel)
    {
      foreach (var item in members)
      {
        if (el.bindingPath.Equals(item.Attribute.Path))
        {
          BindText(el, ref context, in item.Value, in item, panel);
        }
      }
    }
    private static void BindLabel(Label el, ref object context, List<ValueWithAttribute<BindAttribute>> members, Plate panel)
    {
      foreach (var item in members)
      {
        if (el.bindingPath.Equals(item.Attribute.Path) || (string.IsNullOrEmpty(item.Attribute.Path) && item.Value is string))
        {
            BindText(el, ref context, in item.Value, in item, panel);
        }
      }
    }


    private static void BindButton(Button el, ref object context, List<ValueWithAttribute<BindAttribute>> members, Plate panel)
    {
      foreach (var item in members)
      {
        if (el.bindingPath.Equals(item.Attribute.Path))
        {
          if (item.Value is System.Action)
            BindClick(el, (System.Action)item.Value);
          else if (item.Value is UnityEngine.Events.UnityEvent)
            BindClick(el, (UnityEngine.Events.UnityEvent)item.Value);
          else 
            BindText(el, ref context, in item.Value, in item, panel);
        }
      }
    }

    private static void BindRoute(Route el, ref object context, Plate panel)
    {
      // Check if parent is a button -> propagate click
      if (el.parent is Button button)
        button.clicked += el.clicked;
      else
      {
        foreach (var item in el.Children())
          if (item is Button btn)
            btn.clicked += el.clicked;
      }

      el.router = panel.Router as Router<string>;

      // Let the (generic) router handle the way it binds routes
      panel.Router.BindRoute(el, context);
    }

    private static void BindSlider(Slider el, ref object context, List<ValueWithAttribute<BindAttribute>> members, Plate panel)
    {
      // Slider specifics
      foreach (var item in members)
      {
        // Primary
        if (el.bindingPath.Equals(item.Attribute.Path))
        {
          if (item.Attribute is BindFloatAttribute floatAttribute)
          {
            el.value = floatAttribute.startingValue;
            el.lowValue = floatAttribute.lowValue;
            el.highValue = floatAttribute.highValue;
            el.showInputField = floatAttribute.showInputField;
            break;
          }
        }
      }

      // Bind base field value & callback
      BindBaseField(el, ref context, members, panel);
    }

    private static void BindSlider(SliderInt el, ref object context, List<ValueWithAttribute<BindAttribute>> members, Plate panel)
    {
      // Slider specifics
      foreach (var item in members)
      {
        // Primary
        if (el.bindingPath.Equals(item.Attribute.Path))
        {
          if (item.Attribute is BindIntAttribute att)
          {
            el.value = att.startingValue;
            el.lowValue = att.lowValue;
            el.highValue = att.highValue;
            el.showInputField = att.showInputField;
            break;
          }
        }
      }

      // Bind base field value & callback
      BindBaseField(el, ref context, members, panel);
    }


    private static void BindTextField(TextField el, ref object context, List<ValueWithAttribute<BindAttribute>> members, Plate panel)
    {
      foreach (var item in members)
      {
        // Primary
        if (el.bindingPath.Equals(item.Attribute.Path))
        {
          if (item.Attribute is BindStringAttribute stringAttribute)
          {
            el.value = stringAttribute.startingValue;
            el.isPasswordField = stringAttribute.password;
            el.isReadOnly = stringAttribute.readOnly;
            el.multiline = stringAttribute.multiLine;

            if (stringAttribute.maxLength >= 0)
              el.maxLength = stringAttribute.maxLength;
            break;
          }
        }
      }

      BindBaseField(el, ref context, members, panel);
    }

    private static void BindBaseField<TValueType>(BaseField<TValueType> el, ref object context, List<ValueWithAttribute<BindAttribute>> members, Plate panel)
    {
      bool labelFromAttribute = false;
      foreach (var item in members)
      {
        // Primary (value)
        if (el.bindingPath.Equals(item.Attribute.Path) || (string.IsNullOrEmpty(item.Attribute.Path) && item.Value is TValueType))
        {
          if (item.Value is TValueType)
          {
            el.SetValueWithoutNotify((TValueType)item.Value);
            BindingManager.TryCreate(el, ref context, in item, panel);
          }

          // Set label from attribute
          if (item.Attribute is BindBaseFieldAttribute att)
          {
            if (!string.IsNullOrWhiteSpace(att.label))
            {
              el.label = att.label;
              labelFromAttribute = true;
            }
          }
        }
        // Set register callback event
        else if (item.Attribute is BindValueChangeCallbackAttribute callbackAttribute)
          el.RegisterValueChangedCallback(item.Value as EventCallback<ChangeEvent<TValueType>>);
        // Set label from field
        else if (!labelFromAttribute && item.Attribute.Path == "Label" && item.Value is string labelText && !string.IsNullOrWhiteSpace(labelText))
          BindText(el.labelElement, ref context, labelText, in item, panel);
        else if (item.Attribute is BindTooltip)
          el.tooltip = (string)item.Value;
      }
    }

    private static void BindSelectField(SelectField el, ref object context, List<ValueWithAttribute<BindAttribute>> members, Plate panel)
    {
      // Then bind the items
      foreach (var item in members)
      {
        // Model items
        if (item.Attribute.Path.Equals(SelectField.itemsPath))
        {
          el.items = item.Value as List<string>;
        }
      }

      // First bind base field (int)
      BindBaseField(el, ref context, members, panel);
    }

    private static void BindCycleField(CycleField el, ref object context, List<ValueWithAttribute<BindAttribute>> members, Plate panel)
    {
      // Then bind the items
      foreach (var item in members)
      {
        // Model items
        if (item.Attribute.Path.Equals(CycleField.itemsPath))
        {
          el.items = item.Value as List<string>;
        }
      }

      // First bind base field (int)
      BindBaseField(el, ref context, members, panel);
    }

    private static void BindListView(ListView el, ref object context, List<ValueWithAttribute<BindAttribute>> members, Plate panel)
    {
      foreach (var item in members)
      {
        // Primary
        if (el.bindingPath.Equals(item.Attribute.Path))
        {
          Func<VisualElement> makeItem = () => new Label("ListViewItem");
          Action<VisualElement, int> bindItem = (e, i) => (e as Label).text = (e as Label).text + " " + i;

          var options = item.Value as List<string>;
          bindItem = (e, i) => (e as Label).text = options[i];
          el.itemsSource = options;
          //if (item.Value is T)
          //  el.SetValueWithoutNotify((T)item.Value);
          //else if (item.Attribute is BindValueChangeCallbackAttribute callbackAttribute)
          //  el.RegisterValueChangedCallback(item.Value as EventCallback<ChangeEvent<T>>);

          el.makeItem = makeItem;
          el.bindItem = bindItem;

          // Scale in accordance with number of items
          el.style.height = el.itemHeight * el.itemsSource.Count;
        }
      }
    }

    private static void BindIf(If el, ref object context, List<ValueWithAttribute<BindAttribute>> members, Plate panel)
    {
      // Then bind the items
      foreach (var item in members)
      {
        // Model items
        if (el.bindingPath.Equals(item.Attribute.Path))
        {
          el.OnModelChange(item.Value);
          BindingManager.TryCreate<object>(el, ref context, in item, panel);
          return;
        }
      }
    }

    private static void BindText(TextElement el, ref object context, in object obj, in ValueWithAttribute<BindAttribute> member, Plate panel)
    {
      // Add translation here
      if (obj is string str)
        el.text = str;
      else
        el.text = obj.ToString();

      BindingManager.TryCreate(el, ref context, in member, panel);
    }

    private static void BindClick(Button el, System.Action action)
    {
      el.clicked += action;
      OnBindElement?.Invoke(el);
    }

    private static void BindClick(Button el, UnityEngine.Events.UnityEvent unityEvent)
    {
      el.clicked += delegate { unityEvent.Invoke(); };
      OnBindElement?.Invoke(el);
    }

    public static string[] stringSplitOptions = new string[]{ ".", "~", "::"};

    public const char nestedScopeChar = '.';
    public const char relativeScopeChar = '~';
    public const string oneTimeBindingChar = "::";

    private static bool TryBranch(BindableElement el, object data, Plate owner)
    {
      var scopes = el.bindingPath.Split(nestedScopeChar);
      if (scopes.Length == 1)
        return false;

      // Create sub scope '~'
      bool createSubScope = false;
      string bindingPath = el.bindingPath;
      if (el.bindingPath.IndexOf(relativeScopeChar) == 0)
      {
        createSubScope = true;
        bindingPath = bindingPath.Remove(0, 1);
      }

      return DrillDownToChildScopeRecursive(el, data, owner, bindingPath, createSubScope);
    }


    private static bool DrillDownToChildScopeRecursive(BindableElement el, object data, Plate owner, string currentScope, bool createSubScope)
    {
      if (data == null)
      {
        Debug.LogError($"Data was null for scope {currentScope} {owner}", owner);
        return false;
      }

      //Debug.Log($"Drilling down to child scope {currentScope} {data} ({el})", data as UnityEngine.Object);

      // Get binding members info
      List<ValueWithAttribute<BindAttribute>> members = new List<ValueWithAttribute<BindAttribute>>();
      TypeInfoCache.GetMemberValuesWithAttribute<BindAttribute>(data, members);
      // Context doesn't have any bindable members
      if (members.Count == 0)
        return false;

      // Split it & remove '~' and '::'
      var scopes = currentScope.Split(stringSplitOptions, StringSplitOptions.RemoveEmptyEntries);
      // We're at the leaf scope - bind
      if (scopes.Length == 1)
      {
        // Override the element's path now we found the scope
        el.bindingPath = currentScope;

        // Start a new binding branch here and terminate the one we came from
        if (createSubScope)
        {
          BindRecursive(el, data, members, owner, createSubScope);
          return true;
        }
        // Only bind the element values, and carry on with the child binding as usual
        else
        {
          BindElementValues(el, ref data, members, owner);
          return false;
        }
      }

      // Select the topmost scope
      string targetScope = scopes[0];

      ValueWithAttribute<BindAttribute>[] matchingMembers = members.Where(x => x.Attribute.Path.ToLower() == targetScope.ToLower()).ToArray();
      // Might need/want to throw an error here
      if (matchingMembers.Length == 0)
        return false;

      bool startedBranch = false;
      string newPath = currentScope.Substring(currentScope.IndexOf(nestedScopeChar) + 1);
      foreach (var member in matchingMembers)
      {
        if (DrillDownToChildScopeRecursive(el, member.Value, owner, newPath, createSubScope))
          startedBranch = true;
      }
      return startedBranch;
    }
  }
}