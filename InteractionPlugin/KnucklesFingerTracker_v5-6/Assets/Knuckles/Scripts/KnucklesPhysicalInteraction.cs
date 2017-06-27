//=============================================================================
//
// Purpose: Establishes and manages interaction on a per-finger basis. Apply to hand gameobjects.
//
//=============================================================================

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(KnucklesHandControl))]
public class KnucklesPhysicalInteraction : MonoBehaviour {

	//assign the finger tip bones + a palm position
	public Transform tip_thumb;
	public Transform tip_index;
	public Transform tip_middle;
	public Transform tip_ring;
	public Transform tip_pinky;
	public Transform palm;

	//NOTE: the palm basically gets treated as a finger. 

	Transform[] tips = new Transform[6];
	fingertipCollider[] phystips = new fingertipCollider[6];

	[HideInInspector]
	public KnucklesHandControl hc;

	bool grabbing;
	bool wasgrabbing;
	int[] gfingers = new int[2];
	float[] gfingerscurls = new float[2];

	[HideInInspector]
	public KnucklesInteractable grabTarget;

	public bool debug = true;

	float timeSinceGrabbing;

	List<Vector3> historyPos;
	List<Quaternion> historyRot;

	int timeback = 6;

	Transform grabPos;

	Transform snapPos;

	public KnucklesPhysicalInteraction otherHand;

	[HideInInspector]
	public bool grabStarted;

	public Transform renderModel;


	void Start () {
		grabPos = new GameObject ("grabpos").transform;
		grabPos.transform.parent = transform;

		snapPos = new GameObject ("HandSnapPos").transform;

		historyPos = new List<Vector3> ();
		historyRot = new List<Quaternion> ();

		Physics.defaultContactOffset = 0.001f;
		hc = GetComponent<KnucklesHandControl> ();

		//make an array of the fingertips to cycle through them more easily
		tips = new Transform[]{ tip_thumb, tip_index, tip_middle, tip_ring, tip_pinky, palm };

		for (int i = 0; i < tips.Length; i++) {
			//making a new fingertip collider gameObject and naming it
			GameObject g = new GameObject ();
			g.name = tips[i].name + (hc.whichHand == KnucklesHandControl.WhichHand.Right? "_R":"_L");

			g.AddComponent<Rigidbody> ();
			phystips [i] = g.AddComponent<fingertipCollider> ();
			phystips [i].index = i;
			phystips [i].hpi = this;
			phystips[i].rb = phystips[i].GetComponent<Rigidbody> ();


			if (tips [i] == palm) {
				//if it's the palm, make a box collider. For now, I've just hard coded dimensions of 8cm x 8 cm x 2 cm, but for different sized hands these probably need to be different.
				phystips[i].makeRend( new Vector3 (0.08f, 0.08f, 0.02f));
				//g.AddComponent<BoxCollider> ().size = new Vector3 (0.08f, 0.08f, 0.02f);
			} else {
				//if It's not the palm, just make a little 16mm sphere collider for the fingertip.
				phystips[i].makeRend(0.008f);
				//g.AddComponent<SphereCollider> ().radius = 0.008f;

			}
			phystips [i].rb.mass = 0.1f;
			phystips [i].rb.useGravity = false;
			phystips [i].rb.freezeRotation = true;
		}
	}

	void FixedUpdate () {
		timeSinceGrabbing += Time.fixedDeltaTime;
		for (int i = 0; i < tips.Length; i++) {
			phystips [i].col.enabled = timeSinceGrabbing > 0.5f; // turn off finger colliders if I'm grabbing something or just let go of it

			//move the fingertip + palm colliders to the right place. Because I'm setting velocity, there's no guarantee they'll actually get there (ex. if there's something heavy in the way)
			phystips [i].rb.velocity = (tips [i].position - phystips [i].rb.position)/Time.deltaTime;
			phystips [i].rb.MoveRotation (tips [i].rotation);

			phystips [i].rend.enabled = debug;

			//if the fingertip is touching something, and I'm not currently holding anything, check if there's more than one finger holding onto it.
			if (phystips [i].touching != null) {
				if(!grabbing){
					checkFingerGrabbing (i);
				}
			}

		}

		grabStarted = !wasgrabbing && grabbing;
		//if I'm holding something, check and see if I should let go.
		if (grabbing) {
			OnGrabbing ();
		} else {
			grabStarted = false;
			snapPos.parent = null;
		}
		SaveHistory ();

		wasgrabbing = grabbing;
	}


