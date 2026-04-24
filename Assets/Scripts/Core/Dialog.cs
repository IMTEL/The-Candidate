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

    private readonly List<AudioClip> audioClips = new();

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
        audioClips.Add(_stream.Current);
        return _stream.Current;
    }

    public AudioClip GetFullAudioClip()
    {
        return ConcatenateClips(audioClips);
    }

    private static async IAsyncEnumerator<AudioClip> SingleClip(AudioClip clip){ 
        yield return clip;
    }

    
    /// <summary>
    /// Stitces audioclips together to one version
    /// Written by Claude since my knowledge of audio stuff is limited
    /// </summary>
    /// <param name="clips">The clips to be stitched together</param>
    /// <returns>The full audioclip to be</returns>
    private AudioClip ConcatenateClips(List<AudioClip> clips)
    {
        if (clips == null || clips.Count == 0) return null;

        int sampleRate = clips[0].frequency;
        int channels = clips[0].channels;

        // Total samples across all clips (silence clips included)
        int totalSamples = 0;
        foreach (var clip in clips)
            totalSamples += clip.samples * clip.channels;

        float[] combinedData = new float[totalSamples];
        int offset = 0;

        foreach (var clip in clips)
        {
            float[] clipData = new float[clip.samples * clip.channels];
            clip.GetData(clipData, 0);
            System.Array.Copy(clipData, 0, combinedData, offset, clipData.Length);
            offset += clipData.Length;
        }

        AudioClip merged = AudioClip.Create("MergedClip", totalSamples / channels, channels, sampleRate, false);
        merged.SetData(combinedData, 0);

        return merged;
    }

}
