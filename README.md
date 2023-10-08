![Alt Text](https://media.giphy.com/media/hES9wSnCaL2vcWp4QH/giphy.gif)

# SCREENSHOTTER
A simple tool to quickly and easily take cropped screenshots, written in C# using WinForms and the .NET framework...
Designed to be as lightweight and minimalistic as possible, so it's pretty quick and to the point!

* Built with .NET framework v4.7.2

# USAGE:
* Upon launching the program and pressing the default hotkey (CTRL + F12) you'll notice that the screen becomes darker, and that you can now click and drag to select a specific region.
The region that you have selected will be saved as an image to `"C:\Users\[USERNAME]\Pictures\screenshots\"`. The image will automatically be copied
to your clipboard, so you can paste it directly into a Discord chat for example.

* Holding the CTRL key will cause the image to open in your specified image viewer, if you want to quickly use it as a reference!

# NOTE:
* It's worth noting that you cannot currently take screenshots of most fullscreened applications. If you want to screenshot them you will need to
put them in borderless window mode at least! Fullscreen application screenshotting may be a thing in the future though!

# TODO:
* Freeze the screen and cancel any inputs that aren't inside the application...
* Add a exit/cancel screenshot hotkey (Esc)
