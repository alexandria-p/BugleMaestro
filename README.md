<p align="center"><img src="https://raw.githubusercontent.com/alexandria-p/BugleMaestro/main/icon.png" width="150"/></p><h1 align="center">Bugle Maestro (Thunderstore build)</h1>

[![GitHub Page](https://img.shields.io/badge/GitHub-Thunderstore%20Build-blue?logo=github&style=for-the-badge)](https://github.com/alexandria-p/BugleMaestro)

[![Thunderstore Page](https://img.shields.io/thunderstore/v/alexandria_p/BugleMaestro?style=for-the-badge&logo=thunderstore)](https://thunderstore.io/c/peak/p/alexandria_p/BugleMaestro/)
[![Thunderstore Downloads](https://img.shields.io/thunderstore/dt/alexandria_p/BugleMaestro?style=for-the-badge&logo=thunderstore&logoColor=white)](https://thunderstore.io/c/peak/p/alexandria_p/BugleMaestro)

## Description

Play the bugle 📯 using your keyboard keys. Multiplayer-friendly! Access three octaves & all the semitones inbetween.

## Intructions

You no longer need to click your mouse to blow the Bugle. Instead, tap the bottom row of keys on your keyboard.

SCALE:
Notes are represented on your typical QWERTY keyboard, by using the bottom-row of letters for input (as per instructions below).
<p align="center"><img src="https://raw.githubusercontent.com/alexandria-p/BugleMaestro/main/combined_bugle_instructions.png" width="1000"/></p>

OCTAVES:
You can go up or down octaves by holding the UP ARROW or DOWN ARROW keys, respectively.

SEMITONES:
You can sharpen or flatten notes by holding the RIGHT ARROW or LEFT ARROW keys, respectively.

Currently keyboard-only (no controller support).

### What does this look like?

Just like normal. When you are actively blowing a note on your bugle, the note you are playing will display on the bottom-right of the screen. (Other players will not be shown the note you are playing, they will just hear it out loud)
<p align="center"><img src="https://raw.githubusercontent.com/alexandria-p/BugleMaestro/main/maestro_screenshot.jpg" width="1000"/></p>

# Installation steps

### If you've never installed mods for Peak, do this first:

* Install the r2 Modman mod manager via Thunderstore (click "Manual Download"): https://thunderstore.io/c/peak/p/ebkr/r2modman/
* Run r2 Modman
* Select Peak as your game in r2 Modman

### Follow these steps

* Add this mod (Bugle Maestro) to r2 Modman and make sure it is enabled
* Run Peak via r2 Modman. You may need to already have Steam running in the background.
* In a multiplayer lobby, *all players must follow these steps and have Bugle Maestro installed*.

# Changelog

**v1.0.1**
- Fix mp3 load

**v1.0.0**
- Play the bugle

## Future planned features

- Add controller support
- Allow users to change the key-binding for this mod
- Integration with *Virtuoso*
- Add instructions for Bugle controls in-game by displaying instructions onscreen using the UI.

## Known bugs

None. Please report any bugs to the Github page or in the Peak Modding Discord.

## Contact Us

🚨 If you found this mod on any site except for *Thunderstore* or *r2 Modman*, then I cannot guarantee the safety of the file you have downloaded! 🚨

Please report this back to me on my GitHub https://github.com/alexandria-p or my Twitter account https://twitter.com/HumbleKraken.

Feel free to tweet at me too if you enjoyed using this mod - especially if you attach the footage you were able to save!

If you would like to report any bugs, join the Peak modding discord and find me there.

# Can I copy this mod's code? Can I contribute to this project?

*You cannot wholesale copy this mod with the intent of passing it off as your own.*

Ideally, you should be able to raise an issue or pull request on this project, so that any new functionality can stay in a single mod & be toggleable by users in the game settings. If this gives you trouble, please see the "Contact Us" section of this README for details on how to get in touch.

If you'd like to fork the project to add or change functionality, please message me first at my GitHub or Twitter and make sure you link back to my GitHub repository in your mod description.

https://github.com/alexandria-p/BugleMaestro

I wholeheartedly encourage you to look at the mod files on my GitHub to learn more about how it was made 💝 I have learnt so much by reading the source code of other mods.

# What about mod compatibility? Are there any other ways I can play the Bugle?

This mod should work with *Pocket Bugle* (https://thunderstore.io/c/peak/p/mondash/PocketBugle/) also by Matthew Ondash/mondash. You may want to remap the bind key used by *Pocket Bugle* for summoning the Bugle in-game, to make sure it does not clash with this mod's controls.

If you'd like to play the bugle like a real chambered brass instrument, please checkout *Virtuoso* (https://thunderstore.io/c/peak/p/mondash/Virtuoso/) by Matthew Ondash/mondash.

* Unfortunately, you can only choose one way to play your Bugle at a time - ensure all players either enable *Bugle Maestro* or *Virtuoso* for their players. 
* *Whichever mod you decide **not** to use, make sure all players have it disabled.

# Dependencies 
*If you install this mod through Thunderstore/r2Modman, these dependencies will be installed automatically.*
- [BepInEx] BepInExPack PEAK https://thunderstore.io/c/peak/p/BepInEx/BepInExPack_PEAK/

# References

Scaffolded using Hamunii's template: https://github.com/PEAKModding/BepInExTemplate

This template uses Harmony.

Notes about modding are included in a text file in root of the project GitHub.

# Acknowledgements
Thank you to Mondash (https://github.com/mondash) for teaching me about note frequencies and for our great chats. I look forward to working together with you again in the future 🤝

ChatGPT used in one file (ClipHelper.cs -> CreateAudioClipFromMp3() and CreateNewAudioClipByPitchShifting()) to figure out how to import mp3 files as Unity AudioClips. Everything else made by me, with love, by humans for humans.