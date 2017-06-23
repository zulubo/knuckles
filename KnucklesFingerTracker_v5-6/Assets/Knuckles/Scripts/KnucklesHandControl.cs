//=============================================================================
//
// Purpose: Interprets knuckles input and animates a hand model
//
//=============================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class KnucklesHandPose{
	public float middle_curl = 0.0f;
	public float ring_curl = 0.0f;
	public float pinky_curl = 0.0f;
	public float index_curl = 0.0f;
	public Vector2 thumbPos = Vector2.zero;
	public float thumb_lift = 0.0f;
	public float squeeze = 0.0f;
	public KnucklesHandPose(float m=0, float r=0, float p=0, float i=0, Vector2 tp = default(Vector2), float tl = 0, float s=0){
		middle_curl = m;
		ring_curl = r;
		pinky_curl = p;
		index_curl = i;
		thumbPos = tp;
		thumb_lift = tl;
		squeeze = s;
	}
}

public class KnucklesHandControl : MonoBehaviour {

	public enum WhichHand{
		Left,
		Right
	}

	public WhichHand whichHand;

    public SteamVR_TrackedObject controller;



   // private SocketService sockserv;
	[SerializeField]
    private Animator anim;

	public  KnucklesHandPose handPose;

    private float squeeze = 0.0f;

    private Vector2 trackpad_pos;

    private float trigger = 0.0f;
    private bool trigger_touched = false;

    private SteamVR_Controller.Device vrcontroller;

	[Tooltip("use older vr controllers for testing. Trigger controls all fingers")]
	public bool emulate;


	float[] clamps = new float[6];

	public bool handOpenFull{ // all the fingers are open. margin of 0.2
		get{
			if (handPose != null) {
				return handPose.thumb_lift > 0.6f && handPose.index_curl > 0.6f && handPose.middle_curl > 0.6f && handPose.ring_curl > 0.6f && handPose.pinky_curl > 0.6f;
			} else {
				return false;
			}
		}
	}

	public bool handClosedFull{ // all the fingers are closed. margin of 0.1
		get{
			return handPose.thumb_lift <0.1f&&handPose.index_curl<0.1f&&handPose.middle_curl<0.1f&&handPose.ring_curl<0.1f&&handPose.pinky_curl<0.1f;
		}
	}

    // Use this for initialization
    void Start() {
		handPose = new KnucklesHandPose ();
        //sockserv = GetComponent<SocketService>();
       // anim = GetComponent<Animator>();

		vrcontroller = SteamVR_Controller.Input((int)controller.index);
    }

	public void SetClamp(int finger, float val = -1){
		if (val == -1) {
			val = getposfromindex (finger);

			clamps [finger] = val;
		} else {
			clamps[finger] = val;
		}
	}

	public float getposfromindex(int i){
		float val = 1;
		if (i == 0)
			val = handPose.thumb_lift;
		if (i == 1)
			val = handPose.index_curl;
		if (i == 2)
			val = handPose.middle_curl;
		if (i == 3)
			val = handPose.ring_curl;
		if (i == 4)
			val = handPose.pinky_curl;
		//if (i > 4 || i < 0)
		//	val = 1;

		return val;
	}

	public void TriggerHaptics(int length){
		if (vrcontroller != null) {
			vrcontroller.TriggerHapticPulse ((ushort)length);
		}
	}

