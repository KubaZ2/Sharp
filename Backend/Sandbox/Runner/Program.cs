using System.Reflection;

using Sharp.Backend.Sandbox.Shared;

var assembly = Utils.LoadAssembly();

var entryPoint = assembly.EntryPoint!;
var methodParameters = entryPoint.GetParameters();

object?[]? parameters = methodParameters.Length is 0 ? null : [(string[])[]];

entryPoint.Invoke(null, BindingFlags.DoNotWrapExceptions, null, parameters, null);
