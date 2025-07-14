/* Original code[1] Copyright (c) 2022 Shane Celis[2]
   Licensed under the MIT License[3]

   [1]: https://gist.github.com/shanecelis/b6fb3fe8ed5356be1a3aeeb9e7d2c145
   [2]: https://twitter.com/shanecelis
   [3]: https://opensource.org/licenses/MIT
*/

using UnityEngine;
using UnityEngine.UIElements;

/** This manipulator makes a visual element draggable at runtime. Unity's
    UIToolkit also has a [drag-and-drop system][1] but it is only appropriate
    for use within its editor.

    ## Usage

    ```
    element.AddManipulator(new DragManipulator());
    element.RegisterCallback<DropEvent>(evt =>
      Debug.Log($"{evt.target} dropped on {evt.droppable}");
    ```

    OR

    ```
    foreach (var element in root.Query(className: "draggable").Build()) {
      element.AddManipulator(new DragManipulator());
    }
    root.RegisterCallback<DropEvent>(evt =>
      Debug.Log($"{evt.target} dropped on {evt.droppable}");
    ```

    ### Styling

    When dragging, one should be able to style the participating elements.
    Coupled with Unity Style Sheet (USS) transitions, one can provide automatic
    tweens.

    | USS Selectors        | Description                                   |
    |----------------------+-----------------------------------------------|
    | .draggable           | Present on any element with a DragManipulator |
    | .draggable--dragging | Present while dragging                        |
    | .draggable--can-drop | Present while dragging over a droppable       |
    | .droppable           | Identifies a droppable element (editable)     |
    | .droppable--can-drop | Present while a draggable is hovering         |

    A custom property also allows one to disable dragging via the style sheet.

    | USS Properties      | Description                                    |
    |---------------------+------------------------------------------------|
    | --draggable-enabled | When set to false, dragging is disabled        |

    ## Requirements

    - Unity 2020.3 or later

    ## Dragging

    Clicking and dragging on the draggable element will cause it to move. The
    USS class "draggable--dragging" will be present during
    the duration.

    ### Remove USS Class on Drag

    One can remove a USS class while dragging by setting the following
    parameter at initialization:

    ```
    var dragger = new DragManipulator { removeClassOnDrag = "transitions" };
    ```

    Usage: If one has translation USS transitions set, dragging may look wrong
    and may not be smooth. Placing transitions into a special class and removing
    that class during the drag fixed that problem.

    ## Dropping

    Elements that have a "droppable" USS class will be considered droppable.
    When dragging and hovering over a droppable element, the USS class
    "droppable--can-drop" will be added; the draggable element will have
    "draggable--can-drop" added to it.

    If the draggable element is dropped on a non-droppable element, the
    draggable element's position is reset. It is suggested that one turn on USS
    transitions if one wants the draggable to tween back into its original
    place.

    ### Distinct Droppables

    If one has distinct droppable objects, one set the `droppableId` on the
    `DragManipulator` to something other than "droppable".

    ```
    var dragger = new DragManipulator { droppableId = "discard-pile" };
    ```

    ## Handling Events

    When a draggable element is released on a droppable element or its child, a
    `DropEvent` is emitted. The position of the element is not reset
    automatically in that case. If the dropped object is supposed to return to
    its original position, one ought to do that in the callback code.

    ```
    void OnDrag(DropEvent evt) {
      evt.target.transform.position = Vector3.zero;
      // OR
      // evt.dragger.ResetPosition();
    }
    ```

    ## Limitations

    This manipulator changes the `transform.position` of the target element
    while dragging. If one's styling is making use of that, the behavior is
    undefined.

    ## Notes

    The drop event bubbles up, so the callback can be placed on the parent or
    root element.

    Acknowledgments to Crayz[2] and Stacey[3] for their inspiring code.

    [1]: https://forum.unity.com/threads/visualelement-drag-and-drop-during-runtime.930000/#post-6373881
    [2]: https://forum.unity.com/threads/creating-draggable-visualelement-and-clamping-it-to-screen.1017715/
    [3]: https://gamedev-resources.com/create-an-in-game-inventory-ui-with-ui-toolkit/
*/
public class DragManipulator : IManipulator {

