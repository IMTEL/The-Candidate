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
    [ShowInInspector] public Dictionary<VoiceProfile, string> voiceproviderids = new();
    
    public override abstract TTService CreateTypedService();

    public abstract IEnumerable<OptionBase> GetTTSOptions();

}

public abstract class LLMServiceProvider : ServiceProviderSO<ILLMService>
{   
    public override abstract ILLMService CreateTypedService();

    public abstract IEnumerable<OptionBase> GetLLMOptions();

}
