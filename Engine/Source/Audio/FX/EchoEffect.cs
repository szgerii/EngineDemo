using SoLoud;
using System;

namespace Engine.Audio.FX;

/// <summary>
/// A primitive echo effect
/// </summary>
public class EchoEffect : Effect {
	public enum Params {
		Wet = EchoFilter.WET,
		Delay = EchoFilter.DELAY,
		Decay = EchoFilter.DECAY,
		Filter = EchoFilter.FILTER
	}
	private EchoFilter? ef;
	public override SoloudObject? EffectObject => ef;
	private float delay;
	/// <summary>
	/// The ammount of delay (in seconds)
	/// NOTE: this value won't update in live audio.
	/// If you want a better reverb effect, implement freeverb, have fun.
	/// </summary>
	public float Delay {
		get {
			if (Active) {
				delay = AudioManager.Soloud.getFilterParameter(TargetHandle, TargetId, (uint)Params.Delay);
			}
			return delay;
		}
		set {
			delay = value;
			if (delay < 0.000001f) {
				delay = 0.000001f;
			}
			if (Active) {
				AudioManager.Soloud.setFilterParameter(TargetHandle, TargetId, (uint)Params.Delay, delay);
			}
		}
	}
	private float decay;
	/// <summary>
	/// The multiplier on the echoed sound, the closer the value is to 1, the heavier the echoed sound
	/// DO NOT set it to 1, if you want to preserve your hearing.
	/// </summary>
	public float Decay {
		get {
			if (Active) {
				decay = AudioManager.Soloud.getFilterParameter(TargetHandle, TargetId, (uint)Params.Decay);
			}
			return decay;
		}
		set {
			decay = value;
			if (decay < 0f) {
				decay = 0f;
			}
			if (decay > 1f) {
				decay = 1f;
			}
			if (Active) {
				AudioManager.Soloud.setFilterParameter(TargetHandle, TargetId, (uint)Params.Decay, decay);
			}
		}
	}
	private float filter;
	/// <summary>
	/// The filter parameter of the effect
	/// </summary>
	public float Filter {
		get {
			if (Active) {
				filter = AudioManager.Soloud.getFilterParameter(TargetHandle, TargetId, (uint)Params.Filter);
			}
			return filter;
		}
		set {
			filter = value;
			if (filter < 0f) {
				filter = 0f;
			}
			if (filter > 1f) {
				filter = 1f;
			}
			if (Active) {
				AudioManager.Soloud.setFilterParameter(TargetHandle, TargetId, (uint)Params.Filter, filter);
			}
		}
	}

	public EchoEffect(float delay = .01f, float decay = .5f, float filter = 0f, float wet = 1f) : base(wet) {
		Delay = delay;
		Decay = decay;
		Filter = filter;
	}

	public override void Activate(uint targetHandle, uint targetId) {
		base.Activate(targetHandle, targetId);
		ef = new EchoFilter();
		ef.setParams(Delay, Decay, Filter);
	}

	public override void InitializeParams() {
		AudioManager.Soloud.setFilterParameter(TargetHandle, TargetId, (uint)Params.Wet, wet);
		AudioManager.Soloud.setFilterParameter(TargetHandle, TargetId, (uint)Params.Delay, delay);
		AudioManager.Soloud.setFilterParameter(TargetHandle, TargetId, (uint)Params.Decay, decay);
		AudioManager.Soloud.setFilterParameter(TargetHandle, TargetId, (uint)Params.Filter, filter);
	}

	protected override void UpdateLiveWet() {
		AudioManager.Soloud.setFilterParameter(TargetHandle, TargetId, (uint)Params.Wet, wet);
	}

	protected override void ReadLiveWet() {
		wet = AudioManager.Soloud.getFilterParameter(TargetHandle, TargetId, (uint)Params.Wet);
	}

	public override void Deactivate() {
		base.Deactivate();
		ef?.Dispose();
		ef = null;
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
