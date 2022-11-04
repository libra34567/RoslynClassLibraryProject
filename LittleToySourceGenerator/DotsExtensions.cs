using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace LittleToySourceGenerator;

internal static class DotsExtensions
{

    private static Dictionary<string, string> _systemToDotsnetTypeDictionary = new Dictionary<string, string>()
    {
        { "System.Byte", "Byte"},
        { "System.Boolean", "Bool"},
        { "System.Int16", "Short"},
        { "System.UInt16", "UShort"},
        { "System.Int32", "Int"},
        { "System.UInt32", "UInt"},
        { "Unity.Mathematics.int2", "Int2" },
        { "Unity.Mathematics.int3", "Int3" },
        { "Unity.Mathematics.int4", "Int4" },
        { "System.UInt64", "ULong" },
        { "System.Int64", "Long" },
        { "DOTSNET.long3", "Long3" },
        { "System.Single", "Float" },
        { "Unity.Mathematics.float2", "Float2" },
        { "Unity.Mathematics.float3", "Float3" },
        { "Unity.Mathematics.float4", "Float4" },
        { "System.Double", "Double"},
        { "Unity.Mathematics.double2", "Double2" },
        { "Unity.Mathematics.double3", "Double3" },
        { "Unity.Mathematics.double4", "Double4" },
        { "System.Decimal", "Decimal"},
        { "Unity.Mathematics.quaternion", "Quaternion"},
        { "Unity.Collections.FixedBytes16", "Bytes16"},
        { "Unity.Collections.FixedBytes30", "Bytes30"},
        { "Unity.Collections.FixedBytes62", "Bytes62"},
        { "Unity.Collections.FixedBytes126", "Bytes126"},
        { "Unity.Collections.FixedBytes510", "Bytes510"},
        { "Unity.Collections.FixedBytes4094", "Bytes4094"},
        { "Unity.Collections.FixedString32Bytes", "FixedString32"},
        { "Unity.Collections.FixedString64Bytes", "FixedString64"},
        { "Unity.Collections.FixedString128Bytes", "FixedString128"},
        { "Unity.Collections.FixedString512Bytes", "FixedString512"},
        { "Unity.Collections.FixedList32Bytes<T>", "FixedList32Batched"},
        { "Unity.Collections.FixedList64Bytes<T>", "FixedList64Batched"},
        { "Unity.Collections.FixedList128Bytes<T>", "FixedList128Batched"},
        { "Unity.Collections.FixedList512Bytes<T>", "FixedList512Batched"}
    };

    public static bool IsDotsnetCompatibleType(this ITypeSymbol typeSymbol)
    {
        if (typeSymbol is INamedTypeSymbol namedTypeSymbol)
        {
            if (typeSymbol.TypeKind == TypeKind.Enum)
            {
                typeSymbol = namedTypeSymbol.EnumUnderlyingType;
            }

            if (namedTypeSymbol.IsGenericType)
            {
                typeSymbol = namedTypeSymbol.ConstructedFrom;
            }
        }

        return IsDotsnetType(typeSymbol);
    }

    public static bool IsDotsnetType(this ITypeSymbol typeSymbol)
    {
        return _systemToDotsnetTypeDictionary.ContainsKey(GetFullyQualifiedName(typeSymbol));
    }

    public static string GetDotsnetTypeName(this ITypeSymbol typeSymbol)
    {
        if (typeSymbol is INamedTypeSymbol namedTypeSymbol)
        {
            if (typeSymbol.TypeKind == TypeKind.Enum)
            {
                typeSymbol = namedTypeSymbol.EnumUnderlyingType;
            }

            if (namedTypeSymbol.IsGenericType)
            {
                typeSymbol = namedTypeSymbol.ConstructedFrom;
            }
        }

        return _systemToDotsnetTypeDictionary[GetFullyQualifiedName(typeSymbol)];
    }

    public static ITypeSymbol GetDotsnetCompatibleType(this ITypeSymbol typeSymbol)
    {
        if (typeSymbol.TypeKind == TypeKind.Enum)
        {
            return ((INamedTypeSymbol)typeSymbol).EnumUnderlyingType;
        }

        return typeSymbol;
    }

    private static string GetFullyQualifiedName(ITypeSymbol typeSymbol)
    {
        return typeSymbol.SpecialType switch
        {
            SpecialType.System_Byte => "System.Byte",
            SpecialType.System_Boolean => "System.Boolean",
            SpecialType.System_SByte => "System.SByte",
            SpecialType.System_Single => "System.Single",
            SpecialType.System_Double => "System.Double",
            SpecialType.System_Decimal => "System.Decimal",
            SpecialType.System_Int16 => "System.Int16",
            SpecialType.System_Int32 => "System.Int32",
            SpecialType.System_Int64 => "System.Int64",
            SpecialType.System_UInt16 => "System.UInt16",
            SpecialType.System_UInt32 => "System.UInt32",
            SpecialType.System_UInt64 => "System.UInt64",
            _ => typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat).Replace("global::", string.Empty),
        };
    }
}
