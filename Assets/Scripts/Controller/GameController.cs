﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

	public static GameController instance = null;

	[SerializeField]
	private GameObject[] monsters;
	[SerializeField]
	private GameObject[] ambients;
	[SerializeField]
	private int[, ] waveStave; // waveStave [waveNum, monster#, amb#]
	[Header("Fear increase")]
	[SerializeField]
	private float Rate;
	[SerializeField]
	private GameObject lamp;

	float secondsPassedInWave = 0;
	float fearSecondsPassed = 0;
	float timerUntilNextMonster;
	bool monsterActivated = false;
	bool ambientActivated = false;
	int waveNum = 0;
	[SerializeField]
	int maxWave = 3;

	#region MonoDevelop
	public void Awake()
	{
		if(instance == null)
		{
			instance = this;
		}
		else if(instance != this)
		{
			Destroy(this.gameObject);
		}
		DontDestroyOnLoad(this.gameObject);

		InitGame();
	}



	// Update is called once per frame
	void Update () {
		//If it's anything other than in game - nothing will happen
		if (GameStatus.instance.currentStatus != Status.InGame) { return; }

		if (waveNum < maxWave) {
			// While game is active
			bool lampOn = lamp.GetComponent<BedSideLight>().isLightOn;

			secondsPassedInWave += Time.deltaTime * (lampOn ? 1.3f : 1.0f);
			int actorsSize = monsters.Length + ambients.Length;
			for (int i = 0; i < actorsSize; i++) {
				if (waveStave [waveNum, i] >= 0) {
					// If the thingy is larger than 0, this monst/amb is not finished yet.

					if (waveStave [waveNum, i] <= secondsPassedInWave) {
						if (i < monsters.Length) monsterActivated = true;
						Debug.Log ("Spawned #" + i);

						// If this monst/amb is ready to go, disable it's timer and launch it
						waveStave [waveNum, i] = -1;

						if (i >= monsters.Length) {
							ambients [i - monsters.Length].GetComponent<Ambient> ().StartAmb ();
						} else {
							monsters [i].GetComponent<Monster> ().launchAttack ();
						}

					}
				}
			}
			if (monsterActivated) {
				increaseFear ();
			}
		} else {
			//Win State
			EndGame (true);
		}

	}

	#endregion

	#region setup

	// Use this for initialization
	void InitGame()
	{
		setVariables();
		setupDelegates();
	}

	void setVariables()
	{
		timerUntilNextMonster = Random.Range(5, 10);
		setMonsterGameObjectActive(false);
		randomizeTimings ();
	}

	void setupDelegates()
	{
		for (int i = 0; i < monsters.Length; i++)
		{
			monsters[i].GetComponent<Monster>().onPlayerWin += setMonsterActivatedFalse;
		}

		for (int i = 0; i < ambients.Length; i++)
		{
			ambients[i].GetComponent<Ambient>().onEnd += setAmbientActivatedFalse;
		}
	}

	#endregion

	#region Fear

	void increaseFear()
	{
		fearSecondsPassed += Time.deltaTime;
		if (fearSecondsPassed > Rate &&
			!lamp.GetComponent<BedSideLight>().isLightOn)
		{
			//Increase camera shake
			GetComponent<FearShakeController>().increaseCameraShake();
			fearSecondsPassed = 0;
		}
	}
	#endregion

	#region waveController

	void randomizeTimings() {
		waveNum = 0;
		secondsPassedInWave = 0;
		// Randomly assigns timings to monst/ambs
		int actorsSize = monsters.Length + ambients.Length;
		waveStave = new int [maxWave, actorsSize];
		for (int i = 0; i < actorsSize; i++) {
			for (int j = 0; j < maxWave; j++) {
				if (i < monsters.Length)
					waveStave [j, i] = -1;
				else
					waveStave [j, i] = Random.Range (-4, 13);
			}
		}

		for (int i = 0; i < maxWave; i++) {
			if (i == 0) {
				waveStave [i, Random.Range (0, monsters.Length)] = Random.Range (0, 5);
			} else {
				int firstNum = Random.Range (0, monsters.Length);
				int secNum = Random.Range (0, monsters.Length);
				while (firstNum == secNum) {
					secNum = Random.Range (0, monsters.Length);
				}
				waveStave [i, firstNum] = Random.Range (4, 10);
				waveStave [i, secNum] = Random.Range (0, 6);
			}
		}

		string debugStave = "";
		for (int i = 0; i < maxWave; i++) {
			debugStave += "{";
			for (int j = 0; j < actorsSize; j++) {
				debugStave += waveStave [i, j] + ", ";
			}
			debugStave += "}" + System.Environment.NewLine;
		}
		Debug.Log (debugStave);
	}

	#endregion

	#region Monsters
	void setTimeUntilNextMonster() {
		timerUntilNextMonster = Random.Range (5, 10);
	}

	void waveFinished() {

	}

	void setMonsterActivatedFalse() {
		//AUDIO
		AudioController.instance.STOP(TYPE.MONSTER);
		bool waveFinished = true;
		for (int i = 0; i < monsters.Length; i++) {
			if (waveStave [waveNum, i] >= 0) {
				waveFinished = false;
				break;
			}
		}
		if (waveFinished) {
			// Continue to next wave
			Debug.Log("Wave" + waveNum +"completed!");
			secondsPassedInWave = 0;
			waveNum++;
			monsterActivated = false;
		}
		//		setTimeUntilNextMonster ();
	}

	//Randomizer that chooses the monster
	int selectMonster() {
		int max = monsters.Length;
		return Random.Range (0, max);
	}
	void resetAllMonsters() {
		for (int i = 0; i < monsters.Length; i++)
		{
			monsters[i].GetComponent<Monster>().resetMonster();
			monsters[i].SetActive(false);
		}
	}

	#endregion

	#region Ambient


	void setAmbientActivatedFalse()
	{
		AudioController.instance.STOP(TYPE.AMBIENT);
		ambientActivated = false;
	}

	int selectAmbient()
	{
		int max = ambients.Length;
		return Random.Range(0, max);
	}

	#endregion
	#region Game States
	public void StartGame() {
		//Set State
		GameStatus.instance.currentStatus = Status.InGame;
		UserInterfaceController.instance.PlayGame ();
		//AUDIO
		AudioController.instance.PLAY(AudioController.instance.AUDIO.StartGame,TYPE.UI);
	}

	public void EndGame(bool didWin) {
		//Set the State
		GameStatus.instance.currentStatus = Status.EndGame;
		UserInterfaceController.instance.GameOver();
		for (int i = 0; i < monsters.Length; i++)
		{
			monsters[i].SetActive(false);
		}
		//AUDIO
		AudioController.instance.STOPALL();
	}

	public void ResetScene() {
		// Game Controller
		reset();
		//Fear Shake Controller
		GetComponent<FearShakeController>().resetShake();
		//Game Status
		GameStatus.instance.currentStatus = Status.MainMenu;
		//UI Controller
		UserInterfaceController.instance.setupUI();
		//Timer Controller
		GetComponent<TimerController>().resetTime();
	}

	private void reset()
	{
		setMonsterActivatedFalse();
		setVariables();
		resetAllMonsters ();
	}

	private void setMonsterGameObjectActive(bool set)
	{
		for (int i = 0; i < monsters.Length; i++)
		{
			monsters[i].SetActive(set);
		}
	}
	#endregion
}
