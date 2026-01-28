using Whisper.Utils;

public interface MicrophoneEventListener
{
    /// <summary>
    /// Calls when the microphone has started recording
    /// </summary>
    void onMicrophoneStartRecording();

    /// <summary>
    /// Event for when we are done recording.
    /// </summary>
    /// <param name="recordedAudio">The audio from the microphone</param>
    void onMicrophoneStopRecording(AudioChunk recordedaudio);
}