  private VisualElement _target;

  public VisualElement target {
    get => _target;
    set {
      if (_target != null) {
        if (_target == value)
          return;
        _target.UnregisterCallback<PointerDownEvent>(DragBegin);
        _target.UnregisterCallback<PointerUpEvent>(DragEnd);
        _target.UnregisterCallback<PointerMoveEvent>(PointerMove);
        _target.UnregisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
        _target.RemoveFromClassList("draggable");
        lastDroppable?.RemoveFromClassList("droppable--can-drop");
        lastDroppable = null;
      }
      _target = value;
      
      _target.RegisterCallback<PointerDownEvent>(DragBegin);
      _target.RegisterCallback<PointerUpEvent>(DragEnd);
      _target.RegisterCallback<PointerMoveEvent>(PointerMove);
      _target.RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
      _target.AddToClassList("draggable");
    }
  }
  protected static readonly CustomStyleProperty<bool> draggableEnabledProperty
    = new CustomStyleProperty<bool>("--draggable-enabled");
  protected Vector3 offset;
  private bool isDragging = false;
  private VisualElement lastDroppable = null;
  private string _droppableId = "droppable";
  /** This is the USS class that is determines whether the target can be dropped
      on it. It is "droppable" by default. */
  public string droppableId {
    get => _droppableId;
    init => _droppableId = value;
  }
  /** This manipulator can be disabled. */
  public bool enabled { get; set; } = true;
  private PickingMode lastPickingMode;
  private string _removeClassOnDrag;
  /** Optional. Remove the given class from the target element during the drag.
      If removed, replace when drag ends. */
  public string removeClassOnDrag {
    get => _removeClassOnDrag;
    init => _removeClassOnDrag = value;
  }
  private bool removedClass = false;

  private void OnCustomStyleResolved(CustomStyleResolvedEvent e) {
    if (e.customStyle.TryGetValue(draggableEnabledProperty, out bool got))
      enabled = got;
  }

  private void DragBegin(PointerDownEvent ev) {
    if (! enabled)
      return;
    target.AddToClassList("draggable--dragging");

    if (removeClassOnDrag != null) {
      removedClass = target.ClassListContains(removeClassOnDrag);
      if (removedClass)
        target.RemoveFromClassList(removeClassOnDrag);
    }

    lastPickingMode = target.pickingMode;
    target.pickingMode = PickingMode.Ignore;
    isDragging = true;
    offset = ev.localPosition;
    target.CapturePointer(ev.pointerId);
  }

  private void DragEnd(IPointerEvent ev) {
    if (! isDragging)
      return;
    VisualElement droppable;
    bool canDrop = CanDrop(ev.position, out droppable);
    //Debug.Log($"droppable {droppable}");
    if (canDrop)
      droppable.RemoveFromClassList("droppable--can-drop");
    target.RemoveFromClassList("draggable--dragging");
    target.RemoveFromClassList("draggable--can-drop");
    lastDroppable?.RemoveFromClassList("droppable--can-drop");
    lastDroppable = null;
    target.ReleasePointer(ev.pointerId);
    target.pickingMode = lastPickingMode;
    isDragging = false;
    if (canDrop)
      Drop(droppable);
    else
      ResetPosition();
    if (removeClassOnDrag != null && removedClass)
      target.AddToClassList(removeClassOnDrag);
  }

  protected virtual void Drop(VisualElement droppable) {
    var e = DropEvent.GetPooled(this, droppable);
    e.target = this.target;
    // We send the event one tick later so that our changes to the class list
    // will take effect.
    this.target.schedule.Execute(() => e.target.SendEvent(e));
  }

