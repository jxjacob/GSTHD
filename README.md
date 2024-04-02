# Gossip Stones Tracker HD (DK64 Edition)
Fork of Hapax (`Hapax#1729`)'s [fork](https://github.com/HapaxL/GSTHD) of Drekorig (`Drekorig#2506`)'s [Gossip Stones Tracker](https://github.com/Draeko/ootr_gst/tree/ladder_version) for the [Donkey Kong 64 Randomizer](https://dk64randomizer.com)

Bodged together by JXJacob for his own selfish purposes. Compared to the main branch, this fork contains the following code changes:
- Support for saving/loading the tracker state so a run can be resumed at a later point.
- Support for broadcast views. This allows streamers to use a complex layout for their own notes while providing a more compact/basic layout to be synced with it and displayed for their viewers. More information about setting up this feature can be found [here](https://github.com/jxjacob/GSTHD/wiki/Broadcast-View---Setup)
- Support for autotracking (with select N64 emulators). More information about setting up this feature can be found [here](https://github.com/jxjacob/GSTHD/wiki/Autotracking-Creation-(From-existing-database)) and [here](https://github.com/jxjacob/GSTHD/wiki/Autotracking-Creation-(Adding-a-new-game))
- Support for DK64 Spoiler Hint settings.
- Customizable mouse controls
- Support for Item Grids to be placed in a layout.
- Other backend bugfixes to improve system stability 😉


## The Big Picture:
GSTHD is based upon the primary idea of being able to **drag** your items into a "Gossip Stone" (usually a question mark or Wrinkly's greyed-out face in the DK64 layouts) to keep track of where you got said item and its corresponding hint paths.

GSTHD also has a hint system with the ability to note the hint's path destination and the items you got in that region.

For supported N64 titles, GSTHD can also autotrack your items as you collect them. It does NOT autotrack your hints, those (intentionally) still require manual labour.

All GSTHD layouts are in plaintext .json files, so if you want to make a minor tweak to a layout to suit your personal preferences (or make a new one altogether), it can be done so freely.

There are 2 executables. If in doubt, use the one *without* the _32; if you have issues autotracking, try the one *with* the _32. Both are included due to unavoidable silly technical reasons.


## Using GSTHD
1) Click on your items as you get them (or use the autotracker so it clicks for you)
2) As you encounter hints in-game, click on the coloured section and type the location of the hint out (ex: Galleon Outskirts) then press Enter
3) In this newly created hint, there will be (usually) be one Stone to the left, and 2-3 to the right. The LEFT is the goal/destination of the hint, and the RIGHT are the items you find in the hinted region. To fill these stones, click and drag an item from the main section of the tracker into the Stones.
>Ex: If you find a hint that Galleon Outskrits is path to Key 1, you would drag the Key 1 icon into the LEFT stone. If you found Coconut in that region, you would drag Coconut's icon to one of the RIGHT stones. All stones within hints support multiple items dragged into them, so you can track multiple paths or multiple items in a hinted region as you so desire.
4) When you think a hinted region is solved, you can either delete the hint entirely to save space, or click the hint to change the text colour to denote that it's solved. Or just leave it there. I'm just a txt file, not a cop

Some layouts contain different spaces to store specific information (such as Shops or Regions listed individually) and not all layouts are optimized for every scenario or seed. Choose a layout that best fits your needs, or edit a layout file if you really want to. GSTHD is a framework for storing notes, but *how* you go about that is up to you. 


# Controls
As of GSTHD_DK64 v0.5.1, the mouse controls can be customized/rebound in the Options menu tab. For the sake of this document, the customizable controls will be noted `Like This`, even if the exact behaviour doesnt match the input name.

You can rebind several controls to the same button, but some inputs are priorized before others. 

The priority of inputs (and their defualt binding) is listed below. With the top item in the list having the highest priority, etc.

| Input Name  | Default Binding |
| ------------- | ------------- |
| `Autocheck Drag`  | Left + Right Click (and move your mouse)  |
| `Drag`  | Middle Click (and move your mouse)  |
| `Checkmark Item`  | Shift + Left Click  |
| `Increment Item`  | Left Click  |
| `Decrement Item`  | Right Click  |
| `Reset Item`  | Middle Click  |


## Global-ish Controls:
| Input Name  | Behaviour |
| ------------- | ------------- |
| `Increment Item` | Increments the item (ex: Green Slam -> Blue Slam -> Red Slam) |
| `Decrement Item` | Decrements the item (ex: Red Slam -> Blue Slam -> Green Slam) |
| `Reset Item` | Resets the item to its default state |
| `Checkmark Item` | Adds a checkmark to the top right corner of the item |
| `Drag` | Allows you to drag your items into a gossip stone or spoiler hint cell |
| `Autocheck Drag` | Starts a drag while also incrementing the item on the tracker |
| Scroll Wheel | Alternate method of incrementing or decrementing |
> NOTE: Checkmarking an item does not hold any *inherent* meaning, but can be useful for marking an item as required, a region as cleared out, etc


## Double Item Controls:
Same controls as "Global-ish", with the exception of:
| Input Name  | Behaviour |
| ------------- | ------------- |
| `Increment Item` | Toggles the "left" half of the item |
| `Increment Item` + Moving your mouse | Drag with the "left" half of the item |
| `Decrement Item` | Toggles the "right" half of the item |
| `Decrement Item` + Moving your mouse | Drag with the "right" half of the item |

## Medallion/Key Controls:
Same controls as "Global-ish", with the sole exception of:
| Input Name  | Behaviour |
| ------------- | ------------- |
| Scroll Wheel | Increments or decrements the dungeon/world names |


