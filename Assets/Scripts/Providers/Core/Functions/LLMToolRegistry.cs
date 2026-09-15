using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

/// <summary>
/// This class is used to register methods that is meant to be used as a LLM
/// </summary>
[Serializable]
public class LLMToolRegistry
{
    [SerializeReference]
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
        if (classFunctionRegistry.TryGetValue(owner.GetType(), out functionRegistry))
        {
            return;
        }
        functionRegistry = new();
        IEnumerable<MethodInfo> methods = owner.GetType().GetMethods().Where(m => m.GetCustomAttributes(typeof(LLMToolParameter), false).Length > 0);
        foreach (MethodInfo method in methods)
        {
            LLMToolAttribute toolDefinition = method.GetCustomAttribute<LLMToolAttribute>();
            if (toolDefinition.agentRole.Length == 0 || toolDefinition.agentRole.Contains(agentRole))
            {
                try { functionRegistry.Add(LLMToolFunction.FromMethodInfo(method)); }
                catch (Exception) { }
            }
        }
        classFunctionRegistry.Add(owner.GetType(), functionRegistry);
    }

    public bool IsMethodCallable(string name, Dictionary<string, string> parameters, out Action callable)
    {
        callable = null;
        LLMToolFunction function;
        try{function = functionRegistry.First(llmtool => llmtool.GetName().Equals(name));}
        catch(Exception){return false;}
        if(function.CanBeCalled(parameters, out Action<object> method))
        {
            callable = () => {method.Invoke(owner);};
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
        string name;
        string description;

        private LLMToolFunction(MethodInfo methodInfo)
        {
            this.reference = methodInfo;
            this.name = methodInfo.Name;
            this.description = methodInfo.GetCustomAttribute<LLMToolAttribute>().description;
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
            return name; 
        }

        internal bool CanBeCalled(Dictionary<string, string> parameters, out Action<object> action)
        {
            object[] callParams = new object[Parameters.Capacity];
            bool valid = true;
            action = null;
            object castedObject;
            foreach (LLMToolParameter item in Parameters)
            {
                //If it does not exist and is mandatory
                if(!(parameters.TryGetValue(item.GetName(), out string value) || item.IsOptional()))
                {
                    return false;
                }
                try
                {
                    castedObject = value == null ? Type.Missing : Convert.ChangeType(value, item.GetType()); 
                    callParams.Append(value);
                }
                catch(Exception e){Debug.Log(e.Message); return false;}
            }
            action = value => {reference.Invoke(value, callParams);}; 
            return valid;
        }
        public string GetDescription()
        {
            return description;
        }
    }

    public class LLMToolParameter
    {
        string description;
        string name;
        bool optional = false;
        Type type;

        static readonly Type[] supportedTyping = new Type[]
        {
            typeof(string),
            typeof(float),
            typeof(int),
            typeof(bool)
        };

        private LLMToolParameter(string name, string description, Type type)
        {
            this.name = name;
            this.description = description;
            this.type = type;
            this.optional = false;
        }

        private void SetOptional(bool optional)
        {
            this.optional = optional;
        }

        public static LLMToolParameter FromParameterInfo(ParameterInfo parameterInfo)
        {
            LLMParameterAttribute info = parameterInfo.GetCustomAttribute<LLMParameterAttribute>() ?? throw new ArgumentException($"{parameterInfo.Name} is missing the attribute LLMParameter");
            LLMToolParameter parameter = new(parameterInfo.Name, info.description, parameterInfo.ParameterType);
            parameter.SetOptional(parameterInfo.IsOptional);
            return parameter;

        }
        public Type GetParameterType()
        {
            return type;
        }

        internal string GetName()
        {
            return name;
        }

        internal bool IsOptional()
        {
            return optional;
        }

        public string GetDescription()
        {
            return description;
        }
    }
}

