using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KnucklesUIInteractable : MonoBehaviour {
	float initscale; // used for animating clicks

	// Use this for initialization
	void Start () {
		initscale = transform.localScale.x;

		RectTransform t = GetComponent<RectTransform>();
		gameObject.AddComponent<BoxCollider> ().size = new Vector3 (t.rect.size.x, t.rect.size.y, 10); // add a collider so it can be clicked
		gameObject.AddComponent<Rigidbody> ().isKinematic = true; //add a kinematic rigidbody so the finger recognizes it
		gameObject.AddComponent<KnucklesInteractable> (); 
		GetComponent<KnucklesInteractable> ().Kind = KnucklesInteractable.interactType.NonGrabbable;
		GetComponent<KnucklesInteractable> ().onPoked.AddListener (delegate { // make it so when it's poked this script gets the message
			Poked();	
		});
	}

	public void Poked(){
		if (GetComponent<Button> ()) { // if we're a button, click the button
			GetComponent<Button> ().onClick.Invoke ();
		}
		if (GetComponent<Toggle> ()) { // if we're a toggle, toggle the toggle
			GetComponent<Toggle> ().isOn = !GetComponent<Toggle> ().isOn;
		}

		StartCoroutine (clickAnim ());
	}

	IEnumerator clickAnim(){ // this is just a little animation for clicking it.
		float f = 1;
		while (f < 1.2f) {
			transform.localScale = Vector3.one * f * initscale;
			f += Time.deltaTime * 4;
			yield return null;
		}
		while (f > 1) {
			transform.localScale = Vector3.one * f * initscale;
			f -= Time.deltaTime * 2;
			yield return null;
		}
		transform.localScale = Vector3.one * initscale;
	}
	
	// Update is called once per frame
	void Update () {
	}
}
