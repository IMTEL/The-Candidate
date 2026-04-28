using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public class ActorAmount : MonoBehaviour
{

    public TMP_Text AmountText;

    private Environment.Actor actor;

    public Button DecreaseButton;
    public Button IncreaseButton;

    public TMP_Text ActorText;
    
    private ScenarioSettingsWrapper scenarioSettings;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        scenarioSettings = BootStrap.Instance.GetComponentInChildren<ScenarioSettingsWrapper>();
        DecreaseButton.onClick.AddListener(Decrease);
        IncreaseButton.onClick.AddListener(Increase);
        UpdateUI();
    }

    public void SetActor(Environment.Actor actor)
    {
        this.actor ??= actor;
        ActorText.text = actor.getName();
    }

    private void UpdateUI()
    {
        if(actor == null) AmountText.text = "";
        else AmountText.text = scenarioSettings.getScenarioSettings().GetActorAmount(actor).ToString();
    }

    private void Decrease()
    {
        ScenarioSettings settings = scenarioSettings.getScenarioSettings();
        int newAmount = settings.GetActorAmount(actor)-1;
        settings.SetAmount(actor, newAmount);
        UpdateUI();
    }

    private void Increase()
    {
        ScenarioSettings settings = scenarioSettings.getScenarioSettings();
        int newAmount = settings.GetActorAmount(actor)+1;
        settings.SetAmount(actor, newAmount);
        UpdateUI();
    }
}
