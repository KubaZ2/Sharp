using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.CompilerServices;

using Sharp;
using Sharp.Backend.Sandbox.Shared;

var assembly = Utils.LoadAssembly();

const BindingFlags Flags = BindingFlags.DeclaredOnly
                           | BindingFlags.Instance
                           | BindingFlags.Static
                           | BindingFlags.Public
                           | BindingFlags.NonPublic;

foreach (var type in assembly.DefinedTypes)
{
    if (type.IsNested)
        continue;

    PrepareType(type, []);
}

static void PrepareType(Type type, ImmutableList<RuntimeTypeHandle> baseGenericTypes)
{
    if (type.IsGenericType)
    {
        foreach (var jitGenericAttribute in type.GetCustomAttributes<JitGenericAttribute>())
            PrepareTypeMembers(type, baseGenericTypes.AddRange(jitGenericAttribute.Types.Select(t => t.TypeHandle)));
    }
    else
        PrepareTypeMembers(type, baseGenericTypes);
}

static void PrepareMethod(MethodBase method, ImmutableList<RuntimeTypeHandle> genericTypes)
{
    if (method.IsGenericMethod)
    {
        foreach (var jitGenericAttribute in method.GetCustomAttributes<JitGenericAttribute>())
        {
            try
            {
                RuntimeHelpers.PrepareMethod(method.MethodHandle, [.. genericTypes, .. jitGenericAttribute.Types.Select(t => t.TypeHandle)]);
            }
            catch
            {
            }
        }
    }
    else
    {
        try
        {
            RuntimeHelpers.PrepareMethod(method.MethodHandle, [.. genericTypes]);
        }
        catch
        {
        }
    }
}

static void PrepareTypeMembers(Type type, ImmutableList<RuntimeTypeHandle> genericTypes)
{
    foreach (var member in type.GetMembers(Flags))
    {
        if (member is MethodBase method)
        {
            if (method.IsAbstract)
                continue;

            PrepareMethod(method, genericTypes);
        }
        else if (member is Type nestedType)
            PrepareType(nestedType, genericTypes);
    }
}
