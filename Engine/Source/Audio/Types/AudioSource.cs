using System;

namespace Engine.Audio.Types;

/// <summary>
/// Represents any type of audio, that can be stored in the
/// Audio manager, played, stopped, loaded, unloaded, and has the basic
/// properties of an audio source. Note, that the behavior of those functions
/// will greatly differ between specific types of audio
/// </summary>
public abstract class AudioSource {
	public enum Params {
		Volume,
		Pan,
		PlaybackSpeed
	}

	/// <summary>
	/// The unique name of this audio source
	/// </summary>
	public string Name { get; protected set; }
	/// <summary>
	/// Stores whether this audio source has been loaded
	/// </summary>
	public bool Loaded { get; protected set; } = false;
	/// <summary>
	/// Stores the length of this audio source
	/// </summary>
	public TimeSpan Length { get; protected set; } = TimeSpan.Zero;
	protected float volume;
	/// <summary>
	/// Stores the volume of this specific audio source
	/// 0 means completely silent
	/// 1 means normal volume
	/// values higher than 1 will be louder than normal
	/// Note that not all types of audio can be updated live (while they're playing)
	/// </summary>
	public float Volume {
		get {
			ReadLiveVolume();
			return volume;
		}
		set {
			volume = value;
			if (volume < 0f) {
				volume = 0f;
			}
			UpdateLiveVolume();
		}
	}
	protected string targetBus;
	/// <summary>
	/// The bus, through which this audio source will be played.
	/// Changing this value while an instance is playing will have no effect
	/// on that instance.
	/// A null value means that this audio source won't play through a bus, so
	/// only the settings and effects of the AudioManager itself will affect it.
	/// </summary>
	public string TargetBus {
		get => targetBus;
		set {
			targetBus = value;
			UpdateLiveBus();
		}
	}

	protected float pan;
	/// <summary>
	/// Stores the pan of the audio.
	/// 0 means centered audio
	/// -1 means left only
	/// 1 means right only
	/// </summary>
	public float Pan {
		get {
			ReadLivePan();
			return pan;
		}
		set {
			pan = value;
			if (pan < -1f) {
				pan = -1f;
			}
			if (pan > 1f) {
				pan = 1f;
			}
			UpdateLivePan();
		}
	}
	protected float playbackSpeed;
	/// <summary>
	/// Stores the playback speed of the audio
	/// 1 means normal speed
	/// values between 0 and 1 mean speeds slower than normal
	/// values above 1 mean speeds faster than normal
	/// </summary>
	public float PlaybackSpeed {
		get {
			ReadLivePlaybackSpeed();
			return playbackSpeed;
		}
		set {
			playbackSpeed = value;
			if (playbackSpeed < 0f) {
				playbackSpeed = 0f;
			}
			UpdateLivePlaybackSpeed();
		}
	}
	/// <summary>
	/// The starting position of the audio
	/// </summary>
	public double StartPosition { get; set; }

	public AudioSource(string name, float volume = 1f, float pan = 0f, float playbackSpeed = 1f, double startPosition = 0d, string targetBus = "master") {
		Name = name;
		Volume = volume;
		Pan = pan;
		PlaybackSpeed = playbackSpeed;
		StartPosition = startPosition;
		TargetBus = targetBus;
	}

	protected virtual void UpdateLiveVolume() { }

	protected virtual void UpdateLivePan() { }

	protected virtual void UpdateLivePlaybackSpeed() { }

	protected virtual void UpdateLiveBus() { }

	protected virtual void ReadLiveVolume() { }

	protected virtual void ReadLivePan() { }

	protected virtual void ReadLivePlaybackSpeed() { }

	public abstract void Load();

	public abstract void Unload();

	public abstract void Play();

	public abstract void Stop();

	public static void AssertLoad(int status) {
		if (status != 0) {
			throw new AudioLoadException(
				"An error occurred when attempting to load an audio source.\n" +
				$"Error code: {status}; error message: {AudioManager.Soloud.getErrorString(status)}");
		}
	}

	public static void AssertHandle(uint handle) {
		if (handle == 0) {
			throw new AudioPlayException("Couldn't play audio.");
		}
	}
}

public class AudioLoadException : Exception {
	public AudioLoadException() { }
	public AudioLoadException(string message) : base(message) { }
}

public class AudioPlayException : Exception {
	public AudioPlayException() { }
	public AudioPlayException(string message) : base(message) { }
}
