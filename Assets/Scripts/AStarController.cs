using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Diagnostics;

/*
Information includes:
1. Use 0 to indicate a blocked cell - 48
2. Use 1 to indicate a regular unblocked cell - 49
3. Use 2 to indicate a hard to traverse cell - 50
4. Use a to indicate a regulare unblocked cell with a highway - 97
5. use b to indicate a hard to traverse cell with a highway - 98
*/

public class AStarController : MonoBehaviour {

	public GameObject[,] TileArray;
	private MapGeneration mg;
	public GameObject goalTile;
	public GameObject startTile;

	public double weight;

	public List<GameObject> fringe;
	public List<GameObject> closed;
	private GameObject currTile;

	private bool pressed;

	public int algorithm;// 0-UCS, 1-A*, 2-WA*

	public int poppedCount;
	public int maxInFringe;

	public Material path;

	//--------------------------------------------------------------------------------------------

	//public int sequentialHeuristic; //4-manhattan/anchor 0-diagonal distance 1-diagonal distance uniform 2-euclidean distance 3-euclidean distance squared
	public GameObject[,] sequential1;
	public GameObject[,] sequential2;
	public GameObject[,] sequential3;
	public GameObject[,] sequential4;
	public GameObject[,] sequentialAnchor;

	public GameObject[] startTiles;
	public GameObject[] goalTiles;

	public double weight1;
	public double weight2;

	public List<GameObject>[] listOpen;
	public List<GameObject>[] listClosed;

	public List<GameObject> open1;
	public List<GameObject> closed1;
	private GameObject currTile1;

	public List<GameObject> open2;
	public List<GameObject> closed2;
	private GameObject currTile2;

	public List<GameObject> open3;
	public List<GameObject> closed3;
	private GameObject currTile3;

	public List<GameObject> open4;
	public List<GameObject> closed4;
	private GameObject currTile4;

	public List<GameObject> openAnchor;
	public List<GameObject> closedAnchor;
	private GameObject currTileAnchor;


	// Use this for initialization
	void Start () {
		mg = GameObject.Find ("MapGenerationController").GetComponent<MapGeneration> ();
		weight = 2.5;
		algorithm = 1;
	}




	void Update(){
		if(Input.GetKeyDown("space") && !pressed){
			StartCoroutine(aStar ());
			pressed = true;
		}

		if(Input.GetKeyDown("q") && !pressed){
			StartCoroutine(sequentialAStar ());
			pressed = true;
		}
	}


	//=======================================================================A STAR=======================================================================
	//When I ran my aStar for tests i got rid of the Ienumerator and yields since i wanted it to go faster.
	public IEnumerator aStar(){
		poppedCount = 0;
		maxInFringe = 0;
		goalTile = mg.goalTile;
		startTile = mg.startTile;
		startTile.GetComponent<TileData> ().parent = startTile;
		fringe = new List<GameObject>();
		closed = new List<GameObject> ();
		switch (algorithm) {

		case 0:
			updateU (startTile);
			print ("Running Uniform....");
			break;

		case 1:
			updateFS (startTile);
			print ("Running A*.....");
			break;

		case 2:
			updateWFS (startTile);
			print ("Running Weighted A*....");
			break;

		}
		Insert (startTile, "fringe");
		while (fringe.Count != 0) {
			yield return 0;
			if(fringe.Count > maxInFringe){
				maxInFringe = fringe.Count;
				print ("MAX: " + maxInFringe);
			}
			currTile = Pop ();
			poppedCount++;
			//print ("Popping: " + currTile.GetComponent<TileData> ().fs); 
			currTile.GetComponent<TileData> ().colorPath ();
			if (currTile.GetComponent<TileData> ().myNum == goalTile.GetComponent<TileData> ().myNum) {
				print ("Goal was reached...");
				break;
			}
			Insert (currTile, "closed");
			getSucc (currTile);
			foreach (var node in currTile.GetComponent<TileData>().succ) {
				if (!closed.Contains (node)) {
					if (!fringe.Contains (node)) {
						node.GetComponent<TileData> ().gs = 9999;
						node.GetComponent<TileData> ().parent = null;
					}
					updateVertex (currTile, node);
				}
			}

		}
		StartCoroutine (colorPath ());

	}


	public IEnumerator colorPath(){
		GameObject goal = mg.goalTile;
		GameObject curr = goal;

		while (curr.transform.parent != null) {
			yield return 0;
			curr.GetComponent<Renderer> ().material = path;
			if (curr.GetComponent<TileData> ().parent != null) {
				curr = curr.GetComponent<TileData> ().parent;
			}
		}

	}

	public IEnumerator sequentialColorPath(int sequentialHeuristic){
		if (sequentialHeuristic == 0) {
			GameObject goal = mg.goalTile;
			GameObject curr = goal;

			while (curr.transform.parent != null) {
				yield return 0;
				curr.GetComponent<Renderer> ().material = path;
				if (curr.GetComponent<TileData> ().parent1 != null) {
					curr = curr.GetComponent<TileData> ().parent1;
				}
			}
		}
		if (sequentialHeuristic == 1) {
			GameObject goal = mg.goalTile;
			GameObject curr = goal;

			while (curr.transform.parent != null) {
				yield return 0;
				curr.GetComponent<Renderer> ().material = path;
				if (curr.GetComponent<TileData> ().parent2 != null) {
					curr = curr.GetComponent<TileData> ().parent2;
				}
			}
		}
		if (sequentialHeuristic == 2) {
			GameObject goal = mg.goalTile;
			GameObject curr = goal;

			while (curr.transform.parent != null) {
				yield return 0;
				curr.GetComponent<Renderer> ().material = path;
				if (curr.GetComponent<TileData> ().parent3 != null) {
					curr = curr.GetComponent<TileData> ().parent3;
				}
			}
		}
		if (sequentialHeuristic == 3) {
			GameObject goal = mg.goalTile;
			GameObject curr = goal;

			while (curr.transform.parent != null) {
				yield return 0;
				curr.GetComponent<Renderer> ().material = path;
				if (curr.GetComponent<TileData> ().parent4 != null) {
					curr = curr.GetComponent<TileData> ().parent4;
				}
			}
		}

	}
	//=======================================================================A STAR=======================================================================










