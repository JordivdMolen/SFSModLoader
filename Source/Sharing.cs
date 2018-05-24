using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Assets;
using JMtech.JDIS.Web.Request;
using JMtech.Main;
using JMtech.Translations;
using NewBuildSystem;
using ServerControlFramework;
using ServerControlSoftware.Externals.Shared.Objects;
using ServerControlSoftware.Externals.Shared.Packets;
using ServerControlSoftware.Externals.Shared.RequestPackets;
using ServerControlSoftware.Externals.Shared.ResponsePacket;
using ServerTransferProgram.ServerControlLiberary.DataControllers.PacketSystem;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class Sharing : MonoBehaviour
{
	public Sharing()
	{
		Sharing.context = SynchronizationContext.Current;
	}

	private void Awake()
	{
		Sharing.context = SynchronizationContext.Current;
		Sharing.sharing = this;
	}

	private bool PreCheck()
	{
		if (!this.CheckConnection())
		{
			return false;
		}
		if (!GameTracker.Tracker.client.rec.isConnected())
		{
			MsgController.ShowMsg(SFST.T.Sharing_No_Server);
			this.downloadMenu.gameObject.SetActive(false);
			GameTracker.Tracker.client.rec.Delete();
			GameTracker.Tracker.client = null;
			GameTracker.Tracker.ClientInitiated = false;
			return false;
		}
		return true;
	}

	public void NextCategory()
	{
		this.categoryText.text = this.categorys[(this.GetCurrentCategoryIndex() + 1) % (this.categorys.Length - 1)];
	}

	public void PreviousCategory()
	{
		int currentCategoryIndex = this.GetCurrentCategoryIndex();
		if (currentCategoryIndex == 0)
		{
			this.categoryText.text = this.categorys[this.categorys.Length - 1];
		}
		else
		{
			this.categoryText.text = this.categorys[currentCategoryIndex - 1];
		}
	}

	private int GetCurrentCategoryIndex()
	{
		for (int i = 0; i < this.categorys.Length; i++)
		{
			if (this.categorys[i] == this.categoryText.text)
			{
				return i;
			}
		}
		return 0;
	}

	public bool CheckConnection()
	{
		if (Application.internetReachability == NetworkReachability.NotReachable)
		{
			MsgController.ShowMsg(SFST.T.Sharing_No_Connection);
			return false;
		}
		this.latestInfo = MasterInfo.Fetch();
		if (this.latestInfo == null)
		{
			if (Sharing.mgcache0 == null)
			{
				Sharing.mgcache0 = new Action<string>(MsgController.ShowMsg);
			}
			SyncContext.RunOnUI<string>(Sharing.mgcache0, SFST.T.STP_Request_Timeout);
			return false;
		}
		int num = VersionHolder.SharingVersion.CompareVersion(this.latestInfo.Version.Sharing.Client);
		int num2 = VersionHolder.ProtocolVersion.CompareVersion(this.latestInfo.Version.Sharing.Protocol);
		if (num == -1)
		{
			MsgController.ShowMsg(SFST.T.Sharing_Old_Version);
			return false;
		}
		if (num == 1)
		{
			MsgController.ShowMsg(SFST.T.Sharing_New_Version);
			return false;
		}
		if (num2 == -1)
		{
			MsgController.ShowMsg(SFST.T.Sharing_Old_Protocol);
			return false;
		}
		if (num2 == 1)
		{
			MsgController.ShowMsg(SFST.T.Sharing_New_Protocol);
			return false;
		}
		return true;
	}

	public void Upload()
	{
		if (!this.CheckConnection())
		{
			return;
		}
		string text = TouchKeyboard.main.GetText(string.Empty);
		if (text == string.Empty)
		{
			return;
		}
		string buildJson = Build.main.GetBuildJson(text);
		UploadRocketRequest uploadRocketRequest = new UploadRocketRequest();
		uploadRocketRequest.toUpload = new CLRocket
		{
			Title = text,
			Description = "Uploaded before descriptions where supported.",
			JSON = buildJson
		};
		GameTracker.Tracker.client.AuthedRequest(uploadRocketRequest, delegate(DefaultPacket r)
		{
			SyncContext.RunOnUI(delegate
			{
				if (r.Success)
				{
					this.onUploaded.InvokeEvenets();
					MsgController.ShowMsg(SFST.T.Sharing_Upload_Success);
				}
				else
				{
					ServerErrorPacket serverErrorPacket = r.ToTargetPacket<ServerErrorPacket>();
					if (serverErrorPacket.Task == "OP_UPLOAD_LIMIT")
					{
						this.onUploadFailed.InvokeEvenets();
						MsgController.ShowMsg(serverErrorPacket.Error);
					}
					else
					{
						this.onUploadFailed.InvokeEvenets();
						MsgController.ShowMsg(SFST.T.Sharing_Upload_Failed);
					}
				}
			});
		});
	}

	public void OpenUpload()
	{
		if (!this.CheckConnection())
		{
			return;
		}
		if (!GameTracker.Tracker.ClientInitiated)
		{
			GameTracker.Tracker.StartConnection(delegate
			{
				this.openUploadMenu.InvokeEvenets();
			});
		}
		else
		{
			this.openUploadMenu.InvokeEvenets();
		}
	}

	public void CloseBuild()
	{
	}

	public void OpenDownloadMenu()
	{
		if (!this.CheckConnection())
		{
			return;
		}
		if (!GameTracker.Tracker.ClientInitiated)
		{
			GameTracker.Tracker.StartConnection(delegate
			{
				Thread.Sleep(1000);
				this.ShowActualPage();
			});
		}
		else
		{
			this.ShowActualPage();
		}
	}

	private void HidePosts()
	{
		foreach (RocketPost rocketPost in this.rocketPostReference)
		{
			rocketPost.HidePage();
		}
	}

	private void ShowActualPage()
	{
		this.HidePosts();
		SyncContext.RunOnUI<bool>(new Action<bool>(this.downloadMenu.SetActive), true);
		this.LoadPage(TrendCategory.NEW, 0, delegate
		{
			SyncContext.RunOnUI(new Action(this.onSelectedNew.InvokeEvenets));
		}, delegate(string res)
		{
			this.onSelectedNewFail.InvokeEvenets();
			this.categoryEmpty.gameObject.SetActive(true);
			this.onCategorySwitchFail.InvokeEvenets();
		});
	}

	private void Update()
	{
	}

	public void LoadPage(string category, int pageNum, Action doAfter, Action<string> onFail = null)
	{
		if (!this.CheckConnection())
		{
			return;
		}
		if (this.requestingPage)
		{
			return;
		}
		this.requestingPage = true;
		this.pages.Clear();
		ThreadPool.QueueUserWorkItem(delegate(object s)
		{
			CategoryRequestPacket toSend = new CategoryRequestPacket
			{
				Page = pageNum,
				Category = new SharingCategory(category)
			};
			GameTracker.Tracker.client.AuthedRequest(toSend, delegate(DefaultPacket r)
			{
				Debug.Log("Got response!");
				if (r.Success)
				{
					CategoryResponsePacket categoryResponsePacket = r.ToTargetPacket<CategoryResponsePacket>();
					this.pagesIDS = categoryResponsePacket.onPage;
					for (int i = 0; i < this.rocketPostReference.Length; i++)
					{
						int unreffedNum = int.Parse(i.ToString());
						RocketPost post = this.rocketPostReference[i];
						if (i >= categoryResponsePacket.onPage.Count)
						{
							SyncContext.RunOnUI(new Action(post.HidePage));
						}
						else
						{
							string targetRocket = categoryResponsePacket.onPage[i];
							DownloadRequestPacket toSend2 = new DownloadRequestPacket
							{
								targetRocket = targetRocket
							};
							GameTracker.Tracker.client.AuthedRequest(toSend2, delegate(DefaultPacket resp)
							{
								SyncContext.RunOnUI(delegate
								{
									if (resp.Success)
									{
										RocketResponsePacket rocketResponsePacket = resp.ToTargetPacket<RocketResponsePacket>();
										CLRocket response = rocketResponsePacket.response;
										this.pages[unreffedNum] = response;
										Debug.Log(string.Concat(new object[]
										{
											"setting ",
											unreffedNum,
											" to ",
											response.ID
										}));
										post.LoadPage(response.Title, response.ID, response.Score, response.LocalUpvote, response.LocalDownvote, response.CanDelete);
										post.LoadImage(response.JSON);
									}
								});
							});
						}
					}
					SyncContext.RunOnUI(doAfter);
				}
				else
				{
					SyncContext.RunOnUI(new Action(this.HidePosts));
					Debug.Log(r.Error);
					if (onFail != null)
					{
						SyncContext.RunOnUI<string>(onFail, r.Error);
					}
				}
				this.requestingPage = false;
				this.currentCategory = category;
			});
		});
	}

	public void RequestNextPage()
	{
		if (!this.PreCheck())
		{
			return;
		}
		this.LoadPage(this.currentCatgory, this.pageNumber, delegate
		{
			this.onNextPageLoaded.InvokeEvenets();
		}, delegate(string e)
		{
			MsgController.ShowMsg(string.Empty);
		});
	}

	public void RequestPreviousPage()
	{
		if (!this.PreCheck())
		{
			return;
		}
		if (this.pageNumber - 2 < 0)
		{
			return;
		}
		this.LoadPage(this.currentCatgory, this.pageNumber - 2, delegate
		{
			this.onPreviousPageLoaded.InvokeEvenets();
		}, delegate(string e)
		{
			MsgController.ShowMsg(string.Empty);
		});
	}

	public void InitCategoryBackend(string category, Action doAfter)
	{
		this.pageNumber = 1;
		this.UpdatePageNum();
		this.LoadPage(category, this.pageNumber - 1, delegate
		{
			this.categoryEmpty.gameObject.SetActive(false);
			if (category == TrendCategory.TOP)
			{
				this.currentCatgory = category;
				this.onSelectedTop.InvokeEvenets();
			}
			else if (category == TrendCategory.HOT)
			{
				this.currentCatgory = category;
				this.onSelectedTrending.InvokeEvenets();
			}
			else if (category == TrendCategory.OWN)
			{
				this.currentCatgory = category;
				this.onSelectedMyRockets.InvokeEvenets();
			}
			else if (category == TrendCategory.NEW)
			{
				this.currentCatgory = category;
				this.onSelectedNew.InvokeEvenets();
			}
			doAfter();
		}, delegate(string res)
		{
			if (category == TrendCategory.TOP)
			{
				this.onSelectedTopFail.InvokeEvenets();
			}
			else if (category == TrendCategory.HOT)
			{
				this.onSelectedTrendingFail.InvokeEvenets();
			}
			else if (category == TrendCategory.OWN)
			{
				this.onSelectedMyRocketsFail.InvokeEvenets();
			}
			else if (category == TrendCategory.NEW)
			{
				this.onSelectedNewFail.InvokeEvenets();
			}
			this.categoryEmpty.gameObject.SetActive(true);
			this.onCategorySwitchFail.InvokeEvenets();
		});
	}

	public void ReloadPage(Action doafter = null)
	{
		this.LoadPage(this.currentCategory, this.pageNumber - 1, delegate
		{
			if (doafter != null)
			{
				doafter();
			}
		}, delegate(string s)
		{
			this.categoryEmpty.gameObject.SetActive(true);
			this.onCategorySwitchFail.InvokeEvenets();
		});
	}

	public void InitCategory(string category)
	{
		if (!this.PreCheck())
		{
			return;
		}
		this.InitCategoryBackend(category, delegate
		{
		});
	}

	public void UpdatePageNum()
	{
		this.pageNumberText.text = this.pageNumber.ToString();
	}

	public void NextPage()
	{
		if (this.pageNumber == 100)
		{
			return;
		}
		this.pageNumber++;
		this.pageNumberText.text = this.pageNumber.ToString();
		if (this.pageNumber == 100)
		{
			this.nextPageButton.color = new Color(1f, 1f, 1f, 0.5f);
		}
		if (this.pageNumber == 2)
		{
			this.previousPageButton.color = Color.white;
		}
	}

	public void PreviousPage()
	{
		if (this.pageNumber == 1)
		{
			return;
		}
		this.pageNumber--;
		this.pageNumberText.text = this.pageNumber.ToString();
		if (this.pageNumber == 1)
		{
			this.previousPageButton.color = new Color(1f, 1f, 1f, 0.5f);
		}
		if (this.pageNumber == 99)
		{
			this.nextPageButton.color = Color.white;
		}
	}

	public void Upvote(int tabIndex)
	{
		if (!this.PreCheck())
		{
			return;
		}
		if (this.Upvoting)
		{
			MsgController.ShowMsg(SFST.T.Sharing_Wait_Request);
			return;
		}
		string ID = this.pages[tabIndex].ID;
		this.Upvoting = true;
		VotePacket toSend = new VotePacket
		{
			vote = true,
			voteOn = ID
		};
		GameTracker.Tracker.client.AuthedRequest(toSend, delegate(DefaultPacket r)
		{
			SyncContext.RunOnUI(delegate
			{
				if (r.Success)
				{
					VoteResponse voteResponse = r.ToTargetPacket<VoteResponse>();
					Debug.Log(voteResponse.Action);
					if (voteResponse.Action == "VOTE_PLACED")
					{
						MsgController.ShowMsg(SFST.T.Sharing_Vote_Placed);
					}
					else
					{
						MsgController.ShowMsg(SFST.T.Sharing_Vote_Revoked);
					}
					this.ReloadPage(delegate
					{
						RocketPost post = this.GetPost(ID);
						if (post != null)
						{
							post.PlayAnimation();
						}
					});
				}
				else
				{
					MsgController.ShowMsg(SFST.T.Sharing_Vote_Failed);
				}
				this.Upvoting = false;
			});
		});
	}

	public void Downvote(int tabIndex)
	{
		if (!this.PreCheck())
		{
			return;
		}
		if (this.Downvoting)
		{
			MsgController.ShowMsg(SFST.T.Sharing_Wait_Request);
			return;
		}
		string ID = this.pages[tabIndex].ID;
		this.Downvoting = true;
		VotePacket toSend = new VotePacket
		{
			vote = false,
			voteOn = ID
		};
		GameTracker.Tracker.client.AuthedRequest(toSend, delegate(DefaultPacket r)
		{
			SyncContext.RunOnUI(delegate
			{
				if (r.Success)
				{
					VoteResponse voteResponse = r.ToTargetPacket<VoteResponse>();
					if (voteResponse.Action == "VOTE_PLACED")
					{
						MsgController.ShowMsg(SFST.T.Sharing_Vote_Placed);
					}
					else
					{
						MsgController.ShowMsg(SFST.T.Sharing_Vote_Revoked);
					}
					this.ReloadPage(delegate
					{
						RocketPost post = this.GetPost(ID);
						if (post != null)
						{
							post.PlayAnimation();
						}
					});
				}
				else
				{
					MsgController.ShowMsg(SFST.T.Sharing_Vote_Failed);
				}
				this.Downvoting = false;
			});
		});
	}

	public void Download(int tabIndex)
	{
		if (!this.PreCheck())
		{
			return;
		}
		MonoBehaviour.print("Downloaded " + tabIndex);
		CLRocket clrocket = this.pages[tabIndex];
		Build.main.LoadSave(JsonUtility.FromJson<Build.BuildSave>(clrocket.JSON));
		this.downloadMenu.gameObject.SetActive(false);
	}

	public void Delete(int tabIndex)
	{
		if (!this.PreCheck())
		{
			return;
		}
		MonoBehaviour.print("Deleted " + tabIndex);
		if (this.Deleting)
		{
			MsgController.ShowMsg(SFST.T.Sharing_Wait_Request);
			return;
		}
		this.Deleting = true;
		DeletePacket toSend = new DeletePacket
		{
			Target = this.pagesIDS[tabIndex]
		};
		GameTracker.Tracker.client.AuthedRequest(toSend, delegate(DefaultPacket r)
		{
			SyncContext.RunOnUI(delegate
			{
				if (r.Success)
				{
					MsgController.ShowMsg("Rocket deleted");
					this.ReloadPage(null);
				}
				else
				{
					MsgController.ShowMsg("Could not delete rocket because:\n" + r.Error);
				}
				this.Deleting = false;
			});
		});
	}

	public RocketPost GetPost(string ID)
	{
		foreach (RocketPost rocketPost in this.rocketPostReference)
		{
			if (rocketPost.ID == ID)
			{
				return rocketPost;
			}
		}
		return null;
	}

	[FoldoutGroup("Uploading", 0)]
	public Text titleText;

	[FoldoutGroup("Uploading", 0)]
	public Text categoryText;

	[FoldoutGroup("Uploading", 0)]
	public string[] categorys;

	[FoldoutGroup("Uploading", 0)]
	[Space]
	public CustomEvent onUploaded;

	[FoldoutGroup("Uploading", 0)]
	public CustomEvent onUploadFailed;

	[FoldoutGroup("Uploading", 0)]
	public CustomEvent openUploadMenu;

	[FoldoutGroup("Downloading Menu", 0)]
	public GameObject downloadMenu;

	public static Sharing sharing;

	[FoldoutGroup("Downloading Menu/Events", 0)]
	[Title("Finishing Events", "Called when the entire callback (Async) is completed.", TitleAlignments.Left, true, true)]
	[Space]
	public CustomEvent onSelectedNew;

	[FoldoutGroup("Downloading Menu/Events", 0)]
	public CustomEvent onSelectedTrending;

	[FoldoutGroup("Downloading Menu/Events", 0)]
	public CustomEvent onSelectedTop;

	[FoldoutGroup("Downloading Menu/Events", 0)]
	public CustomEvent onSelectedMyRockets;

	[FoldoutGroup("Downloading Menu/Events", 0)]
	public CustomEvent onNextPageLoaded;

	[FoldoutGroup("Downloading Menu/Events", 0)]
	public CustomEvent onPreviousPageLoaded;

	[FoldoutGroup("Downloading Menu/Events", 0)]
	public CustomEvent onCategoryLoaded;

	[FoldoutGroup("Downloading Menu/Events", 0)]
	[Title("RequestFail", "Called when a request failed", TitleAlignments.Left, true, true)]
	[Space]
	public CustomEvent onCategorySwitchFail;

	[FoldoutGroup("Downloading Menu/Events", 0)]
	public CustomEvent onSelectedTrendingFail;

	[FoldoutGroup("Downloading Menu/Events", 0)]
	public CustomEvent onSelectedTopFail;

	[FoldoutGroup("Downloading Menu/Events", 0)]
	public CustomEvent onSelectedMyRocketsFail;

	[FoldoutGroup("Downloading Menu/Events", 0)]
	public CustomEvent onSelectedNewFail;

	[FoldoutGroup("Downloading Menu/Events", 0)]
	public CustomEvent onRocketLoadFail;

	[FoldoutGroup("Downloading Menu/Pages", 0)]
	[ShowInInspector]
	private int pageNumber = 1;

	[FoldoutGroup("Downloading Menu/Pages", 0)]
	public Text pageNumberText;

	[FoldoutGroup("Downloading Menu/Pages", 0)]
	public Image previousPageButton;

	[FoldoutGroup("Downloading Menu/Pages", 0)]
	public Image nextPageButton;

	[FoldoutGroup("Downloading Menu", 0)]
	[Space]
	public Text categoryEmpty;

	[FoldoutGroup("Downloading Menu", 0)]
	public RocketPost[] rocketPostReference;

	[FoldoutGroup("Downloading Menu/Data Storage and Debug", 0)]
	public string currentCatgory = TrendCategory.NEW;

	public static SynchronizationContext context = SynchronizationContext.Current;

	public bool requestingPage;

	public Dictionary<int, CLRocket> pages = new Dictionary<int, CLRocket>();

	public List<string> pagesIDS = new List<string>();

	public bool donePreloading;

	public Action afterPreloading;

	public string currentCategory;

	private MasterInfo latestInfo;

	private bool Upvoting;

	private bool Downvoting;

	private bool Deleting;

	[CompilerGenerated]
	private static Action<string> mgcache0;
}
