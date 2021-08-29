# Unity Favourites Panel

This will add a new panel with a Tree View where you add categories and can drag-and-drop objects from the Hierarchy or Project panel into.

- The panel can be opened from menu: `Window > Favourites`.
- Use the [+] button in the Favourites panel's toolbar to add a new category.
- Use the [-] button to remove the selected category and all its items or to remove a single selected item.
- Double-click on an item to open it. For example to open a scene you have added to Favourites.
- Right-click to ping a Favourites item in the Hierarchy or Project panel.
- Drag-and-drop items to new categories or other areas of Unity. You can for example drag-and-drop a Sprite from the Favourites panel into the property of a component's Inspector (if that property accept Sprites).

### Technical Info

**Since v0.2.0** this package uses unity's GlobalObjectId (added in 2019.2) and saves all data in EditorPrefs so that different users on different machines can pin different favourites.
You no longer need to save any additional data in assets nor on your scenes.

![screenshot](https://user-images.githubusercontent.com/837362/34055429-d059f5ce-e1d7-11e7-8855-1b19dc2ad052.png)

## Installation

### Requirement

* Unity 2019.2 or later

### via Package Manager

Click `+` button in package manager ui, then select `Add package from git URL...` and paste repo url there (ending with `.git`)

To update the package, find the `manifest.json` file in the `Packages` directory in your project and change suffix `#{version}` to the target version.

* e.g. `"com.tools.favourites": "https://github.com/.../FavouritesWindow-package.git#v1.0.0",`

Or, use [UpmGitExtension](https://github.com/mob-sakai/UpmGitExtension) to install and update the package.
