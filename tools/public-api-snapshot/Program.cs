using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using System.Runtime.Loader;
using System.Text;
using Icod.TermInfo;

namespace Icod.TermInfo.PublicApiSnapshot;

internal static class Program
{
    private const string DefaultBaselineRelativePath =
        "docs/1.0.0-PUBLIC-API-BASELINE.txt";

    private static readonly NullabilityInfoContext Nullability =
        new();

    public static int Main(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        if (args.Length >= 1
            && string.Equals(
                args[0],
                "--compare",
                StringComparison.Ordinal))
        {
            return CompareAssemblies(
                args);
        }

        if (args.Length > 3)
        {
            PrintUsage();
            return 2;
        }

        if (args.Length == 0)
        {
            string currentManifest =
                CreateManifest(
                    typeof(TerminalDescription).Assembly);
            Console.Write(currentManifest);
            return 0;
        }

        if (!string.Equals(
                args[0],
                "--write",
                StringComparison.Ordinal)
            && !string.Equals(
                args[0],
                "--check",
                StringComparison.Ordinal))
        {
            PrintUsage();
            return 2;
        }

        string manifest;
        if (args.Length == 3)
        {
            string assemblyPath =
                Path.GetFullPath(
                    args[2]);
            if (!File.Exists(assemblyPath))
            {
                Console.Error.WriteLine(
                    $"Assembly not found: {assemblyPath}");
                return 1;
            }

            manifest =
                CreateManifestFromAssemblyPath(
                    assemblyPath);
        }
        else
        {
            manifest =
                CreateManifest(
                    typeof(TerminalDescription).Assembly);
        }

        string path =
            args.Length >= 2
                ? Path.GetFullPath(args[1])
                : Path.Combine(
                    FindRepositoryRoot(),
                    DefaultBaselineRelativePath.Replace(
                        '/',
                        Path.DirectorySeparatorChar));

        if (string.Equals(
                args[0],
                "--write",
                StringComparison.Ordinal))
        {
            string? directory =
                Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(
                path,
                manifest,
                new UTF8Encoding(
                    encoderShouldEmitUTF8Identifier: false));

            Console.WriteLine(
                $"Wrote {path}");
            Console.WriteLine(
                $"SHA-256 {ComputeSha256(manifest)}");
            return 0;
        }

        if (!File.Exists(path))
        {
            Console.Error.WriteLine(
                $"Public API baseline not found: {path}");
            Console.Error.WriteLine(
                "Run this tool with --write, review the complete manifest, "
                + "and commit the approved baseline.");
            return 1;
        }

        string expected =
            NormalizeLineEndings(
                File.ReadAllText(path));

        if (!string.Equals(
                expected,
                manifest,
                StringComparison.Ordinal))
        {
            Console.Error.WriteLine(
                "Public API differs from the approved baseline.");
            Console.Error.WriteLine(
                $"Expected SHA-256 {ComputeSha256(expected)}");
            Console.Error.WriteLine(
                $"Actual   SHA-256 {ComputeSha256(manifest)}");
            return 1;
        }

        Console.WriteLine(
            $"Public API matches {path}");
        Console.WriteLine(
            $"SHA-256 {ComputeSha256(manifest)}");
        return 0;
    }

    private static int CompareAssemblies(
        string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        if (args.Length != 3)
        {
            PrintUsage();
            return 2;
        }

        string firstPath =
            Path.GetFullPath(
                args[1]);
        string secondPath =
            Path.GetFullPath(
                args[2]);

        if (!File.Exists(firstPath)
            || !File.Exists(secondPath))
        {
            Console.Error.WriteLine(
                "Both assembly paths supplied to --compare must exist.");
            return 1;
        }

        string first =
            CreateManifestFromAssemblyPath(
                firstPath);
        string second =
            CreateManifestFromAssemblyPath(
                secondPath);

        if (!string.Equals(
                first,
                second,
                StringComparison.Ordinal))
        {
            Console.Error.WriteLine(
                "Public API differs between target-framework assemblies.");
            Console.Error.WriteLine(
                $"{firstPath}: {ComputeSha256(first)}");
            Console.Error.WriteLine(
                $"{secondPath}: {ComputeSha256(second)}");
            return 1;
        }

        Console.WriteLine(
            "Public API is equivalent between:");
        Console.WriteLine(
            $"  {firstPath}");
        Console.WriteLine(
            $"  {secondPath}");
        Console.WriteLine(
            $"SHA-256 {ComputeSha256(first)}");
        return 0;
    }