	//=======================================================================Get Successor Nodes=======================================================================
	//Sets successors for the given Tile. 
	//The instance of the tile will then have a list of all successors not including itself.
	public void getSucc(GameObject node){
		int row = node.GetComponent<TileData> ().myRow;
		int col = node.GetComponent<TileData> ().myColumn;
		for (int i = row - 1; i <= row + 1; i++) {
			for (int j = col - 1; j <= col + 1; j++) { 
				if (i == row && j == col) {
					continue;
				}else if((i>=0 && i <120) && (j>=0 && j<160)){ 
					if (mg.TileArray [i, j].GetComponent<TileData> ().identifier != "0") {
						mg.TileArray [row, col].GetComponent<TileData> ().succ.Add (mg.TileArray [i, j]);
					}
				}
			}
		}
	}
	//=======================================================================Get Successor Nodes=======================================================================










	//=======================================================================Update Vertex=======================================================================
	void updateVertex(GameObject s, GameObject s1){
		int srow = s.GetComponent<TileData> ().myRow;
		int scol = s.GetComponent<TileData> ().myColumn;
		int s1row = s1.GetComponent<TileData> ().myRow;
		int s1col = s1.GetComponent<TileData> ().myColumn;
		string direction = "temp";
		if ((s1row == (srow - 1) && s1col == (scol - 1)) ||
			(s1row == (srow - 1) && s1col == (scol + 1)) ||
			(s1row == (srow + 1) && s1col == (scol - 1)) ||
			(s1row == (srow + 1) && s1col == (scol + 1))) {
			direction = "d";
		} else {
			direction = "hv";
		}
		double straightLineCost = getCost (s.GetComponent<TileData> ().identifier, s1.GetComponent<TileData> ().identifier, direction);
		if ((s.GetComponent<TileData> ().gs + straightLineCost) < s1.GetComponent<TileData> ().gs) {
			s1.GetComponent<TileData> ().gs = s.GetComponent<TileData> ().gs + straightLineCost;
			s1.GetComponent<TileData> ().parent = s;
			if (fringe.Contains (s1)) {
				Remove (s1);
			} else {
				switch (algorithm) {
				case 0:
					updateU (s1);
					break;

				case 1:
					updateFS (s1);
					break;

				case 2:
					updateWFS (s1);
					break;

				}
				Insert (s1,"fringe");

			}
		}
	}
	//=======================================================================Update Vertex=======================================================================













	//=======================================================================Heap Library=======================================================================
	void printFringe(){
		int i = 0;
		foreach (var node in fringe) {
			print (i + "  :  " + node.GetComponent<TileData> ().fs);
			i++;
		}
	}
	void Insert(GameObject ins, string list){
		if (list == "fringe") {
			if (fringe.Count == 0) {
				fringe.Add (ins);
				return;
			} else {
				int i = 0;
				foreach (var node in fringe) {
					if (ins.GetComponent<TileData> ().fs <= node.GetComponent<TileData> ().fs) {
						fringe.Insert (i, ins);
						return;
					} else if (i == fringe.Count - 1) {
						fringe.Add (ins);
						return;
					}
					i++;
				}
			}
		} else {
			closed.Add (ins);
		}
	}
	void Remove(GameObject rm){
		if (fringe.Count == 0) {
			print ("Error cannot remove because fringe is empty...");
			return;
		} else {
			foreach (var node in fringe) {
				if (node.GetComponent<TileData> ().myNum == rm.GetComponent<TileData> ().myNum) {
					fringe.Remove (node);
					return;
				}
			}
		}
		print ("Could not find object...");
		return;


	}
	//Pops the head of the list which is the lowest priority always.
	GameObject Pop(){
		GameObject rt = fringe [0];
		fringe.RemoveAt (0);
		return rt;
	}
	//=======================================================================Heap Library=======================================================================












	//=======================================================================Different AStar Libraries=======================================================================
	public void updateFS(GameObject tile){
		tile.GetComponent<TileData>().hs = getEuclideanDistance (tile); 
		tile.GetComponent<TileData> ().fs = tile.GetComponent<TileData> ().gs + tile.GetComponent<TileData>().hs;
	}

	public void updateWFS(GameObject tile){
		tile.GetComponent<TileData>().hs = getEuclideanDistance (tile); 
		tile.GetComponent<TileData> ().fs = tile.GetComponent<TileData> ().gs + weight*tile.GetComponent<TileData>().hs;
	}

	public void updateU(GameObject tile){
		tile.GetComponent<TileData> ().fs = tile.GetComponent<TileData> ().gs;
	}
	//=======================================================================Different AStar Libraries=======================================================================








