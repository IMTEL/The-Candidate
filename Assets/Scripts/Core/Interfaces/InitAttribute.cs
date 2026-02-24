using System;

/// <summary>
/// Attribute for marking a function as the init function. The parameters will be auto imported if it can be found within the bootstrap
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class InitAttribute : Attribute
{
    
}
