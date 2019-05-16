# Namotion.Reflection

Library with advanced .NET reflection APIs.

## C# 8 nullability reflection

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

## Read XML Documentation

- GetXmlSummaryAsync
- GetXmlRemarksAsync
- GetXmlDocumentationAsync: Gets the XElement of the given type

## Extension methods

**IEnumerable extensions**

- FirstAssignableToTypeNameOrDefault
- GetCommonBaseType

**Object extensions**

- HasProperty
- TryGetPropertyValue

**Type extensions**

- IsAssignableToTypeName
- InheritsFromTypeName
- GetEnumerableItemType
- GetDisplayName