	//this grabs an object if both finger (i) and the opposite finger are both touching it. So, thumb + index finger can grab stuff, the middle/ring/pinky + palm can grab stuff
	void checkFingerGrabbing(int i){
		
		if (i == 0) { // we're checking the thumb
			if (hc.handPose.thumb_lift < 0.8f) { //the thumb is not lifted too much
				if (hc.handPose.index_curl < 0.8f) { // neither is the index finger
					if (phystips [1].touching == phystips [0].touching) { //make sure index finger is grabbing the same object
						GrabStart(phystips[0].touching,0,1);
					}
				}
			}
		}

		if (i == 1) { // we're checking the index finger
			if (hc.handPose.thumb_lift < 0.8f) { //the thumb is not lifted too much
				if (hc.handPose.index_curl < 0.8f) { // neither is the index finger
					if (phystips [1].touching == phystips [0].touching) { //make sure thumb is grabbing the same object
						GrabStart(phystips[1].touching,0,1);
					}
				}
			}
		}
		if (2 <= i && i <= 4) { // we're checking one of the middle thru pinky fingers
			float fingerlift = hc.getposfromindex(i);

			if (fingerlift < 0.8f) { // finger is not lifted too much
				if (phystips [5].touching == phystips [i].touching) { //make sure palm is touching the same object
					GrabStart(phystips[i].touching,i,5);
				}
			}

		}
	}

	void CheckGrabDone(){
		if (grabTarget.Kind != KnucklesInteractable.interactType.Tool) {
			int i = gfingers [0];
			if (i <= 1) { // we're checking the thumb or index finger, which means we have to check if you've let go of either your thumb or index finger
				if (hc.handPose.thumb_lift > gfingerscurls [0] || hc.handPose.index_curl > gfingerscurls [1]) {
					GrabEnd (true);
				}
			} else { // we're checking one of the middle thru pinky fingers, and since the palm can't bend we only need to check the active finger
				float fingerlift = hc.getposfromindex (i);

				if (fingerlift > 0.4f) {
					GrabEnd (true);
				}
			}
		} else {
			if (hc.handOpenFull) {
				GrabEnd (true);
			}
		}
	}

	void GrabStart(KnucklesInteractable r, int finger0, int finger1){


		//save which two fingers are holding onto the thing
		gfingers [0] = finger0;
		gfingers [1] = finger1;
		gfingerscurls [0] = hc.getposfromindex (gfingers [0]);
		gfingerscurls [1] = hc.getposfromindex (gfingers [1]);

		grabTarget = r;


		grabbing = true;

		grabTarget.rb.useGravity = false;
		//grabTarget.rb.centerOfMass = grabTarget.rb.transform.InverseTransformPoint (phystips [gfingers [0]].transform.position);

		grabPos.position = grabTarget.transform.position;
		grabPos.rotation = grabTarget.transform.rotation;

		if (grabTarget.isSnapping) { // snapPos marks the position of the hand relative to the grabbable object
			snapPos.position = transform.position;
			snapPos.rotation = transform.rotation;
			snapPos.parent = grabTarget.transform;
		}

		r.Grab (); //tell the Interactable it's been grabbed by this script

	}

	void OnGrabbing(){

		if (grabTarget == otherHand.grabTarget &! grabStarted && otherHand.grabbing) {
			GrabEnd (false);
		} else {
//			print ("boi");
			grabTarget.SetInput(hc.vrcontroller);
			timeSinceGrabbing = 0;
			CheckGrabDone ();
			//make sure to clamp the fingers to the positions they were at when they grabbed the thing
			hc.SetClamp (gfingers [0], gfingerscurls [0]);
			hc.SetClamp (gfingers [1], gfingerscurls [1]);

			ApplyFollowForce ();
		}
	}

