//=============================================================================
//
// Purpose: Marks this rigidbody as interactible and handles interaction events
//
//=============================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
public class KnucklesInteractable : MonoBehaviour {


	public enum interactType
	{
		NonGrabbable,
		Grabbable,
		SnappingGrabbableTranslate,
		SnappingGrabbableRotate,
		Tool
	}

	[SerializeField]
	[Tooltip("Non Grabbable for UI and heavy objects, Grabbable for light pickup-able objects, Snapping Grabbable Translate for levers, etc (hand model follows this transform), Snapping Grabbable Rotate for knobs, etc (same as SGT but affects rotation only), Tool for weapons/tools that should snap to hand rotation and recieve input")]
	public interactType Kind = interactType.Grabbable;

	[Tooltip("Transform for tools to snap to the hand. Leave null for this transform.")]
	public Transform snappingOrigin;

	[HideInInspector]
	public Rigidbody rb;

	[HideInInspector]
	public KnucklesPhysicalInteraction heldBy;

	[Tooltip("Touched by any hand")]
	public UnityEvent onTouched = new UnityEvent();

	[Tooltip("Poked means touched with the index finger")]
	public UnityEvent onPoked = new UnityEvent();

	[Tooltip("Picked up")]
	public UnityEvent onGrabbed = new UnityEvent();

	public UnityEvent onDropped = new UnityEvent();

	[HideInInspector]
	public bool useGravity;

	[HideInInspector]
	public bool isgrabbednow;


	// Get steamVR buttons input if we're being held
	public SteamVR_Controller.Device inputDevice{
		get{
			if (isgrabbednow) {
				return input;
			} else {
				return null;
			}
		}
	}

	SteamVR_Controller.Device input;

	public bool isGrabbable{
		get{
			return Kind != interactType.NonGrabbable;
		}
	}

	public bool isSnapping{
		get{
			return (Kind == interactType.SnappingGrabbableRotate ||  Kind == interactType.SnappingGrabbableTranslate);
		}
	}

	public bool isTool{
		get{
			return (Kind == interactType.Tool);
		}
	}

	public bool snapRotate{
		get{
			return (Kind == interactType.SnappingGrabbableRotate);
		}
	}

	void OnDrawGizmos(){
		if(Kind==interactType.Tool){
			Transform t = snappingOrigin;
			if (t == null)
				t = transform;
			Gizmos.color = Color.green;
			GameObject g = (GameObject)Resources.Load ("HandReference", typeof(GameObject));
			Mesh m = g.GetComponent<MeshFilter> ().sharedMesh;
			Gizmos.DrawWireMesh (m, t.position, t.rotation);
		}

	}

	public void SetInput(SteamVR_Controller.Device device){
		input = device;
	}


	// Use this for initialization
	void Awake () {
		if (snappingOrigin == null)
			snappingOrigin = transform;

		rb = GetComponent<Rigidbody> ();
		rb.maxAngularVelocity = 100000;
		useGravity = rb.useGravity;
	}

	public void Touch(fingertipCollider finger){
		onTouched.Invoke ();
		if (finger.index == 1) { //this is confusing, because index tells you what finger it is, but in this case we're checking whether it's the index finger. index == 1 means index finger
			onPoked.Invoke();
		}
	}

	public void Grab(){
		onGrabbed.Invoke ();
		isgrabbednow = true;
	}

	public void Drop(){
		onDropped.Invoke ();
		isgrabbednow = false;
	}


}