	//=======================================================================Hueristics=======================================================================
	//Gets Manhattan Distance
	double getManhattanDistance(GameObject currentTile){
		int goalRow = goalTile.GetComponent<TileData> ().myRow;
		int goalCol = goalTile.GetComponent<TileData> ().myColumn;
		int currentRow = currentTile.GetComponent<TileData> ().myRow;
		int currentCol = currentTile.GetComponent<TileData> ().myColumn;

		return (Mathf.Abs (currentRow - goalRow) + Mathf.Abs (currentCol - goalCol));
	} 


	//Gets the EuclideanDistance
	double getEuclideanDistance(GameObject currentCell){
		Vector2 curr = new Vector2 (currentCell.GetComponent<TileData>().myRow, currentCell.GetComponent<TileData>().myColumn);
		Vector2 goal = new Vector2 (goalTile.GetComponent<TileData>().myRow, goalTile.GetComponent<TileData>().myColumn);

		return Vector2.Distance (curr, goal);
	}


	//=======================================================================Hueristics=======================================================================







	//=======================================================================Get transition Costs=======================================================================
	//s0 defines the current cell identifier
	//s1 defines the successor cell identifier
	//direction is either hv for horizontal/vertical or d for diagonal.
	public double getCost(string s0, string s1, string direction){
		if (direction == "hv") {
			if (s0 == "1" && s1 == "1") {//Both unblocked
				return 1.0;			
			} else if ((s0 == "1" && s1 == "2") || (s0 == "2" && s1 == "1")) {//1 unblocked 1 hard to traverse
				return 1.5;
			} else if (s0 == "2" && s1 == "2") {//Both hard to traverse
				return 2.0;
			} else if (s0 == "a" && s1 == "a") {//Both unblocked highways
				return 0.25;			
			} else if ((s0 == "a" && s1 == "b") || (s0 == "b" && s1 == "a")) {//1 unblocked 1 hard to traverse highway
				return 0.375;
			} else if (s0 == "b" && s1 == "b") {//Both hard to traverse highways
				return .5;
			}
		} else if (direction == "d") {
			if (s0 == "1" && s1 == "1") {//Both unblocked
				return 1.414;			
			} else if ((s0 == "1" && s1 == "2") || (s0 == "2" && s1 == "1")) {//1 unblocked 1 hard to traverse
				return 2.1213;
			} else if (s0 == "2" && s1 == "2") {//Both hard to traverse
				return 2.828;
			} else if (s0 == "a" && s1 == "a") {//Both unblocked highways
				return 0.3525;			
			} else if ((s0 == "a" && s1 == "b") || (s0 == "b" && s1 == "a")) {//1 unblocked 1 hard to traverse highway
				return 0.5303;
			} else if (s0 == "b" && s1 == "b") {//Both hard to traverse highways
				return .707;
			}
		}
		return -1.0; 

	}
	//=======================================================================Get transition Costs=======================================================================



	//------------------Sequential-----------------
	double getDiagonalDistance(GameObject currentTile){
		int goalRow = goalTiles[0].GetComponent<TileData> ().myRow;
		int goalCol = goalTiles[0].GetComponent<TileData> ().myColumn;
		int currentRow = currentTile.GetComponent<TileData> ().myRow;
		int currentCol = currentTile.GetComponent<TileData> ().myColumn;

		int diagonal_max = Mathf.Max ((Mathf.Abs (currentRow - goalRow)), Mathf.Abs (currentCol - goalCol));
		int diagonal_min = Mathf.Min ((Mathf.Abs (currentRow - goalRow)), Mathf.Abs (currentCol - goalCol));

		int costOfNonDiagonalMovement = 1;
		double costOfDiagonalMovement = 1.414;

		return (costOfDiagonalMovement * diagonal_min + costOfNonDiagonalMovement*(diagonal_max - diagonal_min));
	}

	double getDiagonalDistanceUniformCost(GameObject currentTile){
		int goalRow = goalTiles[1].GetComponent<TileData> ().myRow;
		int goalCol = goalTiles[1].GetComponent<TileData> ().myColumn;
		int currentRow = currentTile.GetComponent<TileData> ().myRow;
		int currentCol = currentTile.GetComponent<TileData> ().myColumn;

		int costOfMovement = 1;

		return (costOfMovement * Mathf.Max (Mathf.Abs (currentRow - goalRow), Mathf.Abs (currentCol - goalCol)));
	}

	/*
	double getEuclideanDistance(GameObject currentTile){
		int goalRow = goalTiles[2].GetComponent<TileData> ().myRow;
		int goalCol = goalTiles[2].GetComponent<TileData> ().myColumn;
		int currentRow = currentTile.GetComponent<TileData> ().myRow;
		int currentCol = currentTile.GetComponent<TileData> ().myColumn;

		int dx = Mathf.Abs(currentRow - goalRow);
		int dy = Mathf.Abs(currentCol - goalCol);

		return (Mathf.Sqrt (Mathf.Pow (dx, 2) + Mathf.Pow (dy, 2)));
	}
	*/

	double getEuclideanDistanceSquared(GameObject currentTile){
		int goalRow = goalTiles[3].GetComponent<TileData> ().myRow;
		int goalCol = goalTiles[3].GetComponent<TileData> ().myColumn;
		int currentRow = currentTile.GetComponent<TileData> ().myRow;
		int currentCol = currentTile.GetComponent<TileData> ().myColumn;

		int dx = Mathf.Abs(currentRow - goalRow);
		int dy = Mathf.Abs(currentCol - goalCol);

		return (Mathf.Pow (dx, 2) + Mathf.Pow (dy, 2));
	}

