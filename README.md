# Namotion.Reflection

[![Azure DevOps](https://img.shields.io/azure-devops/build/rsuter/Namotion.Reflection/16/master.svg)](https://rsuter.visualstudio.com/Namotion.Reflection/_build?definitionId=16)
[![Nuget](https://img.shields.io/nuget/v/Namotion.Reflection.svg)](https://www.nuget.org/packages/Namotion.Reflection/)
[![MyGet](https://img.shields.io/myget/namotion-reflection/v/Namotion.Reflection.svg?label=preview%20nuget)](https://www.myget.org/feed/Packages/namotion-reflection)

<img align="left" src="https://raw.githubusercontent.com/RicoSuter/Namotion.Reflection/master/assets/NuGetIcon.png">

.NET library with advanced reflection APIs like XML documentation reading, Null Reference Types (C# 8) reflection and string based type checks.

This library is mainly used in [NJsonSchema](https://github.com/RicoSuter/NJsonSchema).

## Contextual and cached types

### C# 8 nullability reflection

With the `ContextualType` class you can reflect on the nullability of properties, fields, method parameters and return types which will be available when compiling with the C# 8 compiler with the Nullable Reference Types feature enabled. 

Given the following test class with some C# 8 nullability annotations (?):

```csharp
#nullable enable

public class MyClass
{
    public void MyMethod(Dictionary<string, string?> dictionary)
    {
    }
}
```

To reflect on the first parameter's nullability, we can load a `ContextualType` instance and display the nullability of the parameter's types:

```csharp
var method = typeof(MyClass).GetMethod(nameof(MyClass.MyMethod));
var parameter = method.GetParameters().First();
var contextualParameter = parameter.ToContextualParameter();

Console.WriteLine("Dictionary: " + contextualParameter.Nullability);
Console.WriteLine("Key: " + contextualParameter.GenericArguments[0].Nullability);
Console.WriteLine("Value: " + contextualParameter.GenericArguments[1].Nullability);
```

The output is: 

```
Dictionary: NotNullable
Key: NotNullable
Value: Nullable
```

For more details, see https://blog.rsuter.com/the-output-of-nullable-reference-types-and-how-to-reflect-it/

Methods: 

- CachedType.ClearCache()

## Read XML Documentation

Methods: 

- **Type|MemberInfo.GetXmlDocsSummaryAsync():**
- **Type|MemberInfo.GetXmlDocsRemarksAsync():**
- **ParameterInfo.GetXmlDocsAsync():** Gets the XElement of the given type

- **XmlDocs.ClearCache()**

## Extension methods

Methods: 

**IEnumerable extensions**

- GetAssignableToTypeName: Tries to get the first object which is assignable to the given type nameHasProperty
- FirstAssignableToTypeNameOrDefault: Tries to get the first object which is assignable to the given type name.
- GetCommonBaseType: Finds the first common base type of the given types.

**Object extensions**

- HasProperty: Determines whether the specified property name exists.
- TryGetPropertyValue: Determines whether the specified property name exists.

**Type extensions**

- IsAssignableToTypeName
- InheritsFromTypeName
- GetEnumerableItemType
- GetDisplayName
