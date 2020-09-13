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
        plate.onRefresh += Plate_onRefresh;
    }

    private void Plate_onRefresh()
    {
      // Default use the plate's default container
      VisualElement container = plate.ContentContainer;

      if (contentSelector.Length > 0)
        container = plate.GetVisualElement(contentSelector);

      RenderToContainer(container);
    }

    public void RenderToContainer(VisualElement container)
    {
      // Bind the static panel
      //Binder.BindRecursive(doc.rootVisualElement, this, null, this, true);

      // Initialize & render the form
      if (!blueprint)
        return;

      blueprint.InitModel();

      if (!blueprint.Render)
        return;

      // Bind the static form
      Binder.BindRecursive(container, blueprint, null, plate, true);

      // Render & bind the dynamic items
      RenderUtils.Draw(plate, container, blueprint, templates);
    }
  }
}