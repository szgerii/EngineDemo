# Engine

## Introduction

This project is a rudimentary game engine written in C#, on top of the [MonoGame](https://monogame.net/) framework. The solution is made up of two projects: the engine itself and a (for now) barebones demo project showcasing its usage.

Although the engine is far from production-ready it can be used as a starting point for simple cross-platform 2D games.

## Current State

The engine was used for a semester-long uni assignment and although it performed surprisingly well both in the development and stability departments, its components faced multiple changes, reworks and expansions. The new version has more or less been ported over to work with the old demo, but some rough edges still need to be straightened out (namely the debug mode and UI systems).

The commit history also doesn't represent the development process of the engine, as it has gone through several repository changes (unfortunately without the commit history being handled appropriately).

## Features

The engine currently supports the following features:

- Game object and component system
- Scene system
- Camera with scale and rotation
- Camera controller that can be used in most basic situations (and extended in others)
  - keyboard-based controls (with acceleration)
  - game object following
  - mouse dragging (has to be enabled explicitly)
- Components for basic sprite rendering needs, this includes:
  - static sprites
  - animated sprites (with or without multiple directions)
- Collision system
  - AABB only (for now, see [Planned features](#planned-features))
  - can handle large number of well-distributed entities thanks to quadtree optimization (though its specific parameters might have to be adjusted for each use case)
- Audio (soloud, Windows-only)
  - bus system
  - audio effects
- Input system
  - keyboard, mouse and gamepad support
  - a general Actions interface for grouping more than one type of input together
- Low level UI system (currently broken)
  - it offers a vectorized approach for fonts in contrast to MonoGame's pixelated, fixed-resolution SpriteFonts
  - for now it can only be used via a simple text element or a text-in-a-box element, but this will be expanded upon in the future (see [Planned feature](#planned-features))
- Tilemap importing and handling (for .tmx files)
- Wrapper for display settings (resolution, v-sync, etc.)
- Headless mode (running the game without rendering)
  - this can be useful for CI and testing purposes
- Hook-based state machine component
  - this makes it easy to separate the different behaviors of a single game object based on a state enum
  - it enables having enter/update/leave callbacks for states with the use of attributes, which means no explicit event subscription/callback register code in the game object logic
- Debug mode system (currently broken, needs to be readjusted for the newly ported version of the engine)

## Planned features

We plan to implement the following features before the first release version of the engine:

- complete UI system: the UI system right now "only" handles low-level tasks, like proper font rendering (as MonoGame's SpriteFont is quite limiting for professional project). In the future, we plan to add abstractions for complete UI elements as well as maybe some layout management features.
- better debug mode system: the current identifier based approach to the debugging system can be quite uncomfortable to use. We plan to implement some sort of enum or reference-based interface where you don't have to find and replace all occurences of the identifiers when they change.
- debug console: the engine and its debug mode system could greatly benefit from a debug console, however this requires the UI to be flashed out better first.
- better collision: we aim to implement SAT-based collision detection for better versatility
- better animation system: the current animation system is only fit for specific spritesheet formats, we plan to expand on this in the future
- hooks rework: the current hooks (PreUpdate, PostUpdate, etc.) can handle basic tasks but are also very limiting in nature, and some of the engine/demo components bypass it directly.
- more robust scene systems: this is kind of related to hooks, as they are mostly defined on the scene level, but some additional features would also be nice to have interface-wise
- more tilemap import features: the tilemap loader currently supports only as much of the .tmx format as is used by the demo. We plan to implement much more of its capabilities in the future (e.g. tilemaps without a hardcoded size).

## Documentation

For now, the engine has no standalone documentation, but most of the public API's non-trivial methods have proper XML-docs. The basic usage of the engine can be learned from the Demo project as well.

## Contributors

- [szgerii](https://github.com/szgerii/ "szgerii GitHub Profilja")
- [Bandi1234](https://github.com/Bandi1234 "Bandi1234 GitHub Profilja")
