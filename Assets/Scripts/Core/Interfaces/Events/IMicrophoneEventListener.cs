using Whisper.Utils;

public interface IMicrophoneEventListener
{
    /// <summary>
    /// Calls when the microphone has started recording
    /// </summary>
    void onMicrophoneStartRecording();

    /// <summary>
    /// Event for when we are done recording.
    /// </summary>
    /// <param name="recordedAudio">The audio from the microphone</param>
    void onMicrophoneStopRecording(Dialog recordedaudio);
}