# USD Scene Optimizer - Development Environment Setup

## Prerequisites

- Unity 2022.3 LTS or later
- Visual Studio 2022 or later
- Git
- USD Core package for Unity
- .NET Framework 4.7.1 or later

## Setup Instructions

1. **Clone the Repository**
   ```bash
   git clone https://github.com/amengol/unity-usd-optimizer.git
   cd unity-usd-optimizer
   ```

2. **Unity Setup**
   - Open Unity Hub
   - Install Unity 2022.3 LTS if not already installed
   - Open the project in Unity
   - Unity will automatically import all required packages

3. **USD Package Setup**
   - Open the Package Manager (Window > Package Manager)
   - Click the '+' button and select "Add package from git URL"
   - Add the USD Core package URL
   - Wait for the package to be imported

4. **Development Environment**
   - Open the project in Visual Studio
   - Ensure the Unity development tools are installed in Visual Studio
   - Set up your preferred code style and formatting rules

5. **Testing Setup**
   - Open the Test Runner window in Unity (Window > General > Test Runner)
   - Run the tests to ensure everything is set up correctly

## Project Structure

The project follows a standard Unity project structure:

```
Assets/
├── Scripts/
│   ├── Editor/     # Editor-specific scripts
│   ├── Runtime/    # Runtime scripts
│   └── Tests/      # Test scripts
├── Plugins/        # Third-party plugins
├── Resources/      # Runtime resources
├── Scenes/         # Unity scenes
├── Prefabs/        # Prefab assets
├── Materials/      # Material assets
└── Textures/       # Texture assets
```

## Development Guidelines

1. **Code Style**
   - Follow C# coding conventions
   - Use meaningful variable and method names
   - Add XML documentation for public APIs
   - Keep methods focused and single-purpose

2. **Version Control**
   - Create feature branches for new development
   - Write meaningful commit messages
   - Keep commits atomic and focused
   - Follow the Git Flow branching strategy

3. **Testing**
   - Write unit tests for new features
   - Maintain test coverage above 80%
   - Run tests before committing changes

4. **Documentation**
   - Update documentation when adding new features
   - Keep API documentation up to date
   - Document any complex algorithms or business logic

## Common Issues and Solutions

1. **Package Import Issues**
   - Clear the Package Manager cache
   - Delete the Library folder and let Unity rebuild
   - Check package compatibility with Unity version

2. **Build Issues**
   - Ensure all required packages are installed
   - Check for missing dependencies
   - Verify build settings are correct

## Getting Help

- Check the project documentation in `docs/`
- Review existing issues in the issue tracker
- Contact the development team for support 