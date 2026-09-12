using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

[Serializable]
public class LLMToolRegistry
{
    [SerializeReference]
    object owner;

    [SerializeField]
    string agentRole = "";

    [NonSerialized]
    HashSet<LLMToolFunction> functionRegistry;

    public LLMToolRegistry(object owner, string agentRole)
    {
        this.owner = owner ?? throw new ArgumentException("Missing owner for the agent LLM registration tool");
        this.agentRole = agentRole;
    }

    public IEnumerable<LLMToolFunction> GetMethods()
    {
        RegisterMethods();
        return functionRegistry;
    }

    private void RegisterMethods()
    {
        functionRegistry = new();
        IEnumerable<MethodInfo> methods = owner.GetType().GetMethods().Where(m => m.GetCustomAttributes(typeof(LLMTool), false).Length > 0);
        foreach (MethodInfo method in methods)
        {
            LLMTool toolDefinition = method.GetCustomAttribute<LLMTool>();
            if (toolDefinition.agentRole.Length == 0 || toolDefinition.agentRole.Contains(agentRole))
            {
                try { functionRegistry.Add(LLMToolFunction.FromMethodInfo(method)); }
                catch (Exception) { }
            }
        }
    }

    /// <summary>
    /// Stores the functions  as
    /// </summary>
    public class LLMToolFunction
    {
        HashSet<LLMToolParameter> Parameters = new();
        MethodInfo reference;
        string name;
        string description;

        private LLMToolFunction(MethodInfo methodInfo)
        {
            this.reference = methodInfo;
            this.name = methodInfo.Name;
            this.description = methodInfo.GetCustomAttribute<LLMTool>().description;
        }

        public static LLMToolFunction FromMethodInfo(MethodInfo method)
        {
            LLMToolFunction function = new(method);
            foreach (ParameterInfo info in method.GetParameters())
            {
                function.AddParameter(LLMToolParameter.FromParameterInfo(info));
            }
            return function;
        }

        private void AddParameter(LLMToolParameter parameter)
        {
            this.Parameters.Add(parameter);
        }
        
    }

    public class LLMToolParameter
    {
        string name;
        string description;
        Type type;

        private LLMToolParameter(string name, string description, Type type)
        {
            this.name = name;
            this.description = description;
            this.type = type;
        }

        public static LLMToolParameter FromParameterInfo(ParameterInfo parameterInfo)
        {
            LLMParameter info = parameterInfo.GetCustomAttribute<LLMParameter>() ?? throw new ArgumentException($"{parameterInfo.Name} is missing the attribute LLMParameter");
            return new LLMToolParameter(parameterInfo.Name, info.description, parameterInfo.ParameterType);
        }
    }
}

