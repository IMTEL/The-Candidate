using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using UnityEngine;

/// <summary>
/// This class is used to register methods that is meant to be used as a LLM
/// </summary>
[Serializable]
public class LLMToolRegistry
{
    [SerializeField]
    object owner;

    [SerializeField]
    string agentRole = "";

    [NonSerialized]
    HashSet<LLMToolFunction> functionRegistry;

    [NonSerialized]
    private static readonly Dictionary<Type, HashSet<LLMToolFunction>> classFunctionRegistry = new();

    [NonSerialized]
    readonly SemaphoreSlim requests = new(1);

    public LLMToolRegistry(object owner, string agentRole)
    {
        this.owner = owner ?? throw new ArgumentException("Missing owner for the agent LLM registration tool");
        this.agentRole = agentRole;
    }

    public IEnumerable<LLMToolFunction> GetMethods()
    {
        lock (requests)
        {
            if (functionRegistry == null) RegisterMethods();
        }

        return functionRegistry;
    }

    private void RegisterMethods()
    {
        if (!classFunctionRegistry.TryGetValue(owner.GetType(), out functionRegistry))
        {
            functionRegistry = new();
            IEnumerable<MethodInfo> methods = owner.GetType().GetMethods();
            foreach (MethodInfo method in methods)
            {
                LLMToolAttribute toolDefinition = method.GetCustomAttribute<LLMToolAttribute>();
                if (toolDefinition != null)
                {
                    try { functionRegistry.Add(LLMToolFunction.FromMethodInfo(method)); }
                    catch (Exception) { }
                }
            }
            classFunctionRegistry.Add(owner.GetType(), functionRegistry);
        }
        functionRegistry = functionRegistry.Where(function => function.IsAccessible(agentRole)).ToHashSet();
    }

    public bool IsMethodCallable(string name, Dictionary<string, string> parameters, out Action callable)
    {
        callable = null;
        LLMToolFunction function;
        try { function = GetMethods().First(llmtool => llmtool.GetName().Equals(name)); }
        catch (Exception) { return false; }
        if (function.CanBeCalled(parameters, out Action<object> method))
        {
            callable = () => { method.Invoke(owner); };
        }
        return callable != null;
    }

    /// <summary>
    /// Marks the functions 
    /// </summary>
    public class LLMToolFunction
    {
        readonly List<LLMToolParameter> Parameters = new();
        MethodInfo reference;
        string description;

        string[] roles;

        private LLMToolFunction(MethodInfo methodInfo)
        {
            LLMToolAttribute attribute = methodInfo.GetCustomAttribute<LLMToolAttribute>() ?? throw new ArgumentException("This method does not contain LLMTool");
            this.reference = methodInfo;
            this.description = attribute.description;
            this.roles = attribute.agentRole;
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

        public string GetName()
        {
            return reference.Name;
        }

        internal bool CanBeCalled(Dictionary<string, string> parameters, out Action<object> action)
        {
            object[] callParams = new object[Parameters.Count];
            action = null;
            for (int i = 0; i < Parameters.Count; i++)
            {
                LLMToolParameter item = Parameters[i];
                //If it does not exist and is mandatory
                if (!(parameters.TryGetValue(item.GetName(), out string value) || item.IsOptional()))
                { return false; }
                try
                {
                    callParams[i] = value == null ? Type.Missing : item.ConvertToType(value);
                }
                catch (Exception e) { Debug.Log(e.Message); return false; }
            }
            action = value => { reference.Invoke(value, callParams); };
            return true;
        }
        public string GetDescription()
        {
            return description;
        }
        public bool IsAccessible(string role)
        {
            return roles.Contains(role) || roles.Length == 0;
        }
    }

    public class LLMToolParameter
    {
        string description;
        ParameterInfo parameter;

        static readonly Type[] supportedTyping = new Type[]
        {
            typeof(string),
            typeof(float),
            typeof(int),
            typeof(bool)
        };

        private LLMToolParameter(string description, ParameterInfo parameter)
        {
            this.parameter = parameter;
            this.description = description;
        }

        public static LLMToolParameter FromParameterInfo(ParameterInfo parameterInfo)
        {
            if (!supportedTyping.Contains(parameterInfo.ParameterType)) throw new ArgumentException("This parameter type is not supported...yet");
            LLMParameterAttribute info = parameterInfo.GetCustomAttribute<LLMParameterAttribute>() ?? throw new ArgumentException($"{parameterInfo.Name} is missing the attribute LLMParameter");
            LLMToolParameter parameter = new(info.description, parameterInfo);
            return parameter;
        }

        public object ConvertToType(string text)
        {
            return text == null ? parameter.DefaultValue : Convert.ChangeType(text, GetParameterType());
        }
        public Type GetParameterType()
        {
            return parameter.ParameterType;
        }

        internal string GetName()
        {
            return parameter.Name;
        }

        internal bool IsOptional()
        {
            return parameter.IsOptional;
        }

        public string GetDescription()
        {
            return description;
        }
    }
}