	double getTheManhattanDistance(GameObject currentTile){
		int goalRow = goalTiles[4].GetComponent<TileData> ().myRow;
		int goalCol = goalTiles[4].GetComponent<TileData> ().myColumn;
		int currentRow = currentTile.GetComponent<TileData> ().myRow;
		int currentCol = currentTile.GetComponent<TileData> ().myColumn;

		return (Mathf.Abs (currentRow - goalRow) + Mathf.Abs (currentCol - goalCol));
	} 

	public IEnumerator sequentialAStar(){
		Stopwatch timer = new Stopwatch ();
		timer.Start ();

		int pop1 = 0;
		int pop2 = 0;
		int pop3 = 0;
		int pop4 = 0;
		int popAnchor = 0;
		int expanded = 0;
		int expandedCount = 0;
		poppedCount = 0;
		maxInFringe = 0;



		open1 = new List<GameObject> ();
		open2 = new List<GameObject> ();
		open3 = new List<GameObject> ();
		open4 = new List<GameObject> ();
		openAnchor = new List<GameObject> ();

		closed1 = new List<GameObject> ();
		closed2 = new List<GameObject> ();
		closed3 = new List<GameObject> ();
		closed4 = new List<GameObject> ();
		closedAnchor = new List<GameObject> ();

		startTiles = new GameObject[5];
		goalTiles = new GameObject[5];


		for (int heuristic = 0; heuristic < 5; heuristic++) {

			startTiles [heuristic] = mg.startTile;
			goalTiles [heuristic] = mg.goalTile;

			startTiles [heuristic].GetComponent<TileData> ().gs1 = 0;
			goalTiles [heuristic].GetComponent<TileData> ().gs1 = 9999;
			startTiles [heuristic].GetComponent<TileData> ().gs2 = 0;
			goalTiles [heuristic].GetComponent<TileData> ().gs2 = 9999;
			startTiles [heuristic].GetComponent<TileData> ().gs3 = 0;
			goalTiles [heuristic].GetComponent<TileData> ().gs3 = 9999;
			startTiles [heuristic].GetComponent<TileData> ().gs4 = 0;
			goalTiles [heuristic].GetComponent<TileData> ().gs4 = 9999;
			startTiles [heuristic].GetComponent<TileData> ().gsAnchor = 0;
			goalTiles [heuristic].GetComponent<TileData> ().gsAnchor = 9999;




		}
		startTiles [0].GetComponent<TileData>().parent1 = null;
		goalTiles [0].GetComponent<TileData>().parent1 = null;
		startTiles [1].GetComponent<TileData>().parent2 = null;
		goalTiles [1].GetComponent<TileData>().parent2 = null;
		startTiles [2].GetComponent<TileData>().parent3 = null;
		goalTiles [2].GetComponent<TileData>().parent3 = null;
		startTiles [3].GetComponent<TileData>().parent4 = null;
		goalTiles [3].GetComponent<TileData>().parent4 = null;
		startTiles [4].GetComponent<TileData>().parentAnchor = null;
		goalTiles [4].GetComponent<TileData>().parentAnchor = null;

		currTile1 = startTiles [0];
		currTile2 = startTiles [1];
		currTile3 = startTiles [2];
		currTile4 = startTiles [3];
		currTileAnchor = startTiles [4];


		startTiles [0].GetComponent<TileData> ().fs1 = Key(startTiles [0],0);
		goalTiles [0].GetComponent<TileData> ().fs1 = Key(goalTiles[0],0);
		startTiles [1].GetComponent<TileData> ().fs2 = Key(startTiles [1],1);
		goalTiles [1].GetComponent<TileData> ().fs2 = Key(goalTiles[1],1);
		startTiles [2].GetComponent<TileData> ().fs3 = Key(startTiles [2],2);
		goalTiles [2].GetComponent<TileData> ().fs3 = Key(goalTiles[2],2);
		startTiles [3].GetComponent<TileData> ().fs4 = Key(startTiles [3],3);
		goalTiles [3].GetComponent<TileData> ().fs4 = Key(goalTiles[3],3);
		startTiles [4].GetComponent<TileData> ().fsAnchor = Key(startTiles [4],4);
		goalTiles [4].GetComponent<TileData> ().fsAnchor = Key(goalTiles[4],4);


		sequentialInsert (startTiles [0], "open", 0);
		sequentialInsert (startTiles [1], "open", 1);
		sequentialInsert (startTiles [2], "open", 2);
		sequentialInsert (startTiles [3], "open", 3);
		sequentialInsert (startTiles [4], "open", 4);

		currTileAnchor = openAnchor [0];


		while (openAnchor [0].GetComponent<TileData> ().fsAnchor < 9999) {
			print ("Popping1: " + currTile1.GetComponent<TileData> ().hs1); 
			print ("Popping2: " + currTile2.GetComponent<TileData> ().hs2); 
			print ("Popping3: " + currTile3.GetComponent<TileData> ().hs3); 
			print ("Popping4: " + currTile4.GetComponent<TileData> ().hs4); 

			yield return 0;
			for (int i = 0; i < 4; i++) {
				if(i == 0){
					currTile1 = open1 [0];
					if(open1[0].GetComponent<TileData>().fs1 <= weight2 * openAnchor[0].GetComponent<TileData>().fsAnchor){
						if(goalTiles[i].GetComponent<TileData>().gs1 <= open1[0].GetComponent<TileData>().fs1){
							if (goalTiles [i].GetComponent<TileData> ().gs1 < 9999) {
								if (currTile1.GetComponent<TileData> ().myNum == goalTiles[i].GetComponent<TileData> ().myNum) {
									timer.Stop ();
									print(("Time: " + timer.ElapsedMilliseconds.ToString()));
									print ("Goal was reached...DiagonalDistance");
									poppedCount = pop1;
									expandedCount = expanded;
									print ("expanded count:" + expandedCount);
									print ("path:" + poppedCount);
									print ("max in fringe:" + maxInFringe);
									StartCoroutine (sequentialColorPath (0));
									yield break;
								}
							}
						}
						else{
							currTile1 = open1 [0];
							pop1++;
							currTile1.GetComponent<TileData> ().colorPath ();
							open1.RemoveAt (0);
							expandStates (currTile1, 0);
							expanded++;
							sequentialInsert (currTile1, "closed", 0);
						}
					}
					else{
						if(goalTiles[4].GetComponent<TileData>().gsAnchor <= openAnchor[0].GetComponent<TileData>().fsAnchor){
							if (goalTiles [4].GetComponent<TileData> ().gsAnchor < 9999) {
								if (currTile1.GetComponent<TileData> ().myNum == goalTiles[4].GetComponent<TileData> ().myNum) {
									timer.Stop ();
									print(("Time: " + timer.ElapsedMilliseconds.ToString()));
									print ("Goal was reached...4");
									poppedCount = popAnchor;
									expandedCount = expanded;
									print ("expanded count:" + expandedCount);
									print ("pop count:" + poppedCount);
									print ("max in fringe:" + maxInFringe);
									yield break;
								}
							}
						}
						else{
							currTile1 = openAnchor [0];
							popAnchor++;
							currTileAnchor.GetComponent<TileData> ().colorPath ();
							openAnchor.RemoveAt (0);
							expandStates (currTile1, 4);
							expanded++;
							sequentialInsert (currTile1, "closed", 4);
						}
					}
				}
				if(i == 1) {
					currTile2 = open2 [0];
					if (open2 [0].GetComponent<TileData> ().fs2 <= weight2 * openAnchor [0].GetComponent<TileData> ().fsAnchor) {
						if (goalTiles [i].GetComponent<TileData> ().gs2 <= open2 [0].GetComponent<TileData> ().fs2) {
							if (goalTiles [i].GetComponent<TileData> ().gs2 < 9999) {
								if (currTile2.GetComponent<TileData> ().myNum == goalTiles[i].GetComponent<TileData> ().myNum) {
									timer.Stop ();
									print(("Time: " + timer.ElapsedMilliseconds.ToString()));
									print ("Goal was reached...DiagonalDistanceUniformCost");
									poppedCount = pop2;
									expandedCount = expanded;
									print ("expanded count:" + expandedCount);
									print ("path:" + poppedCount);
									print ("max in fringe:" + maxInFringe);
									StartCoroutine (sequentialColorPath (1));
									yield break;
								}
							}
						} else {
							currTile2 = open2 [0];
							pop2++;
							currTile2.GetComponent<TileData> ().colorPath ();
							open2.RemoveAt (0);
							expandStates (currTile2, 1);
							expanded++;
							sequentialInsert (currTile2, "closed", 1);
						}
					} else {
						if (goalTiles [4].GetComponent<TileData> ().gsAnchor <= openAnchor [0].GetComponent<TileData> ().fsAnchor) {
							if (goalTiles [4].GetComponent<TileData> ().gsAnchor < 9999) {
								if (currTile2.GetComponent<TileData> ().myNum == goalTiles[4].GetComponent<TileData> ().myNum) {
									timer.Stop ();
									print(("Time: " + timer.ElapsedMilliseconds.ToString()));
									print ("Goal was reached...1");
									poppedCount = popAnchor;
									expandedCount = expanded;
									print ("expanded count:" + expandedCount);
									print ("pop count:" + poppedCount);
									print ("max in fringe:" + maxInFringe);
									yield break;
								}
							}
						} else {
							currTile2 = openAnchor [0];
							popAnchor++;
							currTileAnchor.GetComponent<TileData> ().colorPath ();
							openAnchor.RemoveAt (0);
							expandStates (currTile2, 4);
							expanded++;
							sequentialInsert (currTile2, "closed", 4);
						}
					}
				}
				if(i == 2) {
					currTile3 = open3 [0];
					if (open3 [0].GetComponent<TileData> ().fs3 <= weight2 * openAnchor [0].GetComponent<TileData> ().fsAnchor) {
						if (goalTiles [i].GetComponent<TileData> ().gs3 <= open3 [0].GetComponent<TileData> ().fs3) {
							if (goalTiles [i].GetComponent<TileData> ().gs3 < 9999) {
								if (currTile3.GetComponent<TileData> ().myNum == goalTiles[i].GetComponent<TileData> ().myNum) {
									timer.Stop ();
									print(("Time: " + timer.ElapsedMilliseconds.ToString()));
									print ("Goal was reached...ManhattanDistance");
									poppedCount = pop3;
									expandedCount = expanded;
									print ("expanded count:" + expandedCount);
									print ("path:" + poppedCount);
									print ("max in fringe:" + maxInFringe);
									StartCoroutine (sequentialColorPath (2));
									yield break;
								}
							}
						} else {
							currTile3 = open3 [0];
							pop3++;
							currTile3.GetComponent<TileData> ().colorPath ();
							open3.RemoveAt (0);
							expandStates (currTile3, 2);
							expanded++;
							sequentialInsert (currTile3, "closed", 2);
						}
					} else {
						if (goalTiles [4].GetComponent<TileData> ().gsAnchor <= openAnchor [0].GetComponent<TileData> ().fsAnchor) {
							if (goalTiles [4].GetComponent<TileData> ().gsAnchor < 9999) {
								if (currTile3.GetComponent<TileData> ().myNum == goalTiles[4].GetComponent<TileData> ().myNum) {
									timer.Stop ();
									print(("Time: " + timer.ElapsedMilliseconds.ToString()));
									print ("Goal was reached...2");
									poppedCount = popAnchor;
									expandedCount = expanded;
									print ("expanded count:" + expandedCount);
									print ("pop count:" + poppedCount);
									print ("max in fringe:" + maxInFringe);
									yield break;
								}
							}
						} else {
							currTile3 = openAnchor [0];
							popAnchor++;
							currTileAnchor.GetComponent<TileData> ().colorPath ();
							openAnchor.RemoveAt (0);
							expandStates (currTile3, 4);
							expanded++;
							sequentialInsert (currTile3, "closed", 4);
						}
					}
				}
				if(i == 3){
					currTile4 = open4 [0];
					if(open4[0].GetComponent<TileData>().fs4 <= weight2 * openAnchor[0].GetComponent<TileData>().fsAnchor){
						if(goalTiles[i].GetComponent<TileData>().gs4 <= open4[0].GetComponent<TileData>().fs4){
							if (goalTiles [i].GetComponent<TileData> ().gs4 < 9999) {
								if (currTile4.GetComponent<TileData> ().myNum == goalTiles[i].GetComponent<TileData> ().myNum) {
									timer.Stop ();
									print(("Time: " + timer.ElapsedMilliseconds.ToString()));
									print ("Goal was reached...EuclideanDistanceSquared");
									poppedCount = pop4;
									expandedCount = expanded;
									print ("expanded count:" + expandedCount);
									print ("path:" + poppedCount);
									print ("max in fringe:" + maxInFringe);
									StartCoroutine (sequentialColorPath (3));
									yield break;
								}
							}
						}
						else{
							currTile4 = open4 [0];
							pop4++;
							currTile4.GetComponent<TileData> ().colorPath ();
							open4.RemoveAt (0);
							expandStates (currTile4, 3);
							expanded++;
							sequentialInsert (currTile4, "closed", 3);
						}
					}
					else{
						if(goalTiles[4].GetComponent<TileData>().gsAnchor <= openAnchor[0].GetComponent<TileData>().fsAnchor){
							if (goalTiles [4].GetComponent<TileData> ().gsAnchor < 9999) {
								if (currTile4.GetComponent<TileData> ().myNum == goalTiles[4].GetComponent<TileData> ().myNum) {
									timer.Stop ();
									print(("Time: " + timer.ElapsedMilliseconds.ToString()));
									print ("Goal was reached...3");
									poppedCount = popAnchor;
									expandedCount = expanded;
									print ("expanded count:" + expandedCount);
									print ("pop count:" + poppedCount);
									print ("max in fringe:" + maxInFringe);
									yield break;
								}
							}
						}
						else{
							currTile4 = openAnchor [0];
							popAnchor++;
							currTileAnchor.GetComponent<TileData> ().colorPath ();
							openAnchor.RemoveAt (0);
							expandStates (currTile4, 4);
							expanded++;
							sequentialInsert (currTile4, "closed", 4);
						}
					}
				}


			}
		}


	}

