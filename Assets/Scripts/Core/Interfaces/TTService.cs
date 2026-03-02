using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// TTService that provides us wiht a way to get voices
/// </summary>
public interface TTService
{
    /// <summary>
    /// Fetches the audioclip from the service
    /// </summary>
    /// <param name="voice">the voiceprofile</param>
    /// <param name="dialogue">the dialogue to speak</param>
    /// <returns></returns>
    public Task<AudioClip> getAudioClip(VoiceProfile voice, string dialogue);
    public static IEnumerable<OptionBase> GetTTSOptions()  => throw new NotImplementedException("The implementation for option is not implemented within this instance. Add it :)");

}