    // Update is called once per frame
    void Update() {
		//if (vrcontroller == null) {
		vrcontroller = SteamVR_Controller.Input((int)controller.index);
		//}

        // Old Socket-based code (not used)
        //middle_curl = Mathf.Lerp(middle_curl, sockserv.middle_curl, 30.0f*Time.deltaTime);
        //ring_curl = Mathf.Lerp(ring_curl, sockserv.ring_curl, 30.0f * Time.deltaTime);
        //pinky_curl = Mathf.Lerp(pinky_curl, sockserv.pinky_curl, 30.0f * Time.deltaTime);
        //index_curl = Mathf.Lerp(index_curl, sockserv.index_curl, 15.0f * Time.deltaTime);

        // Get finger curl axes and apply filtering
		if (!emulate) {
			handPose.index_curl = Mathf.Lerp (handPose.index_curl, 1f - vrcontroller.GetAxis (Valve.VR.EVRButtonId.k_EButton_Axis3).x, 30.0f * Time.deltaTime);
			handPose.middle_curl = Mathf.Lerp (handPose.middle_curl, 1f - vrcontroller.GetAxis (Valve.VR.EVRButtonId.k_EButton_Axis3).y, 30.0f * Time.deltaTime);
			handPose.ring_curl = Mathf.Lerp (handPose.ring_curl, 1f - vrcontroller.GetAxis (Valve.VR.EVRButtonId.k_EButton_Axis4).x, 30.0f * Time.deltaTime);
			handPose.pinky_curl = Mathf.Lerp (handPose.pinky_curl, 1f - vrcontroller.GetAxis (Valve.VR.EVRButtonId.k_EButton_Axis4).y, 15.0f * Time.deltaTime);

			handPose.thumb_lift = Mathf.Lerp (handPose.thumb_lift, vrcontroller.GetTouch (Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad) ? 0 : 1, Time.deltaTime * 10);

			// Grab trigger position and adjust index finger
			trigger = vrcontroller.GetAxis(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger).x;
			//trigger_touched = vrcontroller.GetTouch(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger);
			handPose.index_curl = 0.9f * handPose.index_curl + -0.1f * trigger;

		} else { //emulates finger tracking using old controller trigger axis.
			float triggerAmount = 1-vrcontroller.GetAxis(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger).x;
			handPose.index_curl = triggerAmount;
			handPose.middle_curl = triggerAmount;
			handPose.ring_curl = triggerAmount;
			handPose.pinky_curl = triggerAmount;
			handPose.thumb_lift = triggerAmount;
		}


        // Grab trackpad coords for showing thumb position
        // Note: X-axis is flipped on the left controller since we use the same animation
		bool xflip = whichHand == WhichHand.Left;
		trackpad_pos = new Vector2((xflip?1:0)+ (xflip?-1:1)*(vrcontroller.GetAxis(Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad).x + 1.0f) / 2.0f, (vrcontroller.GetAxis(Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad).y + 1.0f) / 2.0f);

		handPose.thumbPos = trackpad_pos;



        // Calculate squeeze heuristic
        float squeeze_new;
        //squeeze_new = Mathf.Max(0, -middle_curl*15f) + Mathf.Max(0, -ring_curl*15f) + Mathf.Max(0, -index_curl*15f);
        //squeeze_new = Mathf.Max(Mathf.Max(0, -middle_curl * 20f), Mathf.Max(0, -ring_curl * 30f), Mathf.Max(0, -pinky_curl * 30f));
        //squeeze_new = Mathf.Max(Mathf.Max(0, -middle_curl * 20f), Mathf.Max(0, -ring_curl * 25f), Mathf.Max(0, -pinky_curl * 25f));
        //squeeze_new = Mathf.Max(Mathf.Max(0, -middle_curl * 3.0f), Mathf.Max(0, -ring_curl * 3.0f), Mathf.Max(0, -pinky_curl * 2f));
		squeeze_new = Mathf.Max(Mathf.Max(0, -handPose.middle_curl * 20f), Mathf.Max(0, -handPose.ring_curl * 20f), Mathf.Max(0, -handPose.pinky_curl * 25f));
		handPose.squeeze = Mathf.Lerp(handPose.squeeze, squeeze_new, 30.0f * Time.deltaTime);

		clampPose ();

        // Set animation times
		anim.Play("Ungrasp_Middle", 1, handPose.middle_curl);
		anim.Play("Ungrasp_Ring", 2, handPose.ring_curl);
		anim.Play("Ungrasp_Pinky", 3, handPose.pinky_curl);
		anim.Play("Ungrasp_Index", 4, handPose.index_curl);
        //anim.Play("Ungrasp_Thumb", 5, sockserv.thumb_curl);
		anim.Play("Trackpad_X", 6, handPose.thumbPos.x);
		anim.Play("Trackpad_Y", 7, handPose.thumbPos.y);
		anim.Play("Squeeze", 5, handPose.squeeze);
		anim.Play("Thumb_Lift", 8, handPose.thumb_lift);
        anim.speed = 0;
    }


	void clampPose(){ // apply clamping for finger positions
		handPose.thumb_lift = Mathf.Clamp (handPose.thumb_lift, clamps [0], 1);
		handPose.index_curl = Mathf.Clamp (handPose.index_curl, clamps [1], 1);
		handPose.middle_curl = Mathf.Clamp (handPose.middle_curl, clamps [2], 1);
		handPose.ring_curl = Mathf.Clamp (handPose.ring_curl, clamps [3], 1);
		handPose.pinky_curl = Mathf.Clamp (handPose.pinky_curl, clamps [4], 1);
	}

}
