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
		SnappingGrabbableRotate
	}

	[SerializeField]
	[Tooltip("Non Grabbable for UI and heavy objects, Grabbable for light pickup-able objects, Snapping Grabbable Translate for levers, etc (hand model follows this transform), Snapping Grabbable Rotate for knobs, etc (same as SGT but effects rotation only)")]
	public interactType Kind = interactType.Grabbable;

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

	public bool snapRotate{
		get{
			return (Kind == interactType.SnappingGrabbableRotate);
		}
	}


	// Use this for initialization
	void Awake () {
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
