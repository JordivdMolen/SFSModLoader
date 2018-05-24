using System;
using NewBuildSystem;
using UnityEngine;
using UnityEngine.UI;

public class RocketPost : MonoBehaviour
{
	public void HidePage()
	{
		base.gameObject.SetActive(false);
	}

	public void PlayAnimation()
	{
		this.anim.SetTargetTime(1f);
		this.anim.SetTime(0f);
	}

	public void LoadPage(string title, string rocketId, int score, bool hasUpVoted, bool hasDownVoted, bool canDelete)
	{
		if (!base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(true);
		}
		this.titleText.text = title;
		this.idText.text = rocketId;
		this.scoreText.text = this.GetScoreString(score);
		if (this.upVoteHighlight.gameObject.activeSelf != hasUpVoted)
		{
			this.upVoteHighlight.gameObject.SetActive(hasUpVoted);
		}
		if (this.downVoteHightlight.gameObject.activeSelf != hasDownVoted)
		{
			this.downVoteHightlight.gameObject.SetActive(hasDownVoted);
		}
		if (this.deleteButton.gameObject.activeSelf != canDelete)
		{
			this.deleteButton.gameObject.SetActive(canDelete);
		}
	}

	public void LoadImage(string json)
	{
		Texture2D texture2D = Build.CreatePreviewIcon(json);
		this.previewImage.sprite = Sprite.Create(texture2D, new Rect(0f, 0f, (float)texture2D.width, (float)texture2D.height), new Vector2(0.5f, 0.5f));
	}

	public void Show()
	{
		base.gameObject.SetActive(true);
	}

	public void SetID(string id)
	{
		this.idText.text = id;
	}

	public void SetTitle(string title)
	{
		this.titleText.text = title;
	}

	public void SetScore(int score)
	{
		this.scoreText.text = this.GetScoreString(score);
	}

	public void SetUpVoted(bool has)
	{
		this.upVoteHighlight.gameObject.SetActive(has);
	}

	public void SetDownVoted(bool has)
	{
		this.downVoteHightlight.gameObject.SetActive(has);
	}

	public void SetDeletable(bool able)
	{
		this.deleteButton.gameObject.SetActive(able);
	}

	private string GetScoreString(int score)
	{
		if (score < 1000)
		{
			return score.ToString();
		}
		if (score < 10000)
		{
			return string.Concat(new object[]
			{
				(score / 1000).ToString(),
				".",
				score / 100 % 10,
				"k"
			});
		}
		return (score / 1000).ToString() + "k";
	}

	public string ID = string.Empty;

	public Text titleText;

	public Text idText;

	public Text scoreText;

	public GameObject upVoteHighlight;

	public GameObject downVoteHightlight;

	public GameObject deleteButton;

	public Image previewImage;

	public MoveModule anim;
}
