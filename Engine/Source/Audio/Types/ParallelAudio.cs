using SoLoud;
using System;

namespace Engine.Audio.Types;

/// <summary>
/// Represents an audio file, that can be played multiple times at the same time.
/// The individual instances are not stored, so they cannot be manipulated.
/// I recommend using this for simple sfx
/// </summary>
public class ParallelAudio : AudioSource {
	private string sourceFile;
	private Wav? wavObject;

	public ParallelAudio(string name, string sourceFile, float volume = 1f, float pan = 0f, float playbackSpeed = 1f, double startPosition = 0d, string targetBus = "master")
		: base(name, volume, pan, playbackSpeed, startPosition, targetBus) {
		this.sourceFile = sourceFile;
	}

	/// <summary>
	/// Loads the source for this audio file into memory.
	/// Loaded audio can be played relatively fast, but will take up memory
	/// If it is not manually loaded ahead of time, it will be loaded automatically when it's first played
	/// </summary>
	public override void Load() {
		Stop();
		wavObject = new Wav();
		int status = wavObject.load(AudioManager.REL_AUDIO_PATH + sourceFile);
		AssertLoad(status);
		wavObject.setInaudibleBehavior(1, 0);
		Length = TimeSpan.FromSeconds(wavObject.getLength());
		Loaded = true;
	}

	/// <summary>
	/// Unloads the source of this audio file.
	/// </summary>
	public override void Unload() {
		if (!Loaded) {
			return;
		}
		Stop();
		wavObject?.Dispose();
		wavObject = null;
		Length = TimeSpan.Zero;
		Loaded = false;
	}

	/// <summary>
	/// Plays this audio file once, without saving the handle (no manipulation afterwards)
	/// </summary>
	public override void Play() {
		if (!Loaded) {
			Load();
		}
		uint h = 0;
		h = AudioManager.Buses[TargetBus].BusObject!.play(wavObject!, aVolume: volume, aPan: pan, aPaused: 1);
		AssertHandle(h);
		AudioManager.Soloud.setRelativePlaySpeed(h, playbackSpeed);
		if (StartPosition != 0L) {
			AudioManager.Soloud.seek(h, StartPosition / 1000d);
		}
		AudioManager.Soloud.setPause(h, 0);
	}

	/// <summary>
	/// Stops ALL instances started from this audio file.
	/// </summary>
	public override void Stop() {
		if (Loaded) {
			AudioManager.Soloud.stopAudioSource(wavObject!);
		}
	}
}