	void expandStates(GameObject s, int sequentialHeuristic){
		if (sequentialHeuristic == 0) {
			open1.Remove (s);
			getSucc(s);
			foreach(var sPrime in s.GetComponent<TileData>().succ){
				if(!open1.Contains(sPrime)&& !closed1.Contains(sPrime)){
					sPrime.GetComponent<TileData> ().gs1 = 9999;
					sPrime.GetComponent<TileData> ().parent1 = null;
				}
				if (sPrime.GetComponent<TileData> ().gs1 > s.GetComponent<TileData> ().gs1 + getStraightLineCost (s, sPrime)) {
					sPrime.GetComponent<TileData> ().gs1 = s.GetComponent<TileData> ().gs1 + getStraightLineCost (s, sPrime);
					sPrime.GetComponent<TileData> ().parent1 = s;
					if (!closed1.Contains (sPrime)) {
						sPrime.GetComponent<TileData> ().fs1 = Key (sPrime, sequentialHeuristic);
						sequentialInsert (sPrime, "open", sequentialHeuristic);
						if(open1.Count > maxInFringe){
							maxInFringe = open1.Count;
							//print ("MAX: " + maxInFringe);
						}
					}
				}
			}
		}
		if (sequentialHeuristic == 1) {
			open2.Remove (s);
			getSucc(s);
			foreach(var sPrime in s.GetComponent<TileData>().succ){
				if(!open2.Contains(sPrime) && !closed2.Contains(sPrime)){
					sPrime.GetComponent<TileData> ().gs2 = 9999;
					sPrime.GetComponent<TileData> ().parent2 = null;
				}
				if (sPrime.GetComponent<TileData> ().gs2 > s.GetComponent<TileData> ().gs2 + getStraightLineCost (s, sPrime)) {
					sPrime.GetComponent<TileData> ().gs2 = s.GetComponent<TileData> ().gs2 + getStraightLineCost (s, sPrime);
					sPrime.GetComponent<TileData> ().parent2 = s;
					if (!closed2.Contains (sPrime)) {
						sPrime.GetComponent<TileData> ().fs2 = Key (sPrime, sequentialHeuristic);
						sequentialInsert (sPrime, "open", sequentialHeuristic);
						if(open2.Count > maxInFringe){
							maxInFringe = open2.Count;
							//print ("MAX: " + maxInFringe);
						}
					}
				}
			}
		}
		if (sequentialHeuristic == 2) {
			open3.Remove (s);
			getSucc(s);
			foreach(var sPrime in s.GetComponent<TileData>().succ){
				if(!open3.Contains(sPrime)&& !closed3.Contains(sPrime)){
					sPrime.GetComponent<TileData> ().gs3 = 9999;
					sPrime.GetComponent<TileData> ().parent3 = null;
				}
				if (sPrime.GetComponent<TileData> ().gs3 > s.GetComponent<TileData> ().gs3 + getStraightLineCost (s, sPrime)) {
					sPrime.GetComponent<TileData> ().gs3 = s.GetComponent<TileData> ().gs3 + getStraightLineCost (s, sPrime);
					sPrime.GetComponent<TileData> ().parent3 = s;
					if (!closed3.Contains (sPrime)) {
						sPrime.GetComponent<TileData> ().fs3 = Key (sPrime, sequentialHeuristic);
						sequentialInsert (sPrime, "open", sequentialHeuristic);
						if(open3.Count > maxInFringe){
							maxInFringe = open3.Count;
							//print ("MAX: " + maxInFringe);
						}
					}
				}
			}
		}
		if (sequentialHeuristic == 3) {
			open4.Remove (s);
			getSucc(s);
			foreach(var sPrime in s.GetComponent<TileData>().succ){
				if(!open4.Contains(sPrime)&& !closed4.Contains(sPrime)){
					sPrime.GetComponent<TileData> ().gs4 = 9999;
					sPrime.GetComponent<TileData> ().parent4 = null;
				}
				if (sPrime.GetComponent<TileData> ().gs4 > s.GetComponent<TileData> ().gs4 + getStraightLineCost (s, sPrime)) {
					sPrime.GetComponent<TileData> ().gs4 = s.GetComponent<TileData> ().gs4 + getStraightLineCost (s, sPrime);
					sPrime.GetComponent<TileData> ().parent4 = s;
					if (!closed4.Contains (sPrime)) {
						sPrime.GetComponent<TileData> ().fs4 = Key (sPrime, sequentialHeuristic);
						sequentialInsert (sPrime, "open", sequentialHeuristic);
						if(open4.Count > maxInFringe){
							maxInFringe = open4.Count;
							//print ("MAX: " + maxInFringe);
						}
					}
				}
			}
		}
		if (sequentialHeuristic == 4) {
			openAnchor.Remove (s);
			getSucc(s);
			foreach(var sPrime in s.GetComponent<TileData>().succ){
				if(!openAnchor.Contains(sPrime)&& !closedAnchor.Contains(sPrime)){
					sPrime.GetComponent<TileData> ().gsAnchor = 9999;
					sPrime.GetComponent<TileData> ().parentAnchor = null;
				}
				if (sPrime.GetComponent<TileData> ().gsAnchor > s.GetComponent<TileData> ().gsAnchor + getStraightLineCost (s, sPrime)) {
					sPrime.GetComponent<TileData> ().gsAnchor = s.GetComponent<TileData> ().gsAnchor + getStraightLineCost (s, sPrime);
					sPrime.GetComponent<TileData> ().parentAnchor = s;
					if (!closedAnchor.Contains (sPrime)) {
						sPrime.GetComponent<TileData> ().fsAnchor = Key (sPrime, sequentialHeuristic);
						sequentialInsert (sPrime, "open", sequentialHeuristic);
						if(openAnchor.Count > maxInFringe){
							maxInFringe = openAnchor.Count;
							//print ("MAX: " + maxInFringe);
						}
					}
				}
			}
		}
	}

