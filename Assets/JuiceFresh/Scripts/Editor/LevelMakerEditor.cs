﻿using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine.UI;
using System.Reflection;
using JuiceFresh;
//using JuiceFresh.Scripts.Integrations;
using UnityEditor.SceneManagement;

public class LevelMakerEditor : EditorWindow
{
	bool groupEnabled;
	bool myBool = true;
	float myFloat = 1.23f;
	int levelNumber = 1;
	int maxRows;
	int maxCols;
	public static SquareBlocks[] levelSquares = new SquareBlocks[81];
	SquareTypes squareType;
	private string fileName = "1.txt";
	private Texture squareTex;
	private Texture blockTex;
	private Texture blockTex2;
	private Texture wireBlockTex;
	private Texture solidBlockTex;
	private Texture doublesolidBlockTex;
	private Texture undestroyableBlockTex;
	private Texture thrivingBlockTex;
	public int star1;
	private int star2;
	private int star3;
	private Vector2 scrollViewVector;
	Target target;
	private int[] ingrCount = new int[4];
	private CollectedIngredients[] ingr = new CollectedIngredients[4];
	private CollectedItem[] collectItems = new CollectedItem[6];
	private int limit = 1;
	private bool update;
	private LIMIT limitType;
	private int colorLimit = 4;
	private static int selected;
	string[] toolbarStrings = new string[] { "Editor", "Settings", "Shop", "In-apps", "Ads", "GUI", "Rate", "About" };
	private static LevelMakerEditor window;
	private bool life_settings_show;
	private bool score_settings;
	bool boost_show;
	private bool failed_settings_show;
	private bool gems_shop_show;
	private bool target_description_show;
	int cageHP;
	private int oldcageHP;
	private int bombsAtTheSameTime;
	private int oldbombsAtTheSameTime;
	private int bombTimer;
	private int oldbombTimer;
	private bool enableGoogleAdsProcessing;
	private CollectStars starsTargetCount;
	private CollectStars oldstarsTargetCount;
	private LevelManager lm;
	private InitScript initscript;


	[MenuItem("Window/Juice Fresh editor")]
	public static void Init()
	{
		// Get existing open window or if none, make a new one:
		window = (LevelMakerEditor)EditorWindow.GetWindow(typeof(LevelMakerEditor));
		window.Show();


	}

	public static void ShowHelp()
	{
		selected = 7;
	}

	public static void ShowWindow()
	{
		EditorWindow.GetWindow(typeof(LevelMakerEditor));

	}

	//List<AdEvents> oldList;

	int NumIngredients;
	private bool initDone;

	void OnFocus()
	{

		initDone = false;
		if (maxRows <= 0)
			maxRows = 8;
		if (maxCols <= 0)
			maxCols = 7;

		if (Camera.main == null)
			return;
		lm = Camera.main.GetComponent<LevelManager>();
		if (lm != null)
		{
			initscript = Camera.main.GetComponent<InitScript>();
			ingr = initscript.collectedIngredients.ToArray();
			Initialize();
			//unityAdsSettings = Resources.Load<UnityAdsID>("UnityAdsID");
			LoadDataFromLocal(levelNumber);
		}
		if (EditorSceneManager.GetActiveScene().name == "game")
		{
			NumIngredients = lm.NumIngredients;

			//if (oldList == null)
			//{
			//	oldList = new List<AdEvents>();
			//	oldList.Clear();
			//	for (int i = 0; i < initscript.adsEvents.Count; i++)
			//	{
			//		oldList.Add(new AdEvents());
			//		oldList[i].adType = initscript.adsEvents[i].adType;
			//		oldList[i].everyLevel = initscript.adsEvents[i].everyLevel;
			//		oldList[i].gameEvent = initscript.adsEvents[i].gameEvent;
			//	}
			//}


			//squareTex = Resources.Load("Blocks/square") as Texture;
			//blockTex = Resources.Load("Blocks/block") as Texture;
			//blockTex2 = Resources.Load("Blocks/block_02") as Texture;
			//wireBlockTex = Resources.Load("Blocks/wireBlock") as Texture;
			//solidBlockTex = Resources.Load("Blocks/solidBlock") as Texture;
			//undestroyableBlockTex = Resources.Load("Blocks/undestroyable") as Texture;
			//thrivingBlockTex = Resources.Load("Blocks/thriving_block") as Texture;
			squareTex = lm.squarePrefab.GetComponent<SpriteRenderer>().sprite.texture;
			blockTex = lm.blockPrefab.GetComponent<SpriteRenderer>().sprite.texture;
			blockTex2 = lm.doubleBlock.texture;
			wireBlockTex = lm.wireBlockPrefab.GetComponent<SpriteRenderer>().sprite.texture;
			solidBlockTex = lm.solidBlockPrefab.GetComponent<SpriteRenderer>().sprite.texture;
			doublesolidBlockTex = lm.doubleSolidBlock.texture;
			//undestroyableBlockTex = lm.undesroyableBlockPrefab.GetComponent<SpriteRenderer>().sprite.texture;
			thrivingBlockTex = lm.thrivingBlockPrefab.GetComponent<SpriteRenderer>().sprite.texture;
		}
	}

	void Initialize()
	{


		life_settings_show = true;
		score_settings = true;
		boost_show = true;
		failed_settings_show = true;
		gems_shop_show = true;
		target_description_show = true;
		levelSquares = new SquareBlocks[maxCols * maxRows];
		for (int i = 0; i < levelSquares.Length; i++)
		{

			SquareBlocks sqBlocks = new SquareBlocks();
			sqBlocks.block = SquareTypes.EMPTY;
			sqBlocks.obstacle = SquareTypes.NONE;

			levelSquares[i] = sqBlocks;
		}
		initDone = true;

	}



	void OnGUI()
	{
		if (!initDone)
			return;

		GUI.changed = false;
		if (levelNumber < 1)
			levelNumber = 1;
		GUILayout.Space(20);
		GUILayout.BeginHorizontal();
		GUILayout.Space(30);
		int oldSelected = selected;
		selected = GUILayout.Toolbar(selected, toolbarStrings, new GUILayoutOption[] { GUILayout.Width(450) });
		GUILayout.EndHorizontal();

		scrollViewVector = GUI.BeginScrollView(new Rect(25, 45, position.width - 30, position.height), scrollViewVector, new Rect(0, 0, 400, 1600));
		GUILayout.Space(-30);

		if (oldSelected != selected)
			scrollViewVector = Vector2.zero;

		if (selected == 0)
		{
			if (EditorSceneManager.GetActiveScene().name == "game")
			{
				GUILevelSelector();
				GUILayout.Space(10);

				GUILevelSize();
				GUILayout.Space(10);

				GUILimit();
				GUILayout.Space(10);

				GUIColorLimit();
				GUILayout.Space(10);

				GUIStars();
				GUILayout.Space(10);

				GUITarget();
				GUILayout.Space(10);

				GUIBlocks();
				GUILayout.Space(20);

				GUIGameField();
			}
			else
				GUIShowWarning();
		}
		else if (selected == 1)
		{
			if (EditorSceneManager.GetActiveScene().name == "game")
				GUISettings();
			else
				GUIShowWarning();
		}
		else if (selected == 2)
		{
			if (EditorSceneManager.GetActiveScene().name == "game")
				GUIShops();
			else
				GUIShowWarning();
		}
		else if (selected == 3)
		{
			if (EditorSceneManager.GetActiveScene().name == "game") { }
				//GUIInappSettings();
			else
				GUIShowWarning();
		}
		else if (selected == 4)
		{
			if (EditorSceneManager.GetActiveScene().name == "game") { }
				//GUIAds();
			else
				GUIShowWarning();
		}
		else if (selected == 5)
		{
			if (EditorSceneManager.GetActiveScene().name == "game")
				GUIDialogs();
			else
				GUIShowWarning();
		}
		else if (selected == 6)
		{
			if (EditorSceneManager.GetActiveScene().name == "game")
				GUIRate();
			else
				GUIShowWarning();
		}
		else if (selected == 7)
		{
			//GUIHelp();
		}

		GUI.EndScrollView();
		if (GUI.changed && !EditorApplication.isPlaying)
			EditorSceneManager.MarkAllScenesDirty();

		//if (enableGoogleAdsProcessing) { }
			//RunOnceGoogle();

		//if (enableChartboostAdsProcessing)
		//    RunOnceChartboost();
	}


	void GUIShowWarning()
	{
		GUILayout.Space(100);
		GUILayout.Label("CAUTION!", EditorStyles.boldLabel, new GUILayoutOption[] { GUILayout.Width(600) });
		GUILayout.Label("Please open scene - game ( Assets/JuiceFresh/Scenes/game.unity )", EditorStyles.boldLabel, new GUILayoutOption[] { GUILayout.Width(600) });

	}

	//void SetScriptingDefineSymbols()
	//{
	//	string defines = "";
	//	if (initscript.enableUnityAds)
	//		defines = defines + "; UNITY_ADS";
	//	if (initscript.enableGoogleMobileAds)
	//		defines = defines + "; GOOGLE_MOBILE_ADS";
	//	if (initscript.enableChartboostAds)
	//		defines = defines + "; CHARTBOOST_ADS";
	//	if (lm.FacebookEnable)
	//	{
	//		defines = defines + "; FACEBOOK";
	//		if (Directory.Exists("Assets/PlayFabSDK"))
	//			defines = defines + "; PLAYFAB";
	//	}
	//	if (lm.enableInApps)
	//		defines = defines + "; UNITY_INAPPS";

	//	PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, defines);
	//	PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, defines);
	//	PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.WP8, defines);

	//}

	#region GUIRate

	void GUIRate()
	{

		GUILayout.Label("Rate settings:", EditorStyles.boldLabel, new GUILayoutOption[] { GUILayout.Width(150) });
		GUILayout.Space(10);
		GUILayout.BeginHorizontal();
		initscript.ShowRateEvery = EditorGUILayout.IntField("Show Rate every ", initscript.ShowRateEvery, new GUILayoutOption[] {
			GUILayout.Width (220),
			GUILayout.MaxWidth (220)
		});
		GUILayout.Label(" level (0 = disable)", EditorStyles.label, new GUILayoutOption[] { GUILayout.Width(150) });
		GUILayout.EndHorizontal();
		//initscript.RateURL = EditorGUILayout.TextField("URL", initscript.RateURL, new GUILayoutOption[] {
		//	GUILayout.Width (220),
		//	GUILayout.MaxWidth (220)
		//});
		//initscript.RateURLIOS = EditorGUILayout.TextField("URL iOS", initscript.RateURLIOS, new GUILayoutOption[] {
		//	GUILayout.Width (220),
		//	GUILayout.MaxWidth (220)
		//});

	}

