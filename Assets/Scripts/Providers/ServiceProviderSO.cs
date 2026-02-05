using System.Collections.Generic;
using Alchemy.Inspector;
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

public abstract class TTSserviceProvider : ServiceProviderSO<TTService>
{

    [SerializeField, ShowInInspector] public Dictionary<VoiceProfile, string> voiceproviderids = new();
    
    public override abstract TTService CreateTypedService();

    public override object CreateService() => CreateTypedService();
}