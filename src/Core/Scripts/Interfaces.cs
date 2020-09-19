namespace Graphene
{
  using Elements;

  public interface IGrapheneDependent
  {
  }


  public interface IBindableElement<TValue>
  {
    /// <summary>
    /// Callback to VisualElement control that model changed, and view needs to be updated
    /// </summary>
    /// <param name="newValue"></param>
    void OnModelChange(TValue newValue);
  }

  public interface IGrapheneElement
  {
    void Inject(GrapheneRoot root, Plate plate, Renderer renderer);
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