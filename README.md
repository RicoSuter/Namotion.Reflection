# Namotion.Reflection

.NET library with advanced reflection APIs.

## TypeWithContext

### Reflect over nullability

With the `TypeWithContext` class you can reflect over the nullability of properties, fields, method parameters and return types which will be available when compiling with the C# compiler in version 8+.

Given the following test class with some C# 8 nullability annotations:

```csharp
public class TestClass
{
    public void Process(Dictionary<string, string?> dictionary)
    {
    }
}
```

Now, we can load the `TypeWithContext` instance for the first method parameter and display the nullability of the types:

```csharp
var method = typeof(TestAction).GetMethod(nameof(TestAction.Process));
var parameter = method.GetParameters().First();
var parameterTypeWithContext = parameter.GetTypeWithContext();

Console.WriteLine(parameterTypeWithContext.ToString());
```

This will output: 

```
Dictionary`2: NotNullable
  string: NotNullable
  string: Nullable
```

## XML Docs

TBD
