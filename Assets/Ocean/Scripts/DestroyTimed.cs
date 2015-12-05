using UnityEngine;
using System.Collections;

public class DestroyTimed : MonoBehaviour {

	public float time = 5f;

	void Start () {
		Destroy (gameObject, time);
	}
}
