# Navigator

A tree-based UI navigation system for Unity that manages screen hierarchy, back navigation, and automatic cleanup of child windows.

## Overview

Navigation is modelled as a tree where each node represents an open UI module. Every node knows its depth, parent, and children — enabling features like "close this popup and restore the previous one" or "navigate to a root screen and destroy everything that was open inside it."

```
Root (depth 0)
└── GameHUD (depth 1)          ← screen-level module
    └── InventoryPopup (depth 2)
        └── ItemDetailPopup (depth 3)
            └── Tooltip (depth 4)
```

Closing `InventoryPopup` destroys `ItemDetailPopup` and `Tooltip` automatically. Navigating back to `GameHUD` destroys everything beneath it.

---

## Core Concepts

### Depth

Each UI layer (screens, popups, tooltips, etc.) maps to a fixed depth value. Depth is configured externally via `INavigationDepthResolver` and determines where a module sits in the hierarchy.

### Stack per parent

Each node holds a **stack** of children at a given depth. When you open a second popup over the first, the first is hidden (not destroyed). Closing the second reveals the first automatically. Only the top of the stack can be closed — attempting to close a hidden node is an error.

### NavigationNode

```csharp
public class NavigationNode
{
    public byte Depth { get; }
    public NavigationNode Parent { get; }
    public object Args { get; }
    public UIControl Control { get; }

    public NavigationNode LastChild();
    public void AddChild(NavigationNode child);
    public void RemoveLastChild();
    public NavigationNode Find(byte depth, object args);
    public NavigationNode FindLast(byte depth, object args);
}
```

`Args` holds the `INavigationArgs` that was used to open this node, used later for lookup during `CloseLast`.

`Find` performs a full tree traversal. `FindLast` is an optimized variant used by `CloseLast` — it traverses only the top of each stack level and early-exits if the current depth exceeds the target.

---

## Usage

### Navigating to a module

```csharp
_navigator.Navigate(new InventoryArgs());
```

The navigator resolves the correct container for the target depth, instantiates the UI control, binds the module, and inserts the node into the tree. If another module is already open at the same depth, it is hidden. If the target depth is higher up the hierarchy than the current position, everything below that depth is destroyed first.

### Closing the current module

```csharp
var args = new InventoryArgs();
_navigator.Navigate(args);

// later...
_navigator.CloseLast(args);
```

`CloseLast` finds the node by depth + args reference, verifies it is the top of its parent's stack, closes it (and all its children), then restores the previous node at that depth if one exists.

### Full example

```csharp
public void Start()
{
    // Open game HUD (depth 1)
    _navigator.Navigate(new GameHUDArgs());

    // Open inventory popup (depth 2)
    _navigator.Navigate(new InventoryArgs());

    // Open item detail on top of inventory (depth 2 stack: Inventory hidden, ItemDetail visible)
    var itemArgs = new ItemDetailArgs();
    _navigator.Navigate(itemArgs);

    // Close item detail → inventory becomes visible again
    _navigator.CloseLast(itemArgs);

    // Navigate to main menu (depth 1) → destroys everything below depth 1
    _navigator.Navigate(new MainMenuArgs());
}
```

---

## Setup (VContainer)

```csharp
public class GameSessionScope : LifetimeScope
{
    [SerializeField] private LayoutsContainer layoutsContainer;
    [SerializeField] private DepthContainer[] containers;

    protected override void Configure(IContainerBuilder builder)
    {
        var depthResolver = new NavigationDepthResolver();

        foreach (var container in containers)
            depthResolver.InsertDepth(new LayoutDepthContainer(container.Depth, container.Container));

        builder.RegisterInstance(depthResolver).AsImplementedInterfaces();
        builder.RegisterInstance(layoutsContainer).AsImplementedInterfaces();

        builder.Register<NavigationModuleResolver>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.Register<UIControlResolver>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.Register<Navigator>(Lifetime.Singleton).AsImplementedInterfaces();
    }
}
```

Inject `INavigator` wherever navigation is needed:

```csharp
public class SomePresenter
{
    private readonly INavigator _navigator;

    public SomePresenter(INavigator navigator)
    {
        _navigator = navigator;
    }
}
```

---

## INavigator Interface

