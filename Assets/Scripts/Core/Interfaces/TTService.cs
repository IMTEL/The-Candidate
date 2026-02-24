using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// TTService that provides us wiht a way to get voices
/// </summary>
public abstract class TTService
{

    private Dictionary<VoiceProfile, string> voiceDB;
    


    protected TTService(Dictionary<VoiceProfile, string> voiceDB)
    {
        this.voiceDB = voiceDB;
    }

    /// <summary>
    /// Fethches the voice ID from the provider
    /// </summary>
    /// <param name="voice">The voice profile</param>
    /// <returns>The ID of voice</returns>
    public string getProfileID(VoiceProfile voice)
    {
        string voiceid = null;
        if (voiceDB != null)
        {
            voiceid = voiceDB[voice];
        }
        return voiceid ?? getDefualtVoiceId();
    }


    /// <summary>
    /// The defualt voice in case not found
    /// </summary>
    /// <returns>the defualt voice id</returns>
    public abstract string getDefualtVoiceId();

    /// <summary>
    /// Fetches the audioclip from the service
    /// </summary>
    /// <param name="voice">the voiceprofile</param>
    /// <param name="dialogue">the dialogue to speak</param>
    /// <returns></returns>
    public abstract Task<AudioClip> getAudioClip(VoiceProfile voice, string dialogue);

}