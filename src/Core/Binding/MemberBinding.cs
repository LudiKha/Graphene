using UnityEngine.UIElements;

namespace Graphene
{
  using Kinstrife.Core.ReflectionHelpers;
  using UnityEngine;

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
#if UNITY_ASSERTIONS
	  var val = extendedTypeInfo.Accessor[context, memberName];
	 // if(val == null)
	 // {
		//Debug.LogError($"Trying to cast a null member {memberName} {extendedTypeInfo.Accessor.Type}");
		//return default(T);
	 // }
	  if (val != null && !typeof(T).IsAssignableFrom(val.GetType()))
	  {
		Debug.LogError($"InvalidCastException for member {memberName} {extendedTypeInfo.Accessor.Type}: Trying to cast binding {val.GetType().Name} to {typeof(T).Name}. " +
		  $"\n{element.GetType().Name}");
		return default(T);
	  }
#endif

	  return (T)extendedTypeInfo.Accessor[context, memberName];
	}

	protected override void SetValueFromMemberInfo(T value)
	{
	  extendedTypeInfo.Accessor[context, memberName] = value;
	}
  }
}