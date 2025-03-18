# USD Scene Optimizer Tests

This directory contains tests for the USD Scene Optimizer package. The tests are organized into two main categories:

## Test Structure

- **EditMode/** - Contains tests that run in Unity's Edit mode
- **PlayMode/** - Contains tests that run in Unity's Play mode

## Testing Strategy

The USD Scene Optimizer package is developed with a clear separation between:

1. **Package Code**: Located in `Packages/com.usd.sceneoptimizer/` - Contains only production code
2. **Test Code**: Located in `Assets/Tests/` - Contains all tests for the package

This separation offers several benefits:
- The package remains clean and ready for deployment without test files
- Tests can be maintained alongside the package during development
- Test organization follows Unity's standard practice

## Writing Tests

### EditMode Tests
These tests run in Unity's edit mode and are suitable for testing most package functionality:
- Core model classes
- Utility functions
- Services/managers that don't require a running scene

### PlayMode Tests
These tests run in Unity's play mode and are suitable for testing:
- Runtime behavior
- Integration with Unity's lifecycle events
- Performance under realistic conditions

## Running Tests

1. Open the Unity Test Runner (Window > General > Test Runner)
2. Select the EditMode or PlayMode tab depending on which tests you want to run
3. Click "Run All" to run all tests, or select specific tests to run individually

## Best Practices

1. Keep tests focused on one aspect of functionality
2. Follow the Arrange-Act-Assert pattern
3. Use descriptive test names that explain what's being tested
4. Create helper methods for common test setup
5. Test both normal usage and edge cases

## Adding New Tests

When adding new features to the package:
1. Create corresponding test(s) in the appropriate test directory
2. Run the tests to verify they pass
3. Commit both the feature code and the tests together

This ensures that all package functionality remains tested as it evolves. 