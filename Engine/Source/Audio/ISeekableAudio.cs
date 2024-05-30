using System;

namespace Engine.Audio;

public interface ISeekableAudio {
	public abstract void Seek(double position);

	public abstract void Seek(TimeSpan position);
}
