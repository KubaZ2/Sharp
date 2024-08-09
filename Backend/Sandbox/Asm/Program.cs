using System.Reflection;
using System.Runtime.CompilerServices;

using Sharp.Backend.Sandbox.Shared;

var assembly = Utils.LoadAssembly();

foreach (var type in assembly.DefinedTypes)
{
    const BindingFlags Flags = BindingFlags.DeclaredOnly
                               | BindingFlags.Instance
                               | BindingFlags.Static
                               | BindingFlags.Public
                               | BindingFlags.NonPublic;

    foreach (var method in type.GetMembers(Flags).OfType<MethodBase>())
        RuntimeHelpers.PrepareMethod(method.MethodHandle);
}
