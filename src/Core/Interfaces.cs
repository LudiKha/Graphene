namespace Graphene
{
  using Elements;
  using UnityEngine.UIElements;

  public interface IGrapheneDependent
  {
  }

  public interface IBindableToVisualElement : IHasTooltip
  {
    bool isShown { get; }
    System.Action<bool> onShowHide { get; set; }

    bool isEnabled { get; }
    System.Action<bool> onSetEnabled { get; set; }

	bool isActive2 { get; }
	System.Action<bool> onSetActive { get; set; }

    //event System.Action<VisualElement> onBindToElement;// { get; set; }

	VisualElement boundToElement { get; }

	void ResetCallbacks ();

    void SetBinding(VisualElement boundToElement);

    //System.Action syncVisualElement { get; set; }
  }

  public interface IBindableElement<TValue>
  {
    /// <summary>
    /// Callback to VisualElement control that model changed, and view needs to be updated
    /// </summary>
    /// <param name="newValue"></param>
    void OnModelChange(TValue newValue);
  }

  public enum InteractionMode
  {
    Button,
    Submit,
    Cancel
  }

  public interface IBindableInteractionType
  {
    public InteractionMode InteractionType { get; }
    public int Size { get; }
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
    void Inject(Graphene graphene);
  }
#endif

  #region IModel

  public interface IModel
  {
	void Initialize(VisualElement container, Plate plate);
	bool Render { get; }

	System.Action onModelChange { get; set; }

	void Refresh(VisualElement container);
  }

  /// <summary>
  /// Instructs the <see cref="Renderer"/> to use a custom <see cref="BindAttribute"/> context
  /// </summary>
  public interface ICustomBindContext
  {
	public object GetCustomBindContext { get; }
  }

  /// <summary>
  /// Instructs the <see cref="Renderer"/> to use a custom <see cref="DrawAttribute"/> context
  /// </summary>
  public interface ICustomDrawContext
  {
	public object GetCustomDrawContext { get; }
  }

  public interface IForm
  {
	[Bind("Title")]
	string Title { get; }
	void OnSubmit();
	void OnCancel();
  }
  #endregion

  #region Misc

  public interface IHasTooltip
  {
	string Tooltip { get; }
  }

  public interface IHasCustomVisualTreeAsset
  {
	VisualTreeAsset VisualTreeAsset { get; }
  }

  #endregion

}