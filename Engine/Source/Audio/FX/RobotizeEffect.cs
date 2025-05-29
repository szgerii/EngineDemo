using SoLoud;
using System;

namespace Engine.Audio.FX;

/// <summary>
/// An effect, that can be used to modulate audio based on a waveform
/// </summary>
public class RobotizeEffect : Effect {
	public enum Params : uint {
		Wet = SoLoud.RobotizeFilter.WET,
		Frequency = SoLoud.RobotizeFilter.FREQ,
		Wave = SoLoud.RobotizeFilter.WAVE
	}

	public enum FilterWave : int {
		Square = Soloud.WAVE_SQUARE,
		Saw = Soloud.WAVE_SAW,
		Sin = Soloud.WAVE_SIN,
		Triangle = Soloud.WAVE_TRIANGLE,
		Bounce = Soloud.WAVE_BOUNCE,
		Jaws = Soloud.WAVE_JAWS,
		Humps = Soloud.WAVE_HUMPS,
		FSquare = Soloud.WAVE_FSQUARE,
		FSaw = Soloud.WAVE_FSAW
	}

	private float frequency;
	/// <summary>
	/// The frequency of the wave, ranging from 0,1hz to 100hz
	/// </summary>
	public float Frequency {
		get {
			if (Active) {
				frequency = AudioManager.Soloud.getFilterParameter(TargetHandle, TargetId, (uint)Params.Frequency);
			}
			return frequency;
		}
		set {
			frequency = value;
			if (frequency < .1f) {
				frequency = .1f;
			}
			if (frequency > 100f) {
				frequency = 100f;
			}
			if (Active) {
				AudioManager.Soloud.setFilterParameter(TargetHandle, TargetId, (uint)Params.Frequency, frequency);
			}
		}
	}
	private FilterWave wave;
	/// <summary>
	/// The type of the distortion wave
	/// </summary>
	public FilterWave Wave {
		get => wave;
		set {
			wave = value;
			if (Active) {
				AudioManager.Soloud.setFilterParameter(TargetHandle, TargetId, (uint)Params.Wave, (int)wave);
			}
		}
	}
	private SoLoud.RobotizeFilter? rf;
	public override SoloudObject? EffectObject => rf;

	public RobotizeEffect(float frequency = 1f, FilterWave wave = FilterWave.Square, float wet = 1f) : base(wet) {
		Frequency = frequency;
		Wave = wave;
	}

	public override void Activate(uint targetHandle, uint targetId) {
		base.Activate(targetHandle, targetId);
		rf = new SoLoud.RobotizeFilter();
		rf.setParams(Frequency, (int)Wave);
	}

	public override void InitializeParams() {
		AudioManager.Soloud.setFilterParameter(TargetHandle, TargetId, (uint)Params.Wet, wet);
		AudioManager.Soloud.setFilterParameter(TargetHandle, TargetId, (uint)Params.Frequency, frequency);
		AudioManager.Soloud.setFilterParameter(TargetHandle, TargetId, (uint)Params.Wave, (int)wave);
	}

	protected override void UpdateLiveWet() {
		AudioManager.Soloud.setFilterParameter(TargetHandle, TargetId, (uint)Params.Wet, wet);
	}

	protected override void ReadLiveWet() {
		wet = AudioManager.Soloud.getFilterParameter(TargetHandle, TargetId, (uint)Params.Wet);
	}

	public override void Deactivate() {
		base.Deactivate();
		rf?.Dispose();
		rf = null;
	}

	public void Oscillate(Params parameter, float min, float max, TimeSpan period) {
		if (!Active) {
			return;
		}
		AudioManager.Soloud.oscillateFilterParameter(TargetHandle, TargetId, (uint)parameter, min, max, period.TotalSeconds);
	}

	public void Oscillate(Params parameter, float min, float max, float milliseconds)
		=> Oscillate(parameter, min, max, TimeSpan.FromMilliseconds(milliseconds));

	public void Fade(Params parameter, float to, TimeSpan span) {
		if (!Active) {
			return;
		}
		AudioManager.Soloud.fadeFilterParameter(TargetHandle, TargetId, (uint)parameter, to, span.TotalSeconds);
	}

	public void Fade(Params parameter, float to, float milliseconds)
		=> Fade(parameter, to, TimeSpan.FromMilliseconds(milliseconds));
}
