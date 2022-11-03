using UnityEngine.UIElements;

namespace Graphene
{
    using Kinstrife.Core.ReflectionHelpers;

    public class MemberBinding<T> : Binding<T>
  {
    public MemberBinding(BindableElement el, in object context, in ValueWithAttribute<BindAttribute> member) : base(el, in context, in member)
    {
      lastValue = GetValueFromMemberInfo();
    }

    protected override bool IsValidBinding()
    {
      return context != null;
    }

    protected override T GetValueFromMemberInfo()
    {
      return (T)extendedTypeInfo.Accessor[context, memberName];
    }

    protected override void SetValueFromMemberInfo(T value)
    {
      extendedTypeInfo.Accessor[context, memberName] = value;
    }
  }
}