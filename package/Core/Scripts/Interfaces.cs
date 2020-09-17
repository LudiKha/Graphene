namespace Graphene
{
  public interface IGrapheneDependent
  {
  }
#if !DEPENDENCY_INJEcTION
  public interface IInitializable : IGrapheneDependent
  {
    void Initialize();
  }
  public interface ILateInitializable : IGrapheneDependent
  {
    void LateInitialize();
  }

  public interface IInjectable : IGrapheneDependent
  {    
  }
#endif
}