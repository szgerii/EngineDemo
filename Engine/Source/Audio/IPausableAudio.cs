namespace Engine.Audio;

/// <summary>
/// Represents, that a specific type of audio can be paused and resumed.
/// </summary>
public interface IPausableAudio {

	public abstract void Pause();

	public abstract void Resume();
}
