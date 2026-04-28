using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TTService that provides us wiht a way to get voices
/// </summary>
public interface ITTService
{
    /// <summary>
    /// Fetches the audioclip from the service
    /// </summary>
    /// <param name="voice">the voiceprofile</param>
    /// <param name="dialogue">the dialogue to speak</param>
    /// <returns></returns>
    public IAsyncEnumerator<AudioClip> getAudioClip(string dialogue, IEnumerable<OptionBase> options);
    public static IEnumerable<OptionBase> GetTTSOptions()  => throw new NotImplementedException("The implementation for option is not implemented within this instance. Add it :)");

}