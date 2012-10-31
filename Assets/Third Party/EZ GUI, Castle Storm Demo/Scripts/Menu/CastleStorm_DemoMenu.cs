using UnityEngine;
using System.Collections;

public class CastleStorm_DemoMenu : MonoBehaviour 
{
	public Camera wizardCam;			// Camera to use for our wizard.
	public GameObject fogParent;		// Parent object of all our fog sprites
	public UIScrollList mapList;		// Our map scroll list
	public GameObject listItemPrefab;	// Prefab for our map list items
	public UIPanelManager mainMenu;		// Our main menu manager
	public UIPanelManager wizard;		// Our wizard menu manager
	public UIScrollList teamAList;		// Our player Team A list
	public UIScrollList teamBList;		// Our player Team B list
	public GameObject playerItemPrefab;	// Prefab for our player list items
	

	// Use this for initialization
	void Start () 
	{
		// Do our intro-zoom in 1 second
		Invoke("Begin", 1f);
		
//		// Dummy values for our player team A list:
//		teamAList.CreateItem(playerItemPrefab, "fAn_bOi");
//		teamAList.CreateItem(playerItemPrefab, "-=Devastator=-");
//		teamAList.CreateItem(playerItemPrefab, "Phungai");
//		teamAList.CreateItem(playerItemPrefab, "trooper729");
//		teamAList.CreateItem(playerItemPrefab, "chili");
//
//		// Dummy values for our player team B list:
//		teamBList.CreateItem(playerItemPrefab, "sn00p");
//		teamBList.CreateItem(playerItemPrefab, "Galius");
//		teamBList.CreateItem(playerItemPrefab, "_gamr_grrl_");
	}
	
	public void Begin()
	{
		// Do our initial intro-zoom at start
		mainMenu.BringIn("Main Panel");
		
		// Move our fog to match:
		MoveForward();
	}
	
	// This is invoked by the "Host Multiplayer Game" button.
	public void StartWizard()
	{
		// Bring our initial wizard page in
		wizard.BringIn("Page 1");
		
		UIManager.instance.uiCameras[0].camera = wizardCam;
		
		// Move our fog to match:
		MoveRight();
	}
	
	// Methods that move our fog in a specific direction.
	// These are invoked by our panel change buttons.
	public void MoveForward()
	{
		AnimatePosition.Do(fogParent, EZAnimation.ANIM_MODE.By, Vector3.forward * -10f, EZAnimation.sinusInOut, 1.5f, 0, null, null);
	}

	public void MoveBackward()
	{
		AnimatePosition.Do(fogParent, EZAnimation.ANIM_MODE.By, Vector3.forward * 10f, EZAnimation.sinusInOut, 1.5f, 0, null, null);
	}
	
	public void MoveRight()
	{
		AnimatePosition.Do(fogParent, EZAnimation.ANIM_MODE.By, Vector3.right * -20f, EZAnimation.sinusInOut, 1.5f, 0, null, null);
	}

	public void MoveLeft()
	{
		AnimatePosition.Do(fogParent, EZAnimation.ANIM_MODE.By, Vector3.right * 20f, EZAnimation.sinusInOut, 1.5f, 0, null, null);
	}
}
