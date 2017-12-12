using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
TileData.cs holds information about the tile.
Information includes:
	1. Use 0 to indicate a blocked cell
	2. Use 1 to indicate a regular unblocked cell
	3. Use 2 to indicate a hard to traverse cell
	4. Use a to indicate a regulare unblocked cell with a highway
	5. use b to indicate a hard to traverse cell with a highway

	1. isBoundary Cell
	2. isStart Cell
	3. isGoal Cell

NOTE: Since we are working with both numebrs and characters, the input and identifier will be taken as a string.


*/

public class TileData : MonoBehaviour {

	public string identifier; //Identifies what type of block this is.
	public string prevIdentifier;
	public bool isBoundary;
	public bool isStart;
	public bool isGoal;
	public Material blockedMat;
	public Material unblockedMat;
	public Material hardMat;
	public Material unblockedHighwayMat;
	public Material hardHighwayMat; 

	public Material startMat;
	public Material goalMat;

	public Material pathMat;

	public int myRow;
	public int myColumn;

	public bool endingPoint;

	public double gs; //value from start
	public double fs; //value of gs + hs. This is also considered the key
	public double hs; //hueristic to goal
	public GameObject parent;
	public int myNum;
	public double w;

	public List<GameObject> succ;

	private AStarController asc;

	//---------------------------------------------------------------------

	public double gs1;
	public double gs2;
	public double gs3;
	public double gs4;
	public double gsAnchor;

	public double fs1;
	public double fs2;
	public double fs3;
	public double fs4;
	public double fsAnchor;

	public double hs1;
	public double hs2;
	public double hs3;
	public double hs4;
	public double hsAnchor;

	public List<GameObject> succ1;
	public List<GameObject> succ2;
	public List<GameObject> succ3;
	public List<GameObject> succ4;
	public List<GameObject> succAnchor;

	public GameObject parent1;
	public GameObject parent2;
	public GameObject parent3;
	public GameObject parent4;
	public GameObject parentAnchor;


	// Use this for initialization
	void Start () {
		asc = GameObject.Find ("AStarController").GetComponent<AStarController> ();
		w = asc.weight;
		succ = new List<GameObject> ();
	}

	// Update is called once per frame
	void Update () {

	}

	public void colorPath(){
		GetComponent<Renderer>().material = pathMat;
	}

	public void updateFSW(){
		fs = gs + (w * hs);
	}

	public void colorTile(){

		if (identifier == "0") {
			GetComponent<Renderer> ().material = blockedMat;
		} else if (identifier == "1") {
			GetComponent<Renderer> ().material = unblockedMat;
		} else if (identifier == "2") {
			GetComponent<Renderer> ().material = hardMat;
		} else if (identifier == "a") {
			GetComponent<Renderer> ().material = unblockedHighwayMat;
		} else if (identifier == "b") {
			GetComponent<Renderer> ().material = hardHighwayMat;
		}
	}

	public void colorTile(string s){
		if (s == "Start") {
			GetComponent<Renderer> ().material = startMat;
		} else if (s == "Goal") {
			GetComponent<Renderer> ().material = goalMat;
		}
	}


}
