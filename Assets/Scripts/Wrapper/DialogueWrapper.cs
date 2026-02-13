using UnityEngine;

public class DialogSystemComponent : MonoBehaviour
{
    private DialogueSystem dialogueSystem;
    private DialogueScheduler dialogueScheduler;

    public void Awake()
    {
        
    }

    public void Init(LLMService llm, TTService tts)
    {
        dialogueScheduler = new DialogueSystem(llm, tts, settings, registry);
    }


}