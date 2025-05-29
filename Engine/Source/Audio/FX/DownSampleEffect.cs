using SoLoud;
using System;

namespace Engine.Audio.FX;

/// <summary>
/// A relatively clean downsampling effect
/// </summary>
public class DownSampleEffect : Effect {
	public enum Params : uint {
		Wet = LofiFilter.WET,
		SampleRate = LofiFilter.SAMPLERATE,
		BitDepth = LofiFilter.BITDEPTH
	}

	private LofiFilter? lf;
	public override SoloudObject? EffectObject => lf;
	private float sampleRate;
	/// <summary>
	/// The sample rate of the downsampled audio, ranging from 100hz to 22000hz
	/// </summary>
	public float SampleRate {
		get {
			if (Active) {
				sampleRate = AudioManager.Soloud.getFilterParameter(TargetHandle, TargetId, (uint)Params.SampleRate);
			}
			return sampleRate;
		}
		set {
			sampleRate = value;
			if (sampleRate < 100f) {
				sampleRate = 100f;
			}
			if (sampleRate > 22000f) {
				sampleRate = 22000f;
			}
			if (Active) {
				AudioManager.Soloud.setFilterParameter(TargetHandle, TargetId, (uint)Params.SampleRate, sampleRate);
			}
		}
	}
	private float bitDepth;
	/// <summary>
	/// The bit depth of the effect, ranging from 0,5 to 16
	/// </summary>
	public float BitDepth {
		get {
			if (Active) {
				bitDepth = AudioManager.Soloud.getFilterParameter(TargetHandle, TargetId, (uint)Params.BitDepth);
			}
			return bitDepth;
		}
		set {
			bitDepth = value;
			if (bitDepth < .5) {
				bitDepth = .5f;
			}
			if (bitDepth > 16f) {
				bitDepth = 16f;
			}
			if (Active) {
				AudioManager.Soloud.setFilterParameter(TargetHandle, TargetId, (uint)Params.BitDepth, bitDepth);
			}
		}
	}

	public DownSampleEffect(float sampleRate = 11000f, float bitDepth = 8f, float wet = 1f) : base(wet) {
		SampleRate = sampleRate;
		BitDepth = bitDepth;
	}

	public override void Activate(uint targetHandle, uint targetId) {
		base.Activate(targetHandle, targetId);
		lf = new LofiFilter();
		lf.setParams(SampleRate, BitDepth);
	}

	public override void InitializeParams() {
		AudioManager.Soloud.setFilterParameter(TargetHandle, TargetId, (uint)Params.Wet, wet);
		AudioManager.Soloud.setFilterParameter(TargetHandle, TargetId, (uint)Params.SampleRate, sampleRate);
		AudioManager.Soloud.setFilterParameter(TargetHandle, TargetId, (uint)Params.BitDepth, bitDepth);
	}

	protected override void UpdateLiveWet() {
		AudioManager.Soloud.setFilterParameter(TargetHandle, TargetId, (uint)Params.Wet, wet);
	}

	protected override void ReadLiveWet() {
		wet = AudioManager.Soloud.getFilterParameter(TargetHandle, TargetId, (uint)Params.Wet);
	}

	public override void Deactivate() {
		base.Deactivate();
		lf?.Dispose();
		lf = null;
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
