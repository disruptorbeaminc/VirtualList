# VirtualList
An efficient virtual list view for Unity uGUI UI

The virtual list view allows display of large scrolling lists or grids while
only instantiating enough elements to show what is on screen at a given time.

## Basic Usage

### Types of components
* `VirtualGridList` => grid or vertical/horizontal list with fixed sized
  elements (probably fine for most use-cases)
* `VirtualVerticalList` => vertical list that fits width of elements to parent
* `VirtualHorizontalList` => horizontal list that fits height of elements to
  parent

### In the scene
1. Add the desired component on the gameobject for the list root. This is the
   view inside the scroll rect, which might have otherwise contained a layout
   group.
2. Set up references to scroll rect and tile prefab
3. Set up sizing as desired
4. Remove any layout and ContentSizeFitter components.
5. Remove any children of the list root. These are not automatically cleared,
   and their instantiation has a cost.

### Sources
The virtual list takes a `IListSource` that describes what data to use. You can
manually implement the interface, or you can use the `SimpleSource` in
noninteractive cases.

#### IPrefabSource
An `IListSource` can optionally implement `IPrefabSource`, allowing you to use
multiple different prefabs for list elements.

### Using in code
* add `using VirtualList;`
* Add `AbstractVirtualList` variable in panel (and hook it up)
* Call `SetSource` to make it show the source
* Call Clear() on the list in the panel’s OnExit

### Misc
* You can use the `SetSourceAndCenterOn` method to set the source and center on
  a specific index in a single step (when desired). If done separately, it would
  populate views for the old scroll position, and then potentially throw them
  away.

## Installation
You can copy the files, or add this repo as a dependency in the Unity package
manager. There are no external dependencies, so it is pretty simple.

## Caveats
* All elements of a given list must have the same size and spacing. The list
  relies on this to be able to efficiently calculate what would be visible at
  a given scroll position.
* Instantiated list elements are reused without paying the cost of disabling
  the game object. But sometimes they will be disabled if there are not enough
  elements on screen, which is a bit more expensive. This can happen more often
  if a IPrefabSource is used with a mix of different prefabs.

## Examples
See `Samples~/Sample1` folder

## License
MIT