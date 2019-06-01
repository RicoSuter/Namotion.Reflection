# Namotion.Reflection

[![Azure DevOps](https://img.shields.io/azure-devops/build/rsuter/Namotion.Reflection/16/master.svg)](https://rsuter.visualstudio.com/Namotion.Reflection/_build?definitionId=16)
[![Nuget](https://img.shields.io/nuget/v/Namotion.Reflection.svg)](https://www.nuget.org/packages/Namotion.Reflection/)
[![MyGet](https://img.shields.io/myget/namotion-reflection/v/Namotion.Reflection.svg?label=preview%20nuget)](https://www.myget.org/feed/Packages/namotion-reflection)

<img align="left" src="https://raw.githubusercontent.com/RicoSuter/Namotion.Reflection/master/assets/NuGetIcon.png">

.NET library with advanced reflection APIs like XML documentation reading, Null Reference Types (C# 8) reflection and string based type checks.

This library is mainly used in [NJsonSchema](https://github.com/RicoSuter/NJsonSchema).

## Contextual and cached types

### Nullability reflection (C# 8)

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
using Namotion.Reflection;

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

### Validate nullability (C# 8)

It is important to understand that Null Reference Types are a compiler feature only and the .NET runtime does not do any checks when your app is running. Consider the following class: 

```csharp
public class Person
{
    public string FirstName { get; set; }

    public string? MiddleName { get; set; }

    public string LastName { get; set; }
}
```

Inside your application you'll get warnings when you forget to set the `FirstName`. However when data is coming from outside (e.g. via reflection, serialization, etc.) you could end up with invalid objects. This JSON.NET call throws no exception but will create an invalid object:

```csharp
var person = JsonConvert.DeserializeObject<Person>("{}");
```

The ensure that the object is in a valid state, you can now call `EnsureValidNullability()` which might throw an `InvalidOperationException`:

```csharp
person.EnsureValidNullability();
```

Methods: 

- HasValidNullability();
- EnsureValidNullability();
- ValidateNullability();

## Read XML Documentation

Methods: 

- **Type|MemberInfo.GetXmlDocsSummaryAsync():**
- **Type|MemberInfo.GetXmlDocsRemarksAsync():**
- **ParameterInfo.GetXmlDocsAsync():** Gets the XElement of the given type
- ... and more

- **XmlDocs.ClearCache()**

Can also be used with [Cecil](https://github.com/jbevain/cecil) types, install [Namotion.Reflection.Cecil](https://www.nuget.org/packages/Namotion.Reflection.Cecil/).

## Extension methods

Methods: 

**IEnumerable extensions**

- **GetAssignableToTypeName():**: Gets all objects which are assignable to the given type name as string.
- **FirstAssignableToTypeNameOrDefault():**: Tries to get the first object which is assignable to the given type name as string.
- **GetCommonBaseType():**: Finds the first common base type of the given types.

**Object extensions**

- **HasProperty():** Determines whether the specified property name exists.
- **TryGetPropertyValue():** Determines whether the specified property name exists.

**Type extensions**

- **IsAssignableToTypeName()**
- **InheritsFromTypeName()**
- **GetEnumerableItemType()** 
- **GetDisplayName():** Gets a human readable identifier for the type (eg. "DictionaryOfStringAndInt32").
