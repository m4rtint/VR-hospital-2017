﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BathroomMonster : Monster {

    enum MonsterStage
    {
        DoorOpen,
        DoorCloseChance,
        DoorClosed,
        MonsterMovesDown,
        UserFailed,
    }

    //Current stage
    MonsterStage currentStage = MonsterStage.DoorOpen;

	[SerializeField]
	private GameObject DoorHinge;
	doorOpen door;

    //Open Door
    float doorOpenDuration;

    //Seconds for user to close the door
    public float minSecondsForUserToCloseDoor;
    public float maxSecondsForUserToCloseDoor;
    float secondsBeforeUserLose;

    //Disappearing Monster properties
    float speedOfDisappearance = 1;
    Vector3 disappearDestination;

	protected override void setupMonsterToStartAttack() {
		//Door
		door = DoorHinge.GetComponent<doorOpen>();
		doorOpenDuration = Random.Range(3, 5);

		//Seconds for user to close the door
		secondsBeforeUserLose = Random.Range(minSecondsForUserToCloseDoor, maxSecondsForUserToCloseDoor);

		//Disappearing bathroom monster disapearrance
		disappearDestination = transform.position;
		disappearDestination.y = -2;
	
		door.setDoorAngleWithDuration(door.openDoorAngle, doorOpenDuration);
	}

	protected override void setTimeUntilJumpScare() {
		timeUntilJumpScare = Random.Range(0,2)+doorOpenDuration;
	}

	// Update is called once per frame
	void Update () {
        switch(currentStage)
        {
            case MonsterStage.DoorOpen:
                finishedOpenDoor();
                break;
            case MonsterStage.DoorCloseChance:
                doorClosingChance();
                break;
            case MonsterStage.DoorClosed:
				playerWins();
                break;
            case MonsterStage.MonsterMovesDown:
                monsterDisappears();
                break;
            case MonsterStage.UserFailed:
				gameOver();
                break;
        }
		
	}
    
    void finishedOpenDoor()
    {
        if (door.canInteractWithDoor)
        {
            currentStage = MonsterStage.DoorCloseChance;
        }
    }


    void doorClosingChance()
    {
        secondsBeforeUserLose -= Time.deltaTime;
        if (secondsBeforeUserLose < 0)
        {
            //User loses
            door.setUserLost();
            currentStage = MonsterStage.MonsterMovesDown;
            door.setUserLost();
            door.setDoorAngleWithDuration(door.loseDoorAngle, doorOpenDuration);
        }

        //Check if Door is closed
		if ((int)DoorHinge.transform.rotation.eulerAngles.y == 0)
        {
            Debug.Log(secondsBeforeUserLose);
            //User able to close the door
            currentStage = MonsterStage.DoorClosed;
        }
    }

    void monsterDisappears()
    {
        float step = speedOfDisappearance * Time.deltaTime;
        gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, disappearDestination, step);

        if (gameObject.transform.position == disappearDestination)
        {
            currentStage = MonsterStage.UserFailed;
        }
    }
}