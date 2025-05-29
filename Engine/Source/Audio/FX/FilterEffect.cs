using SoLoud;
using System;

namespace Engine.Audio.FX;

/// <summary>
/// A relatively clean filter effect, supporting three types: high pass, low pass and band pass
/// </summary>
public class FilterEffect : Effect {
	public enum Params : uint {
		Wet = SoLoud.BiquadResonantFilter.WET,
		Type = SoLoud.BiquadResonantFilter.TYPE,
		Frequency = SoLoud.BiquadResonantFilter.FREQUENCY,
		Resonance = SoLoud.BiquadResonantFilter.RESONANCE
	}

	public enum FilterType : int {
		LowPass = SoLoud.BiquadResonantFilter.LOWPASS,
		HighPass = SoLoud.BiquadResonantFilter.HIGHPASS,
		BandPass = SoLoud.BiquadResonantFilter.BANDPASS
	}

	private SoLoud.BiquadResonantFilter? brf;
	public override SoloudObject? EffectObject => brf;
	private FilterType type;
	/// <summary>
	/// The type of the filter (high pass, low pass or band pass)
	/// </summary>
	public FilterType Type {
		get => type;
		set {
			type = value;
			if (Active) {
				AudioManager.Soloud.setFilterParameter(TargetHandle, TargetId, (uint)Params.Type, (int)type);
			}
		}
	}
	private float frequency;
	/// <summary>
	/// The cutoff frequency of the filter ranging from 10hz to 22000hz
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
			if (frequency < 10f) {
				frequency = 10f;
			}
			if (frequency > 22000f) {
				frequency = 22000f;
			}
			if (Active) {
				AudioManager.Soloud.setFilterParameter(TargetHandle, TargetId, (uint)Params.Frequency, frequency);
			}
		}
	}
	private float resonance;
	/// <summary>
	/// The resonance of the filter, ranging from 0,1 to 20
	/// </summary>
	public float Resonance {
		get {
			if (Active) {
				resonance = AudioManager.Soloud.getFilterParameter(TargetHandle, TargetId, (uint)Params.Resonance);
			}
			return resonance;
		}
		set {
			resonance = value;
			if (resonance < .1f) {
				resonance = .1f;
			}
			if (resonance > 20f) {
				resonance = 20f;
			}
			if (Active) {
				AudioManager.Soloud.setFilterParameter(TargetHandle, TargetId, (uint)Params.Resonance, resonance);
			}
		}
	}

	public FilterEffect(FilterType type, float frequency, float resonance = 0f, float wet = 1f) : base(wet) {
		Type = type;
		Frequency = frequency;
		Resonance = resonance;
	}

	public override void Activate(uint targetHandle, uint targetId) {
		base.Activate(targetHandle, targetId);
		brf = new SoLoud.BiquadResonantFilter();
		brf.setParams((int)Type, Frequency, Resonance);
	}

	public override void InitializeParams() {
		AudioManager.Soloud.setFilterParameter(TargetHandle, TargetId, (uint)Params.Wet, wet);
		AudioManager.Soloud.setFilterParameter(TargetHandle, TargetId, (uint)Params.Type, (int)type);
		AudioManager.Soloud.setFilterParameter(TargetHandle, TargetId, (uint)Params.Frequency, frequency);
		AudioManager.Soloud.setFilterParameter(TargetHandle, TargetId, (uint)Params.Resonance, resonance);
	}

	protected override void UpdateLiveWet() {
		AudioManager.Soloud.setFilterParameter(TargetHandle, TargetId, (uint)Params.Wet, wet);
	}

	protected override void ReadLiveWet() {
		wet = AudioManager.Soloud.getFilterParameter(TargetHandle, TargetId, (uint)Params.Wet);
	}

	public override void Deactivate() {
		base.Deactivate();
		brf?.Dispose();
		brf = null;
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