	double Key(GameObject s, int sequentialHeuristic){
		if (sequentialHeuristic == 0) {
			return getGValue (currTile1,s,sequentialHeuristic) + weight1 * getHValue (s,sequentialHeuristic);
		}
		if (sequentialHeuristic == 1) {
			return getGValue (currTile2,s,sequentialHeuristic) + weight1 * getHValue (s,sequentialHeuristic);
		}
		if (sequentialHeuristic == 2) {
			return getGValue (currTile3,s,sequentialHeuristic) + weight1 * getHValue (s,sequentialHeuristic);
		}
		if (sequentialHeuristic == 3) {
			return getGValue (currTile4,s,sequentialHeuristic) + weight1 * getHValue (s,sequentialHeuristic);
		}
		else{
			return getGValue (currTileAnchor,s,sequentialHeuristic) + weight1 * getHValue (s,sequentialHeuristic);
		}


	}

	double getGValue (GameObject s ,GameObject s1, int sequentialHeuristic){
		int srow = s.GetComponent<TileData> ().myRow;
		int scol = s.GetComponent<TileData> ().myColumn;
		int s1row = s1.GetComponent<TileData> ().myRow;
		int s1col = s1.GetComponent<TileData> ().myColumn;
		string direction = "temp";
		if ((s1row == (srow - 1) && s1col == (scol - 1)) ||
			(s1row == (srow - 1) && s1col == (scol + 1)) ||
			(s1row == (srow + 1) && s1col == (scol - 1)) ||
			(s1row == (srow + 1) && s1col == (scol + 1))) {
			direction = "d";
		} else {
			direction = "hv";
		}
		double straightLineCost = getCost (s.GetComponent<TileData> ().identifier, s1.GetComponent<TileData> ().identifier, direction);


		if (sequentialHeuristic == 0) {
			s1.GetComponent<TileData> ().parent1 = s;
			return s1.GetComponent<TileData> ().gs1;
		}
		if (sequentialHeuristic == 1) {
			s1.GetComponent<TileData> ().parent2 = s;
			return s1.GetComponent<TileData> ().gs2;
		}
		if (sequentialHeuristic == 2) {
			s1.GetComponent<TileData> ().parent3 = s;
			return s1.GetComponent<TileData> ().gs3;
		}
		if (sequentialHeuristic == 3) {
			s1.GetComponent<TileData> ().parent4 = s;
			return s1.GetComponent<TileData> ().gs4;
		}
		else {
			s1.GetComponent<TileData> ().parentAnchor = s;
			return s1.GetComponent<TileData> ().gsAnchor;
		}
	}

