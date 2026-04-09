using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.XR.CoreUtils;
using UnityEngine;

//Why this mess instead of linking everything together trough bootstrap?
//Becouse I wanted a solution that could be scaleable and also dynamic.
//While I am probably getting frowned upon using a reflection method to initiate everything,
//this allows me to setup a simple init method within each class that requires component from
//others and fully work

//But why not fetch from the start method?
//Becouse some systems (like dialogue), requires that other components are initilized before hand.
//Yes, my code as it is would probably not benefit from it due to the component being able to initilize in awake, 
//however, there is better to have a robust system and allow for a cleaner solution down the line would such a use case show up

/// <summary>
/// Iniates the requirments for running the game
/// </summary>
public class BootStrap : MonoBehaviour
{
    private static BootStrap instance = null;

    public List<IInit> inits = new();

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
    }

    void Start()
    {
        if (instance == null)
        {
            DontDestroyOnLoad(this);
            instance = this;
            List<DependencyNode> dependencies = new(GetComponenentDependencyOrdered());
            foreach (DependencyNode node in dependencies)
            {
                Debug.Log("Initiating " + node.GetContainedType().Name);
                runInit(node.GetContainedType());
            }
        }

    }


    private void runInit(Type classType)
    {
        IInit iniObject = inits.Find((initObject) => initObject.GetType().Equals(classType)) ?? throw new ArgumentException("Can't find the class for " + classType.Name);

        //Try and fetch the method
        MethodInfo initMethod = iniObject.GetType().GetMethod("Init", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        object[] parameters = new object[initMethod.GetParameters().Length];
        for (int i = 0; i < parameters.Length; i++)
        {
            object parameter = inits.Find((initObject) => iniObject.GetType().Equals(initMethod.GetParameters()[i].ParameterType));
            parameter ??= GetComponentInChildren(initMethod.GetParameters()[i].ParameterType, true);
            parameters[i] = parameter;
        }
        initMethod.Invoke(iniObject, parameters);
    }

    private IEnumerable<DependencyNode> GetComponenentDependencyOrdered()
    {
        //Ensure all the components from the children are added
        IInit[] components = GetComponentsInChildren<IInit>();
        foreach (IInit init in components)
        {
            if (!inits.Contains(init))
            {
                inits.Add(init);
            }
        }
        String debug = "The following classes will be checked: ";
        inits.ForEach((initObject) => debug += (initObject.GetType().Name + ", "));
        Debug.Log(debug);
        List<DependencyNode> dependencies = new();
        //For each component
        foreach (IInit component in inits)
        {
            checkComponentOrder(component, dependencies);
        }
        return dependencies;
    }

    private IEnumerable<DependencyNode> checkComponentOrder(IInit component, List<DependencyNode> checknodes = null)
    {
        DependencyNode dependencyNode = new(component.GetType(), null);
        return checkComponentOrder(dependencyNode, checknodes);
    }

    private IEnumerable<DependencyNode> checkComponentOrder(DependencyNode component, List<DependencyNode> checknodes = null)
    {
        checknodes ??= new List<DependencyNode>();

        //Try and fetch the method
        MethodInfo initMethod = component.GetContainedType().GetMethod("Init", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        //If it does not contain a init method
        if (initMethod == null) return checknodes;
        //Or if it hsa already been checked previously
        else if (checknodes.Any<DependencyNode>(node => node.Equals(component))) return checknodes;
        //return backs

        //Check for round dependency, aka A -> B -> C -> A
        for (DependencyNode parent = component.GetParent(); parent != null; parent = parent.GetParent())
        {
            if (parent.GetContainedType().Equals(component.GetType()))
                throw new ArgumentException(string.Format("Component %s requires component %s which causes a round dependency",
                                                component.GetType().GetNameWithGenericArguments(),
                                                parent.GetType().GetNameWithGenericArguments()));
        }
        //Run trough each paramter, if they are init run trough the same check
        foreach (ParameterInfo parameter in initMethod.GetParameters())
        {
            Type parameterType = parameter.ParameterType;
            if (typeof(IInit).IsAssignableFrom(parameterType))
            {
                DependencyNode node = new(parameterType, component);
                checkComponentOrder(node, checknodes);
            }
        }
        checknodes.Add(component);
        return checknodes;
    }

    private void OnValidate()
    {
        GetComponenentDependencyOrdered();
    }

    private class DependencyNode
    {
        Type type;
        DependencyNode parent;

        public DependencyNode(Type type, DependencyNode parent = null)
        {
            this.type = type;
            this.parent = parent;
        }

        public Type GetContainedType()
        {
            return type;
        }

        public DependencyNode GetParent()
        {
            return parent;
        }
        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            return type.Equals(((DependencyNode)obj).GetContainedType());
        }

    }

}
