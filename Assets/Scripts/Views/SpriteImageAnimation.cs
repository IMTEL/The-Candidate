using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class SpriteImageAnimation : MonoBehaviour
{
    public Sprite[] sprites;
    public double interval;

    private double time;

    private Image image;
    private bool playAnimation;

    private Color defualtColor;

    private void Awake()
    {
        image = GetComponent<Image>();
        defualtColor = image.color;
        if(!playAnimation)
        {
            defualtColor.a = 0f;
            image.color = defualtColor;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(!playAnimation) return;
        time += Time.deltaTime;
        if(interval*sprites.Count() <= time) time -= interval*sprites.Count();
        int index = Math.DivRem((int)(time*100), (int)(interval*100), out _ );
        image.sprite = sprites[index];
    }

    public void PlayAnimation()
    {
        playAnimation = true;
        defualtColor.a = 1.0f;
        image.color = defualtColor;
    }

    public void StopAnimation()
    {
        playAnimation = false;
        time = 0;
        defualtColor.a = 0.0f;
        image.color = defualtColor;
    }
}
