namespace Sharp;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = true)]
public sealed class JitGenericAttribute(params Type[] types) : Attribute
{
    public IReadOnlyList<Type> Types { get; } = types;
}
