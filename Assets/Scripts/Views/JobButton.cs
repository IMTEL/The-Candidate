using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JobButton : MonoBehaviour
{
    public Button Button;

    private JobContainer Job;

    public TMP_Text Text;

    public RawImage Picture;


    void Awake()
    {
        Button.onClick.AddListener(() => {});
    }

    public void SetJob(JobContainer job)
    {
        Job = job;
        if(job == null)
        {
            Button.interactable = false;
            Text.text = "";
            Picture.texture = null;    
        }
        else
        {
            Button.interactable = true;
            Text.text = job.GetJob().name;
            Picture.texture = job.GetTexture2D();    
        }        
    }

}