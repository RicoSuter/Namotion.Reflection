# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build Commands

```bash
# Build the solution
dotnet build src/Namotion.Reflection.sln

# Run all tests
dotnet test src/Namotion.Reflection.sln

# Run a specific test class
dotnet test src/Namotion.Reflection.Tests --filter "FullyQualifiedName~ClassName"

# Run a specific test method
dotnet test src/Namotion.Reflection.Tests --filter "FullyQualifiedName~MethodName"

# Build in Release mode
dotnet build src/Namotion.Reflection.sln --configuration Release
```

## Project Overview

Namotion.Reflection is a .NET library providing advanced reflection APIs:
- **Nullable Reference Types (C# 8) reflection** - Reflect on nullability annotations
- **XML documentation reading** - Extract XML docs from assemblies at runtime
- **String-based type checks** - Type checking without direct type references

Used extensively in [NJsonSchema](https://github.com/RicoSuter/NJsonSchema) and [NSwag](https://github.com/RicoSuter/NSwag).

## Architecture

### Core Type Hierarchy (Context namespace)

```
CachedType                    - Base cached Type wrapper, handles Nullable<T> unwrapping
└── ContextualType            - Adds context (attributes, nullability info)
    ├── ContextualParameterInfo
    └── ContextualMemberInfo
        ├── ContextualMethodInfo
        └── ContextualAccessorInfo
            ├── ContextualPropertyInfo
            └── ContextualFieldInfo
```

**Key design points:**
- `CachedType` instances are cached per `Type`, `ParameterInfo`, or `MemberInfo`
- If original `Type` is `Nullable<T>`, the `Type` property returns `T` (unwrapped); use `OriginalType` for the wrapped type
- Contextual and type attributes are evaluated once and cached
- Use `CachedType.ClearCache()` or `XmlDocs.ClearCache()` to clear caches

### Main Components

- **Context/** - Contextual type system with nullability reflection
- **XmlDocsExtensions.cs** - XML documentation extraction from assemblies
- **Performance/** - Optimized property readers/writers (`IPropertyReader`, `IPropertyWriter`)
- **Extension methods** - `TypeExtensions`, `EnumerableExtensions`, `ObjectExtensions`

## Target Frameworks

The main library targets: `netstandard2.0`, `net462`, `net8.0`

Tests target: `net8.0`, `net462`

## Solution Structure

- `Namotion.Reflection` - Main library
- `Namotion.Reflection.Cecil` - Mono.Cecil integration for XML docs
- `Namotion.Reflection.Tests` - Main test project (xUnit)
- `Namotion.Reflection.Cecil.Tests` - Cecil-specific tests
- `Namotion.Reflection.Tests.FullAssembly` - Test helper assembly
- `Namotion.Reflection.Demo` - Demo/sample project
- `Namotion.Reflection.Benchmark` - Performance benchmarks