	#endregion

	#region GUIDialogs

	void GUIDialogs()
	{
		GUILayout.Label("GUI elements:", EditorStyles.boldLabel, new GUILayoutOption[] { GUILayout.Width(150) });
		GUILayout.Space(10);
		ShowMenuButton("Menu Play", "MenuPlay");
		ShowMenuButton("Menu Complete", "MenuComplete");
		ShowMenuButton("Menu Failed", "MenuFailed");
		ShowMenuButton("Pause", "MenuPause");
		ShowMenuButton("Boost Shop", "BoostShop");
		ShowMenuButton("Live Shop", "LiveShop");
		ShowMenuButton("Gems Shop", "GemsShop");
		ShowMenuButton("Reward", "Reward");
		ShowMenuButton("Tutorial", "Tutorial");

	}

	void ShowMenuButton(string label, string name)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label(label, EditorStyles.label, new GUILayoutOption[] { GUILayout.Width(100) });
		GameObject obj = GameObject.Find("CanvasGlobal").transform.Find(name).gameObject;
		if (GUILayout.Button(obj.activeSelf ? "hide" : "show", new GUILayoutOption[] { GUILayout.Width(100) }))
		{
			EditorGUIUtility.PingObject(obj);
			Selection.activeGameObject = obj;
			obj.SetActive(!obj.activeSelf);
		}
		//if (GUILayout.Button("fonts", new GUILayoutOption[] { GUILayout.Width(100) }))
		//{
		//    EditorGUIUtility.PingObject(obj);
		//    Selection.activeGameObject = obj;
		//    obj.SetActive(!obj.activeSelf);
		//    Transform objTransform = obj.transform;
		//    GameObject[] objects = new GameObject[2];
		//    int i = 0;
		//    foreach (Transform item in objTransform)
		//    {
		//        if (item.GetComponent<Text>() != null)
		//        {
		//            objects[i] = item.gameObject;
		//            i++;
		//        }
		//    }
		//    //Selection.objects = objects;
		//   // Selection.GetFiltered(typeof( Text), SelectionMode.TopLevel);
		//    SetSearchFilter("text", 2);
		//}

		GUILayout.EndHorizontal();
	}

	public static void SetSearchFilter(string filter, int filterMode)
	{

		SearchableEditorWindow[] windows = (SearchableEditorWindow[])Resources.FindObjectsOfTypeAll(typeof(SearchableEditorWindow));
		SearchableEditorWindow hierarchy = null;
		foreach (SearchableEditorWindow window in windows)
		{

			if (window.GetType().ToString() == "UnityEditor.SceneHierarchyWindow")
			{

				hierarchy = window;
				break;
			}
		}

		if (hierarchy == null)
			return;

		MethodInfo setSearchType = typeof(SearchableEditorWindow).GetMethod("SetSearchFilter", BindingFlags.NonPublic | BindingFlags.Instance);
		object[] parameters = new object[] { filter, filterMode, false };

		setSearchType.Invoke(hierarchy, parameters);
	}

	#endregion

	#region ads_settings

//	void RunOnceGoogle()
//	{
//		if (Directory.Exists("Assets/PlayServicesResolver"))
//		{
//			Debug.Log("assets try reimport");
//#if GOOGLE_MOBILE_ADS && UNITY_ANDROID
//			//			GooglePlayServices.PlayServicesResolver.MenuResolve ();1.4.5
//			Debug.Log("assets reimorted");
//			enableGoogleAdsProcessing = false;
//#endif
//		}


//	}

//	void GUIAds()
//	{


//		bool oldenableAds = initscript.enableUnityAds;

//		GUILayout.Label("Ads settings:", EditorStyles.boldLabel, new GUILayoutOption[] { GUILayout.Width(150) });
//		GUILayout.BeginHorizontal();

//		//UNITY ADS

//		//		initscript.enableUnityAds = EditorGUILayout.Toggle ("Enable Unity ads", initscript.enableUnityAds, new GUILayoutOption[] {//1.3
//		//			GUILayout.Width (200)
//		//		});
//		GUILayout.Label("Unity ads", EditorStyles.boldLabel, new GUILayoutOption[] { GUILayout.Width(150) });//1.3
//		GUILayout.Label("Install: Windows->\n Services->Ads - OFF", new GUILayoutOption[] { GUILayout.Width(130) });

//		if (GUILayout.Button("Help", new GUILayoutOption[] { GUILayout.Width(80) }))
//		{
//			Application.OpenURL("https://docs.google.com/document/d/1HeN8JtQczTVetkMnd8rpSZp_TZZkEA7_kan7vvvsMw0");
//		}

//		GUILayout.EndHorizontal();
//		GUILayout.Space(10);
//		//		}
//		GUILayout.BeginHorizontal();
//		GUILayout.Space(20);
//		EditorGUI.BeginChangeCheck();
//		unityAdsSettings.androidID = EditorGUILayout.TextField("Android ID ", unityAdsSettings.androidID , new GUILayoutOption[] {
//			GUILayout.Width (220),
//			GUILayout.MaxWidth (220)
//		});
//		GUILayout.EndHorizontal();
//		GUILayout.BeginHorizontal();
//		GUILayout.Space(20);
//		unityAdsSettings.iOSID = EditorGUILayout.TextField("iOS ID", unityAdsSettings.iOSID , new GUILayoutOption[] {
//			GUILayout.Width (220),
//			GUILayout.MaxWidth (220)
//		});
//		if(EditorGUI.EndChangeCheck())
//			EditorUtility.SetDirty(unityAdsSettings);
//		GUILayout.EndHorizontal();
//		GUILayout.Space(10);


//		//		if (oldenableAds != initscript.enableUnityAds)//1.3
//		//			SetScriptingDefineSymbols ();
//		//		if (initscript.enableUnityAds) {
//		GUILayout.BeginHorizontal();
//		initscript.rewardedGems = EditorGUILayout.IntField("Rewarded gems", initscript.rewardedGems, new GUILayoutOption[] {
//			GUILayout.Width (200),
//			GUILayout.MaxWidth (200)
//		});
//		GUILayout.EndHorizontal();

//		GUILayout.Space(20);

		
//		//GOOGLE MOBILE ADS

//		bool oldenableGoogleMobileAds = initscript.enableGoogleMobileAds;
//		GUILayout.BeginHorizontal();
//		//		initscript.enableGoogleMobileAds = EditorGUILayout.Toggle ("Enable Google Mobile Ads", initscript.enableGoogleMobileAds, new GUILayoutOption[] {//1.3
//		//			GUILayout.Width (50),
//		//			GUILayout.MaxWidth (200)
//		//		});
//		GUILayout.Label("Google mobile ads", EditorStyles.boldLabel, new GUILayoutOption[] { GUILayout.Width(150) });//1.3
//		if (GUILayout.Button("Install", new GUILayoutOption[] { GUILayout.Width(100) }))
//		{
//			Application.OpenURL("https://github.com/googleads/googleads-mobile-unity/releases");//1.3
//		}
//		if (GUILayout.Button("Help", new GUILayoutOption[] { GUILayout.Width(80) }))
//		{
//			Application.OpenURL("https://docs.google.com/document/d/1I69mo9yLzkg35wtbHpsQd3Ke1knC5pf7G1Wag8MdO-M/edit?usp=sharing");
//		}

//		GUILayout.EndHorizontal();

//		GUILayout.Space(10);
//		//		if (oldenableGoogleMobileAds != initscript.enableGoogleMobileAds) {//1.3
//		//
//		//			SetScriptingDefineSymbols ();
//		//			if (initscript.enableGoogleMobileAds) {
//		//				enableGoogleAdsProcessing = true;
//		//			}
//		//		}
//		//		if (initscript.enableGoogleMobileAds) {

//		GUILayout.BeginHorizontal();
//		GUILayout.Space(20);
//		initscript.admobUIDAndroid = EditorGUILayout.TextField("Admob Interstitial ID Android ", initscript.admobUIDAndroid, new GUILayoutOption[] {
//			GUILayout.Width (220),
//			GUILayout.MaxWidth (220)
//		});
//		GUILayout.EndHorizontal();
//		GUILayout.BeginHorizontal();
//		GUILayout.Space(20);
//		initscript.admobUIDIOS = EditorGUILayout.TextField("Admob Interstitial ID iOS", initscript.admobUIDIOS, new GUILayoutOption[] {
//			GUILayout.Width (220),
//			GUILayout.MaxWidth (220)
//		});
//		GUILayout.EndHorizontal();
//		GUILayout.Space(10);
//		GUILayout.BeginHorizontal();
//		GUILayout.Space(20);
//		initscript.admobRewardedUIDAndroid = EditorGUILayout.TextField("Admob Rewarded ID Android ", initscript.admobRewardedUIDAndroid, new GUILayoutOption[] {
//			GUILayout.Width (220),
//			GUILayout.MaxWidth (220)
//		});
//		GUILayout.EndHorizontal();
//		GUILayout.BeginHorizontal();
//		GUILayout.Space(20);
//		initscript.admobRewardedUIDIOS = EditorGUILayout.TextField("Admob Rewarded ID iOS", initscript.admobRewardedUIDIOS, new GUILayoutOption[] {
//			GUILayout.Width (220),
//			GUILayout.MaxWidth (220)
//		});
//		GUILayout.EndHorizontal();
//		//		}

//		//CHARTBOOST ADS

//		GUILayout.BeginHorizontal();
//		bool oldenableChartboostAds = initscript.enableChartboostAds;
//		//		initscript.enableChartboostAds = EditorGUILayout.Toggle ("Enable Chartboost Ads", initscript.enableChartboostAds, new GUILayoutOption[] {
//		//			GUILayout.Width (50),
//		//			GUILayout.MaxWidth (200)
//		//		});
//		GUILayout.Label("Chartboost ads", EditorStyles.boldLabel, new GUILayoutOption[] { GUILayout.Width(150) });//1.3
//		if (GUILayout.Button("Install", new GUILayoutOption[] { GUILayout.Width(100) }))
//		{
//			Application.OpenURL("http://www.chartboo.st/sdk/unity");//1.3
//		}
//		if (GUILayout.Button("Help", new GUILayoutOption[] { GUILayout.Width(80) }))
//		{
//			Application.OpenURL("https://docs.google.com/document/d/1ibnQbuxFgI4izzyUtT45WH5m1ab3R5d1E3ke3Wrb10Y");
//		}

