# CorvallisTransitForWindows
A rudimentary port of Corvallis Transit for the [Universal Windows Application Platform](https://msdn.microsoft.com/en-us/library/windows/apps/xaml/dn894631.aspx).

## Prerequisites

- Visual Studio 2015

- Windows 10

## Why?

To learn new things, of course!

## Disclaimer

This code probably has some bugs and is absolutely *not* feature-complete nor production-worthy.

## Samples I found helpful

- [ResizeView](https://github.com/Microsoft/Windows-universal-samples/tree/master/ResizeView/cs)
- [mapcontrolsample](https://github.com/Microsoft/Windows-universal-samples/tree/master/mapcontrolsample)
- [xaml_navigation](https://github.com/Microsoft/Windows-universal-samples/tree/master/xaml_navigation/CS)

## Features (small in number as they are)

- Seeing all routes on a pretty map

- Seeing the route your selected and all of its stops as pins on the map

- Flyout with ETA for a route on a stop

- Walking directions to selected stop (via Maps app)

## Work that needs to be done

- Getting the UI Theme consistent w.r.t Window color and text color.  Currently the window which "holds" the app is still white with black text, and that needs to be inverted.
- Different Navigation format.  Something along the lines of this:

```
<F> | Favorites
    |  |
    |  --- 49th Street & Technology Loop
    |  --- 26th Street & Madison or whatever
<R> | CTS Routes
    |  |
    |  --- Route 1
    |  --- Route 2
<D> | Directions
    | To Stop [    ] [go]
<S> | Settings
```

The left-hand side represents the "bar" that you see when the panel isn't expanded.  When it's expanded the right-hand size is visible.  On phones (720p or lower), the left-hand side is not visible.  Look at the Maps app on Windows 10 for an example of this.

`<F>` is an icon for Favorites, `<R>` is an icon for Routes, `<D>` is some Directions icon, and `<S>` is some Settings icon.

- Ability to add a stop to a favorites list, which is then persisted throughout
- Ability to search stops and launch directs from the nav menu
- Settings menu to allow user to clear out everything, show open source licenses, etc
- Splash screen with proper logo
