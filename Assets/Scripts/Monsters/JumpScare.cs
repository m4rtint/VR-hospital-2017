﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EZCameraShake;

public class JumpScare : MonoBehaviour {
    bool endGame = false;

    Vector3 spawnLocation;
    Vector3 spawnRotation;

    Vector3 destination;

    public float speedOfJumpScare;

    public float Magnitude = 10f;
    public float Roughness = 5;
    public float FadeInTime = 0.1f;

    [SerializeField]
    GameObject crawler;

    private void Start()
    {
        spawnLocation = new Vector3(0, -9, 2.86f);
        spawnRotation = new Vector3(0, -90, 0);

        destination = spawnLocation;
        destination.y = -4.8f;
    }

    public void startJumpScare()
    {
        crawler.SetActive(true);
        crawler.transform.localPosition = spawnLocation;
        crawler.transform.localRotation = Quaternion.Euler(spawnRotation);
        endGame = true;

        //Shake the screen
        shakeEnemy();

        //Black Screen
		StartCoroutine("SetGameOverScreen");
	
        //Add Audio
        crawler.GetComponent<AudioSource>().Play();
    }

    // Update is called once per frame
    void Update () {
        if (endGame)
        {
            float step = speedOfJumpScare * Time.deltaTime;
            crawler.transform.localPosition = Vector3.MoveTowards(crawler.transform.localPosition, destination, step);
            if (crawler.transform.localPosition == destination)
            {
                endGame = false;
                
            }
            gameObject.transform.rotation = Quaternion.Slerp(gameObject.transform.rotation, Quaternion.Euler(Vector3.zero), step);
        }
    }

	IEnumerator SetGameOverScreen() {
		yield return new WaitForSeconds (1);
		GameController.instance.EndGame ();
	}

    void shakeEnemy()
    {
        CameraShaker.Instance.StartShake(Magnitude, Roughness, FadeInTime);
    }
}
