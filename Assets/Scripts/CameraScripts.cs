using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScripts : MonoBehaviour
{
	private float time;
	private GameObject t;
	private float rad = 7.0f;
	void Start () {
		t = GameObject.Find("PointSprite");
	}
	
	void Update ()
	{
		
		time += Time.deltaTime;
		transform.LookAt(t.transform);
		this.transform.position = new Vector3(-3.0f + rad*Mathf.Sin(70.0f*time*Mathf.Deg2Rad), rad*Mathf.Sin(50.0f*time*Mathf.Deg2Rad), -3.0f + rad*Mathf.Cos(50*time*Mathf.Deg2Rad));
		//this.transform.Rotate(new Vector3(1.0f, 1.0f, 1.0f), Time.deltaTime*20.0f);
	}
}
