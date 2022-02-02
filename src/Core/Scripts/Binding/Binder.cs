using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Graphene
{
  using Elements;
  using Kinstrife.Core.ReflectionHelpers;
  using System.Collections;

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
    public static VisualElement Instantiate(in object context, VisualTreeAsset template, Plate plate)
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
        Binder.BindRecursive(clone, context, members, plate, false);
      }
      return clone;
    }

    /// <summary>
    /// Binds the tree recursively
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="element"></param>
    /// <param name="fieldValue"></param>
    public static VisualElement InstantiatePrimitive(in object context, ref ValueWithAttribute<BindAttribute> bindableMember, VisualTreeAsset template, Plate plate)
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
      Binder.BindRecursive(clone, context, members, plate, false);

      return clone;
    }

    /// <summary>
    /// Binds the tree recursively
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="element"></param>
    /// <param name="context"></param>
    public static void BindRecursive(VisualElement element, object context, List<ValueWithAttribute<BindAttribute>> members, Plate plate, bool notFullyDrilledDown)
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
        bool branched = notFullyDrilledDown && TryBranch(el, context, plate);
        if (branched) // Started branch via drilled down scope branch
          return;

        BindElementValues(el, ref context, members, plate);

        // Context potentially has routing binding (TODO remove interface check)
        if (context is IRoute && plate.Router)
          plate.Router.BindRouteToContext(el, context);
      }
      // Image (not bindable)
      else if (element is Image image)
      {
        BindImage(image, ref context, members, plate);
      }
      // Rout el special case
      else if (element is Route route)
      {
        BindRoute(route, ref context, plate);
      }

      BindChildren(element, context, members, plate, notFullyDrilledDown);
    }

    static void BindChildren(VisualElement element, object context, List<ValueWithAttribute<BindAttribute>> members, Plate plate, bool scopeDrillDown)
    {
      //element.BindValues(data);
      if (element.childCount == 0)
      {
        return;
      }

      // Loop through children and bind data to them
      foreach (var child in element.Children())
      {
        BindRecursive(child, context, members, plate, scopeDrillDown);
      }
    }

    /// <summary>
    /// Binds values of a particular VisualElement to an IBindable
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="el"></param>
    /// <param name="data"></param>
    private static void BindElementValues<V>(V el, ref object context, List<ValueWithAttribute<BindAttribute>> members, Plate plate) where V : BindableElement
    {
      // Pass in list of properties for possible custom logic spanning multiple properties
      if (el is Label)
        BindLabel(el as Label, ref context, members, plate);
      else if (el is Button)
        BindButton(el as Button, ref context, members, plate);
      else if (el is If)
        BindIf(el as If, ref context, members, plate);
      else if (el is Image image)
        BindImage(image, ref context, members, plate);
      else if (el is CycleField)
        BindCycleField(el as CycleField, ref context, members, plate);
      else if (el is DropdownField)
        BindDropdownField(el as DropdownField, ref context, members, plate);
      else if (el is ListView)
        BindListView(el as ListView, ref context, members, plate);
      else if (el is SelectField)
        BindSelectField(el as SelectField, ref context, members, plate);
      else if (el is Toggle)
        BindBaseField<bool>(el as Toggle, ref context, members, plate);
      else if (el is Slider)
        BindSlider(el as Slider, ref context, members, plate);
      else if (el is SliderInt)
        BindSlider(el as SliderInt, ref context, members, plate);
      else if (el is Foldout foldout)
        BindFoldout(foldout, ref context, members, plate);
      else if (el is ButtonGroup buttonGroup)
        BindButtonGroup(buttonGroup, ref context, members, plate);
      else if (el is TextField)
        BindTextField(el as TextField, ref context, members, plate);
      else if (el is TextElement)
        BindTextElement(el as TextElement, ref context, members, plate);
    }

    private static void BindTextElement(TextElement el, ref object context, List<ValueWithAttribute<BindAttribute>> members, Plate plate)
    {
      foreach (var item in members)
      {
        if (BindingPathOrTypeMatch<string>(el, in item))
        {
          BindText(el, ref context, in item.Value, in item, plate);
        }
      }
    }
    private static void BindLabel(Label el, ref object context, List<ValueWithAttribute<BindAttribute>> members, Plate plate)
    {
      foreach (var item in members)
      {
        if (BindingPathOrTypeMatch<string>(el, in item))
        {
          BindText(el, ref context, in item.Value, in item, plate);
        }
      }
    }


    private static void BindButton(Button el, ref object context, List<ValueWithAttribute<BindAttribute>> members, Plate plate)
    {
      foreach (var item in members)
      {
        if (BindingPathAndTypeMatch<string>(el, in item))
          BindText(el, ref context, in item.Value, in item, plate);
        else if (BindingPathAndTypeMatch<Action>(el, in item))
          BindClick(el, (Action)item.Value);
        else if (BindingPathOrTypeMatch<UnityEngine.Events.UnityEvent>(el, in item))
          BindClick(el, (UnityEngine.Events.UnityEvent)item.Value);
      }
    }

    internal static void BindRoute(Route el, ref object context, Plate plate)
    {
      // Check if parent is a button -> propagate click
      if (el.parent is Button button)
      {
        BindClick(button, el.clicked);
      }
      else
      {
        foreach (var item in el.Children())
        {
          if (item is Button btn)
          {
            BindClick(btn, el.clicked);
          }
          else if (item is ButtonGroup btnGroup)
          {
            btnGroup.clicked += (string route) => { el.route = route; el.clicked?.Invoke(); }; // A bit hacky perhaps
            OnBindElement?.Invoke(btnGroup);
          }
        }
      }

      el.SetRouter(plate.Router);

      // Let the (generic) router handle the way it binds routes
      plate.Router.BindRoute(el, context);
    }

    private static void BindSlider(Slider el, ref object context, List<ValueWithAttribute<BindAttribute>> members, Plate plate)
    {
      // Slider specifics
      foreach (var item in members)
      {
        // Primary
        if (BindingPathOrTypeMatch<float>(el, in item))
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
        else if (BindingPathAndTypeMatch<float>("Min", item))
          el.lowValue = (float)item.Value;
        else if (BindingPathAndTypeMatch<float>("Max", item))
          el.highValue = (float)item.Value;
      }

      // Bind base field value & callback
      BindBaseField(el, ref context, members, plate);
    }

    private static void BindSlider(SliderInt el, ref object context, List<ValueWithAttribute<BindAttribute>> members, Plate plate)
    {
      // Slider specifics
      foreach (var item in members)
      {
        // Primary
        if (BindingPathOrTypeMatch<int>(el, item))
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
        else if (BindingPathAndTypeMatch<int>("Min", item))
          el.lowValue = (int)item.Value;
        else if (BindingPathAndTypeMatch<int>("Max", item))
          el.highValue = (int)item.Value;
      }

      // Bind base field value & callback
      BindBaseField(el, ref context, members, plate);
    }


    private static void BindTextField(TextField el, ref object context, List<ValueWithAttribute<BindAttribute>> members, Plate plate)
    {
      foreach (var item in members)
      {
        // Primary
        if (BindingPathOrTypeMatch<string>(el, in item))
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

      BindBaseField(el, ref context, members, plate);
    }

    private static (TValueType value, string label) BindNotifyValueChange<TElementType, TValueType>(TElementType el, ref object context, List<ValueWithAttribute<BindAttribute>> members, Plate plate) where TElementType : BindableElement, INotifyValueChanged<TValueType>
    {
      bool labelFromAttribute = false;
      string label = null;
      foreach (var item in members)
      {
        // Primary (value)
        if (BindingPathOrTypeMatch<TValueType>(el, in item))
        {
          if (item.Value is TValueType value)
          {
            el.SetValueWithoutNotify(value);
            BindingManager.TryCreate<TValueType>(el, in context, in item, plate);
          }

          // Set label from attribute
          if (item.Attribute is BindBaseFieldAttribute att)
          {
            if (!string.IsNullOrWhiteSpace(att.label))
            {
              //el.text = att.label;
              labelFromAttribute = true;
              label = att.label;
            }
          }
        }
        // Set register callback event
        else if (item.Attribute is BindValueChangeCallbackAttribute callbackAttribute)
          el.RegisterValueChangedCallback(item.Value as EventCallback<ChangeEvent<TValueType>>);
        // Set label from field, if not from attribute
        else if (!labelFromAttribute && item.Attribute.Path == "Label" && item.Value is string labelText)
          label = labelText;
        //BindText(el.labelElement, ref context, labelText, in item, plate);
        else if (item.Attribute is BindTooltip)
          el.tooltip = (string)item.Value;
      }

      // Returning value & label tuple because each element implements text differently. (E.g. Foldout vs. Basefield)
      return (el.value, label);
    }

    private static void BindBaseField<TValueType>(BaseField<TValueType> el, ref object context, List<ValueWithAttribute<BindAttribute>> members, Plate plate)
    {
      var results = BindNotifyValueChange<BaseField<TValueType>, TValueType>(el, ref context, members, plate);
      el.label = results.label;
      OnBindElement?.Invoke(el);
    }

    private static void BindDropdownField(DropdownField el, ref object context, List<ValueWithAttribute<BindAttribute>> members, Plate plate)
    {
      // Then bind the items
      foreach (var item in members)
      {
        // Model items
        if (BindingPathMatch(item.Attribute.Path, SelectField.itemsPath))
        {
          el.choices = item.Value as List<string>;
          break;
        }
      }

      // First bind base field (string)
      BindBaseField(el, ref context, members, plate);
    }

    private static void BindSelectField(SelectField el, ref object context, List<ValueWithAttribute<BindAttribute>> members, Plate plate)
    {
      // Then bind the items
      foreach (var item in members)
      {
        // Model items
        if (BindingPathMatch(item.Attribute.Path, SelectField.itemsPath))
        {
          el.items = item.Value as List<string>;
          break;
        }
      }

      // First bind base field (int)
      BindBaseField(el, ref context, members, plate);
    }

    private static void BindListView(ListView el, ref object context, List<ValueWithAttribute<BindAttribute>> members, Plate plate)
    {
      foreach (var bindMember in members)
      {
        // Primary
        if (BindingPathMatch(bindMember.Attribute.Path, el.bindingPath))
        {
          IList list = bindMember.Value as IList;
          var templateAsset = plate.renderer.Templates.TryGetTemplateAsset(ControlType.ListItem);

          Func<VisualElement> makeItem = () => { return templateAsset.Instantiate(); };
          Action<VisualElement, int> bindItem = (e, i) => { Binder.BindRecursive(e, list[i], null, plate, false); };
          el.makeItem = makeItem;
          el.bindItem = bindItem;
          el.itemsSource = list;
          //if (templateAsset.ForceHeight > 0)
          //  el.fixedItemHeight = (int)templateAsset.ForceHeight;


          BindingManager.TryCreate<IList>(el, in context, in bindMember, plate);
          break;
        }
      }
    }

    internal static void BindListView(ListView el, in object context, Plate plate, VisualTreeAsset templateAsset, in ValueWithAttribute<BindAttribute> member)
    {
      IList list = member.Value as IList;

      Func<VisualElement> makeItem = () => { return templateAsset.Instantiate(); };
      Action<VisualElement, int> bindItem = (e, i) => { Binder.BindRecursive(e, list[i], null, plate, false); };
      el.makeItem = makeItem;
      el.bindItem = bindItem;
      el.itemsSource = list;

      BindingManager.TryCreate<IList>(el, in context, in member, plate);
    }

    private static void BindCycleField(CycleField el, ref object context, List<ValueWithAttribute<BindAttribute>> members, Plate plate)
    {
      // Then bind the items
      foreach (var item in members)
      {
        // Model items
        if (BindingPathMatch(item.Attribute.Path, CycleField.itemsPath))
        {
          el.items = item.Value as List<string>;
          break;
        }
      }

      // First bind base field (int)
      BindBaseField(el, ref context, members, plate);
    }

    private static void BindIf(If el, ref object context, List<ValueWithAttribute<BindAttribute>> members, Plate plate)
    {
      // Then bind the items
      foreach (var item in members)
      {
        // Model items
        if (BindingPathMatch(el, in item))
        {
          el.OnModelChange(item.Value);
          BindingManager.TryCreate<object>(el, in context, in item, plate);
          return;
        }
      }
    }

    private static void BindImage(Image el, ref object context, List<ValueWithAttribute<BindAttribute>> members, Plate plate)
    {
      // Then bind the items
      foreach (var item in members)
      {
        // Model items
        if (BindingPathOrTypeMatch<Texture>("Image", item))
        {
          el.image = item.Value as Texture;
          return;
        }
        else if (BindingPathOrTypeMatch<Sprite>("Image", item))
        {
          el.sprite = item.Value as Sprite;
          return;
        }
      }
    }



    private static void BindFoldout(Foldout el, ref object context, List<ValueWithAttribute<BindAttribute>> members, Plate plate)
    {
      var results = BindNotifyValueChange<Foldout, bool>(el, ref context, members, plate);
      el.text = results.label;
    }

    private static void BindButtonGroup(ButtonGroup el, ref object context, List<ValueWithAttribute<BindAttribute>> members, Plate plate)
    {
      // Then bind the items
      foreach (var item in members)
      {
        // Model items
        if (BindingPathAndTypeMatch<ICollection<string>>(el, item))
        {
          el.items = item.Value as List<string>;
          break;
        }
      }
    }

    //private static void BindFoldout(Foldout el, ref object context, List<ValueWithAttribute<BindAttribute>> members, Plate plate)
    //{
    //  foreach (var item in members)
    //  {
    //    if (BindingPathOrTypeMatch<string>(el, in item))
    //      el.text = ObjectToString(in item.Value);
    //    else if (BindingPathAndTypeMatch<bool>(el.bindingPath, in item))
    //    {
    //      el.value = (bool)item.Value;
    //      BindingManager.TryCreate<bool>(el, in context, in item, plate);
    //    }
    //  }
    //}

    private static void BindText(TextElement el, ref object context, in object valueObject, in ValueWithAttribute<BindAttribute> member, Plate plate)
    {
      // Add translation here
      el.text = ObjectToString(in valueObject);

      BindingManager.TryCreate(el, ref context, in member, plate);
    }

    private static void BindClick(Button el, System.Action action)
    {
      el.clicked += action;
      OnBindElement?.Invoke(el);
    }

    private static void BindClick(Button el, UnityEngine.Events.UnityEvent unityEvent)
    {
      el.clicked += delegate { unityEvent?.Invoke(); };
      OnBindElement?.Invoke(el);
    }

    public static string[] stringSplitOptions = new string[] { ".", "~", "::" };

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

    #region Internals
    internal static bool BindingPathMatch(in string a, in string b)
    {
      return string.CompareOrdinal(a, b) == 0;
    }
    internal static bool BindingPathMatch(BindableElement el, in ValueWithAttribute<BindAttribute> member)
    {
      return string.CompareOrdinal(el.bindingPath, member.Attribute.Path) == 0;
    }
    internal static bool BindingPathOrTypeMatch<T>(BindableElement el, in ValueWithAttribute<BindAttribute> member)
    {
      return string.CompareOrdinal(el.bindingPath, member.Attribute.Path) == 0 || (string.IsNullOrEmpty(member.Attribute.Path) && typeof(T).IsAssignableFrom(member.Type));
    }
    internal static bool BindingPathOrTypeMatch<T>(in string path, in ValueWithAttribute<BindAttribute> member)
    {
      return string.CompareOrdinal(path, member.Attribute.Path) == 0 || (string.IsNullOrEmpty(member.Attribute.Path) && typeof(T).IsAssignableFrom(member.Type));
    }
    internal static bool BindingPathAndTypeMatch<T>(in BindableElement el, in ValueWithAttribute<BindAttribute> member)
    {
      return string.CompareOrdinal(el.bindingPath, member.Attribute.Path) == 0 && typeof(T).IsAssignableFrom(member.Type);
    }
    internal static bool BindingPathAndTypeMatch<T>(in string a, in ValueWithAttribute<BindAttribute> member)
    {
      return string.CompareOrdinal(a, member.Attribute.Path) == 0 && typeof(T).IsAssignableFrom(member.Type);
    }
    internal static string ObjectToString(in object obj)
    {
      // Add translation here
      if (obj is string str)
        return str;
      else if (obj != null)
        return obj.ToString();

      return default;
    }
    #endregion
  }
}