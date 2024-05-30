using System;

namespace Engine.Audio;

/// <summary>
/// Represents an audio bus, through which any types of audio can be played.
/// A bus can impact the volume, panning, and filters of the audio coming through.
/// Changing these will be reflected in live audio playback, even if that specific
/// type of audio doesn't store the handles for its instances. (Eg. changing the volume
/// of the "sfx" bus will change the playback volume of all audio that targets it, even
/// if the type of the source of those instances is SerialAudio)
/// </summary>
public class Bus {
	public enum Params {
		Volume,
		Pan
	}

	/// <summary>
	/// The unique name of this audio bus
	/// </summary>
	public string Name { get; private set; }
	/// <summary>
	/// Stores whether this audio bus has been loaded
	/// </summary>
	public bool Loaded { get; private set; } = false;
	/// <summary>
	/// Stores whether this audio bus is running
	/// </summary>
	public bool Running { get; private set; } = false;
	private float volume;
	/// <summary>
	/// Stores the volume of this audio bus
	/// 0 means completely silent
	/// 1 means normal volume
	/// values higher than 1 will be louder than normal
	/// Note that the volume of an audio bus is NOT the same as the volume of the individual audio sources
	/// playing through it.
	/// </summary>
	public float Volume {
		get {
			if (Running) {
				volume = AudioManager.Soloud.getVolume(BusHandle);
			}
			return volume;
		}
		set {
			volume = value;
			if (volume < 0.0f) {
				volume = 0.0f;
			}
			if (Running) {
				AudioManager.Soloud.setVolume(BusHandle, volume);
			}
		}
	}
	protected float pan;
	/// <summary>
	/// Stores the pan of the audio coming through this bus.
	/// 0 means centered audio
	/// -1 means left only
	/// 1 means right only
	/// </summary>
	public float Pan {
		get {
			if (Running) {
				pan = AudioManager.Soloud.getPan(BusHandle);
			}
			return pan;
		}
		set {
			pan = value;
			if (pan < -1.0f) {
				pan = -1.0f;
			}
			if (pan > 1.0f) {
				pan = 1.0f;
			}
			if (Running) {
				AudioManager.Soloud.setPan(BusHandle, pan);
			}
		}
	}
	/// <summary>
	/// The effects of this audio bus
	/// </summary>
	public Effects Effects { get; set; } = new Effects();
	private string targetBus;
	/// <summary>
	/// The bus, into which this bus will be routed to.
	/// Changing this value while an instance is playing will have no effect
	/// on that instance.
	/// A null value means that this audio source won't play through a bus, so
	/// only the settings and effects of the AudioManager itself will affect it.
	/// </summary>
	public string TargetBus {
		get => targetBus;
		set {
			targetBus = value;
		}
	}
	public uint BusHandle { get; private set; } = 0;
	public SoLoud.Bus BusObject { get; private set; }

	public Bus(string name, float volume = 1.0f, float pan = 0.0f, string targetBus = "master") {
		Name = name;
		Volume = volume;
		Pan = pan;
		TargetBus = targetBus;

		Load();
	}

	/// <summary>
	/// Starts the audio bus
	/// </summary>
	public void Init() {
		Stop();
		if (TargetBus == null) {
			BusHandle = AudioManager.Soloud.play(BusObject);
		} else {
			BusHandle = AudioManager.Buses[TargetBus].BusObject.play(BusObject);
		}

		AudioManager.Soloud.setVolume(BusHandle, Volume);
		AudioManager.Soloud.setPan(BusHandle, pan);
		Effects.Activate(BusObject, BusHandle);
		Running = true;
	}

	/// <summary>
	/// Stops the audio bus
	/// </summary>
	public void Stop() {
		if (Running) {
			AudioManager.Soloud.stop(BusHandle);
			Effects.Deactivate();
			Running = false;
			BusHandle = 0;
		}
	}

	/// <summary>
	/// Loads the audio bus
	/// </summary>
	public void Load() {
		Stop();
		BusObject = new SoLoud.Bus();
		Loaded = true;
	}

	/// <summary>
	/// Unloads the audio bus
	/// </summary>
	public void Unload() {
		Stop();
		BusObject.Dispose();
		BusObject = null;
		Loaded = false;
	}

	public void Oscillate(Params parameter, float min, float max, TimeSpan period) {
		if (!Running) {
			return;
		}
		switch (parameter) {
			case Params.Volume:
				AudioManager.Soloud.oscillateVolume(BusHandle, min, max, period.TotalSeconds);
				break;
			case Params.Pan:
				AudioManager.Soloud.oscillatePan(BusHandle, min, max, period.TotalSeconds);
				break;
		}
	}

	public void Oscillate(Params parameter, float min, float max, float milliseconds)
		=> Oscillate(parameter, min, max, TimeSpan.FromMilliseconds(milliseconds));

	public void Fade(Params parameter, float to, TimeSpan span) {
		if (!Running) {
			return;
		}
		switch (parameter) {
			case Params.Volume:
				AudioManager.Soloud.fadeVolume(BusHandle, to, span.TotalSeconds);
				break;
			case Params.Pan:
				AudioManager.Soloud.fadePan(BusHandle, to, span.TotalSeconds);
				break;
		}
	}

	public void Fade(Params parameter, float to, float milliseconds)
		=> Fade(parameter, to, TimeSpan.FromMilliseconds(milliseconds));
}