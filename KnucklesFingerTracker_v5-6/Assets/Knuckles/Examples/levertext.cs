using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class levertext : MonoBehaviour {
	public Lever l;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		GetComponent<TextMesh> ().text = ""+Mathf.Round(l.value*100)/100;
	}
}
