![Graphene](docs/images/graphene-logo-full.png)

# Graphene

> `com.cupbearer.graphene": "https://github.com/LudiKha/Graphene.git"`

Graphene is a lightweight and modular framework for building runtime user interfaces with Unity's [UI Toolkit][0f273cb2].

  [0f273cb2]: https://docs.unity3d.com/2020.1/Documentation/Manual/UIElements.html "UI Toolkit"

## Intro

Graphene **superconducts** your creativity for efficiently building modern interactive UI for your games. It takes care of the heavy lifting by providing a framework inspired by Web standards, right into Unity.

It's **lightweight** and modular - you get to pick and choose which parts you need for your projects.

- **Dynamic Hierarchy**: Use the familiar GameObjects-based workflow to author the high-level hierarchy. The UI tree is built to reflect the GameObject hierarchy, using `Views` to designate sections of the screen, or components.
- **Template Composition**: Reuse your static assets by writing atomic components, and dynamically compose them in runtime.
- **Attribute-Based**: Instruct your UI to both draw _and_ bind templates using any data-container with a `[Bind]` attribute. Primitives, objects, collections, one-way, two-way binding, specific control selection: the parts you'll be most frequently developing with in C# are  exposed via attributes
- **State-Based Routing**: Use the GameObject hierarchy dynamically construct your router's states. Its functionality mimics url-based addresses: `index/settings/video`.

It comes with a **component-kit** library, many VisualElement extensions and a sample application to get you started.

---

## Demo
#### [Check out the WebGL demo ][f45eaa31]

  [f45eaa31]: https://ludikha.github.io/Graphene-Demo/ "Graphene WebGL demo"

---

# Quickstart
For a quick start, Graphene comes with a Bootstrapping library and demo scene - it is **highly** recommended to start your new project using the demo scene and resources provided within this project.

- Construct the high-level UI hierarchy, where each unique state is represented by a GameObject
- Add a [`Plate`][0fb2479e] component to each GameObject in the tree, with a `Graphene` component at the root.
- For each Plate in the tree, assign a static asset to its UIDocument. Root states will typically need a Layout-style [`template`](https://github.com/LudiKha/Graphene#theming) for their children to be fitted in.

Press play - Graphene will now dynamically construct the VisualTree based on your GameObject hierarchy. You've completed the required part of Graphene - however, we are still getting started.

Let's draw and bind some data onto our UI.

- Add a [`Theme`][a617f693] to the root Graphene component.
- Add one or more [`Renderer`][b39c255d] components to each `Plate` that has dynamic (instantiated) content
- Assign a [`Model`][19f2ae47] to the Renderer - this is a data container that serves as the model for the data-binding.
- In the type(s) assigned as model, select the members you wish to expose for binding by adding [`BindAttribute`][b3387189]s. Add an additional `DrawAttribute` to dynamically instantiate controls in runtime using [`Templates`][fe269940].

  [a617f693]: https://github.com/LudiKha/Graphene#theming "Theming"

Press play - Graphene will draw templates, and bind them to the model. If a static asset contained a control with a binding-path (e.g. a label with "Model/Title"), this will be bound to the model too.

The hierarchy is created and detail fields are rendered dynamically - now all that remains is to switch states.

- Add a [`StringRouter`][1015cb88] to the root GameObject.
- Add a `StringStateHandle` to each `Plate` GameObject that needs to be activated or deactivated based on states. Children are automatically deactivated with their parents. Give the StateHandle `StateId` unique names (e.g. "start", "load", "exit").
- For each `Plate` that has one or more children using states, select which child state is enabled by default by ticking `enableWithParent`
- In order to navigate, we can instantiate controls with a `RouteAttribute` or statically type them in UXML using `<gr:Route route="/settings" />`. It is also possible to encapsulate a button with the Route control. Make sure the routes correspond to the available states.

Press play - When clicking a route element (or child button), the router will attempt to change states and the view will display this accordingly.

  [0fb2479e]: https://github.com/LudiKha/Graphene#plates "Plates"
  [b39c255d]: https://github.com/LudiKha/Graphene#rendering "Renderer"
  [19f2ae47]: https://github.com/LudiKha/Graphene#model "Model"
  [1015cb88]: https://github.com/LudiKha/Graphene#routing "Router"
  [b3387189]: https://github.com/LudiKha/Graphene#binding "Binding"  
  [fe269940]: https://github.com/LudiKha/Graphene#templating "Templating"

Congrats! You're now done with the Quickstart and ready to tackle your first project using Graphene.

---

# Core Concepts

Graphene decouples fine-grained authoring from high-level logic, and in doing so aims to leverage UI Toolkit's innovations to the fullest.

## Plates
A Graphene hierarchy consists of nested components called `Plates`, with a `Graphene` component at the root. `Plate`s are the core of Graphene, are analogous for a general-purpose UI controller that can be switched on or off. Other, optional MonoBehaviour components may hook into a plate, and have their functionality based on whether a plate is active or not.

The following components and logic depends on plates:
- View

These can be authored in the familiar GameObject hierarchy. Graphene then constructs the VisualElement tree in runtime into a nested view.

## Views

## Rendering

## Model

## Templating

## Binding

### Binding Modes
Graphene supports 3 modes of binding a model to the view:
- **OneTime**: Instructs the `Binder` to only "print" the model once onto the view. No continuous binding will be attempted. Useful for immutable data, such as titles, labels or button callbacks.
  - Syntax: `::`
```html
<ui:Label binding-path="::Title" />
```
- **OneWay**: Creates a continuous binding from the model to the view. Updates are polled continuously but only for bindings that are currently visible (based on `Plate` state). Polling rate can be configured in the `Graphene` component.
- **TwoWay**: Creates a two-directional binding (from model to view, and view to model) for controls that support two-way binding. View to model binding is based on `INotifyPropertyChange` callbacks.

### Binding Passes
1. Static
2. Dynamic
### Static Binding

### Dynamic Binding

## Routing

## Theming

## Localization

## Step-by-step process
1. Static view composition
2. Static binding pass
3. Render templates from dynamic model
4. Dynamic binding pass
5. Runtime one-way/two-way binding
