# FpsGame

## Welcome

This project is an experiment into making a cross-platform multiplayer fps game with the MonoGame library.

- Last updated: 9/10/2023

## Building the Project

### Requirements

This project is designed to work with .Net 7. The x64 version of the SDK will need to be installed regardless of the OS being used due to MonoGame not being completely compatible with ARM based Macs.

### Windows

Recommend using Visual Studio 2022 to build and run the project. It should work as expected from there, but please make an issue if it does not so I can look into it.

### Mac

Using VS Code, you can use the following lines in your terminal to pull, build, and run the repo

```
git clone git@github.com:proc-gen/FpsGame.git fps-game
cd fps-game
dotnet build
cd FpsGame/bin/Debug/net7.0
./FpsGame
```

You can also use Visual Studio 2022 for Mac, although that is being discontinued August 31, 2024 per Microsoft. The project should run as simply as it does on Windows this way, although you may need to force it to get all the Nuget packages.

Note: Textures are currently unavailable for ARM based Macs because of missing dylib's for FreeImage and NVidia Texture Tools. I do plan on figuring out a fix for this in the future.

### Linux

I don't have a test system for this at the moment. If you know how to get it running please let me know so I can add the instructions here. I did have to get a specific dylib file for things to work on an M1 based Mac, so I anticipate there being similar issues running Linux on an ARM CPU.

## Instructions

### Game Types

- Single Player
  - This spawns a server on localhost (127.0.0.1) and lets no other players into the game. Eventually, there will not need to be a TCP connection created to run the game locally.
- Host Multiplayer Game
  - This spawns a server at the selected IP address and port. As long as the IP address and port are available to other computers, they can join the game.
- Join Multiplayer Game
  - This allows you to join any other game you can connect to and does not spawn a local server instance.
- Dedicated Host
  - Similar to Host Multiplayer Game, but does not spawn the client side of the game when running.

### Controls

- Keyboard & Mouse
  - WASD to move
  - Space to jump
  - Hold the right mouse button down to look around
  - Tab to show the player list
  - Esc to quit the game
- Controller
  - Left Joystick to move
  - Right Joystick to look around
  - A to Jump
  - Start to quit the game