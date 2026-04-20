using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(Image))]
public class JobButton : MonoBehaviour
{
    private Button Button;

    private JobContainer Job;

    public TMP_Text Text;

    public RawImage Picture;

    private Image ButtonImage;


    void Awake()
    {
        Button = GetComponent<Button>();
        ButtonImage = GetComponent<Image>();
        Button.onClick.AddListener(() => {});
        SetJob(null);
    }

    public void SetJob(JobContainer job)
    {
        Job = job;
        if(job == null)
        {
            Button.interactable = false;
            Text.text = "";
            Picture.texture = null;
            Picture.enabled = false;
            ButtonImage.enabled = false; 
        }
        else
        {
            Button.interactable = true;
            Text.text = job.GetJob().name;
            Picture.texture = job.GetTexture2D();
            Picture.enabled = true;
            ButtonImage.enabled = true;
        }        
    }

}