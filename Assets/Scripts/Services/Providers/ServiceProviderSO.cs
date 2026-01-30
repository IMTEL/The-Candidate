using UnityEngine;

public abstract class ServiceProviderSO : ScriptableObject
{
    public abstract object CreateService();
}

public abstract class ServiceProviderSO<T> : ServiceProviderSO
{
    public abstract T CreateTypedService();

    public override object CreateService() => CreateTypedService();
}
