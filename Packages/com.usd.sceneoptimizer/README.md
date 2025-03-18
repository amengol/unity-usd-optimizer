# USD Scene Optimizer

A Unity tool for analyzing and optimizing USD scenes for real-time performance.

## Features

- Scene Analysis
  - Mesh analysis (polygon count, vertex density, UV mapping)
  - Material analysis (texture usage, shader complexity)
  - Scene hierarchy analysis
  - Performance metrics collection

- Optimization
  - Mesh optimization (LOD generation, simplification)
  - Material optimization (texture compression, batching)
  - Scene optimization (instance optimization, hierarchy flattening)

- Unity Integration
  - Editor UI for easy scene management
  - Optimization profiles
  - Preview system
  - Batch processing

## Requirements

- Unity 2022.3 or later
- USD SDK (version 21.11 or later)
- Visual Studio 2022 (for development)

## Installation

1. Open the Package Manager (Window > Package Manager)
2. Click the + button in the top-left corner
3. Select "Add package from git URL"
4. Enter the repository URL
5. Click Add

## Usage

1. Open the USD Scene Optimizer window (Window > USD Scene Optimizer)
2. Import a USD scene using the Import button
3. Analyze the scene to see current metrics
4. Create or select an optimization profile
5. Preview the optimization
6. Apply optimization if satisfied
7. Export the optimized scene

## Documentation

For detailed documentation, please visit the [Documentation](Documentation~/Documentation.md) page.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE.md) file for details. 