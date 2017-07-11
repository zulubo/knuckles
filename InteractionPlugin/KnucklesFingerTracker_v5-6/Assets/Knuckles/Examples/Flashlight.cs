using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flashlight : MonoBehaviour {
	public GameObject beam;

	public KnucklesInteractable interaction;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (interaction.inputDevice != null) {
			if (interaction.inputDevice.GetPressDown (Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad)) {
				beam.SetActive (!beam.activeSelf);
			}
		}
	}
}
