using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testScript : MonoBehaviour
{

testScript testScriptCache0;




	// Use this for initialization
	void Start ()
	{

testScriptCache0=GetComponent<testScript>();



		string testString = "Testing 1 2 3";
	}
	
	// Update is called once per frame
	void Update ()
	{
		testScriptCache0.enabled = false;
	}
}
