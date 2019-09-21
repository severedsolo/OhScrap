# KerbalChangeLog
This project is meant to be a simple way for mod creators to add an ingame changelog for their users when they release a new version.  
**THIS WILL DO NOTHING ON ITS OWN**

## Adding a changelog
To add a changelog, simply create a config file (.cfg) with the following nodes and fields (as an example):
```
KERBALCHANGELOG //Required to have this name
{
  showChangelog = True //To show the changelog, this must be set to True
  modName = KerbalChangeLog //Add your mod's name here
  VERSION //Declares a version node
  {
    version = 1.1 //Version number, numbers only with no spaces!
    change = Fixed window scrolling //any changes in that version. There can be as many change fields as you want
    change = Added shiny buttons
    change = Removed bugs
  }
  VERSION
  {
    version = 1.0
    change = First release!
  }
}
```
This will then be outputted in a changelog window that appears in the space center view the first time the user loads a game with a changelog that has the `showChangelog` set to  `True`. After this initial load, the user will no longer see the changelog for that mod until the mod creator releases a new version with the changelog cfg file's `showChangelog` field set to `True`.  

This will handle as many mods as have changelogs the user has installed, but please do not create multiple changelog files for a single mod. This will lead to multiple changelog pages showing up in the window, and confusion for everyone. 
