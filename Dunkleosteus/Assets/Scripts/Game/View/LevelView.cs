﻿using UnityEngine;
using System.Collections;
using GlobalDefines;
public class LevelView : MonoBehaviour {

    public string levelName;
    private UISprite levelSprite;
    private GameObject shine;
    private UIButton button;
	// Use this for initialization
    void Awake()
    {
    }
    
	void Start () {
        // Set listener
        GameObject controller = GameObject.Find("UI Root/GameController");
        UIEventTrigger trigger = this.gameObject.GetComponent<UIEventTrigger>();
        EventDelegate eventDel = new EventDelegate(controller.GetComponent<EventController>(), "OnSelectLevel");
        eventDel.parameters[0] = new EventDelegate.Parameter(this, "levelName");
        trigger.onClick.Add(eventDel);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void Init(string name) {
        levelSprite = this.gameObject.GetComponent<UISprite>();
        shine = this.gameObject.transform.Find("Shine").gameObject;
        levelName = name;
    }
    public void Show(LevelState state)
    {
        switch (state) {
            case LevelState.Unabled:
                levelSprite.gameObject.SetActive(false);
                shine.SetActive(false);
                break;
            case LevelState.Current:
                levelSprite.gameObject.SetActive(true);
                levelSprite.spriteName = PathContainer.YellowLargeCircle;
                shine.SetActive(true);
                break;
            case LevelState.Finished:
                levelSprite.gameObject.SetActive(true);
                levelSprite.spriteName = PathContainer.BlueLargeCircle;
                shine.SetActive(false);
                break;
            case LevelState.Spot:
                levelSprite.gameObject.SetActive(true);
                levelSprite.spriteName = PathContainer.SpotSquare;
                shine.SetActive(false);
                break;
            default:
                break;
        }
    }

}
