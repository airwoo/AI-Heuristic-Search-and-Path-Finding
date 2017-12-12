using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System.IO;

public class TestBench : MonoBehaviour {

	public GameObject parent;
	public MapGeneration mg;
	public AStarController asc;
	private bool pressed;

	// Use this for initialization
	void Start () {
		mg = GameObject.Find ("MapGenerationController").GetComponent<MapGeneration>();
		asc = GameObject.Find ("AStarController").GetComponent<AStarController> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown ("2")) {
			if (!pressed) {
				StartCoroutine(runTest ());
				pressed = true;
			}

		}
	}

	public IEnumerator runTest(){
		StreamWriter writer = new StreamWriter ("Assets/outputSQ1.txt", true);
		for (int i = 1; i < 6; i++) {
			for (int j = 1; j < 11; j++) {
				print ("Iteration: " + i + "_" + j);
				yield return 0;

				string filepath = "Assets/map" + i + "_" + j + ".txt";
				mg.inputFile (filepath);
				Stopwatch timer = new Stopwatch ();
				timer.Start ();
				asc.sequentialAStar ();
				timer.Stop ();

				writer.WriteLine ("Map: " + i + " Set: " + j);
				writer.WriteLine ("Popped: " + asc.poppedCount.ToString());
				writer.WriteLine ("Time: " + timer.ElapsedMilliseconds.ToString());
				writer.WriteLine ("MaxInFringe: " + asc.maxInFringe);
				writer.WriteLine ("-----------------------------------------");
				Destroy (GameObject.Find("TileParent")); 


			}
		}
		writer.Close ();
	





	}
}
