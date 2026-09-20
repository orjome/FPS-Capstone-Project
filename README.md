# FPS Capstone Project

This project is a two-player first-person shooter prototype developed in s&box for my CSC-480 Capstone Project at Colorado State University Global.

## Project Purpose

The goal of this project was to create a low-cost multiplayer FPS prototype that could be used by a small independent game studio to test gameplay, networking, and technical feasibility before investing in a larger production.

## Features

- Multiplayer player spawning
- First-person player controls
- Hitscan weapon combat
- Health and death system
- Player respawning
- Kill and death tracking
- Kill-limit win condition
- Weapon pickups
- Three-slot weapon inventory
- Ammo and reload system
- Recoil and hitmarker feedback
- XP and leveling
- Skill tree system
- HUD with health, ammo, score, and player statistics
- Game-over screen
- Disconnect cleanup

## Main Components

The project includes several main gameplay systems:

- `ArenaGameManager` - Handles multiplayer spawning, kills, respawning, disconnects, and match completion.
- `PlayerHealth` - Manages player health, death, and respawn behavior.
- `PlayerStats` - Tracks player kills, deaths, and display name.
- `PlayerXP` - Handles experience points and player leveling.
- `WeaponManager` - Manages weapon inventory and weapon switching.
- `WeaponBase` - Handles firing, hitscan detection, ammo, recoil, and reloading.
- `WeaponPickup` - Allows players to pick up and configure weapons.
- `SkillTree` - Provides combat, survival, and mobility upgrades.
- `GameHUD` - Displays health, ammo, score, XP, and weapon information.
- `Crosshair` - Displays the aiming reticle and hitmarker.
- `GameOver` - Displays the winning player and allows the scene to restart.

## Technology

- C#
- s&box
- Git
- GitHub
- Razor UI

## Project Status

This repository contains the final capstone implementation. The project is intended as a functional proof of concept rather than a production-ready multiplayer game.

## Author

Orion Lytle  
CSC-480 Capstone Project  
Colorado State University Global