```csharp
public interface INavigator
{
    ReadOnlyReactiveProperty<NavigationNode> Current { get; }
    void Navigate<T>(INavigationArgs<T> navigationArgs) where T : IUIModuleBehaviour;
    void CloseLast<T>(INavigationArgs<T> navigationArgs) where T : IUIModuleBehaviour;
}
```

`Current` exposes the currently active `NavigationNode` as a reactive property. Subscribe to it to react to navigation changes:

```csharp
_navigator.Current
    .Subscribe(node => Debug.Log($"Navigated to depth {node.Depth}"))
    .AddTo(this);
```

---

## Key Behaviours

| Scenario | Result |
|---|---|
| Navigate to deeper depth | Current node hidden, new node pushed onto parent's stack |
| Navigate to same depth | Current node hidden, new node pushed |
| Navigate to higher depth | Everything below target depth is destroyed, previous node at target depth hidden |
| CloseLast on top node | Node and all its children destroyed, previous node at same depth restored |
| CloseLast on hidden node | Error logged, nothing happens |
| CloseLast on unknown node | Error logged, nothing happens |

---

## Architecture

```
INavigator
    └── Navigator
            ├── INavigationDepthResolver   → maps depth byte to a scene Transform container
            ├── IUIControlResolver         → instantiates UIControl prefab into the container
            └── INavigationModuleResolver  → creates and binds the IUIModuleBehaviour to the control
```

`INavigationArgs<T>` carries both the `NavMetaData` (depth, layout name) and whatever data the target module needs to initialise itself.

If a module's model implements `IDisposable`, `Navigator` calls `Dispose()` automatically when the node is destroyed.

---

## Default Resolvers

All three resolvers are interfaces — you can provide your own implementations. The defaults cover most cases out of the box.

### NavigationDepthResolver

Maps a depth index to a Unity `Transform` that acts as the parent container for that layer.

```csharp
var depthResolver = new NavigationDepthResolver();
depthResolver.InsertDepth(new LayoutDepthContainer(depth: 1, container: hudContainer));
depthResolver.InsertDepth(new LayoutDepthContainer(depth: 2, container: popupContainer));
```

Internally keeps a resizable array indexed by depth. Gaps in depth are allowed — the array grows automatically with extra headroom when a new depth is inserted. Logs an error if a depth that has not been registered is requested at runtime.

### LayoutsContainer

A `ScriptableObject` that implements `ILayoutContainer` — the source of UI prefabs looked up by name.

Create one via **Assets → Create → TiredSiren → Navigation → Layouts Container**, then populate the list in the Inspector with name/prefab pairs. Register it in your scope:

```csharp
builder.RegisterInstance(layoutsContainer).AsImplementedInterfaces();
```

Internally the list is converted to a `Dictionary<int, GameObject>` keyed by `string.GetHashCode()` on first access. If a name is left empty, the prefab's own name is used as the key. Duplicate or colliding entries are logged and skipped.

You can replace `LayoutsContainer` with any `ILayoutContainer` implementation — for example one that loads prefabs from Addressables by address instead of holding direct references.

```csharp
public interface ILayoutContainer
{
    GameObject GetLayout(string layoutName);
}
```

### UIControlResolver

Instantiates a prefab from `ILayoutContainer` by name and retrieves its `UIControl` component.

```csharp
// NavMetaData.LayoutName must match a key in your LayoutsContainer
public record InventoryArgs : INavigationArgs<InventoryModule>
{
    public NavMetaData NavMetaData => new(depth: 2, layoutName: "InventoryPopup");
}
```

Logs an error if the layout name is not found in the container, or if the instantiated prefab has no `UIControl` component, and in both cases returns `null` so `Navigator` can abort the navigation cleanly.

### NavigationModuleResolver

Creates a **child VContainer scope** per navigation, resolves `T` inside it, and ties the scope lifetime to the `UIControl` game object via R3's `AddTo`.

```csharp
// Inside the scope, navigationArgs is registered as an instance,
// so T can receive it via constructor injection:
public class InventoryModule : IUIModuleBehaviour
{
    public InventoryModule(INavigationArgs<InventoryModule> args) { ... }
}
```

When the `UIControl` is destroyed (e.g. via `CloseLast`), the scope is disposed automatically, which disposes all objects registered inside it. This means you do not need to manually unsubscribe or clean up anything that lives inside the module scope.
