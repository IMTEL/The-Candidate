using System;

/// <summary>
/// Mark a methods as a LLMMethod allowing to be called from a Agent
/// </summary>
public class LLMTool : Attribute
{
    public LLMTool(string description)
    {
        this.description = description;
    }
    /// <summary>
    /// What agent roles are allowed to use this method
    /// </summary>
    public string[] agentRole = new string[10];

    /// <summary>
    /// A description of a method. Should contain:
    /// 1. What it does
    /// 2. When it should be called
    /// </summary>
    public string description = "";
}

/// <summary>
/// The parameter used for the method
/// </summary>
public class LLMParameter : Attribute
{

    /// <summary>
    /// Description of the parameter
    /// </summary>
    public string description = "";

    /// <summary>
    /// Used on the parameters on a LLMTool
    /// </summary>
    /// <param name="description">a description of a value of the method</param>
    LLMParameter(string description)
    {
        this.description = description;
    }

}