	void ApplyFollowForce(){ // makes the object we're holding actually follow our hand
		if (grabTarget) {
			if (grabTarget.Kind == KnucklesInteractable.interactType.Grabbable) {
				grabTarget.rb.velocity = (grabPos.position - grabTarget.transform.position) / Time.deltaTime;
				grabTarget.rb.angularVelocity = OffsetToAngular (grabTarget.transform.rotation, grabPos.rotation, Time.fixedDeltaTime);
			}
			if (grabTarget.Kind == KnucklesInteractable.interactType.SnappingGrabbableTranslate) {
				grabTarget.rb.AddForceAtPosition (-grabTarget.rb.GetPointVelocity (snapPos.position) + (transform.position - snapPos.position) / Time.fixedDeltaTime * 0.3f, snapPos.position, ForceMode.VelocityChange);
			}
			if (grabTarget.Kind == KnucklesInteractable.interactType.SnappingGrabbableRotate) {
				grabTarget.rb.angularVelocity = OffsetToAngular (grabTarget.transform.rotation, grabPos.rotation, Time.fixedDeltaTime);
			}
			if (grabTarget.Kind == KnucklesInteractable.interactType.Tool) {
				grabTarget.rb.velocity = (transform.position - grabTarget.snappingOrigin.position) / Time.deltaTime;
				grabTarget.rb.angularVelocity = OffsetToAngular (grabTarget.snappingOrigin.rotation, transform.rotation, Time.fixedDeltaTime);
			}
		}
	}

	void GrabEnd(bool drop){ // if drop is false, a different hand has taken control of the thing and we don't need to worry about detaching it
		grabbing = false;

		if (drop) {
			//release the thing
			grabTarget.Drop();//tell it it got dropped
			grabTarget.rb.useGravity = grabTarget.useGravity;
			if (!grabTarget.isSnapping) {
				grabTarget.rb.velocity = GetVelocity ();
			}
			grabTarget.rb.angularVelocity = GetAngularVelocity ();
		}
		//reset the fingers that are holding the thing, just for neatness' sake
		gfingers = new int[2]{0,0};
		grabTarget = null;
	}

	Vector3 GetVelocity(){ // get two old positions, [timeback] and [timeback / 2] frames ago, and extrapolate velocity.
		Vector3 a = historyPos [0];
		Vector3 b = historyPos [Mathf.RoundToInt(timeback / 2)];
		Vector3 vel = (b - a) / ((timeback / 2) * Time.fixedDeltaTime);
		return vel;
	}

	Vector3 GetAngularVelocity(){ // get two old rotations, [timeback] and [timeback / 2] frames ago, and extrapolate angular velocity.
		Quaternion a = historyRot [0];
		Quaternion b = historyRot [Mathf.RoundToInt(timeback / 2)];
		return OffsetToAngular(a,b, (timeback / 2) * Time.fixedDeltaTime);
	}


	void SaveHistory(){ // save some data so we can get a velocity + angular velocity estimate, going back [timeback] frames.
		historyPos.Add (transform.position);
		historyRot.Add (transform.rotation);
		if (historyPos.Count > timeback) {
			historyPos.RemoveAt (0);
		}
		if (historyRot.Count > timeback) {
			historyRot.RemoveAt (0);
		}
	}

	Vector3 OffsetToAngular(Quaternion a, Quaternion b, float timestep){ // turn the difference between two quaternions to an angular velocity
		Quaternion deltaRot = b * Quaternion.Inverse (a);
		Vector3 vel = new Vector3( Mathf.DeltaAngle( 0, deltaRot.eulerAngles.x ), Mathf.DeltaAngle( 0, deltaRot.eulerAngles.y ),Mathf.DeltaAngle( 0, deltaRot.eulerAngles.z ) );
		vel = vel /  timestep;
		vel = vel * Mathf.Deg2Rad;
		return vel;
	}

	void LateUpdate(){
		if (grabbing) {
			if (grabTarget.isSnapping) { // move the hand rendermodel so it appears to be attached to the interactible
				renderModel.position = snapPos.position;
				renderModel.rotation = snapPos.rotation;
			} else {
				ClearRendermodelPos ();
			}
		} else {
			ClearRendermodelPos ();
		}
	}

	void ClearRendermodelPos(){
		renderModel.localPosition = Vector3.zero;
		renderModel.localRotation = Quaternion.identity;
	}

}
