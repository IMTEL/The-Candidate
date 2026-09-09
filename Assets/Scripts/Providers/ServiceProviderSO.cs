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

public abstract class JobSelection : ServiceProviderSO<IJobSelectionService>
{
    public Texture2D icon;

    public override abstract IJobSelectionService CreateTypedService();

    public Texture2D GetIcon()
    {
        return icon;
    }

}

public abstract class TTSServiceProvider : ServiceProviderSO<ITTService>
{
    
    public override abstract ITTService CreateTypedService();

    public abstract OptionSet GetTTSOptions();

}

public abstract class LLMServiceProvider : ServiceProviderSO<ILLMService>
{   
    public override abstract ILLMService CreateTypedService();

    public abstract OptionSet GetLLMOptions();

}
