# CMod_Aliser_one_click_ship_screenshot_export_button

A [CMod](https://github.com/murolem/CMod_Loader) (C# mod) for Cosmoteer for making screenshots. Yes, the game already has some buttons to do that, but wouldn't it be better to do it all in one click?

Adds a button to the ship panel (lower left part of the UI) that generates screenshots of all views for a selected ship. Screenshots are saved as PNGs to the screenshots folder, with filenames modified to include ship name, view type (exterior, blueprint) and built in path (eg `CabalCombat`).

Made for wiki maintainers to make updating ship images easier.

## Developing

The source code is not the cleanest it could be, but it does the job. Feel free to look up anything you need.

The code for loading in image assets (the button image) is a bit buggy, as it can cause crashes during the game. Not sure why.

If you want to make a CMod, use the [CMod Example Mod template](https://github.com/murolem/CMod_Example), **do not use this mod** as a base because it's not being updated with the template mod.

Any improvements are welcome!

## Links

-   [This mod in Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=3430202913)
