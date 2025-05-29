using SoLoud;
using System;

namespace Engine.Audio.Types;

/// <summary>
/// Represents an audio file that is streamed in one instance.
/// This instance can be manipulated live, paused, etc.
/// Streamed audio takes up way less memory than regular, but will take more processing power.
/// Note that seeking streamed audio is way too slow to be acceptable
/// </summary>
public class StreamedAudio : AudioSource, IPausableAudio, ISeekableAudio {
	private bool playing = false;
	/// <summary>
	/// Stores whether this audio file is currently "playing"
	/// (Paused audio is considered "playing")
	/// </summary>
	public bool Playing {
		get {
			if (playing && AudioManager.Soloud.isValidVoiceHandle(VoiceHandle) == 0) {
				playing = false;
			}
			return playing;
		}
		private set {
			playing = value;
		}
	}
	/// <summary>
	/// Stores whether this audio file is currently paused
	/// </summary>
	public bool Paused { get; private set; } = false;
	private bool looping = false;
	/// <summary>
	/// Stores whether this audio file should loop once it finishes.
	/// </summary>
	public bool Looping {
		get => looping;
		set {
			looping = value;
			if (Playing) {
				AudioManager.Soloud.setLooping(VoiceHandle, looping ? 1 : 0);
			}
		}
	}
	public double CurrentPosition => Playing ? AudioManager.Soloud.getStreamPosition(VoiceHandle) * 1000f : 0;
	private string sourceFile;
	private WavStream? wavStream;
	public uint VoiceHandle { get; private set; } = 0;

	public StreamedAudio(string name, string sourceFile, float volume = 1f, float pan = 0f, float playbackSpeed = 1f, bool looping = false, double startPosition = 0d, string targetBus = "master")
		: base(name, volume, pan, playbackSpeed, startPosition, targetBus) {
		this.sourceFile = sourceFile;
		Looping = looping;
	}

	/// <summary>
	/// Doesn't really do much, gets this object ready for streaming.
	/// (Since this is a streamed audio source, load won't load the whole file into memory)
	/// </summary>
	public override void Load() {
		Stop();
		wavStream = new WavStream();
		int status = wavStream.load(AudioManager.REL_AUDIO_PATH + sourceFile);
		AssertLoad(status);
		wavStream.setInaudibleBehavior(1, 0);
		Length = TimeSpan.FromSeconds(wavStream.getLength());
		Loaded = true;
	}

	/// <summary>
	/// Unloads the source of this audio file
	/// </summary>
	public override void Unload() {
		if (!Loaded) {
			return;
		}
		Stop();
		wavStream?.Dispose();
		wavStream = null;
		Length = TimeSpan.Zero;
		Loaded = false;
	}

	/// <summary>
	/// Starts playing this audio.
	/// Note that if the audio file is already playing when this method is called, it will be restarted
	/// </summary>
	public override void Play() {
		Stop();
		if (!Loaded) {
			Load();
		}

		VoiceHandle = AudioManager.Buses[TargetBus].BusObject!.play(wavStream!, aVolume: Volume, aPan: Pan, aPaused: 1);
		AssertHandle(VoiceHandle);
		if (looping) {
			AudioManager.Soloud.setLooping(VoiceHandle, 1);
		}
		if (StartPosition != 0L) {
			AudioManager.Soloud.seek(VoiceHandle, StartPosition / 1000d);
		}
		AudioManager.Soloud.setRelativePlaySpeed(VoiceHandle, playbackSpeed);
		AudioManager.Soloud.setPause(VoiceHandle, 0);
		Playing = true;
		Paused = false;
	}

	/// <summary>
	/// Stops the audio playback
	/// </summary>
	public override void Stop() {
		if (Playing) {
			AudioManager.Soloud.stop(VoiceHandle);
			Playing = false;
			VoiceHandle = 0;
		}
	}

	protected override void UpdateLiveVolume() {
		if (Playing) {
			AudioManager.Soloud.setVolume(VoiceHandle, volume);
		}
	}

	protected override void UpdateLivePan() {
		if (Playing) {
			AudioManager.Soloud.setPan(VoiceHandle, pan);
		}
	}

	protected override void UpdateLivePlaybackSpeed() {
		if (Playing) {
			AudioManager.Soloud.setRelativePlaySpeed(VoiceHandle, playbackSpeed);
		}
	}

	protected override void UpdateLiveBus() {
		if (Playing) {
			AudioManager.Buses[TargetBus].BusObject!.annexSound(VoiceHandle);
		}
	}

	protected override void ReadLiveVolume() {
		if (Playing) {
			volume = AudioManager.Soloud.getVolume(VoiceHandle);
		}
	}

	protected override void ReadLivePan() {
		if (Playing) {
			pan = AudioManager.Soloud.getPan(VoiceHandle);
		}
	}

	protected override void ReadLivePlaybackSpeed() {
		if (Playing) {
			playbackSpeed = AudioManager.Soloud.getRelativePlaySpeed(VoiceHandle);
		}
	}

	/// <summary>
	/// Pauses the audio playback
	/// </summary>
	public void Pause() {
		if (!Paused) {
			AudioManager.Soloud.setPause(VoiceHandle, 1);
			Paused = true;
		}
	}

	/// <summary>
	/// Resumes the audio playback
	/// </summary>
	public void Resume() {
		if (Paused) {
			AudioManager.Soloud.setPause(VoiceHandle, 0);
			Paused = false;
		}
	}

	public void Oscillate(Params parameter, float min, float max, TimeSpan period) {
		if (!Playing) {
			return;
		}
		switch (parameter) {
			case Params.Volume:
				AudioManager.Soloud.oscillateVolume(VoiceHandle, min, max, period.TotalSeconds);
				break;
			case Params.Pan:
				AudioManager.Soloud.oscillatePan(VoiceHandle, min, max, period.TotalSeconds);
				break;
			case Params.PlaybackSpeed:
				AudioManager.Soloud.oscillateRelativePlaySpeed(VoiceHandle, min, max, period.TotalSeconds);
				break;
		}
	}

	public void Oscillate(Params parameter, float min, float max, float milliseconds)
		=> Oscillate(parameter, min, max, TimeSpan.FromMilliseconds(milliseconds));

	public void Fade(Params parameter, float to, TimeSpan span) {
		if (!Playing) {
			return;
		}
		switch (parameter) {
			case Params.Volume:
				AudioManager.Soloud.fadeVolume(VoiceHandle, to, span.TotalSeconds);
				break;
			case Params.Pan:
				AudioManager.Soloud.fadePan(VoiceHandle, to, span.TotalSeconds);
				break;
			case Params.PlaybackSpeed:
				AudioManager.Soloud.fadeRelativePlaySpeed(VoiceHandle, to, span.TotalSeconds);
				break;
		}
	}

	public void Fade(Params parameter, float to, float milliseconds)
		=> Fade(parameter, to, TimeSpan.FromMilliseconds(milliseconds));

	public void Seek(double position) {
		if (Playing) {
			AudioManager.Soloud.seek(VoiceHandle, position / 1000d);
		}
	}

	public void Seek(TimeSpan position) => Seek(position.TotalMilliseconds);
}
