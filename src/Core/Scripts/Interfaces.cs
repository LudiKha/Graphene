﻿namespace Graphene
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
  public interface IGrapheneInitializable : IGrapheneDependent
  {
    void Initialize();
  }
  public interface IGrapheneLateInitializable : IGrapheneDependent
  {
    void LateInitialize();
  }

  public interface IGrapheneInjectable : IGrapheneDependent
  {    
  }
#endif
}