using SoLoud;
using System;

namespace Engine.Audio.Types;

public class TextToSpeechAudio : AudioSource {
	private string text;
	/// <summary>
	/// The text this text to speech audio will read out
	/// </summary>
	public string Text {
		get => text;
		set {
			text = value;
			if (Loaded) {
				speechObj.setText(text);
			}
		}
	}
	private bool playing = false;
	/// <summary>
	/// Stores whether this speech is currently "playing"
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
	private bool looping;
	/// <summary>
	/// Stores whether this speech should loop once it finishes.
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
	private Speech speechObj;
	public uint VoiceHandle { get; private set; } = 0;

	public TextToSpeechAudio(string name, string text, float volume = 1f, float pan = 0f, float playbackSpeed = 1f, bool looping = false, double startPosition = 0d, string targetBus = "master")
		: base(name, volume, pan, playbackSpeed, startPosition, targetBus) {
		Text = text;
		Looping = looping;
	}

	/// <summary>
	/// Loads the this speech into memory.
	/// Loaded audio can be played relatively fast, but will take up memory
	/// If it is not manually loaded ahead of time, it will be loaded automatically when it's first played.
	/// Note: speeches don't take as long to load, and don't consume much memory. Loading speeches isn't as
	/// important as with other types of audio.
	/// </summary>
	public override void Load() {
		Stop();
		speechObj = new Speech();
		int status = speechObj.setText(Text);
		AssertLoad(status);
		speechObj.setInaudibleBehavior(1, 0);
		Loaded = true;
	}

	/// <summary>
	/// Unloads the speech
	/// </summary>
	public override void Unload() {
		if (!Loaded) {
			return;
		}
		Stop();
		speechObj.Dispose();
		speechObj = null;
		Loaded = false;
	}

	/// <summary>
	/// Starts playing this speech
	/// Note that if the speech is already playing when this method is called, it will be restarted
	/// </summary>
	public override void Play() {
		Stop();
		if (!Loaded) {
			Load();
		}

		VoiceHandle = AudioManager.Buses[TargetBus].BusObject.play(speechObj, aVolume: volume, aPan: pan, aPaused: 1);
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
	}

	/// <summary>
	/// Stops the speech playback
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
			AudioManager.Buses[TargetBus].BusObject.annexSound(VoiceHandle);
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
}
