using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(HingeJoint))]
public class Lever : MonoBehaviour {

	public float value;

	HingeJoint j;

	public float threshold = 0.001f;

	void Start(){
		j = GetComponent<HingeJoint> ();
	}
	
	// Update is called once per frame
	void Update () {
		value = Mathf.InverseLerp (j.limits.min, j.limits.max, j.angle);
		if (value < threshold) {
			value = 0;
		}
		if (value > 1-threshold) {
			value = 1;
		}
	}
}
