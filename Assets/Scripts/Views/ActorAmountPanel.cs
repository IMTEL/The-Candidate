using UnityEngine;

public class ActorAmountPanel : MonoBehaviour
{

    public GameObject buttonPanel;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ScenarioSettings settings = BootStrap.Instance.GetComponentInChildren<ScenarioSettingsWrapper>().getScenarioSettings();
        foreach (var (actor, _) in settings.GetActors())
        {
            GameObject buttonObject = Instantiate(buttonPanel, transform);
            buttonObject.GetComponent<ActorAmount>().SetActor(actor);
        }
    }
}
