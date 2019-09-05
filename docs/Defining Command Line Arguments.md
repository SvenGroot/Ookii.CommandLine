# Defining command line arguments

In order to use Ookii.CommandLine, you must create a class that defines the arguments accepted by your application. This type will specify the names, types and attributes (required, positional) of each argument.

There are two ways to define arguments: using the properties of the class, or using constructor parameters for the class.

## Using properties

The preferred way to define arguments is by using properties. A property defines an argument only when it has the `CommandLineArgumentAttribute` attribute applied to it. The property must have a getter and setter, except for multi-value and dictionary arguments which can be defined by read-only properties under certain conditions.

The type of the argument is the type of the property, and the name of the argument matches the property name by default, but this can be overridden using the `CommandLineArgumentAttribute` constructor.

An argument defined by a property is by default optional and not positional. Its default value can be set using the `CommandLineArgumentAttribute.DefaultValue` property.

To create a required argument, set the `CommandLineArgumentAttribute.IsRequired` property to true.

To create a positional argument, set the `CommandLineArgumentAttribute.Position` property to a non-negative number. This property determines the relative ordering of the positional arguments only, not their actual position, so it’s fine if you skip numbers. Positional arguments defined by properties come after arguments defined by constructor parameters, so for example if there are three constructor parameters, the property with the lowest position value will be the fourth positional argument. Remember that you cannot have required positional arguments after optional ones, and that a multi-value positional argument must be the last positional argument. If your properties violate these rules, the `CommandLineParser` class’s constructor will throw an exception.

To define a multi-value argument, you can use either a read-write property of an array type (e.g. `int[]`) or a read-only property of any type implementing `ICollection<T>` (e.g. `List<int>`).

To define a dictionary argument, you can use either a read-write property of type `Dictionary<TKey, TValue>` (e.g. `Dictionary<string, int>`) or a read-only property of any type implementing `IDictionary<TKey, TValue>`.

If the type of an argument is a Boolean, a nullable Boolean, or an array of Booleans, this defines a switch argument unless the argument is positional.

Consider the following properties:

```csharp
[CommandLineArgument(DefaultValue = "default")]
public string SomeArgument { get; set; }

[CommandLineArgument("OtherName", Position = 0)]
public int OtherArgument { get; set; }

[CommandLineArgument]
public bool Switch { get; set; }

public string NotAnArgument { get; set; }
```

The first property defines an optional non-positional argument of type string with the name “SomeArgument” and the default value “default”.

The second property defines an optional positional argument of type int with the name “OtherName”. The argument will be the first positional argument (after the arguments defined by constructor parameters, if there are any).

The third property defines a switch argument, because its type is a Boolean. Switch arguments can be supplied without a value; they will be true if present and false is not.

The fourth property does not define an argument, because it doesn’t have the `CommandLineArgumentAttribute` applied.

## Using constructor parameters

An alternative way to define positional parameters is using a constructor. The parameters of the public constructor for the class that defines the arguments will be used to define arguments. These arguments will be positional arguments, and required if the parameter is a required parameter.

Every constructor parameter creates a positional argument with its position matching the position of the constructor parameter. The type of the constructor parameter is the type of the argument, and by default the name of the constructor parameter is used of as the argument name, but this can be overridden using the `ArgumentNameAttribute` attribute.

For example, consider a class with the following constructor:

```csharp
public MyArguments(string arg1,
                   int arg2,
                   [ArgumentName("CustomName")] float arg3 = 0f)
{
}
```

This constructor defines the following arguments: a required positional argument of type `string` with the name “arg1”, a required positional argument of type `int` with the name “arg3”, and an optional positional argument of type `float` with the name “CustomName” and a default value of 0.

If you are not using the C# 4.0 compiler, you can apply the `System.Runtime.InteropServices.OptionalAttribute` attribute and the `System.Runtime.InteropServices.DefaultParameterValueAttribute` to the parameter to define an optional argument. In Visual Basic, you can use the usual syntax for optional parameters (for an example, see the source code of the Visual Basic sample application included with the library).

If the type of a constructor parameter is an array, this defines a multi-value positional argument.

If your type has more than one constructor, you must mark one of them using the `CommandLineConstructorAttribute` attribute. You don’t need to use this attribute if you have only one constructor.

If you don’t wish to define arguments using the constructor, simply use a constructor without any parameters (or don’t define an explicit constructor).

## Defining aliases

An alias is an alternative name that can be used to specify a command line argument. Aliases can be added to a command line argument by applying the `AliasAttribute` to the property or constructor parameter that defines the argument.

For example, the following code defines a switch argument that can be specified using either the name “Verbose” or the alias “v”:

```csharp
[CommandLineArgument, Alias("v")]
public bool Verbose { get; set; }
```

To specify more than one alias for an argument, simply apply the `AliasAttribute` multiple times.
