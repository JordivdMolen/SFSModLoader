using Sirenix.OdinInspector;
using System;
using UnityEngine;

public class Soundtrack : MonoBehaviour
{
	[Serializable]
	public class SoundtrackPiece
	{
		public AudioClip soundtracks;

		[Range(0f, 3f)]
		public float pitch = 1f;

		[Range(0f, 3f)]
		public float volume = 0.25f;

		public bool enabled = true;
	}

	[TableList, Space]
	public Soundtrack.SoundtrackPiece[] soundtracks;

	[Header(" Delay between soundtracks")]
	public float delay;

	[Space]
	public AudioSource audioSource;

	public bool playingSoundtrack;

	public int index;

	public void SelectRandomSoundtrack()
	{
		int num = this.index;
		while (num == this.index || !this.soundtracks[num].enabled)
		{
			num = UnityEngine.Random.Range(0, this.soundtracks.Length);
		}
		this.index = num;
		AudioClip audioClip = this.soundtracks[this.index].soundtracks;
		this.audioSource.pitch = this.soundtracks[this.index].pitch;
		this.audioSource.clip = audioClip;
		this.audioSource.Play();
		base.Invoke("SelectRandomSoundtrack", audioClip.length / this.audioSource.pitch + this.delay);
	}

	private void Update()
	{
		if (Ref.currentScene == Ref.SceneType.Game)
		{
			this.playingSoundtrack = (Ref.mainVessel != null && (Ref.mainVesselHeight > Ref.controller.loadedPlanet.atmosphereData.atmosphereHeightM || Ref.controller.loadedPlanet.bodyName != Ref.controller.startAdress));
		}
		if (this.playingSoundtrack)
		{
			if (!this.audioSource.isPlaying && !base.IsInvoking("SelectRandomSoundtrack"))
			{
				base.Invoke("SelectRandomSoundtrack", 1f);
			}
			if (this.index != -1 && this.audioSource.volume < this.soundtracks[this.index].volume)
			{
				this.audioSource.volume = Mathf.Min(this.audioSource.volume + Time.deltaTime * 0.1f * this.soundtracks[this.index].volume, this.soundtracks[this.index].volume);
			}
		}
		else if (this.audioSource.isPlaying)
		{
			if (this.audioSource.volume > 0f)
			{
				this.audioSource.volume -= Time.deltaTime * 0.1f * this.soundtracks[this.index].volume;
			}
			else
			{
				this.audioSource.Stop();
				base.CancelInvoke("SelectRandomSoundtrack");
			}
		}
	}
}
