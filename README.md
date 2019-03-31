# Namotion.Reflection

Library with advanced .NET reflection APIs.

## TypeWithContext

### C# 8 nullability reflection

With the `TypeWithContext` class you can reflect on the nullability of properties, fields, method parameters and return types which will be available when compiling with the C# 8 compiler with the Nullable Reference Types feature enabled. 

Given the following test class with some C# 8 nullability annotations (?):

```csharp
#nullable enable

public class TestClass
{
    public void Process(Dictionary<string, string?> dictionary)
    {
    }
}
```

To reflect on the first parameter's nullability, we can load a `TypeWithContext` instance and display the nullability of the parameter's types:

```csharp
var method = typeof(TestClass).GetMethod(nameof(TestClass.Process));
var parameter = method.GetParameters().First();
var parameterTypeWithContext = parameter.GetTypeWithContext();

Console.WriteLine("Dictionary: " + parameterTypeWithContext.Nullability);
Console.WriteLine("Key: " + parameterTypeWithContext.GenericArguments[0].Nullability);
Console.WriteLine("Value: " + parameterTypeWithContext.GenericArguments[1].Nullability);
```

The output is: 

```
Dictionary: NotNullable
Key: NotNullable
Value: Nullable
```

## XML Docs

TODO: Move into this repo

https://github.com/RicoSuter/NJsonSchema/blob/master/src/NJsonSchema/Infrastructure/XmlDocumentationExtensions.cs

## Other APIs

TODO: Move into this repo

https://github.com/RicoSuter/NJsonSchema/blob/master/src/NJsonSchema/Infrastructure/ReflectionExtensions.cs
https://github.com/RicoSuter/NJsonSchema/blob/master/src/NJsonSchema/Infrastructure/ReflectionCache.cs