    private static string CreateManifestFromAssemblyPath(
        string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            path);

        SnapshotAssemblyLoadContext context =
            new();

        try
        {
            Assembly assembly =
                context.LoadFromAssemblyPath(
                    Path.GetFullPath(path));

            return CreateManifest(
                assembly);
        }
        finally
        {
            context.Unload();
        }
    }

    private static string CreateManifest(
        Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        AssemblyName assemblyName =
            assembly.GetName();
        string simpleName =
            assemblyName.Name
            ?? throw new InvalidOperationException(
                "The assembly does not have a simple name.");
        Version assemblyVersion =
            assemblyName.Version
            ?? throw new InvalidOperationException(
                "The assembly does not have an assembly version.");

        StringBuilder builder = new();
        builder.AppendLine(
            $"# {simpleName} public API baseline");
        builder.AppendLine(
            "# Format: Icod.TermInfo.PublicApiSnapshot/v1");
        builder.AppendLine(
            $"# AssemblyVersion: {assemblyVersion}");
        builder.AppendLine();

        foreach (
            Type type
            in assembly
                .GetExportedTypes()
                .OrderBy(
                    item => FormatType(item),
                    StringComparer.Ordinal))
        {
            AppendType(
                builder,
                type);
        }

        return NormalizeLineEndings(
            builder.ToString());
    }

    private static void AppendType(
        StringBuilder builder,
        Type type)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(type);

        builder.Append("TYPE ");
        builder.Append(GetTypeKind(type));
        builder.Append(' ');
        builder.Append(FormatType(type));
        builder.Append(" [");
        builder.Append(FormatTypeModifiers(type));
        builder.AppendLine("]");

        if (type.BaseType is not null
            && type.BaseType != typeof(object)
            && !type.IsEnum)
        {
            builder.Append("  BASE ");
            builder.AppendLine(
                FormatType(type.BaseType));
        }

        string[] interfaces =
            type.GetInterfaces()
                .Select(FormatType)
                .OrderBy(
                    item => item,
                    StringComparer.Ordinal)
                .ToArray();
        if (interfaces.Length != 0)
        {
            builder.Append("  INTERFACES ");
            builder.AppendLine(
                string.Join(
                    ", ",
                    interfaces));
        }

        AppendGenericParameters(
            builder,
            type.IsGenericTypeDefinition
                ? type.GetGenericArguments()
                : Array.Empty<Type>(),
            "  ");

        if (type.IsEnum)
        {
            builder.Append("  UNDERLYING ");
            builder.AppendLine(
                FormatType(
                    Enum.GetUnderlyingType(type)));

            foreach (
                FieldInfo field
                in type
                    .GetFields(
                        BindingFlags.Public
                        | BindingFlags.Static
                        | BindingFlags.DeclaredOnly)
                    .OrderBy(
                        item => item.Name,
                        StringComparer.Ordinal))
            {
                builder.Append("  ENUM ");
                builder.Append(field.Name);
                builder.Append(" = ");
                builder.AppendLine(
                    FormatConstant(
                        field.GetRawConstantValue()));
            }
        }
        else
        {
            foreach (
                FieldInfo field
                in type
                    .GetFields(
                        BindingFlags.Public
                        | BindingFlags.Instance
                        | BindingFlags.Static
                        | BindingFlags.DeclaredOnly)
                    .OrderBy(
                        FormatField,
                        StringComparer.Ordinal))
            {
                builder.Append("  FIELD ");
                builder.AppendLine(
                    FormatField(field));
            }
        }

        foreach (
            ConstructorInfo constructor
            in type
                .GetConstructors(
                    BindingFlags.Public
                    | BindingFlags.NonPublic
                    | BindingFlags.Instance
                    | BindingFlags.DeclaredOnly)
                .Where(
                    constructor =>
                        IsApiVisible(constructor))
                .OrderBy(
                    FormatConstructor,
                    StringComparer.Ordinal))
        {
            builder.Append("  CTOR ");
            builder.AppendLine(
                FormatConstructor(constructor));
        }

        foreach (
            PropertyInfo property
            in type
                .GetProperties(
                    BindingFlags.Public
                    | BindingFlags.NonPublic
                    | BindingFlags.Instance
                    | BindingFlags.Static
                    | BindingFlags.DeclaredOnly)
                .Where(
                    IsApiVisible)
                .OrderBy(
                    FormatProperty,
                    StringComparer.Ordinal))
        {
            builder.Append("  PROPERTY ");
            builder.AppendLine(
                FormatProperty(property));
        }

        foreach (
            EventInfo eventInfo
            in type
                .GetEvents(
                    BindingFlags.Public
                    | BindingFlags.NonPublic
                    | BindingFlags.Instance
                    | BindingFlags.Static
                    | BindingFlags.DeclaredOnly)
                .Where(
                    IsApiVisible)
                .OrderBy(
                    FormatEvent,
                    StringComparer.Ordinal))
        {
            builder.Append("  EVENT ");
            builder.AppendLine(
                FormatEvent(eventInfo));
        }

        foreach (
            MethodInfo method
            in type
                .GetMethods(
                    BindingFlags.Public
                    | BindingFlags.NonPublic
                    | BindingFlags.Instance
                    | BindingFlags.Static
                    | BindingFlags.DeclaredOnly)
                .Where(
                    method =>
                        IsApiVisible(method)
                        && !IsAccessor(method))
                .OrderBy(
                    FormatMethod,
                    StringComparer.Ordinal))
        {
            builder.Append("  METHOD ");
            builder.AppendLine(
                FormatMethod(method));

            AppendGenericParameters(
                builder,
                method.IsGenericMethodDefinition
                    ? method.GetGenericArguments()
                    : Array.Empty<Type>(),
                "    ");
        }

        builder.AppendLine("END");
        builder.AppendLine();
    }

    private static string GetTypeKind(
        Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        if (type.IsEnum)
        {
            return "enum";
        }

        if (type.IsInterface)
        {
            return "interface";
        }

        if (type.IsValueType)
        {
            return "struct";
        }

        return "class";
    }

    private static string FormatTypeModifiers(
        Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        List<string> modifiers = [];

        if (type.IsAbstract
            && type.IsSealed)
        {
            modifiers.Add("static");
        }
        else
        {
            if (type.IsAbstract)
            {
                modifiers.Add("abstract");
            }

            if (type.IsSealed)
            {
                modifiers.Add("sealed");
            }
        }

        if (type.IsByRefLike)
        {
            modifiers.Add("ref-like");
        }

        if (type.CustomAttributes.Any(
                attribute =>
                    attribute.AttributeType.FullName
                        == "System.Runtime.CompilerServices.IsReadOnlyAttribute"))
        {
            modifiers.Add("readonly");
        }

        string attributes =
            FormatRelevantAttributes(
                type.CustomAttributes);
        if (!string.IsNullOrEmpty(attributes))
        {
            modifiers.Add(attributes);
        }

        return modifiers.Count == 0
            ? "-"
            : string.Join(
                ",",
                modifiers);
    }

    private static string FormatField(
        FieldInfo field)
    {
        ArgumentNullException.ThrowIfNull(field);

        List<string> modifiers =
        [
            FormatAccessibility(field),
        ];

        if (field.IsStatic)
        {
            modifiers.Add("static");
        }

        if (field.IsLiteral)
        {
            modifiers.Add("const");
        }
        else if (field.IsInitOnly)
        {
            modifiers.Add("readonly");
        }

        string result =
            $"{string.Join(" ", modifiers)} "
            + $"{FormatType(field.FieldType)} "
            + $"{field.Name}"
            + $" null={FormatNullability(Nullability.Create(field))}";

        if (field.IsLiteral)
        {
            result +=
                $" value={FormatConstant(field.GetRawConstantValue())}";
        }

        string attributes =
            FormatRelevantAttributes(
                field.CustomAttributes);
        if (!string.IsNullOrEmpty(attributes))
        {
            result +=
                $" attrs={attributes}";
        }

        return result.TrimStart();
    }

    private static string FormatConstructor(
        ConstructorInfo constructor)
    {
        ArgumentNullException.ThrowIfNull(constructor);

        return
            $"{FormatAccessibility(constructor)} "
            + $"{constructor.DeclaringType!.Name}"
            + $"({FormatParameters(constructor.GetParameters())})";
    }

    private static string FormatProperty(
        PropertyInfo property)
    {
        ArgumentNullException.ThrowIfNull(property);

        MethodInfo? getter =
            property.GetGetMethod(
                nonPublic: true);
        MethodInfo? setter =
            property.GetSetMethod(
                nonPublic: true);
        bool isStatic =
            getter?.IsStatic
            ?? setter?.IsStatic
            ?? false;

        List<string> accessors = [];

        if (getter is not null
            && IsApiVisible(getter))
        {
            accessors.Add(
                $"{FormatAccessibility(getter)} get;");
        }

        if (setter is not null
            && IsApiVisible(setter))
        {
            string setterText =
                $"{FormatAccessibility(setter)} set;";

            Type[] requiredModifiers =
                setter.ReturnParameter.GetRequiredCustomModifiers();
            if (requiredModifiers.Any(
                    modifier =>
                        modifier.FullName
                            == "System.Runtime.CompilerServices.IsExternalInit"))
            {
                setterText =
                    $"{FormatAccessibility(setter)} init;";
            }

            accessors.Add(
                setterText);
        }

        ParameterInfo[] indexParameters =
            property.GetIndexParameters();
        string index =
            indexParameters.Length == 0
                ? string.Empty
                : $"[{FormatParameters(indexParameters)}]";

        string attributes =
            FormatRelevantAttributes(
                property.CustomAttributes);

        return
            $"{(isStatic ? "static " : string.Empty)}"
            + $"{FormatType(property.PropertyType)} "
            + $"{property.Name}{index} "
            + $"{{ {string.Join(" ", accessors)} }} "
            + $"null={FormatNullability(Nullability.Create(property))}"
            + (string.IsNullOrEmpty(attributes)
                ? string.Empty
                : $" attrs={attributes}");
    }

    private static string FormatEvent(
        EventInfo eventInfo)
    {
        ArgumentNullException.ThrowIfNull(eventInfo);

        MethodInfo? addMethod =
            eventInfo.GetAddMethod(
                nonPublic: true);
        MethodInfo? removeMethod =
            eventInfo.GetRemoveMethod(
                nonPublic: true);

        List<string> accessors = [];

        if (addMethod is not null
            && IsApiVisible(addMethod))
        {
            accessors.Add(
                $"{FormatAccessibility(addMethod)} add;");
        }

        if (removeMethod is not null
            && IsApiVisible(removeMethod))
        {
            accessors.Add(
                $"{FormatAccessibility(removeMethod)} remove;");
        }

        string attributes =
            FormatRelevantAttributes(
                eventInfo.CustomAttributes);

        return
            $"{(addMethod?.IsStatic == true ? "static " : string.Empty)}"
            + $"{FormatType(eventInfo.EventHandlerType!)} "
            + $"{eventInfo.Name} "
            + $"{{ {string.Join(" ", accessors)} }} "
            + $"null={FormatNullability(Nullability.Create(eventInfo))}"
            + (string.IsNullOrEmpty(attributes)
                ? string.Empty
                : $" attrs={attributes}");
    }

    private static string FormatMethod(
        MethodInfo method)
    {
        ArgumentNullException.ThrowIfNull(method);

        List<string> modifiers =
        [
            FormatAccessibility(method),
        ];

        if (method.IsStatic)
        {
            modifiers.Add("static");
        }

        if (method.IsAbstract)
        {
            modifiers.Add("abstract");
        }
        else if (method.IsVirtual
            && !method.IsFinal)
        {
            modifiers.Add("virtual");
        }

        string methodName =
            method.Name;
        if (method.IsGenericMethodDefinition)
        {
            methodName +=
                "<"
                + string.Join(
                    ",",
                    method.GetGenericArguments()
                        .Select(argument => argument.Name))
                + ">";
        }

        string attributes =
            FormatRelevantAttributes(
                method.ReturnParameter.CustomAttributes);

        string result =
            $"{string.Join(" ", modifiers)} "
            + $"{FormatType(method.ReturnType)} "
            + $"{methodName}"
            + $"({FormatParameters(method.GetParameters())}) "
            + $"return-null={FormatNullability(Nullability.Create(method.ReturnParameter))}"
            + (string.IsNullOrEmpty(attributes)
                ? string.Empty
                : $" return-attrs={attributes}");

        return result.TrimStart();
    }

    private static string FormatParameters(
        IReadOnlyList<ParameterInfo> parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        return string.Join(
            ", ",
            parameters.Select(
                FormatParameter));
    }

    private static string FormatParameter(
        ParameterInfo parameter)
    {
        ArgumentNullException.ThrowIfNull(parameter);

        Type parameterType =
            parameter.ParameterType;
        string modifier = string.Empty;

        if (parameter.GetCustomAttribute<ParamArrayAttribute>() is not null)
        {
            modifier = "params ";
        }
        else if (parameterType.IsByRef)
        {
            if (parameter.IsOut)
            {
                modifier = "out ";
            }
            else if (parameter.IsIn)
            {
                modifier = "in ";
            }
            else
            {
                modifier = "ref ";
            }

            parameterType =
                parameterType.GetElementType()!;
        }

        string result =
            $"{modifier}{FormatType(parameterType)} {parameter.Name}"
            + $" null={FormatNullability(Nullability.Create(parameter))}";

        if (parameter.HasDefaultValue)
        {
            result +=
                $" default={FormatConstant(parameter.DefaultValue)}";
        }

        string attributes =
            FormatRelevantAttributes(
                parameter.CustomAttributes);
        if (!string.IsNullOrEmpty(attributes))
        {
            result +=
                $" attrs={attributes}";
        }

        return result;
    }

    private static void AppendGenericParameters(
        StringBuilder builder,
        IReadOnlyList<Type> genericParameters,
        string indent)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(genericParameters);
        ArgumentNullException.ThrowIfNull(indent);

        foreach (Type parameter in genericParameters)
        {
            List<string> constraints = [];
            GenericParameterAttributes attributes =
                parameter.GenericParameterAttributes;

            if ((attributes
                & GenericParameterAttributes.ReferenceTypeConstraint) != 0)
            {
                constraints.Add("class");
            }

            if ((attributes
                & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0)
            {
                constraints.Add("struct");
            }

            foreach (
                Type constraint
                in parameter
                    .GetGenericParameterConstraints()
                    .OrderBy(
                        FormatType,
                        StringComparer.Ordinal))
            {
                if (constraint != typeof(ValueType))
                {
                    constraints.Add(
                        FormatType(constraint));
                }
            }

            if ((attributes
                & GenericParameterAttributes.DefaultConstructorConstraint) != 0)
            {
                constraints.Add("new()");
            }

            builder.Append(indent);
            builder.Append("GENERIC ");

            GenericParameterAttributes variance =
                attributes
                & GenericParameterAttributes.VarianceMask;
            if (variance
                == GenericParameterAttributes.Covariant)
            {
                builder.Append("out ");
            }
            else if (variance
                == GenericParameterAttributes.Contravariant)
            {
                builder.Append("in ");
            }

            builder.Append(parameter.Name);
            builder.Append(" : ");
            builder.AppendLine(
                constraints.Count == 0
                    ? "-"
                    : string.Join(
                        ", ",
                        constraints));
        }
    }

    private static string FormatType(
        Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        if (type.IsByRef)
        {
            return FormatType(
                type.GetElementType()!);
        }

        if (type.IsPointer)
        {
            return
                FormatType(
                    type.GetElementType()!)
                + "*";
        }

        if (type.IsArray)
        {
            int rank =
                type.GetArrayRank();
            return
                FormatType(
                    type.GetElementType()!)
                + "["
                + new string(
                    ',',
                    rank - 1)
                + "]";
        }

        if (type.IsGenericParameter)
        {
            return type.Name;
        }

        if (type.IsGenericType)
        {
            Type definition =
                type.GetGenericTypeDefinition();
            string name =
                definition.FullName
                ?? definition.Name;
            int tick =
                name.IndexOf(
                    '`');
            if (tick >= 0)
            {
                name =
                    name[..tick];
            }

            return
                name.Replace(
                    '+',
                    '.')
                + "<"
                + string.Join(
                    ",",
                    type.GetGenericArguments()
                        .Select(FormatType))
                + ">";
        }

        return
            (type.FullName
                ?? type.Name)
            .Replace(
                '+',
                '.');
    }

    private static string FormatNullability(
        NullabilityInfo info)
    {
        ArgumentNullException.ThrowIfNull(info);

        StringBuilder builder = new();
        builder.Append(
            FormatNullabilityState(
                info.ReadState));
        builder.Append('/');
        builder.Append(
            FormatNullabilityState(
                info.WriteState));

        if (info.ElementType is not null)
        {
            builder.Append('<');
            builder.Append(
                FormatNullability(
                    info.ElementType));
            builder.Append('>');
        }

        if (info.GenericTypeArguments.Length != 0)
        {
            builder.Append('<');
            builder.Append(
                string.Join(
                    ",",
                    info.GenericTypeArguments
                        .Select(FormatNullability)));
            builder.Append('>');
        }

        return builder.ToString();
    }

    private static string FormatNullabilityState(
        NullabilityState state)
    {
        return state switch
        {
            NullabilityState.NotNull => "not-null",
            NullabilityState.Nullable => "nullable",
            _ => "unknown",
        };
    }

    private static string FormatRelevantAttributes(
        IEnumerable<CustomAttributeData> attributes)
    {
        ArgumentNullException.ThrowIfNull(attributes);

        string[] relevant =
            attributes
                .Where(
                    attribute =>
                        attribute.AttributeType.Namespace
                            == "System.Diagnostics.CodeAnalysis"
                        || attribute.AttributeType == typeof(FlagsAttribute)
                        || attribute.AttributeType == typeof(ObsoleteAttribute))
                .Select(FormatAttribute)
                .OrderBy(
                    value => value,
                    StringComparer.Ordinal)
                .ToArray();

        return string.Join(
            ",",
            relevant);
    }

    private static string FormatAttribute(
        CustomAttributeData attribute)
    {
        ArgumentNullException.ThrowIfNull(attribute);

        string arguments =
            string.Join(
                ",",
                attribute.ConstructorArguments
                    .Select(
                        argument =>
                            FormatConstant(
                                argument.Value)));

        return
            attribute.AttributeType.Name
            + "("
            + arguments
            + ")";
    }

    private static string FormatConstant(
        object? value)
    {
        return value switch
        {
            null => "null",
            string text =>
                "\""
                + text
                    .Replace("\\", "\\\\", StringComparison.Ordinal)
                    .Replace("\"", "\\\"", StringComparison.Ordinal)
                + "\"",
            char character =>
                "'"
                + character.ToString()
                + "'",
            bool boolean =>
                boolean
                    ? "true"
                    : "false",
            IFormattable formattable =>
                formattable.ToString(
                    null,
                    CultureInfo.InvariantCulture)
                ?? string.Empty,
            _ =>
                value.ToString()
                ?? string.Empty,
        };
    }

    private static bool IsApiVisible(
        FieldInfo field)
    {
        ArgumentNullException.ThrowIfNull(field);

        return field.IsPublic
            || field.IsFamily
            || field.IsFamilyOrAssembly;
    }

    private static bool IsApiVisible(
        MethodBase method)
    {
        ArgumentNullException.ThrowIfNull(method);

        return method.IsPublic
            || method.IsFamily
            || method.IsFamilyOrAssembly;
    }

    private static bool IsApiVisible(
        PropertyInfo property)
    {
        ArgumentNullException.ThrowIfNull(property);

        MethodInfo? getter =
            property.GetGetMethod(
                nonPublic: true);
        MethodInfo? setter =
            property.GetSetMethod(
                nonPublic: true);

        return (getter is not null
                && IsApiVisible(getter))
            || (setter is not null
                && IsApiVisible(setter));
    }

    private static bool IsApiVisible(
        EventInfo eventInfo)
    {
        ArgumentNullException.ThrowIfNull(eventInfo);

        MethodInfo? addMethod =
            eventInfo.GetAddMethod(
                nonPublic: true);
        MethodInfo? removeMethod =
            eventInfo.GetRemoveMethod(
                nonPublic: true);

        return (addMethod is not null
                && IsApiVisible(addMethod))
            || (removeMethod is not null
                && IsApiVisible(removeMethod));
    }

    private static string FormatAccessibility(
        FieldInfo field)
    {
        ArgumentNullException.ThrowIfNull(field);

        if (field.IsPublic)
        {
            return "public";
        }

        if (field.IsFamilyOrAssembly)
        {
            return "protected-internal";
        }

        if (field.IsFamily)
        {
            return "protected";
        }

        throw new InvalidOperationException(
            $"Field '{field.Name}' is not externally visible.");
    }

    private static string FormatAccessibility(
        MethodBase method)
    {
        ArgumentNullException.ThrowIfNull(method);

        if (method.IsPublic)
        {
            return "public";
        }

        if (method.IsFamilyOrAssembly)
        {
            return "protected-internal";
        }

        if (method.IsFamily)
        {
            return "protected";
        }

        throw new InvalidOperationException(
            $"Method '{method.Name}' is not externally visible.");
    }

    private static bool IsAccessor(
        MethodInfo method)
    {
        ArgumentNullException.ThrowIfNull(method);

        if (!method.IsSpecialName)
        {
            return false;
        }

        return method.Name.StartsWith("get_", StringComparison.Ordinal)
            || method.Name.StartsWith("set_", StringComparison.Ordinal)
            || method.Name.StartsWith("add_", StringComparison.Ordinal)
            || method.Name.StartsWith("remove_", StringComparison.Ordinal);
    }

    private static string NormalizeLineEndings(
        string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        string normalized =
            value
                .Replace(
                    "\r\n",
                    "\n",
                    StringComparison.Ordinal)
                .Replace(
                    '\r',
                    '\n');

        return normalized.TrimEnd(
                '\n')
            + "\n";
    }

    private static string ComputeSha256(
        string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        byte[] digest =
            SHA256.HashData(
                Encoding.UTF8.GetBytes(value));

        return Convert.ToHexString(
            digest)
            .ToLowerInvariant();
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? current =
            new(
                Directory.GetCurrentDirectory());

        while (current is not null)
        {
            if (File.Exists(
                    Path.Combine(
                        current.FullName,
                        "Icod.TermInfo.csproj")))
            {
                return current.FullName;
            }

            current =
                current.Parent;
        }

        throw new InvalidOperationException(
            "Unable to locate the Icod.TermInfo repository root.");
    }

    private static void PrintUsage()
    {
        Console.Error.WriteLine(
            "Usage:");
        Console.Error.WriteLine(
            "  public-api-snapshot");
        Console.Error.WriteLine(
            "  public-api-snapshot --write [baseline-path]");
        Console.Error.WriteLine(
            "  public-api-snapshot --check [baseline-path]");
        Console.Error.WriteLine(
            "  public-api-snapshot --write <baseline-path> <assembly-path>");
        Console.Error.WriteLine(
            "  public-api-snapshot --check <baseline-path> <assembly-path>");
        Console.Error.WriteLine(
            "  public-api-snapshot --compare <assembly-a> <assembly-b>");
    }

    private sealed class SnapshotAssemblyLoadContext
        : AssemblyLoadContext
    {
        internal SnapshotAssemblyLoadContext()
            : base(
                isCollectible: true)
        {
        }

        protected override Assembly? Load(
            AssemblyName assemblyName)
        {
            ArgumentNullException.ThrowIfNull(
                assemblyName);

            return null;
        }
    }
}
