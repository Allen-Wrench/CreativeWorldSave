# Creative World Save
###### A Space Engineers client-side plugin by **dude**

---

This plugin will construct an empty offline world with the mods and settings of the session that is currently loaded. Intended to be used to easily create an offline creative world for an online server you play on that is as close to 1:1 as possible.

In order to prevent any unintended shenanigans, the save files are constructed from the ground up including only world configuration settings, mods, and a few other things required for the world to load.

- World configuration settings (sync range, block limits, pcu settings, etc) will persist, but settings from mods that are stored server-side will not as they are not available for clients unless the mod includes a method for a player to recieve them.  
*An example of this is the Relative Top Speed mod, you can use the chat command /rts config and a window will appear with the current configuration. You can then copy those values into the configuration file that is saved into the worlds 'Storage' folder.*

- Worlds will not contain any grids, floating objects, antenna signals, or other characters. 

- Planets will remain in the world at their correct location, but in an unmodified state (voxel changes get reset).

- The save will also be set to creative mode as an offline world. These can be changed like any other save before you load the world if you want your friends to be able to join.

Let me know via a github issue if you run into any problems, and dont forget to include the log!

### Usage Directions: 
To save the current world, open the players screen (F3) and click the Profile button. A new save will be created with the same name as the server you are connected to.