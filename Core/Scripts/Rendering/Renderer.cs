using UnityEngine;
using UnityEngine.UIElements;

namespace Graphene
{
  public class Renderer : MonoBehaviour, IInitializable
  {
    [SerializeField] Plate plate;

    [SerializeField] Form blueprint;
    [Bind("Form")] public Form Blueprint => blueprint;
    [SerializeField] protected ComponentTemplates templates; public ComponentTemplates Templates => templates;

    /// <summary>
    /// Overriding this will
    /// </summary>
    [SerializeField] protected string[] contentSelector = new string[] {  };

    public void Initialize()
    {
      if (plate || (plate = GetComponent<Plate>()))
      {
        plate.onRefreshStatic += Plate_onRefreshStatic;
        plate.onRefreshDynamic += Plate_onRefreshDynamic;
      }
    }

    private void Plate_onRefreshStatic()
    {
      // Bind the static plate
      Binder.BindRecursive(plate.Doc.rootVisualElement, this, null, plate, true);

      // Bind the static form
      //Binder.BindRecursive(plate.Doc.rootVisualElement, blueprint, null, plate, true);
    }

    private void Plate_onRefreshDynamic()
    {
      // Default use the plate's default container
      VisualElement container = plate.ContentContainer;

      if (contentSelector.Length > 0)
        container = plate.GetVisualElement(contentSelector);

      RenderToContainer(container);
    }

    public void RenderToContainer(VisualElement container)
    {

      // Initialize & render the form
      if (!blueprint)
        return;

      blueprint.InitModel();

      if (!blueprint.Render)
        return;


      // Render & bind the dynamic items
      RenderUtils.Draw(plate, container, blueprint, templates);
    }
  }
}