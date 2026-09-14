using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Unity.Collections.LowLevel.Unsafe;
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

    [NonSerialized]
    private static Dictionary<Type, HashSet<LLMToolFunction>> classFunctionRegistry = new();

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
        if (classFunctionRegistry.TryGetValue(typeof(object), out functionRegistry))
        {
            return;
        }
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

    public bool IsMethodCallable(string name, Dictionary<string, object> parameters, out Action callable)
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

        public string GetName()
        {
            return name; 
        }

        internal bool CanBeCalled(Dictionary<string, object> parameters, out Action<object> action)
        {
            object[] callParams = new object[Parameters.Capacity];
            bool valid = true;
            action = null;
            foreach (LLMToolParameter item in Parameters)
            {
                //If it does not exist and is mandatory
                if(!(parameters.TryGetValue(item.GetName(), out object value) || item.IsOptional()))
                {
                    return false;
                }
                try
                {
                    value = value == null ? Type.Missing : Convert.ChangeType(value, item.GetType()); 
                    callParams.Append(value);
                }
                catch(Exception e){Debug.Log(e.Message);}
            }
            action = value => {reference.Invoke(value, callParams);}; 
            return valid;
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
            LLMParameter info = parameterInfo.GetCustomAttribute<LLMParameter>() ?? throw new ArgumentException($"{parameterInfo.Name} is missing the attribute LLMParameter");
            return new LLMToolParameter(parameterInfo.Name, info.description, parameterInfo.ParameterType);
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
    }
}