//		GUILayout.EndHorizontal();

//		GUILayout.Space(10);
//		//		if (oldenableChartboostAds != initscript.enableChartboostAds) {//1.3
//		//			SetScriptingDefineSymbols ();
//		//			if (initscript.enableChartboostAds) {
//		//				//enableChartboostAdsProcessing = true;
//		//			}
//		//
//		//		}
//		//		if (initscript.enableChartboostAds) {
//		GUILayout.BeginHorizontal();
//		GUILayout.Space(20);
//		EditorGUILayout.LabelField("menu Chartboost->Edit settings", new GUILayoutOption[] {
//			GUILayout.Width (50),
//			GUILayout.MaxWidth (200)
//		});
//		GUILayout.EndHorizontal();
//		GUILayout.BeginHorizontal();
//		GUILayout.Space(20);
//		EditorGUILayout.LabelField("Put ad ID to appropriate platform to prevent crashing!", EditorStyles.boldLabel, new GUILayoutOption[] {
//			GUILayout.Width (100),
//			GUILayout.MaxWidth (400)
//		});
//		GUILayout.EndHorizontal();

//		GUILayout.Space(10);
//		//		}


//		GUILayout.Space(10);

//		GUILayout.Label("Ads controller:", EditorStyles.boldLabel, new GUILayoutOption[] { GUILayout.Width(150) });

//		EditorGUILayout.Space();

//		GUILayout.Label("Event:               Status:                            Show after n call:", new GUILayoutOption[] { GUILayout.Width(350) });



//		foreach (AdEvents item in initscript.adsEvents)
//		{
//			EditorGUILayout.BeginHorizontal();
//			item.gameEvent = (GameState)EditorGUILayout.EnumPopup(item.gameEvent, new GUILayoutOption[] { GUILayout.Width(100) });
//			item.adType = (AdType)EditorGUILayout.EnumPopup(item.adType, new GUILayoutOption[] { GUILayout.Width(150) });
//			item.everyLevel = EditorGUILayout.IntPopup(item.everyLevel, new string[] {
//				"1",
//				"2",
//				"3",
//				"4",
//				"5",
//				"6",
//				"7",
//				"8",
//				"9",
//				"10"
//			}, new int[] {
//				1,
//				2,
//				3,
//				4,
//				5,
//				6,
//				7,
//				8,
//				9,
//				10
//			}, new GUILayoutOption[] { GUILayout.Width(100) });

//			EditorGUILayout.EndHorizontal();

//		}
//		EditorGUILayout.Space();

//		EditorGUILayout.BeginHorizontal();
//		if (GUILayout.Button("Add"))
//		{
//			AdEvents adevent = new AdEvents();
//			adevent.everyLevel = 1;
//			initscript.adsEvents.Add(adevent);

//		}
//		if (GUILayout.Button("Delete"))
//		{
//			if (initscript.adsEvents.Count > 0)
//				initscript.adsEvents.Remove(initscript.adsEvents[initscript.adsEvents.Count - 1]);

//		}


//		GUILayout.Space(10);



