<p align="center">
  <img src="https://raw.githubusercontent.com/LudiKha/Graphene/master/docs/images/graphene-logo-full.png" />
</p>

&nbsp;

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT) ![last-commit](https://img.shields.io/github/last-commit/LudiKha/Graphene) ![open-issues](https://img.shields.io/github/issues/LudiKha/Graphene) [![](https://img.shields.io/twitter/follow/LudiKha.svg?label=Follow&style=social)](https://twitter.com/intent/follow?screen_name=LudiKha)

Graphene is a lightweight and modular framework for building runtime user interfaces with Unity's [UI Toolkit][0f273cb2].

[0f273cb2]: https://docs.unity3d.com/2020.1/Documentation/Manual/UIElements.html "UI Toolkit"

&nbsp;

# Intro

Graphene **superconducts** your creativity for efficiently building modern interactive UI for games. It takes care of the heavy lifting by providing a framework inspired by Web standards, right into Unity.

It's **lightweight** and modular - you get to pick and choose which parts you need for your projects.

- **Declarative Hierarchy**: Graphene makes it painless to design interactive UI. Use the familiar GameObjects hierarchy to design simple `Views` as sections of the screen or nested states.
- **Reduce Boilerplate**: Focus on your custom logic and design of building UI instead of repeating low-level tasks for each unique screen you are building. Graphene comes with a number of `Controls` that greatly enhance the speed of creating interactive UI, whilst reducing the need of custom view controllers in C# by exposing vital functionality in Uxml.
- **Attribute-Based**: Instruct your UI to both draw _and_ bind templates using any data-container with a `[Bind]` attribute. Primitives, objects, collections, one-way, two-way binding, specific control selection: the parts you'll be most frequently developing with in C# are  exposed via attributes
- **State-Based Routing**: Use the GameObject hierarchy dynamically construct your router's states. Its functionality mimics url-based addresses: `index/settings/video`.
- **Template Composition**: Reuse your static assets by writing atomic templates, and dynamically compose them in runtime.

It comes with a **[component-kit][e593c071]** library, **[sample project][05f1d8f6]** several VisualElement extensions and an **[online demo][84ed822d]** to get you started.

  [e593c071]: https://github.com/LudiKha/Graphene-Components "Graphene Components"
  [05f1d8f6]: https://github.com/LudiKha/Graphene-Sample-Project "Graphene-Sample-Project"
  [84ed822d]: https://ludikha.github.io/Graphene-WebGL-Demo/ "Graphene-WebGL-Demo"


## Online Demo
### [Check out the WebGL demo ][f45eaa31]

  [f45eaa31]: https://ludikha.github.io/Graphene-WebGL-Demo/ "Graphene WebGL demo"


## Installation
### Using Unity Package Manager (For Unity 2018.3 or later)

<details>
  <summary>You can install the package via UPM by adding the line below to your project's `Packages/manifest.json` file.</summary>

&nbsp;

  >You can find this file by opening your project's *Packages* folder in a file browser, it is not displayed in the editor.

</details>

```
{
  "dependencies": {
    "com.graphene.core": "https://github.com/LudiKha/Graphene.git?path=/src",
    "com.graphene.components": "https://github.com/LudiKha/Graphene-Components.git?path=/src",
    ...
  },
}
```

>Do note that although both components and demo are optional packages, it is recommended you use them to kickstart your own Graphene-based development environment.

#### Staying updated
Updating the package can be done via `Window/Graphene/Check for updates`. Unity currently does not support updating Git packages via the Package Manager automatically.


### Using UPM Git Extension
The best way to install Graphene and stay up to date with the latest versions, is to use [UPM Git Extension][49fed258].
   1. Follow the [installation instructions][2ddc031d]
   2. In the Package Manager, click the ![Git button](docs/images/installation/git.png) button, and add `https://github.com/LudiKha/Graphene.git` under subdirectory `src`, with the latest version.
   3. Voilá! As an added bonus you are now able to update the package via the package manager.

  [49fed258]: https://github.com/mob-sakai/UpmGitExtension "Upm Git Extension"
  [2ddc031d]: https://github.com/mob-sakai/UpmGitExtension#installation "UP Git Extension Installation Instructions"


&nbsp;

---

&nbsp;

# Quickstart

For a quick start, Graphene comes with a component library and [sample project][52a86b14] - it is **highly** recommended to start your new project using the demo scene and resources provided within this project.

  [52a86b14]: https://github.com/LudiKha/Graphene-Sample-Project "Graphene Sample Project"

#### 1: Constructing the hierarchy
- Construct the high-level UI hierarchy, where each unique state is represented by a GameObject
- Add a [`Plate`][0fb2479e] component to each GameObject in the tree, with a `Graphene` component at the root.
- For each Plate in the tree, assign a static asset to its UIDocument. Root states will typically need a Layout-style [`template`](https://github.com/LudiKha/Graphene#template) for their children to be fitted in.

Press play - Graphene will now dynamically construct the VisualTree based on your GameObject hierarchy. You've completed the required part of Graphene - however, we are still getting started.

Let's draw and bind some data onto our UI.

#### 2: Rendering & binding a model
- Add a [`Theme`][a617f693] to the root Graphene component.
- Add one or more [`Renderer`][b39c255d] components to each `Plate` that has dynamic (instantiated) content
- Assign a [`Model`][19f2ae47] to the Renderer - this is a data container that serves as the model for the data-binding.
- In the type(s) assigned as model, select the members you wish to expose for binding by adding [`BindAttribute`][b3387189]s. Add an additional `DrawAttribute` to dynamically instantiate controls in runtime using [`Templates`][fe269940].

  [a617f693]: https://github.com/LudiKha/Graphene#theming "Theming"

Press play - Graphene will draw templates, and bind them to the model. If a static asset contained a control with a binding-path (e.g. a label with [`Model.Title`][04efb446]), this will be bound to the model too.

  [04efb446]: https://github.com/LudiKha/Graphene#scopes "Scopes"

The hierarchy is created and detail fields are rendered dynamically - now all that remains is to switch states.

#### 3: Routing
- Add a [`StringRouter`][1015cb88] to the root GameObject.
- Add a `StringStateHandle` to each `Plate` GameObject that needs to be activated or deactivated based on states. Children are automatically deactivated with their parents. Give the StateHandle `StateId` unique names (e.g. "start", "load", "exit").
- For each `Plate` that has one or more children using states, select which child state is enabled by default by ticking `enableWithParent`
- In order to navigate, we can instantiate controls with a `RouteAttribute` or statically type them in UXML. Make sure to set the route member to a value that corresponds the available states.


> Note: It is also possible to encapsulate a button within a Route element.
> ```html
><gr:Route route="/settings">
>  <ui:Button text="Clicking me will change state"/>
></gr:Route>
>
><gr:Route binding-path="~Model/ExitState">
>```


Press play - The router constructs its state tree from the `Plate` hierarchy. When clicking a route element (or child button), the router will attempt to change states and the view will display this state change accordingly.

  [0fb2479e]: https://github.com/LudiKha/Graphene#plates "Plates"
  [b39c255d]: https://github.com/LudiKha/Graphene#rendering "Renderer"
  [19f2ae47]: https://github.com/LudiKha/Graphene#model "Model"
  [1015cb88]: https://github.com/LudiKha/Graphene#routing "Router"
  [b3387189]: https://github.com/LudiKha/Graphene#binding "Binding"  
  [fe269940]: https://github.com/LudiKha/Graphene#templating "Templating"

Congrats! You're now done with the Quickstart and ready to tackle your first project using Graphene.

&nbsp;

---

&nbsp;

# Core Concepts

Graphene decouples fine-grained authoring from high-level logic, and in doing so aims to leverage UI Toolkit's innovations to the fullest.

## Plates
A `Plate` represents a view controller in the VisualTree, and is used by Graphene to display the hierarchy, its states and views.

A Graphene hierarchy consists of nested components called `Plates`, with a `Graphene` component at the root. `Plate`s are the core of Graphene, are analogous for a general-purpose UI controller that can be switched on or off. Other, optional MonoBehaviour components may hook into a plate, and have their functionality based on whether a plate is active or not.

The following components and logic depends on plates:
- View

These can be authored in the familiar GameObject hierarchy. Graphene then constructs the VisualElement tree at runtime into a nested view.

## Views

## Rendering

## Model

## Templating
### Template
A `Template` is a semantic name for static asset that represents a chunk of UXML of varying granularity and complexity, which are used as building blocks to build and render the application. Moreover, templates can be declared directly in UXML based, and will be rendered at runtime based on the `Renderer` template configuration. Templates are wrapped in a `TemplateAsset` ScriptableObject, where additional variants can be created without needing to create and maintain copies of the base template.

#### Why use templates

When creating a simple element, such as a button, it may quickly end up consisting of several carefully configured elements and bindings:

```html
<ui:Button text="Button" name="ButtonFramed" focusable="true" tooltip="This is a button" binding-path="Value" class="button button-framed light">
  <ui:Style src="ButtonFramed.uss" />
  <gr:Route binding-path="Route" class="button__route" />
  <gr:Tooltip binding-path="Description" class="button__tooltip" />
  <ui:VisualElement class="button__background" />
  <ui:Label text="Label" binding-path="Label" />
  <ui:VisualElement class="button__hover" />
  <ui:VisualElement class="button__frame" />
</ui:Button>
```

Maintaining multiple versions and instances of the same chunk of UXML throughout multiple files can be both error prone and time intensive. Graphene allows you to reuse the same template, and instantiate them at runtime when required.

#### Creating a template

Create a `TemplateAsset` via the following menu command:
>  `Assets/Create/Graphene/Templating/TemplateAsset`

Assign a static UXML template, and give it an appropriate name.

#### Instantiating a template via CSharp
Templates can be instantiated directly in C# via a reference of the `TemplateAsset`.
```csharp
var clone = myTemplateAsset.Instantiate();
...

```
#### Instantiating a template via UXML
 Graphene allows you to statically type control types using the following syntax:

```html
<gr:Button text="Button" name="ButtonFramed" focusable="true" tooltip="This is a button" binding-path="Value" class="button button-framed light" />
```
At runtime, the button will be rendered to the full syntax of the first snippet, using the `Template` configuration of the `Renderer` component that initiates the binding.

## Binding

### Binding Modes
Graphene supports 3 modes of binding a model to the view. These can either be specified in the BindAttribute on the model, or using the Binder API directly.
- **OneTime**: Instructs the `Binder` to only "print" the model once onto the view. No continuous binding will be attempted. Useful for immutable data, such as titles, labels or button callbacks.
>Note: Prefixing the binding-path with the `::` syntax instructs the binder to use a one-time binding.
>```html
><ui:Label binding-path="::Title" />
>```

- **OneWay**: Creates a continuous binding from the model to the view. Updates are polled continuously but only for bindings that are currently visible (based on `Plate` state). Polling rate can be configured in the `Graphene` component.
- **TwoWay**: Creates a two-directional binding (from model to view, and view to model) for controls that support two-way binding. View to model binding is based on `INotifyPropertyChange` callbacks.

### Scopes
- Scope drilldown `.` `Renderer.Model.ChildObject.Title`
- Scope transferral `~` `~Renderer.Model` > `ChildObject.Title`

### Binding Passes
1. Static
2. Dynamic
### Static Binding

### Dynamic Binding

## Routing

## Theming
### Theme
A `Theme` is a data asset that can be used to author high-level styling configurations for (parts of) the VisualTree. It consists of a collection of [StyleSheets][3ca719d7], and can be nested to create reusable styling configurations using overrides.

  [3ca719d7]: https://docs.unity3d.com/Manual/UIE-USS.html "Unity USS Stylesheet"

#### Using themes

Create a theme via the following menu command:
>  `Assets/Create/Graphene/Theming/Theme`

Themes are assigned to the VisualTree when assigned to the `Graphene` component, or a sub-section when assigned to a `Plate`.

To manually apply a theme to a VisualElement, the following method is available.

```csharp
myTheme.ApplyStyles(el);

```

## Localization

## Step-by-step process
1. Static view composition
2. Static binding pass
3. Render templates from dynamic model
4. Dynamic binding pass
5. Runtime one-way/two-way binding


&nbsp;

---

&nbsp;

# Inspector Extensions
Graphene supports two third party inspector extensions out of the box: [Sirenix Odin Inspector][5855fee6] (paid) and [NaughtyAttributes][06602d8c] (free). Graphene relies on these to expose optional enhanced functionality to the inspector windows.

[5855fee6]: https://odininspector.com/ "Sirenix Odin Inspector"
[06602d8c]: https://github.com/dbrizov/NaughtyAttributes "Naughty Attributes"

## Installation
### Odin Inspector
This asset is automatically setup when it is included in the project.
 > Define symbol: `ODIN_INSPECTOR`

### Naughty Attributes
This package requires you to follow the following steps:
1. Add the package to the project `Packages/manifest.json` file:

   >`"com.dbrizov.naughtyattributes": "https://github.com/dbrizov/NaughtyAttributes.git#upm"`
2. In Project Settings/Player/Scripting define symbols, add the following entry: `NAUGHTY_ATTRIBUTES;`

After recompilation, your project will now have enhanced inspector functionality for Graphene components.