## Gossip Stone Controls:
Same controls as "Global-ish", with the sole exception of:
| Input Name  | Behaviour |
| ------------- | ------------- |
| `Reset Item` | (when items have been dragged into it) Removes the currently-displayed item from the stone |
> NOTE: Gossip Stones (usually a question mark or Wrinkly's face) are best used with an item that has been `Drag`ged into them. Without a dragged-into item, a gossip stone is essentially an item that has generic placeholder icons.


## Hint Controls

### While Empty
| Input Name  | Behaviour |
| ------------- | ------------- |
| Left Click | Enables the hint to be typed into |
| Typing | Write your hint |
| Enter | (after typing) Adds your typed words to the list of filled hints |


### While Filled
| Input Name  | Behaviour |
| ------------- | ------------- |
| `Increment Item` | Increments the colour of the hint text |
| `Decrement Item` | Decrements the colour of the hint text |
| `Reset Item` | Deletes the hint |
| Scroll Wheel | (Where applicable) Scrolls the hint window to display other hints you've written down |
> NOTE: The hint text colour doesn't hold any inherit meaning, but can be useful for marking a region as cleared out, resolved, needs to be prioritized, etc


## Spoiler Hint Panel Controls

### While Empty

| Input Name  | Behaviour |
| ------------- | ------------- |
| Dragging a DK64 Spoiler Log file into the panel | Imports the spoiler log (as an alternative to the menu option) |


### While Filled:
| Input Name  | Behaviour |
| ------------- | ------------- |
| `Drag` an Item into the Cell | Adds the item to the cell (semi-transparent) |
| `Autocheck Drag` an Item into the Cell | Adds the item to the cell |
| `Increment Item` / `Decrement Item` | Toggles the item between a semi-transparent mode |
| `Reset Item` | Deletes the item |
> NOTE: Semi-transparency denotes whether the item has been acquired or is a "placeholder" that will be filled when the real item is found


## Broadcast View:
Press F2 on your keyboard to bring it up, press it again to close it (on some supported layouts). This additional window will update automatically as you make changes to the main tracker window.


# Autotracking:
Select your emulator from the menu tab, make sure your ROM is loaded into said emulator, and then click "Connect To Emulator".

That's it.

***That's the whole setup.***

It Just Works™

Once you enter the game (ie, not the main menu), the items on the tracker will update automatically. Hints will NOT be updated automatically, you must manage those yourself.

If you fail to connect, try switching between the 64-bit and 32-bit versions of the application (GSTHD.exe and GSTHD_32.exe, respectively) and try to connect again. Some emulators (like RMG) **require** the 64-bit version to function, and some emulators (like Bizhawk-DK64) behave weirdly on some PCs with the 64-bit version. 


# Settings Clarifications
Options -> Gossip Stones -> "Allow Override of Held Image"
- Once an image has been dragged into a gossip stone, incrementing/decrementing that stone will discard any dragged images and increment/decrement as normal. This is how GSTHD behaved prior to v0.4.0

Options -> Gossip Stones -> "Ignore incoming checkmarks"
- By default, gossip stones will inherit the "checkmarked" state of the item dragged into it. Enabling this option will make all stones ignore the inherited checkmark.

Options -> Gossip Stones -> "Force All Stones to Cycle"
- By default, gossip stones must be explicity set to be cyclable (contain more than one dragged image) in the layout json. Enabling this option allows for *all* stones to be cyclable

Options -> Autosaves -> "Automatically Delete Old Autosaves"
- On program startup, GST will automatically delete older autosaves once 25 have accumulated in the autosave folder

Options -> DK64 Spoiler Hints -> "Hide Starting Moves"
- Will hide any starting moves from appearing in the "Isles" section of the spoiler hint panel to save space (only works with autotracking)

Options -> DK64 Spoiler Hints -> "Ignore incoming checkmarks"
- By default, spoiler hint items will inherit the "checkmarked" state of the item dragged into it. Enabling this option will make all cells ignore the inherited checkmark.


Autotracker -> "Subtract from Collectables"
- Will allow for certain collectables' total to be subtracted from a different specified value (in DK64, this would be subtracting turned in blueprints from your total collected blueprint count)

# Common Issues

Scroll wheel doesn't work with Items/Collectables/Dungeon-Labels
- Go to your Windows settings -> "Devices" -> "Mouse" and enable "Scroll inactive windows when I hover over them".

Menu Bar dissapears
- Hit F10 to toggle it.
- If your keyboard lacks F10, open "settings.json" and change "ShowMenuBar": false to "ShowMenuBar": true.

Keys being autotracked incorrectly:
- You are almost certainly playing DK64Rando with the Keys **not** in the item pool.
- Due to how it was implemented in-game at the time (predating the modern item rando system), the keys in-game are assigned the flags in memory of other keys based on level order (ie: Key 4 could be assigned the flag of Key 6 if Caves is in Lobby 4). Due to level order not being autotrackable, there is no good way to account for this for the time being.
- The DK64Rando devs are aware of this issue, but for obvious reasons it is a low priority fix to something that only affects the autotrackers.
- For now, the keys will have to be autotracked incorrectly with this setting. Apologies for the inconvenience.

Some antivirus programs can **incorrectly** flag GSTHD_DK64 as malicious (due to the method used to autotrack via the emulators), this can be fixed by making an exception in your antivirus program. I have better things to do with my time than run malicious code to your machine, trust me.


**If you find any other issues with GSTHD_DK64, please track down JXJacob on Discord and he'll try to resolve whatever technical issue you're having.**




### Special Thanks
- Drekorig and Hapax, for the original GST and GSTHD versions, respectively.
- Selene-T, for creating [Tracker of Time](https://github.com/Selene-T/Tracker-of-Time), which was used as the reference for GSTHD_DK64's autotracking implementation.
- All the monkey scientists for the documentation of DK64 memory addresses used in the autotracking
- Viewers like you
