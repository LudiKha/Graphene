using System.Collections;
using UnityEngine.UIElements;

namespace Graphene
{
    using Kinstrife.Core.ReflectionHelpers;

    public class CollectionBinding : Binding<ICollection>
  {
    protected int? lastLength;

    public CollectionBinding(BindableElement el, in object context, in ValueWithAttribute<BindAttribute> member) : base(el, in context, in member)
    {
      if (member.Value is ICollection)
      {
      }
      else
      {
        scheduleDispose = true;
        return;
      }

      lastValue = GetValueFromMemberInfo();
      lastLength = lastValue?.Count;
    }

    protected override bool IsValidBinding()
    {
      return memberName != null;
    }
    protected override ICollection GetValueFromMemberInfo()
    {
      return (ICollection)extendedTypeInfo.Accessor[context, memberName];
    }
    protected override void SetValueFromMemberInfo(ICollection value)
    {
      extendedTypeInfo.Accessor[context, memberName] = value;
    }

    protected override void UpdateFromModel(in ICollection newValue)
    {
      // Collection reference/count changed -> Assign new list
      if (!this.lastValue.Equals(newValue) || newValue.Count != lastLength.Value)
        if (element is ListView listView && newValue is IList iList)
        {
          listView.itemsSource = iList;
          lastLength = iList?.Count;
        }
    }
  }
}