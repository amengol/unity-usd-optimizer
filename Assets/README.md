# USD Scene Optimizer - Project Structure

This directory contains the main Unity project assets and scripts for the USD Scene Optimizer.

## Directory Structure

- `Scripts/` - Contains all C# scripts
  - `Editor/` - Editor-specific scripts and windows
  - `Runtime/` - Runtime scripts and components
  - `Tests/` - Test scripts and test data

- `Plugins/` - Third-party plugins and dependencies
  - `USD/` - USD-related plugins and configurations

- `Resources/` - Runtime resources and data

- `Scenes/` - Unity scene files

- `Prefabs/` - Prefab assets

- `Materials/` - Material assets

- `Textures/` - Texture assets

## Development Guidelines

1. All new scripts should be placed in the appropriate subdirectory under `Scripts/`
2. Editor-specific code must be in the `Editor/` directory
3. Keep the directory structure organized and clean
4. Follow Unity's naming conventions for assets 