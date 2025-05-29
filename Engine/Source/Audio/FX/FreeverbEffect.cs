using SoLoud;
using System;

namespace Engine.Audio.FX;

public class FreeverbEffect : Effect {
	public enum Params : uint {
		Wet = FreeverbFilter.WET,
		Freeze = FreeverbFilter.FREEZE,
		RoomSize = FreeverbFilter.ROOMSIZE,
		Damp = FreeverbFilter.DAMP,
		Width = FreeverbFilter.WIDTH
	}

	private FreeverbFilter? ff;
	public override SoloudObject? EffectObject => ff;
	private float freeze;
	/// <summary>
	/// The freeze parameter of the effect
	/// </summary>
	public float Freeze {
		get {
			if (Active) {
				freeze = AudioManager.Soloud.getFilterParameter(TargetHandle, TargetId, (uint)Params.Freeze);
			}
			return freeze;
		}
		set {
			freeze = value;
			if (freeze < 0f) {
				freeze = 0f;
			}
			if (freeze > 1f) {
				freeze = 1f;
			}
			if (Active) {
				AudioManager.Soloud.setFilterParameter(TargetHandle, TargetId, (uint)Params.Freeze, freeze);
			}
		}
	}
	private float roomSize;
	/// <summary>
	/// The room size parameter of the effect
	/// </summary>
	public float RoomSize {
		get {
			if (Active) {
				roomSize = AudioManager.Soloud.getFilterParameter(TargetHandle, TargetId, (uint)Params.RoomSize);
			}
			return roomSize;
		}
		set {
			roomSize = value;
			if (roomSize < 0f) {
				roomSize = 0f;
			}
			if (roomSize > 1f) {
				roomSize = 1f;
			}
			if (Active) {
				AudioManager.Soloud.setFilterParameter(TargetHandle, TargetId, (uint)Params.RoomSize, roomSize);
			}
		}
	}
	private float damp;
	/// <summary>
	/// The damp parameter of the effect
	/// </summary>
	public float Damp {
		get {
			if (Active) {
				damp = AudioManager.Soloud.getFilterParameter(TargetHandle, TargetId, (uint)Params.Damp);
			}
			return damp;
		}
		set {
			damp = value;
			if (damp < 0f) {
				damp = 0f;
			}
			if (damp > 1f) {
				damp = 1f;
			}
			if (Active) {
				AudioManager.Soloud.setFilterParameter(TargetHandle, TargetId, (uint)Params.Damp, damp);
			}
		}
	}
	private float width;
	/// <summary>
	/// The width parameter of the effect
	/// </summary>
	public float Width {
		get {
			if (Active) {
				width = AudioManager.Soloud.getFilterParameter(TargetHandle, TargetId, (uint)Params.Width);
			}
			return width;
		}
		set {
			width = value;
			if (width < 0f) {
				width = 0f;
			}
			if (width > 1f) {
				width = 1f;
			}
			if (Active) {
				AudioManager.Soloud.setFilterParameter(TargetHandle, TargetId, (uint)Params.Width, width);
			}
		}
	}

	public FreeverbEffect(float freeze = 0f, float roomSize = .5f, float damp = .5f, float width = .5f, float wet = 1f) : base(wet) {
		Freeze = freeze;
		RoomSize = roomSize;
		Damp = damp;
		Width = width;
	}

	public override void Activate(uint targetHandle, uint targetId) {
		base.Activate(targetHandle, targetId);
		ff = new FreeverbFilter();
		ff.setParams(Freeze, RoomSize, damp, width);
	}

	public override void InitializeParams() {
		AudioManager.Soloud.setFilterParameter(TargetHandle, TargetId, (uint)Params.Wet, wet);
		AudioManager.Soloud.setFilterParameter(TargetHandle, TargetId, (uint)Params.Freeze, freeze);
		AudioManager.Soloud.setFilterParameter(TargetHandle, TargetId, (uint)Params.RoomSize, roomSize);
		AudioManager.Soloud.setFilterParameter(TargetHandle, TargetId, (uint)Params.Damp, damp);
		AudioManager.Soloud.setFilterParameter(TargetHandle, TargetId, (uint)Params.Width, width);
	}

	protected override void UpdateLiveWet() {
		AudioManager.Soloud.setFilterParameter(TargetHandle, TargetId, (uint)Params.Wet, wet);
	}

	protected override void ReadLiveWet() {
		wet = AudioManager.Soloud.getFilterParameter(TargetHandle, TargetId, (uint)Params.Wet);
	}

	public override void Deactivate() {
		base.Deactivate();
		ff?.Dispose();
		ff = null;
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
