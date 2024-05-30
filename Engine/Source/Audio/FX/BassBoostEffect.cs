using SoLoud;
using System;

namespace Engine.Audio.FX;

/// <summary>
/// A simple bass boost effect. Kinda noisy
/// </summary>
public class BassBoosEffect : Effect {
	public enum Params : uint {
		Wet = BassboostFilter.WET,
		Boost = BassboostFilter.BOOST
	}

	private BassboostFilter bbf;
	public override SoloudObject EffectObject => bbf;
	private float boost;
	/// <summary>
	/// The bass boost strength property, ranging from 0 to 10
	/// </summary>
	public float Boost {
		get {
			if (Active) {
				boost = AudioManager.Soloud.getFilterParameter(TargetHandle, TargetId, (uint)Params.Boost);
			}
			return boost;
		}
		set {
			boost = value;
			if (boost < 0f) {
				boost = 0f;
			}
			if (boost > 10f) {
				boost = 10f;
			}
			if (Active) {
				AudioManager.Soloud.setFilterParameter(TargetHandle, TargetId, (uint)Params.Boost, boost);
			}
		}
	}

	public BassBoosEffect(float boost = 0f, float wet = 1f) : base(wet) {
		Boost = boost;
	}

	public override void Activate(uint targetHandle, uint targetId) {
		base.Activate(targetHandle, targetId);
		bbf = new BassboostFilter();
		bbf.setParams(Boost);
	}

	public override void InitializeParams() {
		AudioManager.Soloud.setFilterParameter(TargetHandle, TargetId, (uint)Params.Wet, wet);
		AudioManager.Soloud.setFilterParameter(TargetHandle, TargetId, (uint)Params.Boost, boost);
	}

	protected override void UpdateLiveWet() {
		AudioManager.Soloud.setFilterParameter(TargetHandle, TargetId, (uint)Params.Wet, wet);
	}

	protected override void ReadLiveWet() {
		wet = AudioManager.Soloud.getFilterParameter(TargetHandle, TargetId, (uint)Params.Wet);
	}

	public override void Deactivate() {
		base.Deactivate();
		bbf.Dispose();
		bbf = null;
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
