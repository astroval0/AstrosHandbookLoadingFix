# Astro's Handbook Loading Fix

a Vintage Story mod that stops the handbook from freezing your game. 

## what it does

- the game searches the entire guidebook on the main thread every time you open it, and with 10000 pages that makes the game freeze. 
this mod fixes that by skipping the search when nothing is typed.
- the game also bulk loads the entire guidebook on a worker thread when you join a world, and it stays empty until every page is loaded. 
this mod fixes that by batch loading the guidebook so pages show up while it loads.
 
 

### compatible with mods that change the handbook search too, like Attribute Rendering Library.
- if you find any incompatibilities then please make an issue.


## client side only

it only changes how the handbook loads and searches on your end, so the server does not need the mod. it works on any server.

## building

needs the .NET 10 SDK and Vintage Story 1.22 installed in the default place (`%APPDATA%\Vintagestory`). building copies the mod straight into `VintagestoryData\Mods\AstrosHandbookLoadingFix`, so close the game first.

## license

LGPL 2.1, see `LICENSE`.