	double getStraightLineCost (GameObject s ,GameObject s1){
		int srow = s.GetComponent<TileData> ().myRow;
		int scol = s.GetComponent<TileData> ().myColumn;
		int s1row = s1.GetComponent<TileData> ().myRow;
		int s1col = s1.GetComponent<TileData> ().myColumn;
		string direction = "temp";
		if ((s1row == (srow - 1) && s1col == (scol - 1)) ||
			(s1row == (srow - 1) && s1col == (scol + 1)) ||
			(s1row == (srow + 1) && s1col == (scol - 1)) ||
			(s1row == (srow + 1) && s1col == (scol + 1))) {
			direction = "d";
		} else {
			direction = "hv";
		}
		double straightLineCost = getCost (s.GetComponent<TileData> ().identifier, s1.GetComponent<TileData> ().identifier, direction);
		return straightLineCost;
	}


	double getHValue (GameObject s , int sequentialHeuristic){
		if (sequentialHeuristic == 0) {
			s.GetComponent<TileData> ().hs1 = getDiagonalDistance (s);
			return getDiagonalDistance (s); 
		}
		if (sequentialHeuristic == 1) {
			s.GetComponent<TileData> ().hs2 = getDiagonalDistanceUniformCost (s);
			return getDiagonalDistanceUniformCost (s);
		}
		if (sequentialHeuristic == 2) {
			s.GetComponent<TileData> ().hs3 = getEuclideanDistance (s);
			return getEuclideanDistance(s);

		}
		if (sequentialHeuristic == 3) {
			s.GetComponent<TileData> ().hs4 = getEuclideanDistanceSquared (s);
			return getEuclideanDistanceSquared(s);
		}
		else{
			s.GetComponent<TileData> ().hsAnchor = getTheManhattanDistance (s);
			return getTheManhattanDistance(s);

		}

	}

