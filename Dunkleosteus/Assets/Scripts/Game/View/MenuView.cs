﻿using UnityEngine;
using System.Collections;
using GlobalDefines;

public class MenuView : MonoBehaviour {
    public string catagory;
    public string abbr;
    private UILabel labelSelf;

    void Awake() {
        labelSelf = this.gameObject.GetComponent<UILabel>();
        GameObject controller = GameObject.Find("UI Root/GameController");
        UIEventTrigger trigger = this.gameObject.GetComponent<UIEventTrigger>();
        EventDelegate eventDel = new EventDelegate(controller.GetComponent<EventController>(), "OnClickCardMenu");
        eventDel.parameters[0] = new EventDelegate.Parameter(this, "catagory");
        trigger.onClick.Add(eventDel);
    }
	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void Show(string catagoryName)
    {
        if(catagory == catagoryName) {
            // dark
            labelSelf.text = DefineString.DarkBlueColor + abbr + "[-]";
        } else {
            // bright
            labelSelf.text = DefineString.NormalBlueColor + abbr + "[-]";
        }
    }
}
