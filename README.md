# Namotion.Reflection

Library with advanced .NET reflection APIs.

TODO: The idea is to move all reflection APIs from my other libraries (mainly NJsonSchema) into this general purpose library.

## TypeWithContext

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

## XML Docs

TODO: Move into this repo

https://github.com/RicoSuter/NJsonSchema/blob/master/src/NJsonSchema/Infrastructure/XmlDocumentationExtensions.cs

## Other APIs

TODO: Move into this repo

https://github.com/RicoSuter/NJsonSchema/blob/master/src/NJsonSchema/Infrastructure/ReflectionExtensions.cs
https://github.com/RicoSuter/NJsonSchema/blob/master/src/NJsonSchema/Infrastructure/ReflectionCache.cs
