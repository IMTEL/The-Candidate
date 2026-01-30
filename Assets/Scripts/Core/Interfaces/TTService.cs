using System.Threading.Tasks;
using UnityEngine;

public abstract class TTService
{
    public abstract Task<AudioClip> getAudioClip(Voice voice, string dialogue);

    public enum Voice
    {
        Johannes,
        Ola
    }
}