  /** Change parent while preserving position via `transform.position`.

      Usage: While dragging-and-dropping an element, if the dropped element were
      to change its parent in the hierarchy, but preserve its position on
      screen, which can be done with `transform.position`. Then one can lerp
      that position to zero for a nice clean transition.

      Notes: The algorithm isn't difficult. It's find position wrt new parent,
      zero out the `transform.position`, add it to the parent, find position wrt
      new parent, set `transform.position` such that its screen position will be
      the same as before.

      The tricky part is when you add this element to a newParent, you can't
      query for its position (at least not in a way I could find). You have to
      wait a beat. Then whatever was necessary to update will update.
   */
  public static IVisualElementScheduledItem ChangeParent(VisualElement target,
                                                         VisualElement newParent) {
    var position_parent = target.ChangeCoordinatesTo(newParent, Vector2.zero);
    target.RemoveFromHierarchy();
    target.transform.position = Vector3.zero;
    newParent.Add(target);
    // ChangeCoordinatesTo will not be correct unless you wait a tick. #hardwon
    // target.transform.position = position_parent - target.ChangeCoordinatesTo(newParent,
    //                                                                      Vector2.zero);
    return target.schedule.Execute(() => {
      var newPosition = position_parent - target.ChangeCoordinatesTo(newParent,
                                                                     Vector2.zero);
      target.RemoveFromHierarchy();
      target.transform.position = newPosition;

      newParent.Add(target);
    });
  }

  /** Reset the target's position to zero.

      Note: Schedules the change so that the USS classes will be restored when
      run. (Helps when a "transitions" USS class is used.)
   */
  public virtual void ResetPosition() {
    target.transform.position = Vector3.zero;
  }

  protected virtual bool CanDrop(Vector3 position, out VisualElement droppable) {
    droppable = target.panel.Pick(position);
    var element = droppable;
    // Walk up parent elements to see if any are droppable.
    while (element != null && ! element.ClassListContains(droppableId))
      element = element.parent;
    if (element != null) {
      droppable = element;
      return true;
    }
    return false;
  }

  private void PointerMove(PointerMoveEvent ev) {
    if (! isDragging)
      return;
    if (! enabled) {
      DragEnd(ev);
      return;
    }
    Vector3 delta = ev.localPosition - (Vector3) offset;
    target.transform.position += delta;
    if (CanDrop(ev.position, out var droppable)) {
      target.AddToClassList("draggable--can-drop");
      droppable.AddToClassList("droppable--can-drop");
      if (lastDroppable != droppable)
        lastDroppable?.RemoveFromClassList("droppable--can-drop");
      lastDroppable = droppable;
    } else {
      target.RemoveFromClassList("draggable--can-drop");
      lastDroppable?.RemoveFromClassList("droppable--can-drop");
      lastDroppable = null;
    }
  }
}

/** This event represents a runtime drag and drop event. */
public class DropEvent : EventBase<DropEvent> {
  public DragManipulator dragger { get; protected set; }
  public VisualElement droppable { get; protected set; }

  protected override void Init() {
    base.Init();
    this.LocalInit();
  }

  private void LocalInit() {
    this.bubbles = true;
    this.tricklesDown = false;
  }

  public static DropEvent GetPooled(DragManipulator dragger, VisualElement droppable) {
    DropEvent pooled = EventBase<DropEvent>.GetPooled();
    pooled.dragger = dragger;
    pooled.droppable = droppable;
    return pooled;
  }

  public DropEvent() => this.LocalInit();
}

// This hack allows us to use init properties in earlier versions of Unity.
#if UNITY_5_3_OR_NEWER && ! UNITY_2021_OR_NEWER
// https://stackoverflow.com/a/62656145
namespace System.Runtime.CompilerServices {
  using System.ComponentModel;
  [EditorBrowsable(EditorBrowsableState.Never)]
  internal class IsExternalInit{}
}
#endif
