using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public abstract class TTService
{

    private Dictionary<VoiceProfile, string> voiceDB;
    public abstract Task<AudioClip> getAudioClip(VoiceProfile voice, string dialogue);

    protected TTService(Dictionary<VoiceProfile, string> voiceDB)
    {
        this.voiceDB = voiceDB;
    }

    public string getProfileID(VoiceProfile voice)
    {
        string voiceid = voiceDB[voice];
        return voiceid ?? getDefualtVoiceId();
    }

    public abstract string getDefualtVoiceId();

}