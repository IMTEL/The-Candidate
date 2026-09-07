using UnityEditor.Search;
using UnityEngine.UIElements;

[UxmlElement]
public partial class AgentProperty : BindableElement, INotifyValueChanged<Agent>
{
    public Agent value
    {
        get => agent; set
        {
            if (value == this.value)
                return;

            var previous = this.value;
            SetValueWithoutNotify(value);

            using var evt = ChangeEvent<Agent>.GetPooled(previous, value);
            evt.target = this;
            SendEvent(evt);
        }
    }

    Agent agent;
    readonly ObjectField TTService = new();
    readonly ObjectField LLMService = new();

    public AgentProperty()
    {
        AddToClassList("agent-providers");
        TTService.objectType = typeof(TTSServiceProvider);
        LLMService.objectType = typeof(LLMServiceProvider);
        TTService.RegisterValueChangedCallback(OnObjectFieldValueChanges);
        LLMService.RegisterValueChangedCallback(OnObjectFieldValueChanges);
        if (agent != null)
        {
            agent.GetTTSSettings(out TTSServiceProvider TTSprovider, out OptionSet TTSoptions);
            TTService.value = TTSprovider;
            agent.GetLLMSettings(out LLMServiceProvider LLMProvider, out OptionSet LLMoptions);
            LLMService.value = LLMProvider;
        }
        Add(LLMService);
        Add(TTService);
    }

    void OnObjectFieldValueChanges(ChangeEvent<UnityEngine.Object> evt)
    {
        if (evt.newValue is LLMServiceProvider LLMProvider)
        {
            SetValueWithoutNotify(LLMProvider);
        }
        else if (evt.newValue is TTSServiceProvider TTSProvider)
        {
            SetValueWithoutNotify(TTSProvider);
        }
        // ← Add this: notify that the Agent's contents changed
        using var changeEvt = ChangeEvent<Agent>.GetPooled(agent, agent);
        changeEvt.target = this;
        SendEvent(changeEvt);
    }

    public void SetValueWithoutNotify(Agent newValue)
    {
        agent = newValue;

        if (agent != null)
        {
            agent.GetLLMSettings(out LLMServiceProvider llm, out _);
            LLMService.SetValueWithoutNotify(llm);

            agent.GetTTSSettings(out TTSServiceProvider tts, out _);
            TTService.SetValueWithoutNotify(tts);
        }
        else
        {
            LLMService.SetValueWithoutNotify(null);
            TTService.SetValueWithoutNotify(null);
        }
    }

    public void SetValueWithoutNotify(TTSServiceProvider newValue)
    {
        agent.SetTTSServiceProvider(newValue);
    }

    public void SetValueWithoutNotify(LLMServiceProvider newValue)
    {
        agent.SetLLMServiceProvider(newValue);
    }

}