//	}

	#endregion

	#region inapps_settings

	//void GUIInappSettings()
	//{

	//	GUILayout.Label("In-apps settings:", EditorStyles.boldLabel, new GUILayoutOption[] { GUILayout.Width(150) });

	//	if (GUILayout.Button("Reset to default", new GUILayoutOption[] { GUILayout.Width(150) }))
	//	{
	//		ResetInAppsSettings();
	//	}

	//	GUILayout.Space(10);

	//	bool oldenableInApps = lm.enableInApps;

	//	GUILayout.BeginHorizontal();
	//	//		lm.enableInApps = EditorGUILayout.Toggle ("Enable In-apps", lm.enableInApps, new GUILayoutOption[] {//1.3
	//	//			GUILayout.Width (180)
	//	//		});
	//	//if (GUILayout.Button("Help", new GUILayoutOption[] { GUILayout.Width(80) }))
	//	//{
	//	//	Application.OpenURL("https://docs.google.com/document/d/1HeN8JtQczTVetkMnd8rpSZp_TZZkEA7_kan7vvvsMw0#bookmark=id.b1efplsspes5");
	//	//}
	//	GUILayout.EndHorizontal();


	//	GUILayout.BeginHorizontal();
	//	GUILayout.Space(20);
	//	GUILayout.Label("Install: Windows->Services->\n In-app Purchasing - ON->Import", new GUILayoutOption[] { GUILayout.Width(400) });
	//	GUILayout.EndHorizontal();

	//	GUILayout.Space(10);

	//	GUILayout.Space(10);

	//	//		if (oldenableInApps != lm.enableInApps) {//1.3
	//	//			SetScriptingDefineSymbols ();
	//	//		}


	//	GUILayout.BeginHorizontal();
	//	GUILayout.Space(30);
	//	GUILayout.BeginVertical();
	//	for (int i = 0; i < lm.InAppIDs.Length; i++)
	//	{
	//		lm.InAppIDs[i] = EditorGUILayout.TextField("Product id " + (i + 1), lm.InAppIDs[i], new GUILayoutOption[] {
	//			GUILayout.Width (300),
	//			GUILayout.MaxWidth (300)
	//		});

	//	}
	//	GUILayout.Space(10);

	//	GUILayout.Label("Android:", EditorStyles.boldLabel, new GUILayoutOption[] { GUILayout.Width(150) });

	//	GUILayout.BeginHorizontal();
	//	GUILayout.Space(30);

	//	GUILayout.BeginVertical();
	//	GUILayout.Space(10);
	//	// GUILayout.Label(" Put Google license key into the field \n from the google play account ", EditorStyles.label, new GUILayoutOption[] { GUILayout.Width(300) });
	//	// GUILayout.Space(10);

	//	// lm.GoogleLicenseKey = EditorGUILayout.TextField("Google license key", lm.GoogleLicenseKey, new GUILayoutOption[] {
	//	//     GUILayout.Width (300),
	//	//     GUILayout.MaxWidth (300)
	//	// });

	//	GUILayout.Space(10);
	//	if (GUILayout.Button("Android account help", new GUILayoutOption[] { GUILayout.Width(400) }))
	//	{
	//		Application.OpenURL("http://developer.android.com/google/play/billing/billing_admin.html");
	//	}
	//	GUILayout.EndVertical();
	//	GUILayout.EndHorizontal();
	//	GUILayout.EndVertical();
	//	GUILayout.EndHorizontal();

	//	GUILayout.BeginHorizontal();
	//	GUILayout.Space(30);
	//	GUILayout.BeginVertical();

	//	GUILayout.Space(10);
	//	GUILayout.Label("iOS:", EditorStyles.boldLabel, new GUILayoutOption[] { GUILayout.Width(150) });
	//	GUILayout.BeginHorizontal();
	//	GUILayout.Space(30);

	//	GUILayout.BeginVertical();

	//	// GUILayout.Label(" StoreKit library must be added \n to the XCode project, generated by Unity ", EditorStyles.label, new GUILayoutOption[] { GUILayout.Width(300) });
	//	GUILayout.Space(10);
	//	if (GUILayout.Button("iOS account help", new GUILayoutOption[] { GUILayout.Width(400) }))
	//	{
	//		Application.OpenURL("https://developer.apple.com/library/ios/qa/qa1329/_index.html");
	//	}
	//	GUILayout.EndVertical();

	//	GUILayout.EndHorizontal();
	//	GUILayout.EndVertical();
	//	GUILayout.EndHorizontal();

	//}

	#endregion

	//void GUIHelp()
	//{
	//	GUILayout.Label("Juice Fresh game template - v 1.5.", EditorStyles.boldLabel, new GUILayoutOption[] { GUILayout.Width(350) });
	//	GUILayout.Space(10);

	//	GUILayout.Label("Please read our documentation:", EditorStyles.boldLabel, new GUILayoutOption[] { GUILayout.Width(200) });
	//	if (GUILayout.Button("DOCUMENTATION", new GUILayoutOption[] { GUILayout.Width(150) }))
	//	{
	//		Application.OpenURL("https://docs.google.com/document/d/1ex3rvCDWAc3geATi66s3lpBzwzhJbjd4A2Fo19HgdcQ/edit");
	//	}
	//	GUILayout.Space(10);
	//	GUILayout.Label("To start work with project - \n go to Editor Section of this menu", EditorStyles.boldLabel, new GUILayoutOption[] { GUILayout.Width(350) });
	//	GUILayout.Space(10);
	//	GUILayout.Label("To get support you should provide \n ORDER NUMBER (asset store) \n or NICKNAME and DATE of purchase (other stores):", EditorStyles.boldLabel, new GUILayoutOption[] { GUILayout.Width(350) });
	//	GUILayout.Space(10);
	//	GUILayout.TextArea("info@candy-smith.com", EditorStyles.boldLabel, new GUILayoutOption[] { GUILayout.Width(350) });

	//}

	#region settings
	private bool share_settings;
	//private UnityAdsID unityAdsSettings;

	void GUISettings()
	{
		GUILayout.Label("Game settings:", EditorStyles.boldLabel, new GUILayoutOption[] { GUILayout.Width(150) });

		GUILayout.BeginHorizontal();//1.3.3
		if (GUILayout.Button("Reset to default", new GUILayoutOption[] { GUILayout.Width(150) }))
		{
			ResetSettings();
		}
		if (GUILayout.Button("Clear player prefs", new GUILayoutOption[] { GUILayout.Width(150) }))
		{
			PlayerPrefs.DeleteAll();
			PlayerPrefs.Save();
			Debug.Log("Player prefs cleared");
		}
		GUILayout.EndHorizontal();//1.3.3

		GUILayout.Space(10);

		//bool oldFacebookEnable = lm.FacebookEnable;
		GUILayout.BeginHorizontal();
		//GUILayout.Label("Facebook", EditorStyles.boldLabel, new GUILayoutOption[] { GUILayout.Width(150) });//1.6.1
		//if (GUILayout.Button("Install", new GUILayoutOption[] { GUILayout.Width(70) }))
		//{
		//	Application.OpenURL("https://developers.facebook.com/docs/unity/downloads");
		//}
		//if (GUILayout.Button("Account", new GUILayoutOption[] { GUILayout.Width(70) }))
		//{
		//	Application.OpenURL("https://developers.facebook.com");
		//}
		//if (GUILayout.Button("Help", new GUILayoutOption[] { GUILayout.Width(60) }))
		//{
		//	Application.OpenURL("https://docs.google.com/document/d/1bTNdM3VSg8qu9nWwO7o7WeywMPhVLVl8E_O0gMIVIw0/edit?usp=sharing");
		//}
		GUILayout.EndHorizontal();

//#if FACEBOOK
//		// share_settings = EditorGUILayout.Foldout(share_settings, "Share settings:");
//		// if (share_settings)
//		// {
//		// 	GUILayout.BeginHorizontal();
//		// 	GUILayout.Space(30);
//		// 	GUILayout.BeginVertical();
//		// 	{
//		// 		lm.androidSharingPath = EditorGUILayout.TextField("Android path", lm.androidSharingPath, new GUILayoutOption[] { GUILayout.MaxWidth(500) });
//		// 		lm.iosSharingPath = EditorGUILayout.TextField("iOS path", lm.iosSharingPath, new GUILayoutOption[] { GUILayout.MaxWidth(500) });
//		// 	}
//		// 	GUILayout.EndVertical();
//		// 	GUILayout.EndHorizontal();

//		// 	GUILayout.Space(10);
//		// }
//#endif

		GUILayout.BeginHorizontal();
		//GUILayout.Label("Leadboard Gamesparks", EditorStyles.boldLabel, new GUILayoutOption[] { GUILayout.Width(150) });//1.6.1
		//if (GUILayout.Button("Install", new GUILayoutOption[] { GUILayout.Width(70) }))
		//{
		//	Application.OpenURL("https://docs.gamesparks.com/sdk-center/unity.html");
		//}
		//if (GUILayout.Button("Account", new GUILayoutOption[] { GUILayout.Width(70) }))
		//{
		//	Application.OpenURL("https://portal.gamesparks.net");
		//}
		//if (GUILayout.Button("Help", new GUILayoutOption[] { GUILayout.Width(60) }))
		//{
		//	Application.OpenURL("https://docs.google.com/document/d/1JcQfiiD2ALz6v_i9UIcG93INWZKC7z6FHXH_u6w9A8E");
		//}
		GUILayout.EndHorizontal();
//#if GAMESPARKS
//		GUILayout.BeginHorizontal();
//		{
//            GUILayout.Space(150);
//			if(GUILayout.Button("Create game", GUILayout.Width(100)))//1.4.9
//			{
//				GamesparksConfiguration window = ScriptableObject.CreateInstance<GamesparksConfiguration>();
//				window.position = new Rect(Screen.width / 2, Screen.height / 2, 250, 200);
//				window.ShowPopup();
//			}
		
//		}
//		GUILayout.EndHorizontal();
//#endif

		//		if (oldFacebookEnable != lm.FacebookEnable) {//1.3
		//			SetScriptingDefineSymbols ();
		//		}
		//if (lm.FacebookEnable)
		//{
		//	GUILayout.BeginHorizontal();
		//	GUILayout.Space(20);
		//	GUILayout.Label("menu Facebook-> Edit settings", new GUILayoutOption[] { GUILayout.Width(300) });
		//	GUILayout.EndHorizontal();
		//}

		GUILayout.Space(10);

		score_settings = EditorGUILayout.Foldout(score_settings, "Score settings:");
		if (score_settings)
		{
			GUILayout.Space(10);
			GUILayout.BeginHorizontal();
			GUILayout.Space(30);
			GUILayout.BeginVertical();
			lm.scoreForItem = EditorGUILayout.IntField("Score for item", lm.scoreForItem, new GUILayoutOption[] {
				GUILayout.Width (50),
				GUILayout.MaxWidth (200)
			});
			lm.scoreForBlock = EditorGUILayout.IntField("Score for block", lm.scoreForBlock, new GUILayoutOption[] {
				GUILayout.Width (50),
				GUILayout.MaxWidth (200)
			});
			lm.scoreForWireBlock = EditorGUILayout.IntField("Score for wire block", lm.scoreForWireBlock, new GUILayoutOption[] {
				GUILayout.Width (50),
				GUILayout.MaxWidth (200)
			});
			lm.scoreForSolidBlock = EditorGUILayout.IntField("Score for solid block", lm.scoreForSolidBlock, new GUILayoutOption[] {
				GUILayout.Width (50),
				GUILayout.MaxWidth (200)
			});
			lm.scoreForThrivingBlock = EditorGUILayout.IntField("Score for thriving block", lm.scoreForThrivingBlock, new GUILayoutOption[] {
				GUILayout.Width (50),
				GUILayout.MaxWidth (200)
			});
			GUILayout.Space(10);

			lm.showPopupScores = EditorGUILayout.Toggle("Show popup scores", lm.showPopupScores, new GUILayoutOption[] {
				GUILayout.Width (50),
				GUILayout.MaxWidth (200)
			});
			GUILayout.Space(10);

			lm.scoresColors[0] = EditorGUILayout.ColorField("Score color item 1", lm.scoresColors[0], new GUILayoutOption[] {
				GUILayout.Width (200),
				GUILayout.MaxWidth (200)
			});
			lm.scoresColors[1] = EditorGUILayout.ColorField("Score color item 2", lm.scoresColors[1], new GUILayoutOption[] {
				GUILayout.Width (200),
				GUILayout.MaxWidth (200)
			});
			lm.scoresColors[2] = EditorGUILayout.ColorField("Score color item 3", lm.scoresColors[2], new GUILayoutOption[] {
				GUILayout.Width (200),
				GUILayout.MaxWidth (200)
			});
			lm.scoresColors[3] = EditorGUILayout.ColorField("Score color item 4", lm.scoresColors[3], new GUILayoutOption[] {
				GUILayout.Width (200),
				GUILayout.MaxWidth (200)
			});
			lm.scoresColors[4] = EditorGUILayout.ColorField("Score color item 5", lm.scoresColors[4], new GUILayoutOption[] {
				GUILayout.Width (200),
				GUILayout.MaxWidth (200)
			});
			lm.scoresColors[5] = EditorGUILayout.ColorField("Score color item 6", lm.scoresColors[5], new GUILayoutOption[] {
				GUILayout.Width (200),
				GUILayout.MaxWidth (200)
			});
			GUILayout.Space(10);

			lm.scoresColorsOutline[0] = EditorGUILayout.ColorField("Score color outline item 1", lm.scoresColorsOutline[0], new GUILayoutOption[] {
				GUILayout.Width (200),
				GUILayout.MaxWidth (200)
			});
			lm.scoresColorsOutline[1] = EditorGUILayout.ColorField("Score color outline item 2", lm.scoresColorsOutline[1], new GUILayoutOption[] {
				GUILayout.Width (200),
				GUILayout.MaxWidth (200)
			});
			lm.scoresColorsOutline[2] = EditorGUILayout.ColorField("Score color outline item 3", lm.scoresColorsOutline[2], new GUILayoutOption[] {
				GUILayout.Width (200),
				GUILayout.MaxWidth (200)
			});
			lm.scoresColorsOutline[3] = EditorGUILayout.ColorField("Score color outline item 4", lm.scoresColorsOutline[3], new GUILayoutOption[] {
				GUILayout.Width (200),
				GUILayout.MaxWidth (200)
			});
			lm.scoresColorsOutline[4] = EditorGUILayout.ColorField("Score color outline item 5", lm.scoresColorsOutline[4], new GUILayoutOption[] {
				GUILayout.Width (200),
				GUILayout.MaxWidth (200)
			});
			lm.scoresColorsOutline[5] = EditorGUILayout.ColorField("Score color outline item 6", lm.scoresColorsOutline[5], new GUILayoutOption[] {
				GUILayout.Width (200),
				GUILayout.MaxWidth (200)
			});
			GUILayout.EndVertical();
			GUILayout.EndHorizontal();
		}
		GUILayout.Space(20);

		life_settings_show = EditorGUILayout.Foldout(life_settings_show, "Lifes settings:");
		if (life_settings_show)
		{
			GUILayout.Space(10);
			GUILayout.BeginHorizontal();
			GUILayout.Space(30);
			GUILayout.BeginVertical();


			initscript.CapOfLife = EditorGUILayout.IntField("Max of lifes", initscript.CapOfLife, new GUILayoutOption[] {
				GUILayout.Width (200),
				GUILayout.MaxWidth (200)
			});
			GUILayout.Space(10);

			GUILayout.Label("Total time for refill lifes:", EditorStyles.label);
			GUILayout.BeginHorizontal();
			GUILayout.Space(30);
			GUILayout.Label("Hour", EditorStyles.label, GUILayout.Width(50));
			GUILayout.Label("Min", EditorStyles.label, GUILayout.Width(50));
			GUILayout.Label("Sec", EditorStyles.label, GUILayout.Width(50));
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Space(30);
			initscript.TotalTimeForRestLifeHours = EditorGUILayout.FloatField("", initscript.TotalTimeForRestLifeHours, new GUILayoutOption[] { GUILayout.Width(50) });
			initscript.TotalTimeForRestLifeMin = EditorGUILayout.FloatField("", initscript.TotalTimeForRestLifeMin, new GUILayoutOption[] { GUILayout.Width(50) });
			initscript.TotalTimeForRestLifeSec = EditorGUILayout.FloatField("", initscript.TotalTimeForRestLifeSec, new GUILayoutOption[] { GUILayout.Width(50) });
			GUILayout.EndHorizontal();
			GUILayout.Space(10);


			lm.lifeShop.CostIfRefill = EditorGUILayout.IntField("Cost of refilling lifes", lm.lifeShop.CostIfRefill, new GUILayoutOption[] {
				GUILayout.Width (200),
				GUILayout.MaxWidth (200)
			});
			GUILayout.EndVertical();
			GUILayout.EndHorizontal();
		}
		GUILayout.Space(20);

		initscript.FirstGems = EditorGUILayout.IntField("Start gems", initscript.FirstGems, new GUILayoutOption[] {
			GUILayout.Width (200),
			GUILayout.MaxWidth (200)
		});
		GUILayout.Space(20);

		initscript.losingLifeEveryGame = EditorGUILayout.Toggle("Losing a life every game", initscript.losingLifeEveryGame, new GUILayoutOption[] {
			GUILayout.Width (200),
			GUILayout.MaxWidth (200)
		});
		GUILayout.Space(20);


		failed_settings_show = EditorGUILayout.Foldout(failed_settings_show, "Failed settings:");
		if (failed_settings_show)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Space(30);
			GUILayout.BeginVertical();

			lm.FailedCost = EditorGUILayout.IntField(new GUIContent("Cost of continue", "Cost of continue after failed"), lm.FailedCost, new GUILayoutOption[] {
				GUILayout.Width (200),
				GUILayout.MaxWidth (200)
			});
			lm.ExtraFailedMoves = EditorGUILayout.IntField(new GUIContent("Extra moves", "Extra moves after continue"), lm.ExtraFailedMoves, new GUILayoutOption[] {
				GUILayout.Width (200),
				GUILayout.MaxWidth (200)
			});
			lm.ExtraFailedSecs = EditorGUILayout.IntField(new GUIContent("Extra seconds", "Extra seconds after continue"), lm.ExtraFailedSecs, new GUILayoutOption[] {
				GUILayout.Width (200),
				GUILayout.MaxWidth (200)
			});
			GUILayout.EndVertical();
			GUILayout.EndHorizontal();

		}
		GUILayout.Space(20);

		target_description_show = EditorGUILayout.Foldout(target_description_show, "Targets description:");
		if (target_description_show)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Space(30);
			GUILayout.BeginVertical();
			for (int i = 0; i < lm.targetDiscriptions.Length; i++)
			{
				lm.targetDiscriptions[i] = EditorGUILayout.TextField("", lm.targetDiscriptions[i], new GUILayoutOption[] {
					GUILayout.Width (200),
					GUILayout.MaxWidth (200)
				});

			}
			GUILayout.EndVertical();
			GUILayout.EndHorizontal();

		}

		//  EditorUtility.SetDirty(lm);
	}

	void ResetSettings()
	{
		lm.scoreForItem = 100;
		lm.scoreForBlock = 100;
		lm.scoreForWireBlock = 100;
		lm.scoreForSolidBlock = 100;
		lm.scoreForThrivingBlock = 100;
		lm.showPopupScores = true;
		lm.scoresColors[0] = new Color(183 / 255f, 3 / 255f, 3 / 255f);
		lm.scoresColors[1] = new Color(255 / 255f, 193 / 255f, 22 / 255f);
		lm.scoresColors[2] = new Color(237 / 255f, 13 / 255f, 233 / 255f);
		lm.scoresColors[3] = new Color(37 / 255f, 219 / 255f, 0 / 255f);
		lm.scoresColors[4] = new Color(41 / 255f, 157 / 255f, 255 / 255f);
		lm.scoresColors[5] = new Color(255 / 255f, 255 / 255f, 38 / 255f);

		lm.scoresColorsOutline[0] = new Color(255f / 255f, 255f / 255f, 255f / 255f);
		lm.scoresColorsOutline[1] = new Color(255f / 255f, 255f / 255f, 255f / 255f);
		lm.scoresColorsOutline[2] = new Color(255f / 255f, 255f / 255f, 255f / 255f);
		lm.scoresColorsOutline[3] = new Color(255f / 255f, 255f / 255f, 255f / 255f);
		lm.scoresColorsOutline[4] = new Color(255f / 255f, 255f / 255f, 255f / 255f);
		lm.scoresColorsOutline[5] = new Color(255f / 255f, 255f / 255f, 255f / 255f);

		initscript.CapOfLife = 5;
		initscript.TotalTimeForRestLifeHours = 0;
		initscript.TotalTimeForRestLifeMin = 15;
		initscript.TotalTimeForRestLifeSec = 0;
		lm.lifeShop.CostIfRefill = 12;
		lm.FailedCost = 12;
		lm.ExtraFailedMoves = 5;
		lm.ExtraFailedSecs = 30;
		EditorUtility.SetDirty(lm);
	}

	#endregion

	#region shop

	void GUIShops()
	{

		GUILayout.Label("Shop settings:", EditorStyles.boldLabel, new GUILayoutOption[] { GUILayout.Width(150) });

		if (GUILayout.Button("Reset to default", new GUILayoutOption[] { GUILayout.Width(150) }))
		{
			ResetShops();
		}
		GUILayout.Space(10);
		gems_shop_show = EditorGUILayout.Foldout(gems_shop_show, "Gems shop settings:");
		if (gems_shop_show)
		{
			int i = 1;
			foreach (GemProduct item in lm.gemsProducts)
			{
				GUILayout.BeginHorizontal();
				GUILayout.BeginVertical();

				GUILayout.BeginHorizontal();
				GUILayout.Space(30);
				GUILayout.Label("Gems count", new GUILayoutOption[] { GUILayout.Width(100) });
				GUILayout.Label("Price $", new GUILayoutOption[] { GUILayout.Width(100) });
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				GUILayout.Space(30);
				item.count = EditorGUILayout.IntField("", item.count, new GUILayoutOption[] {
					GUILayout.Width (100),
					GUILayout.MaxWidth (100)
				});
				item.price = EditorGUILayout.FloatField("", item.price, new GUILayoutOption[] {
					GUILayout.Width (100),
					GUILayout.MaxWidth (100)
				});
				GUILayout.EndHorizontal();
				GUILayout.EndVertical();
				GUILayout.EndHorizontal();
				i++;
			}
		}

		GUILayout.Space(10);
		boost_show = EditorGUILayout.Foldout(boost_show, "Boosts shop settings:");
		if (boost_show)
		{
			BoostShop bs = GameObject.Find("CanvasGlobal").transform.Find("BoostShop").GetComponent<BoostShop>();
			List<BoostProduct> bp = bs.boostProducts;
			foreach (BoostProduct item in bp)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Space(30);
				GUILayout.BeginVertical();

				GUILayout.BeginHorizontal();
				GUILayout.Label("Description");
				item.description = EditorGUILayout.TextField("", item.description, new GUILayoutOption[] {
					GUILayout.Width (400),
					GUILayout.MaxWidth (400)
				});
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();

				GUILayout.Label(item.icon.texture, new GUILayoutOption[] { GUILayout.Width(50), GUILayout.Height(50) });
				GUILayout.BeginVertical();
				GUILayout.BeginHorizontal();

				GUILayout.Label("Count", EditorStyles.label, new GUILayoutOption[] { GUILayout.Width(80) });
				GUILayout.Label("Price(gem)", EditorStyles.label, new GUILayoutOption[] { GUILayout.Width(80) });

				GUILayout.EndHorizontal();

				for (int i = 0; i < 3; i++)
				{
					GUILayout.BeginHorizontal();

					item.count[i] = EditorGUILayout.IntField("", item.count[i], new GUILayoutOption[] {
						GUILayout.Width (80),
						GUILayout.MaxWidth (80)
					});
					item.GemPrices[i] = EditorGUILayout.IntField("", item.GemPrices[i], new GUILayoutOption[] {
						GUILayout.Width (80),
						GUILayout.MaxWidth (80)
					});
					GUILayout.EndHorizontal();

				}
				GUILayout.EndVertical();

				GUILayout.EndHorizontal();
				GUILayout.EndVertical();
				GUILayout.EndHorizontal();
				GUILayout.Space(20);
			}

		}
	}

	void ResetShops()
	{

		lm.gemsProducts[0].count = 10;
		lm.gemsProducts[0].price = 0.99f;
		lm.gemsProducts[1].count = 50;
		lm.gemsProducts[1].price = 4.99f;
		lm.gemsProducts[2].count = 100;
		lm.gemsProducts[2].price = 9.99f;
		lm.gemsProducts[3].count = 150;
		lm.gemsProducts[3].price = 14.99f;

		BoostShop bs = GameObject.Find("CanvasGlobal").transform.Find("BoostShop").GetComponent<BoostShop>();
		bs.boostProducts[0].description = "Gives you the 5 extra moves";
		bs.boostProducts[1].description = "Place this special item in game";
		bs.boostProducts[2].description = "Place this special item in game";
		bs.boostProducts[3].description = "Gives you the 30 extra seconds";
		bs.boostProducts[4].description = "Destroy the item";
		bs.boostProducts[5].description = "Place this special item in game";
		bs.boostProducts[6].description = "Switch to item that don't match";
		bs.boostProducts[7].description = "Replace the near items color";

		for (int i = 0; i < 8; i++)
		{
			bs.boostProducts[i].count[0] = 3;
			bs.boostProducts[i].count[1] = 5;
			bs.boostProducts[i].count[2] = 10;

			bs.boostProducts[i].GemPrices[0] = 5;
			bs.boostProducts[i].GemPrices[1] = 6;
			bs.boostProducts[i].GemPrices[2] = 11;

		}
		EditorUtility.SetDirty(lm);
		EditorUtility.SetDirty(bs);

	}

	#endregion

	#region leveleditor

	void TestLevel(bool playNow = true, bool testByPlay = true)
	{
		PlayerPrefs.SetInt("OpenLevelTest", levelNumber);
		PlayerPrefs.SetInt("OpenLevel", levelNumber);
		PlayerPrefs.Save();
		if (!testByPlay)
		{
			PlayerPrefs.SetInt("OpenLevelTest", 0);
			PlayerPrefs.SetInt("OpenLevel", 0);
			PlayerPrefs.Save();
		}

		if (playNow)
		{
			if (EditorApplication.isPlaying)
				EditorApplication.isPlaying = false;
			else
				EditorApplication.isPlaying = true;
		}

		lm.LoadLevel();
	}



	void GUILevelSelector()
	{
		GUILayout.Label("Level editor", EditorStyles.boldLabel, new GUILayoutOption[] { GUILayout.Width(150) });
		//bool testByPlayOld = lm.testByPlay;
		//lm.testByPlay = EditorGUILayout.Toggle("Test By Play", lm.testByPlay, new GUILayoutOption[] {
		//    GUILayout.Width (50),
		//    GUILayout.MaxWidth (200)
		//});
		//if (testByPlayOld != lm.testByPlay)
		//    TestLevel(false, lm.testByPlay);
		GUILayout.BeginHorizontal();
		{
			if (GUILayout.Button("Test level", new GUILayoutOption[] { GUILayout.Width(250) }))
			{
				TestLevel();
			}
			if (GUILayout.Button("Save level", new GUILayoutOption[] { GUILayout.Width(100) }))
			{
				SaveLevel();
			}

		}
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.Space(30);
		GUILayout.BeginVertical();
		GUILayout.BeginHorizontal();
		GUILayout.Label("Level:", EditorStyles.boldLabel, new GUILayoutOption[] { GUILayout.Width(50) });
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.Space(30);
		if (GUILayout.Button("<<", new GUILayoutOption[] { GUILayout.Width(50) }))
		{
			PreviousLevel();
		}
		string changeLvl = GUILayout.TextField(" " + levelNumber, new GUILayoutOption[] { GUILayout.Width(50) });
		try
		{
			if (int.Parse(changeLvl) != levelNumber)
			{
				if (LoadDataFromLocal(int.Parse(changeLvl)))
					levelNumber = int.Parse(changeLvl);

			}
		}
		catch (Exception)
		{

			throw;
		}

		if (GUILayout.Button(">>", new GUILayoutOption[] { GUILayout.Width(50) }))
		{
			NextLevel();
		}

		if (GUILayout.Button("New level", new GUILayoutOption[] { GUILayout.Width(100) }))
		{
			AddLevel();
		}


		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Space(60);

		GUILayout.Label("Assets/JuiceFresh/Resouces/Levels/", EditorStyles.label, new GUILayoutOption[] { GUILayout.Width(200) });
		GUILayout.EndHorizontal();
		GUILayout.EndVertical();

		GUILayout.EndHorizontal();

	}

	void GUILevelSize()
	{
		GUILayout.BeginHorizontal();
		GUILayout.Space(60);
		GUILayout.BeginVertical();

		int oldValue = maxRows + maxCols;
		maxRows = EditorGUILayout.IntField("Rows", maxRows, new GUILayoutOption[] {
			GUILayout.Width (50),
			GUILayout.MaxWidth (200)
		});
		maxCols = EditorGUILayout.IntField("Columns", maxCols, new GUILayoutOption[] {
			GUILayout.Width (50),
			GUILayout.MaxWidth (200)
		});
		if (maxRows < 3)
			maxRows = 3;
		if (maxCols < 3)
			maxCols = 3;
		if (maxRows > 10)
			maxRows = 8;
		if (maxCols > 9)
			maxCols = 7;
		if (oldValue != maxRows + maxCols)
		{
			Initialize();
			SaveLevel();
		}

		GUILayout.EndVertical();
		GUILayout.EndHorizontal();


	}

	void GUILimit()
	{
		GUILayout.BeginHorizontal();
		GUILayout.Space(60);

		GUILayout.Label("Limit:", EditorStyles.label, new GUILayoutOption[] { GUILayout.Width(50) });
		LIMIT limitTypeSave = limitType;
		int oldLimit = limit;
		limitType = (LIMIT)EditorGUILayout.EnumPopup(limitType, GUILayout.Width(93));
		if (limitType == LIMIT.MOVES)
			limit = EditorGUILayout.IntField(limit, new GUILayoutOption[] { GUILayout.Width(50) });
		else
		{
			GUILayout.BeginHorizontal();
			int limitMin = EditorGUILayout.IntField(limit / 60, new GUILayoutOption[] { GUILayout.Width(30) });
			GUILayout.Label(":", new GUILayoutOption[] { GUILayout.Width(10) });
			int limitSec = EditorGUILayout.IntField(limit - (limit / 60) * 60, new GUILayoutOption[] { GUILayout.Width(30) });
			limit = limitMin * 60 + limitSec;
			GUILayout.EndHorizontal();
		}
		if (limit <= 0)
			limit = 1;
		GUILayout.EndHorizontal();

		if (limitTypeSave != limitType || oldLimit != limit)
			SaveLevel();

	}

	void GUIColorLimit()
	{
		GUILayout.BeginHorizontal();
		GUILayout.Space(60);

		int saveInt = colorLimit;
		GUILayout.Label("Color limit:", EditorStyles.label, new GUILayoutOption[] { GUILayout.Width(100) });
		colorLimit = (int)GUILayout.HorizontalSlider(colorLimit, 3, 6, new GUILayoutOption[] { GUILayout.Width(100) });
		colorLimit = EditorGUILayout.IntField("", colorLimit, new GUILayoutOption[] { GUILayout.Width(50) });
		if (colorLimit < 3)
			colorLimit = 3;
		if (colorLimit > 6)
			colorLimit = 6;

		GUILayout.EndHorizontal();

		if (saveInt != colorLimit)
		{
			for (int i = 0; i < collectItems.Length; i++)
			{
				if (collectItems[i] != null)
					collectItems[i].enable = true;
			}
			for (int i = collectItems.Length - 1; i >= colorLimit; i--)
			{
				if (collectItems[i] != null)
					collectItems[i].enable = false;
			}
			//if (collectItems[0] > colorLimit + 2)
			//    collectItems[0] = (CollectItems)(int)(CollectItems.Item1) + 0;
			//if (collectItems[1] > colorLimit + 2)
			//    collectItems[1] = (CollectItems)(int)(CollectItems.Item1) + 1;
			SaveLevel();
		}

	}


	void GUIStars()
	{
		GUILayout.BeginHorizontal();
		GUILayout.Space(30);
		GUILayout.BeginVertical();

		GUILayout.Label("Stars:", EditorStyles.boldLabel);

		GUILayout.BeginHorizontal();
		GUILayout.Space(30);
		GUILayout.Label("Star1", new GUILayoutOption[] { GUILayout.Width(100) });
		GUILayout.Label("Star2", new GUILayoutOption[] { GUILayout.Width(100) });
		GUILayout.Label("Star3", new GUILayoutOption[] { GUILayout.Width(100) });
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.Space(30);
		int s = 0;
		s = EditorGUILayout.IntField("", star1, new GUILayoutOption[] { GUILayout.Width(100) });
		if (s != star1)
		{
			star1 = s;
			SaveLevel();
		}
		if (star1 < 0)
			star1 = 10;
		s = EditorGUILayout.IntField("", star2, new GUILayoutOption[] { GUILayout.Width(100) });
		if (s != star2)
		{
			star2 = s;
			SaveLevel();
		}
		if (star2 < star1)
			star2 = star1 + 10;
		s = EditorGUILayout.IntField("", star3, new GUILayoutOption[] { GUILayout.Width(100) });
		if (s != star3)
		{
			star3 = s;
			SaveLevel();
		}
		if (star3 < star2)
			star3 = star2 + 10;
		GUILayout.EndHorizontal();
		GUILayout.EndVertical();
		GUILayout.EndHorizontal();

	}


	void ClearIngredients()
	{
		ingr = initscript.collectedIngredients.ToArray();
		//for (int i = 0; i < initscript.collectedIngredients.Count; i++)
		//{
		//    ingr[i] = i;
		//    ingrCount[i] = 0;
		//}
		SaveLevel();
	}

	void ClearItems()
	{
		for (int i = 0; i < collectItems.Length; i++)
		{
			if (collectItems[i] == null)
				collectItems[i] = new CollectedItem();
			collectItems[i].name = "item_" + (i + 1);
			collectItems[i].check = false;
			collectItems[i].count = 1;
			collectItems[i].enable = true;

			//ingrCount[i] = 0;
		}
		for (int i = collectItems.Length - 1; i >= colorLimit; i--)
		{
			collectItems[i].enable = false;
		}

	}

	void GUITarget()
	{
		GUILayout.BeginHorizontal();
		GUILayout.Space(30);
		GUILayout.BeginVertical();
		GUILayout.Label("Target:", EditorStyles.boldLabel);
		GUILayout.BeginHorizontal();
		GUILayout.Space(30);
		GUILayout.BeginVertical();
		Target saveTarget = target;
		target = (Target)EditorGUILayout.EnumPopup(target, GUILayout.Width(100));
		if (saveTarget != target)
		{
			if (saveTarget == Target.COLLECT)
			{
				ClearIngredients();
			}
			if (saveTarget == Target.ITEMS || target == Target.ITEMS)
			{
				ClearItems();
				SaveLevel();

			}
			if (saveTarget == Target.CAGES || saveTarget == Target.BLOCKS)
			{
				for (int i = 0; i < levelSquares.Length; i++)
				{
					levelSquares[i].block = SquareTypes.EMPTY;
					levelSquares[i].obstacle = SquareTypes.NONE;
				}

			}
			SaveLevel();
		}

		if (target == Target.COLLECT)
		{
			if (GUILayout.Button("Edit", new GUILayoutOption[] { GUILayout.Width(100) }))
			{
				CollectItemsWindow.Init();
			}

			for (int i = 0; i < ingr.Length; i++)
			{

				GUILayout.BeginHorizontal();
				CollectedIngredients item = ingr[i];

				int oldCount = item.count;
				bool oldCheck = item.check;
				item.check = EditorGUILayout.Toggle(item.check, new GUILayoutOption[] { GUILayout.Width(20) });
				EditorGUILayout.LabelField(item.name, new GUILayoutOption[] { GUILayout.Width(100) });
				if (item.count <= 0)
					item.count = 1;
				item.count = EditorGUILayout.IntField("", item.count, new GUILayoutOption[] { GUILayout.Width(100) });

				GUILayout.EndHorizontal();
				if (oldCheck != item.check)
					SaveLevel();
				if (oldCount != item.count)
				{
					item.check = true;
					SaveLevel();
				}
			}
		}
		else if (target == Target.ITEMS)
		{

			for (int i = 0; i < collectItems.Length; i++)
			{
				if (collectItems[i] == null)
					collectItems[i] = new CollectedItem();

				CollectedItem item = collectItems[i];

				GUILayout.BeginHorizontal();
				int oldCount = item.count;
				bool oldCheck = item.check;
				EditorGUI.BeginDisabledGroup(!item.enable);
				item.check = EditorGUILayout.Toggle(item.check, new GUILayoutOption[] { GUILayout.Width(20) });
				EditorGUILayout.LabelField(item.name, new GUILayoutOption[] { GUILayout.Width(100) });
				if (item.count <= 0 && item.check)
					item.count = 1;

				item.count = EditorGUILayout.IntField("", item.count, new GUILayoutOption[] { GUILayout.Width(100) });
				EditorGUI.EndDisabledGroup();
				GUILayout.EndHorizontal();
				if (oldCheck != item.check)
					SaveLevel();
				if (oldCount != item.count)
				{
					item.check = true;
					SaveLevel();
				}
			}
		}
		else if (target == Target.BOMBS)
		{
			oldbombsAtTheSameTime = bombsAtTheSameTime;
			bombsAtTheSameTime = EditorGUILayout.IntField("bombs collect", bombsAtTheSameTime, new GUILayoutOption[] { GUILayout.Width(200) });
			if (bombsAtTheSameTime <= 0)
				bombsAtTheSameTime = 1;
			if (oldbombsAtTheSameTime != bombsAtTheSameTime)
				SaveLevel();
			oldbombTimer = bombTimer;
			bombTimer = EditorGUILayout.IntField("bombs counter", bombTimer, new GUILayoutOption[] { GUILayout.Width(200) });
			if (bombTimer <= 0)
				bombTimer = 1;
			if (oldbombTimer != bombTimer)
				SaveLevel();

		}
		else if (target == Target.CAGES)
		{

			oldcageHP = cageHP;
			cageHP = EditorGUILayout.IntField("cageHP", cageHP, new GUILayoutOption[] { GUILayout.Width(200) });
			if (cageHP <= 0)
				cageHP = 1;
			if (oldcageHP != cageHP)
				SaveLevel();
		}
		else if (target == Target.SCORE)
		{
			if (starsTargetCount == 0)
				starsTargetCount = CollectStars.STAR_1;
			oldstarsTargetCount = starsTargetCount;
			starsTargetCount = (CollectStars)EditorGUILayout.EnumPopup(starsTargetCount, GUILayout.Width(100));
			if (oldstarsTargetCount != starsTargetCount)
				SaveLevel();
		}


		GUILayout.EndVertical();
		GUILayout.EndHorizontal();
		GUILayout.EndVertical();
		GUILayout.EndHorizontal();
	}


	void GUIBlocks()
	{
		GUILayout.BeginHorizontal();
		{

			GUILayout.Space(30);
			GUILayout.BeginVertical();

			GUILayout.Label("Tools:", EditorStyles.boldLabel);
			GUILayout.BeginHorizontal();
			GUILayout.Space(30);
			if (GUILayout.Button("Clear", new GUILayoutOption[] { GUILayout.Width(50), GUILayout.Height(50) }))
			{
				for (int i = 0; i < levelSquares.Length; i++)
				{
					levelSquares[i].block = SquareTypes.EMPTY;
					levelSquares[i].obstacle = SquareTypes.NONE;
				}
				SaveLevel();
			}
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();
		}
		GUILayout.EndHorizontal();


		GUILayout.Space(10);
		GUILayout.BeginHorizontal();
		{
			GUILayout.Space(30);
			GUILayout.BeginVertical();
			{
				GUILayout.Label("Blocks:", EditorStyles.boldLabel);
				GUILayout.BeginHorizontal(new GUILayoutOption[] { GUILayout.MaxWidth(150) });
				{
					GUILayout.Space(30);
					GUILayout.BeginVertical();
					{
						GUILayout.BeginHorizontal();
						{
							GUI.color = new Color(1, 1, 1, 1f);
							if (GUILayout.Button(squareTex, new GUILayoutOption[] {
								GUILayout.Width (50),
								GUILayout.Height (50)
							}))
							{
								squareType = SquareTypes.EMPTY;
							}

							GUILayout.Label(" - empty", EditorStyles.boldLabel);
						}
						GUILayout.EndHorizontal();

						if (target == Target.BLOCKS)
						{
							GUILayout.BeginHorizontal();
							{
								GUI.color = new Color(0.8f, 1, 1, 1f);
								if (GUILayout.Button(blockTex, new GUILayoutOption[] {
									GUILayout.Width (50),
									GUILayout.Height (50),
									GUILayout.MaxWidth (50)
								}))
								{
									squareType = SquareTypes.BLOCK;

								}

								GUILayout.Label(" - block /\n  double click x2", EditorStyles.boldLabel);
							}
							GUILayout.EndHorizontal();
						}

						GUILayout.BeginHorizontal();
						{

							GUI.color = new Color(1, 1, 1, 1f);

							if (GUILayout.Button("X", new GUILayoutOption[] {
								GUILayout.Width (50),
								GUILayout.Height (50)
							}))
							{
								squareType = SquareTypes.NONE;
							}

							GUILayout.Label(" - none", EditorStyles.boldLabel);
						}
						GUILayout.EndHorizontal();
						GUILayout.BeginHorizontal();
						{
							if (GUILayout.Button(thrivingBlockTex, new GUILayoutOption[] {
								GUILayout.Width (50),
								GUILayout.Height (50)
							}))
							{
								squareType = SquareTypes.THRIVING;

							}

							GUILayout.Label("-thriving\n block", EditorStyles.boldLabel);
						}
						GUILayout.EndHorizontal();
					}
					GUILayout.EndVertical();

					GUILayout.BeginVertical();
					{
						if (target == Target.CAGES)
						{
							GUILayout.BeginHorizontal();
							{
								GUIStyle style = new GUIStyle();

								if (GUILayout.Button(wireBlockTex, new GUILayoutOption[] {
									GUILayout.Width (50),
									GUILayout.Height (50)
								}))
								{
									squareType = SquareTypes.WIREBLOCK;

								}

								GUILayout.Label(" - cage block", EditorStyles.boldLabel);
							}
							GUILayout.EndHorizontal();
						}
						GUILayout.BeginHorizontal();
						{

							if (GUILayout.Button(solidBlockTex, new GUILayoutOption[] {
								GUILayout.Width (50),
								GUILayout.Height (50)
							}))
							{
								squareType = SquareTypes.SOLIDBLOCK;

							}

							GUILayout.Label(" - solid /\n  double click x2", EditorStyles.boldLabel);
						}
						GUILayout.EndHorizontal();
						//GUILayout.BeginHorizontal();
						//if (GUILayout.Button(undestroyableBlockTex, new GUILayoutOption[] { GUILayout.Width(50), GUILayout.Height(50) }))
						//{
						//    squareType = SquareTypes.UNDESTROYABLE;

						//}

						//GUILayout.Label("-undestroyable\n block", EditorStyles.boldLabel);

						//GUILayout.EndHorizontal();
					}
					GUILayout.EndVertical();
				}
				GUILayout.EndHorizontal();
			}
			GUILayout.EndVertical();
		}
		GUILayout.EndHorizontal();

	}

	void GUIGameField()
	{
		if (levelSquares.Length == 0)
			return;
		GUILayout.BeginVertical();
		for (int row = 0; row < maxRows; row++)
		{
			GUILayout.BeginHorizontal();

			for (int col = 0; col < maxCols; col++)
			{
				Color squareColor = new Color(0.8f, 0.8f, 0.8f);

				var imageButton = new object();
				if (levelSquares[row * maxCols + col].block == SquareTypes.NONE)
				{
					imageButton = "X";
					squareColor = new Color(0.8f, 0.8f, 0.8f);
				}
				else if (levelSquares[row * maxCols + col].block == SquareTypes.EMPTY)
				{
					imageButton = squareTex;
					squareColor = Color.white;
					if (levelSquares[row * maxCols + col].obstacle == SquareTypes.WIREBLOCK)
					{
						imageButton = wireBlockTex;
						squareColor = Color.white;
					}
					else if (levelSquares[row * maxCols + col].obstacle == SquareTypes.SOLIDBLOCK)
					{
						imageButton = solidBlockTex;
						squareColor = Color.white;
					}
					else if (levelSquares[row * maxCols + col].obstacle == SquareTypes.DOUBLESOLIDBLOCK)
					{
						imageButton = doublesolidBlockTex;
						squareColor = Color.white;
					}
					//else if (levelSquares[row * maxCols + col].obstacle == SquareTypes.UNDESTROYABLE)
					//{
					//    imageButton = undestroyableBlockTex;
					//    squareColor = Color.white;
					//}
					else if (levelSquares[row * maxCols + col].obstacle == SquareTypes.THRIVING)
					{
						imageButton = thrivingBlockTex;
						squareColor = Color.white;
					}

				}
				else if (levelSquares[row * maxCols + col].block == SquareTypes.BLOCK)
				{
					imageButton = blockTex;
					if (levelSquares[row * maxCols + col].obstacle == SquareTypes.WIREBLOCK)
					{
						imageButton = wireBlockTex;
						squareColor = Color.white;
					}
					else if (levelSquares[row * maxCols + col].obstacle == SquareTypes.SOLIDBLOCK)
					{
						imageButton = solidBlockTex;
						squareColor = Color.white;
					}
					else if (levelSquares[row * maxCols + col].obstacle == SquareTypes.DOUBLESOLIDBLOCK)
					{
						imageButton = doublesolidBlockTex;
						squareColor = Color.white;
					}
					//else if (levelSquares[row * maxCols + col].obstacle == SquareTypes.UNDESTROYABLE)
					//{
					//    imageButton = undestroyableBlockTex;
					//    squareColor = Color.white;
					//}
					else if (levelSquares[row * maxCols + col].obstacle == SquareTypes.THRIVING)
					{
						imageButton = thrivingBlockTex;
						squareColor = Color.white;
					}
					//     squareColor = new Color(0.8f, 1, 1, 1f);
				}
				else if (levelSquares[row * maxCols + col].block == SquareTypes.DOUBLEBLOCK)
				{
					imageButton = blockTex2;
					if (levelSquares[row * maxCols + col].obstacle == SquareTypes.WIREBLOCK)
					{
						imageButton = wireBlockTex;
						squareColor = Color.white;
					}
					else if (levelSquares[row * maxCols + col].obstacle == SquareTypes.SOLIDBLOCK)
					{
						imageButton = solidBlockTex;
						squareColor = Color.white;
					}
					else if (levelSquares[row * maxCols + col].obstacle == SquareTypes.DOUBLESOLIDBLOCK)
					{
						imageButton = doublesolidBlockTex;
						squareColor = Color.white;
					}
					//else if (levelSquares[row * maxCols + col].obstacle == SquareTypes.UNDESTROYABLE)
					//{
					//    imageButton = undestroyableBlockTex;
					//    squareColor = Color.white;
					//}
					else if (levelSquares[row * maxCols + col].obstacle == SquareTypes.THRIVING)
					{
						imageButton = thrivingBlockTex;
						squareColor = Color.white;
					}
					// squareColor = new Color(0.3f, 1, 1, 1f);
				}
				GUI.color = squareColor;
				if (GUILayout.Button(imageButton as Texture, new GUILayoutOption[] {
					GUILayout.Width (50),
					GUILayout.Height (50)
				}))
				{
					SetType(col, row);
				}
			}
			GUILayout.EndHorizontal();
		}
		GUILayout.EndVertical();
	}


	void SaveLevel()
	{
		if (!fileName.Contains(".txt"))
			fileName += ".txt";
		SaveMap(fileName);
	}

	void AddLevel()
	{
		SaveLevel();
		levelNumber = GetLastLevel() + 1;
		Initialize();
		SaveLevel();
	}

	void CreateLevel()
	{
		//levelSquares = new LevelSquare[81];
		//for (int i = 0; i < levelSquares.Length; i++)
		//{
		//    levelSquares[i] = new LevelSquare();
		//}
		//Level newLevel = new Level();
		//newLevel.number = levelNumber;
		//newLevel.squares = levelSquares;
		levelNumber++;
	}

	int GetLastLevel()
	{
		TextAsset mapText = null;
		for (int i = levelNumber; i < 50000; i++)
		{
			mapText = Resources.Load("Levels/" + i) as TextAsset;
			if (mapText == null)
			{
				return i - 1;
			}
		}
		return 0;
	}

	void DeleteLevel()
	{

	}

	void NextLevel()
	{
		levelNumber++;
		if (!LoadDataFromLocal(levelNumber))
			levelNumber--;
	}

	void PreviousLevel()
	{
		levelNumber--;
		if (levelNumber < 1)
			levelNumber = 1;
		if (!LoadDataFromLocal(levelNumber))
			levelNumber++;


	}

	void SetType(int col, int row)
	{
		if (squareType == SquareTypes.BLOCK)
		{
			if (levelSquares[row * maxCols + col].block == SquareTypes.BLOCK)
				levelSquares[row * maxCols + col].block = SquareTypes.DOUBLEBLOCK;
			else
				levelSquares[row * maxCols + col].block = SquareTypes.BLOCK;
		}
		else if (squareType == SquareTypes.WIREBLOCK || squareType == SquareTypes.SOLIDBLOCK || squareType == SquareTypes.DOUBLESOLIDBLOCK || squareType == SquareTypes.UNDESTROYABLE || squareType == SquareTypes.THRIVING)
		{
			if (levelSquares[row * maxCols + col].obstacle == SquareTypes.SOLIDBLOCK && squareType == SquareTypes.SOLIDBLOCK)
				squareType = SquareTypes.DOUBLESOLIDBLOCK;
			else if (levelSquares[row * maxCols + col].obstacle == SquareTypes.DOUBLESOLIDBLOCK && squareType == SquareTypes.SOLIDBLOCK)
				squareType = SquareTypes.SOLIDBLOCK;
			levelSquares[row * maxCols + col].obstacle = squareType;
			if (squareType == SquareTypes.DOUBLESOLIDBLOCK)
				squareType = SquareTypes.SOLIDBLOCK;
		}
		else
		{
			levelSquares[row * maxCols + col].block = squareType;
			levelSquares[row * maxCols + col].obstacle = SquareTypes.NONE;
		}
		update = true;
		SaveLevel();
		// GetSquare(col, row).type = (int) squareType;
	}

	public void SaveMap(string fileName)
	{
		string saveString = "";
		//Create save string
		saveString += "MODE " + (int)target;
		saveString += "\r\n";
		saveString += "SIZE " + maxCols + "/" + maxRows;
		saveString += "\r\n";
		saveString += "LIMIT " + (int)limitType + "/" + limit;
		saveString += "\r\n";
		saveString += "COLOR LIMIT " + colorLimit;
		saveString += "\r\n";
		saveString += "STARS " + star1 + "/" + star2 + "/" + star3;
		saveString += "\r\n";

		if (target == Target.COLLECT)
		{

			saveString += "COLLECT ITEMS ";
			for (int i = 0; i < ingr.Length; i++)
			{
				if (ingr[i].check)
					saveString += i + "/";
			}
			saveString += "\r\n";

			saveString += "COLLECT COUNT ";
			for (int i = 0; i < ingr.Length; i++)
			{
				if (ingr[i].check)
					saveString += (int)ingr[i].count + "/";
			}
		}
		else if (target == Target.ITEMS)
		{

			saveString += "COLLECT ITEMS ";
			for (int i = 0; i < collectItems.Length; i++)
			{
				if (collectItems[i].check && collectItems[i].enable)
					saveString += i + "/";
			}
			saveString += "\r\n";

			saveString += "COLLECT COUNT ";
			for (int i = 0; i < collectItems.Length; i++)
			{
				if (collectItems[i].check && collectItems[i].enable)
					saveString += (int)collectItems[i].count + "/";
			}
		}
		else if (target == Target.CAGES)
			saveString += "CAGE " + (int)cageHP;
		else if (target == Target.BOMBS)
		{
			saveString += "GETSTARS " + (int)starsTargetCount;
			saveString += "\r\n";
			saveString += "BOMBS " + bombsAtTheSameTime + "/" + bombTimer;

		}
		else if (target == Target.SCORE)
			saveString += "GETSTARS " + (int)starsTargetCount;
		else
		{

			saveString += "COLLECT ITEMS ";
			for (int i = 0; i < NumIngredients; i++)
			{
				if (ingr[i].check)
					saveString += i + "/";
			}
		}



		saveString += "\r\n";


		//set map data
		for (int row = 0; row < maxRows; row++)
		{
			for (int col = 0; col < maxCols; col++)
			{
				saveString += (int)levelSquares[row * maxCols + col].block + "" + (int)levelSquares[row * maxCols + col].obstacle;
				//if this column not yet end of row, add space between them
				if (col < (maxCols - 1))
					saveString += " ";
			}
			//if this row is not yet end of row, add new line symbol between rows
			if (row < (maxRows - 1))
				saveString += "\r\n";
		}
		if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.LinuxEditor) //1.4.9
		{
			//Write to file
			string activeDir = Application.dataPath + @"/JuiceFresh/Resources/Levels/";
			string newPath = System.IO.Path.Combine(activeDir, levelNumber + ".txt");
			StreamWriter sw = new StreamWriter(newPath);
			sw.Write(saveString);
			sw.Close();
		}
		AssetDatabase.Refresh();
	}

	public bool LoadDataFromLocal(int currentLevel)
	{
		//Read data from text file
		TextAsset mapText = Resources.Load("Levels/" + currentLevel) as TextAsset;
		if (mapText == null)
		{
			return false;
			SaveLevel();
			mapText = Resources.Load("Levels/" + currentLevel) as TextAsset;
		}
		ProcessGameDataFromString(mapText.text);
		return true;
	}

	void ProcessGameDataFromString(string mapText)
	{

		string[] lines = mapText.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
		int[] indexItems = new int[10];
		ingr = initscript.collectedIngredients.ToArray();
		int mapLine = 0;
		foreach (string line in lines)
		{
			if (line.StartsWith("MODE "))
			{
				string modeString = line.Replace("MODE", string.Empty).Trim();
				target = (Target)int.Parse(modeString);
			}
			else if (line.StartsWith("SIZE "))
			{
				string blocksString = line.Replace("SIZE", string.Empty).Trim();
				string[] sizes = blocksString.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
				maxCols = int.Parse(sizes[0]);
				maxRows = int.Parse(sizes[1]);
				Initialize();
			}
			else if (line.StartsWith("LIMIT "))
			{
				string blocksString = line.Replace("LIMIT", string.Empty).Trim();
				string[] sizes = blocksString.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
				limitType = (LIMIT)int.Parse(sizes[0]);
				limit = int.Parse(sizes[1]);

			}
			else if (line.StartsWith("COLOR LIMIT "))
			{
				string blocksString = line.Replace("COLOR LIMIT", string.Empty).Trim();
				colorLimit = int.Parse(blocksString);
				ClearItems();

			}
			else if (line.StartsWith("STARS "))
			{
				string blocksString = line.Replace("STARS", string.Empty).Trim();
				string[] blocksNumbers = blocksString.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
				star1 = int.Parse(blocksNumbers[0]);
				star2 = int.Parse(blocksNumbers[1]);
				star3 = int.Parse(blocksNumbers[2]);
			}
			else if (line.StartsWith("COLLECT ITEMS "))
			{
				string blocksString = line.Replace("COLLECT ITEMS", string.Empty).Trim();
				string[] blocksNumbers = blocksString.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);

				for (int i = 0; i < blocksNumbers.Length; i++)
				{
					if (collectItems[i] == null)
						collectItems[i] = new CollectedItem();

					if (collectItems.Length > i)
					{
						collectItems[int.Parse(blocksNumbers[i])].check = true;
						indexItems[i] = int.Parse(blocksNumbers[i]);
					}

					if (target == Target.COLLECT)
					{
						if (ingr.Length > i)
							ingr[int.Parse(blocksNumbers[i])].check = true;
					}

				}
			}
			else if (line.StartsWith("COLLECT COUNT "))
			{
				string blocksString = line.Replace("COLLECT COUNT", string.Empty).Trim();
				string[] blocksNumbers = blocksString.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
				for (int i = 0; i < blocksNumbers.Length; i++)
				{
					if (collectItems[i] == null)
						collectItems[i] = new CollectedItem();

					collectItems[indexItems[i]].count = int.Parse(blocksNumbers[i]);

					if (target == Target.COLLECT)
					{
						if (ingr.Length > i)
							ingr[i].count = int.Parse(blocksNumbers[i]);
					}

				}
			}
			else if (line.StartsWith("CAGE "))
			{
				string blocksString = line.Replace("CAGE ", string.Empty).Trim();
				cageHP = int.Parse(blocksString);
			}
			else if (line.StartsWith("BOMBS "))
			{
				string blocksString = line.Replace("BOMBS ", string.Empty).Trim();
				string[] blocksNumbers = blocksString.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
				bombsAtTheSameTime = int.Parse(blocksNumbers[0]);
				bombTimer = int.Parse(blocksNumbers[1]);
			}
			else if (line.StartsWith("GETSTARS "))
			{
				string blocksString = line.Replace("GETSTARS ", string.Empty).Trim();
				starsTargetCount = (CollectStars)int.Parse(blocksString);
			}
			else
			{ //Maps
			  //Split lines again to get map numbers
				string[] st = line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
				for (int i = 0; i < st.Length; i++)
				{
					levelSquares[mapLine * maxCols + i].block = (SquareTypes)int.Parse(st[i][0].ToString());
					levelSquares[mapLine * maxCols + i].obstacle = (SquareTypes)int.Parse(st[i][1].ToString());
				}
				mapLine++;
			}
		}
	}

	#endregion
}

[System.Serializable]
public class CollectedItem
{
	public string name;
	public bool check;
	public int count;
	public bool enable;

}
