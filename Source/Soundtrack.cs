using System;
using Sirenix.OdinInspector;
using UnityEngine;

public class Soundtrack : MonoBehaviour
{
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
		bool flag = Ref.currentScene == Ref.SceneType.Game;
		if (flag)
		{
			this.playingSoundtrack = (Ref.mainVessel != null && (Ref.mainVesselHeight > Ref.controller.loadedPlanet.atmosphereData.atmosphereHeightM || Ref.controller.loadedPlanet.bodyName != Ref.controller.startAdress));
		}
		bool flag2 = this.playingSoundtrack;
		if (flag2)
		{
			bool flag3 = !this.audioSource.isPlaying && !base.IsInvoking("SelectRandomSoundtrack");
			if (flag3)
			{
				base.Invoke("SelectRandomSoundtrack", 1f);
			}
			bool flag4 = this.index != -1 && this.audioSource.volume < this.soundtracks[this.index].volume;
			if (flag4)
			{
				this.audioSource.volume = Mathf.Min(this.audioSource.volume + Time.deltaTime * 0.1f * this.soundtracks[this.index].volume, this.soundtracks[this.index].volume);
			}
		}
		else
		{
			bool isPlaying = this.audioSource.isPlaying;
			if (isPlaying)
			{
				bool flag5 = this.audioSource.volume > 0f;
				if (flag5)
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

	public Soundtrack()
	{
	}

	[Space]
	[TableList]
	public Soundtrack.SoundtrackPiece[] soundtracks;

	[Header(" Delay between soundtracks")]
	public float delay;

	[Space]
	public AudioSource audioSource;

	public bool playingSoundtrack;

	public int index;

	[Serializable]
	public class SoundtrackPiece
	{
		public SoundtrackPiece()
		{
		}

		public AudioClip soundtracks;

		[Range(0f, 3f)]
		public float pitch = 1f;

		[Range(0f, 3f)]
		public float volume = 0.25f;

		public bool enabled = true;
	}
}
