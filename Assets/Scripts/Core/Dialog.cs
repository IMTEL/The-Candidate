using UnityEngine;

/// <summary>
/// A Dialog represents a dialog which is played within the game.
/// 
/// It contains the information of who, what and how the dialog got said trough the attributes
/// </summary>
public class Dialog
{
    /// <summary>
    /// The actor that said the dialog
    /// </summary>
    public Environment.Actor actor {get; private set;}
    
    /// <summary>
    /// The content of the dialog
    /// </summary>
    public string dialog {get; private set;}

    /// <summary>
    /// The audioclip of the dialog if created.
    /// </summary>
    public AudioClip audioClip;

    /// <summary>
    /// Constructor of the dialog
    /// </summary>
    /// <param name="actor">The actor of the dialog</param>
    /// <param name="dialog">The content of the dialog</param>
    /// <param name="audioClip">A optional audioclip</param>
    public Dialog(Environment.Actor actor, string dialog, AudioClip audioClip = null)
    {
        this.actor = actor;
        this.dialog = dialog;
        this.audioClip = audioClip;
    }
}
