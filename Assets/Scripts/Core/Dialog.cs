using System.Collections.Generic;
using System.Threading.Tasks;
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
    private readonly IAsyncEnumerator<AudioClip> _stream ;

    public bool IsFinished {get; private set;} = false; 


    public Dialog(Environment.Actor actor, string dialog, IAsyncEnumerator<AudioClip> stream = null)
    {
        this.actor = actor;
        this.dialog = dialog;
        _stream = stream;
    }

    /// <summary>
    /// Constructor of the dialog
    /// This assumes the dialog is already finshed with creating and is ready to be sent
    /// </summary>
    /// <param name="actor">The actor of the dialog</param>
    /// <param name="dialog">The content of the dialog</param>
    /// <param name="audioClip">A optional audioclip</param>
    public Dialog(Environment.Actor actor, string dialog, AudioClip audioClip): this(actor, dialog, SingleClip(audioClip)){}

    public async Task<AudioClip> Next()
    {
        if(IsFinished) return null;
        if(_stream == null || !await _stream.MoveNextAsync()) {IsFinished = true; return null;}
        return _stream.Current;
    }

    private static async IAsyncEnumerator<AudioClip> SingleClip(AudioClip clip){ 
        yield return clip;
    }

}
