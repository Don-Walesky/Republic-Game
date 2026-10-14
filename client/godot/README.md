# Republic - Godot Client

This folder contains the minimal Godot 4 client for Republic.

## Requirements

- [Godot Engine 4.x](https://godotengine.org/download) (Standard or .NET build)

## Opening the Project in Godot

1. Launch **Godot 4**.
2. In the Project Manager, click **Import** (or **Scan**).
3. Browse to this folder (`client/godot/`) and select `project.godot`.
4. Click **Import & Edit**.

## Running the Scene

- **Run Main Scene**: Press **F5** (or click the **Play** button in the upper-right corner). `PresidentialDesk.tscn` is configured as the main scene and will launch automatically.
- **Run Current Scene**: Open `PresidentialDesk.tscn` in the FileSystem dock and press **F6** (or click **Play Current Scene**).

## Scene Overview

- `PresidentialDesk.tscn`: Presidential desk dashboard scene displaying national identity, treasury balance, public approval, ruling party, and election status.
- `PresidentialDesk.gd`: Script attached to the root `Control` node managing the presidential desk labels.