	void sequentialInsert(GameObject ins, string list , int heuristic){
		if (heuristic == 0) {
			ins.GetComponent<TileData> ().fs1 = Key (ins, heuristic);
			if (list == "open") {
				if (open1.Count == 0) {
					open1.Add (ins);
					return;
				} else {
					int i = 0;
					foreach (var node in open1) {
						if (ins.GetComponent<TileData> ().fs1 <= node.GetComponent<TileData> ().fs1) {
							open1.Insert (i, ins);
							return;
						} else if (i == open1.Count - 1) {
							open1.Add (ins);
							return;
						}
						i++;
					}
				}
			} else {
				closed1.Add (ins);
			}
		}
		if (heuristic == 1) {
			ins.GetComponent<TileData> ().fs2 = Key (ins, heuristic);
			if (list == "open") {
				if (open2.Count == 0) {
					open2.Add (ins);
					return;
				} else {
					int i = 0;
					foreach (var node in open2) {
						if (ins.GetComponent<TileData> ().fs2 <= node.GetComponent<TileData> ().fs2) {
							open2.Insert (i, ins);
							return;
						} else if (i == open2.Count - 1) {
							open2.Add (ins);
							return;
						}
						i++;
					}
				}
			} else {
				closed2.Add (ins);
			}
		}
		if (heuristic == 2) {
			ins.GetComponent<TileData> ().fs3 = Key (ins, heuristic);
			if (list == "open") {
				if (open3.Count == 0) {
					open3.Add (ins);
					return;
				} else {
					int i = 0;
					foreach (var node in open3) {
						if (ins.GetComponent<TileData> ().fs3 <= node.GetComponent<TileData> ().fs3) {
							open3.Insert (i, ins);
							return;
						} else if (i == open3.Count - 1) {
							open3.Add (ins);
							return;
						}
						i++;
					}
				}
			} else {
				closed3.Add (ins);
			}
		}
		if (heuristic == 3) {
			ins.GetComponent<TileData> ().fs4 = Key (ins, heuristic);
			if (list == "open") {
				if (open4.Count == 0) {
					open4.Add (ins);
					return;
				} else {
					int i = 0;
					foreach (var node in open4) {
						if (ins.GetComponent<TileData> ().fs4 <= node.GetComponent<TileData> ().fs4) {
							open4.Insert (i, ins);
							return;
						} else if (i == open4.Count - 1) {
							open4.Add (ins);
							return;
						}
						i++;
					}
				}
			} else {
				closed4.Add (ins);
			}
		}
		if (heuristic == 4) {
			ins.GetComponent<TileData> ().fsAnchor = Key (ins, heuristic);
			if (list == "open") {
				if (openAnchor.Count == 0) {
					openAnchor.Add (ins);
					return;
				} else {
					int i = 0;
					foreach (var node in openAnchor) {
						if (ins.GetComponent<TileData> ().fsAnchor <= node.GetComponent<TileData> ().fsAnchor) {
							openAnchor.Insert (i, ins);
							return;
						} else if (i == openAnchor.Count - 1) {
							openAnchor.Add (ins);
							return;
						}
						i++;
					}
				}
			} else {
				closedAnchor.Add (ins);
			}
		}

	}





}

