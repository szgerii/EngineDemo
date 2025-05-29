using SoLoud;

namespace Engine.Audio.FX;

public abstract class Effect {
	public abstract SoloudObject? EffectObject { get; }
	public uint TargetHandle { get; set; } = 0;
	public uint TargetId { get; set; } = 0;
	public bool Active { get; set; } = false;
	protected float wet;
	/// <summary>
	/// The dry/wet signal strength of the effect
	/// 1 means wet only (affected only)
	/// 0 means dry only (original only)
	/// </summary>
	public float Wet {
		get {
			if (Active) {
				ReadLiveWet();
			}
			return wet;
		}
		set {
			wet = value;
			if (wet < 0f) {
				wet = 0f;
			}
			if (wet > 1f) {
				wet = 1f;
			}
			if (Active) {
				UpdateLiveWet();
			}
		}
	}

	public Effect(float wet = 1f) {
		this.wet = wet;
	}

	public virtual void Activate(uint targetHandle, uint targetId) {
		TargetHandle = targetHandle;
		TargetId = targetId;
	}

	protected abstract void UpdateLiveWet();

	protected abstract void ReadLiveWet();

	public abstract void InitializeParams();

	public virtual void Deactivate() {
		TargetHandle = 0;
		TargetId = 0;
		Active = false;
	}
}
