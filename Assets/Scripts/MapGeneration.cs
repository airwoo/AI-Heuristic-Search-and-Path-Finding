using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class MapGeneration : MonoBehaviour {

	public GameObject Tile;
	public GameObject[,] TileArray;
	public GameObject tileParent;

	private int row;
	private int column;

	private bool atBoundary;
	private int totalPaths;
	private bool rejectPath;

	private int assignCount;//This is used to identify which tile is which.

	public GameObject goalTile;
	public GameObject startTile;

	private List<GameObject> centers;

	public int startRow;
	public int startCol;
	public int goalRow;
	public int goalCol;

	private int fileNumber = 0;
	public GameObject emptyParent;


	void Start () {
		TileArray = new GameObject[120, 160];
		assignCount = 1;
		centers = new List<GameObject>();

		print ("Choose 'p' to input a file and generate a map from the input file...\n" +
			"Choose 'o' to output a generated map to a text file...\n");

		print ("Choose 'm' to generate a new map...\n" +
			"Choose 'c' to clear Start/Goal tiles...\n");

		print ("Choose 'space' to run astar AFTER map has been generated...");


		//StartCoroutine (GenerateMap ());




	}

	//=======================================================================BASIC MAP GENERATION. NOT INPUTTING FILES=======================================================================
	IEnumerator GenerateMap(){
		row = 0;
		column = 0;
		GameObject parentTile = Instantiate (emptyParent, new Vector3 (0, 0, 0), Quaternion.identity);
		parentTile.name = "TileParent";
		//======================Instantiate all the tiles======================
		for (int i = 0; i < 120; i++) {
			for (int j = 0; j < 160; j++) {
				TileArray [i, j] = Instantiate (Tile, new Vector3 (row, 0, column), Quaternion.identity);
				TileArray [i, j].GetComponent<TileData> ().identifier = "1";
				TileArray [i, j].GetComponent<TileData> ().prevIdentifier = "1";
				TileArray [i, j].transform.parent = parentTile.transform;
				TileArray [i, j].GetComponent<TileData> ().myColumn = j;
				TileArray [i, j].GetComponent<TileData> ().myRow = i;
				TileArray [i, j].GetComponent<TileData> ().myNum = assignCount;
				row += 15;
				assignCount++;
			}
			column -= 15;
			row = 0;
		}
		print (TileArray.GetLength(1)); //GetLength(0/1) 0 is rows 1 is columns
		//======================Instantiate all the tiles======================


		//======================Mark Harder to traverse tiles======================
		for (int iterations = 0; iterations < 8; iterations++) {
			int randomRow = Random.Range (0, 160);
			int randomColumn = Random.Range (0, 120);
			print ("Row: " + randomRow + "   Column: " + randomColumn); 
			int rowMin = 0;
			int rowMax = 0;
			int columnMin = 0;
			int columnMax = 0;

			//Pick the Row range
			if (randomRow < 15) {
				rowMin = 0;
				rowMax = randomRow + 15;
			} else if (randomRow > (TileArray.GetLength (0) - 15)) {
				rowMin = randomRow - 15;
				rowMax = TileArray.GetLength (0);
			} else {
				rowMin = randomRow - 15;
				rowMax = randomRow + 15;
			}

			//Pick the Column range
			if (randomColumn < 15) {
				columnMin = 0;
				columnMax = randomColumn + 15;
			} else if (randomColumn > (TileArray.GetLength (1) - 15)) {
				columnMin = randomColumn - 15;
				columnMax = TileArray.GetLength (1);
			} else {
				columnMin = randomColumn - 15;
				columnMax = randomColumn + 15;
			}

			//Select cells to be hard to traverse with 50% probability. 
			for (int i = rowMin; i < rowMax; i++) {
				for (int j = columnMin; j < columnMax; j++) {
					int rn = Random.Range (1, 101);
					if (rn <= 50) {
						TileArray [i, j].GetComponent<TileData> ().identifier = "2";
						TileArray [i, j].GetComponent<TileData> ().prevIdentifier = "2";
						TileArray [i, j].GetComponent<TileData> ().colorTile ();
					}
				}
			}
		}
		//======================Mark Harder to traverse tiles======================



		//======================Highways======================

		//Mark boundary cells to make it easier when creating highways.
		for (int i = 0; i < 120; i++) {
			TileArray [i, 0].GetComponent<TileData> ().isBoundary = true;
		}
		for (int i = 0; i < 160; i++) {
			TileArray [0, i].GetComponent<TileData> ().isBoundary = true;
		}
		for (int i = 0; i < 160; i++) {
			TileArray [119, i].GetComponent<TileData> ().isBoundary = true;
		}
		for (int i = 0; i < 120; i++) {
			TileArray [i, 159].GetComponent<TileData> ().isBoundary = true;
		}

		//Pick a random point on a boundary.
		//Once the random point is picked, declare the opposite starting direction.
		while (totalPaths < 4) {
			yield return 0;
			int rn1 = Random.Range (1, 101);
			int randomRow1 = 0;
			int randomColumn1 = 0;
			string direction = "Temp";
			if (rn1 <= 25) {
				randomColumn1 = 0;
				randomRow1 = Random.Range (0, 120);
				direction = "Right";
			} else if (rn1 > 25 && rn1 <= 50) {
				randomRow1 = 0;
				randomColumn1 = Random.Range (0, 160);
				direction = "Down";
			} else if (rn1 > 50 && rn1 <= 75) {
				randomColumn1 = 159;
				randomRow1 = Random.Range (0, 120);
				direction = "Left";
			} else if (rn1 > 75 && rn1 <= 100) {
				randomRow1 = 119;
				randomColumn1 = Random.Range (0, 160);
				direction = "Up";
			}

			int currentRow = randomRow1;
			int currentColumn = randomColumn1;
			int newColumn = 0;
			int newRow = 0;
			int pathCount = 0;
			List<GameObject> currentPath = new List<GameObject> ();
			while (!atBoundary) {
				if (direction == "Right") {
					for (int i = currentColumn; i < (currentColumn + 20); i++) {
						if (i > 159) {
							atBoundary = true; 
							break;
						}
						if (TileArray [currentRow, i].GetComponent<TileData> ().identifier == "a" || TileArray [currentRow, i].GetComponent<TileData> ().identifier == "b") {
							if (!TileArray [currentRow, i].GetComponent<TileData> ().endingPoint) {
								RevertPath (currentPath);
								rejectPath = true;
								break;							
							} else {
								TileArray [currentRow, i].GetComponent<TileData> ().endingPoint = false;
								currentColumn++;
							}


						} else {
							if (TileArray [currentRow, i].GetComponent<TileData> ().identifier == "2") {
								TileArray [currentRow, i].GetComponent<TileData> ().identifier = "b"; 
							} else {
								TileArray [currentRow, i].GetComponent<TileData> ().identifier = "a";
							}
							TileArray [currentRow, i].GetComponent<TileData> ().colorTile ();
							newColumn = i;
							newRow = currentRow;
							pathCount++;
							currentPath.Add (TileArray [currentRow, i]);
							if (TileArray [currentRow, i].GetComponent<TileData> ().isBoundary && i != currentColumn) {
								atBoundary = true;
								break; 
							}
						}
					}
				} else if (direction == "Left") {
					for (int i = currentColumn; i > (currentColumn - 21); i--) {
						if (i < 0) {
							atBoundary = true; 
							break;
						}
						if (TileArray [currentRow, i].GetComponent<TileData> ().identifier == "a" || TileArray [currentRow, i].GetComponent<TileData> ().identifier == "b") {
							if (!TileArray [currentRow, i].GetComponent<TileData> ().endingPoint) {
								RevertPath (currentPath);
								rejectPath = true;
								break;	
							} else {
								TileArray [currentRow, i].GetComponent<TileData> ().endingPoint = false;
								currentColumn--;
							}

						} else {
							if (TileArray [currentRow, i].GetComponent<TileData> ().identifier == "2") {
								TileArray [currentRow, i].GetComponent<TileData> ().identifier = "b"; 
							} else {
								TileArray [currentRow, i].GetComponent<TileData> ().identifier = "a";
							}
							TileArray [currentRow, i].GetComponent<TileData> ().colorTile ();
							newColumn = i;
							newRow = currentRow;
							pathCount++;
							currentPath.Add (TileArray [currentRow, i]);
							if (TileArray [currentRow, i].GetComponent<TileData> ().isBoundary && i != currentColumn) {
								atBoundary = true;
								break;
							}
						}
					}
				} else if (direction == "Up") {
					for (int i = currentRow; i > (currentRow - 20); i--) {
						if (i < 0) {
							atBoundary = true; 
							break;
						}
						if (TileArray [i,currentColumn].GetComponent<TileData> ().identifier == "a" || TileArray [i,currentColumn].GetComponent<TileData> ().identifier == "b") {
							if (!TileArray [i, currentColumn].GetComponent<TileData> ().endingPoint) {
								RevertPath (currentPath);
								rejectPath = true;
								break;
							} else {
								TileArray [i, currentColumn].GetComponent<TileData> ().endingPoint = false;
								currentRow--;
							}

						} else {
							if (TileArray [i, currentColumn].GetComponent<TileData> ().identifier == "2") {
								TileArray [i, currentColumn].GetComponent<TileData> ().identifier = "b"; 
							} else {
								TileArray [i, currentColumn].GetComponent<TileData> ().identifier = "a";
							}
							TileArray [i, currentColumn].GetComponent<TileData> ().colorTile ();
							newRow = i;
							newColumn = currentColumn;
							pathCount++;
							currentPath.Add (TileArray [i, currentColumn]);
							if (TileArray [i, currentColumn].GetComponent<TileData> ().isBoundary && i != currentRow) {
								atBoundary = true;
								break;
							}
						}
					}		
				} else if (direction == "Down") {
					for (int i = currentRow; i < (currentRow + 20); i++) {
						if (i > 119) {
							atBoundary = true; 
							break;
						}
						if (TileArray [i,currentColumn].GetComponent<TileData> ().identifier == "a" || TileArray [i, currentColumn].GetComponent<TileData> ().identifier == "b") {
							if (!TileArray [i, currentColumn].GetComponent<TileData> ().endingPoint) {
								RevertPath (currentPath);
								rejectPath = true;
								break;
							} else {
								TileArray [i, currentColumn].GetComponent<TileData> ().endingPoint = false;
								currentRow++;
							}

						} else {
							if (TileArray [i, currentColumn].GetComponent<TileData> ().identifier == "2") {
								TileArray [i, currentColumn].GetComponent<TileData> ().identifier = "b"; 
							} else {
								TileArray [i, currentColumn].GetComponent<TileData> ().identifier = "a";
							}
							TileArray [i, currentColumn].GetComponent<TileData> ().colorTile ();
							newRow = i;
							newColumn = currentColumn;
							pathCount++;
							currentPath.Add (TileArray [i, currentColumn]);
							if (TileArray [i, currentColumn].GetComponent<TileData> ().isBoundary && i != currentRow) {
								atBoundary = true;
								break;
							}
						}
					}			
				}
				if (rejectPath) {
					rejectPath = false;
					break;
				}

				//If we reached another boundary we break.
				//Otherwise we update the the currentRow and currentColumn and switch directions with a 20% probability.
				if (atBoundary) {
					if (pathCount < 100) {
						RevertPath (currentPath);
					} else {
						totalPaths++;
					}
					break;
				} else {
					currentRow = newRow;
					currentColumn = newColumn;
					TileArray [currentRow, currentColumn].GetComponent<TileData> ().endingPoint = true;
					int rn2 = Random.Range (0, 101);
					int rn3 = Random.Range (0, 101);
					while (rn2 > 60) {
						rn2 = Random.Range (0, 101);
					}
					if (rn2 < 20) {
						if (direction == "Right" || direction == "Left") {
							if (rn3 < 50) {
								direction = "Up";
							} else {
								direction = "Down";
							}

						} else if (direction == "Up" || direction == "Down") {
							if (rn3 < 50) {
								direction = "Left";
							} else {
								direction = "Right";
							}
						} 

					}
				}



			}//while !atBoundary
			atBoundary = false;

		}//Highway Total
		//======================Highways======================


		//======================Blocked Cells======================
		int blockedCount = 0;
		while (blockedCount < 3840) {
			int rnRow = Random.Range (0, 120);
			int rnColumn = Random.Range (0, 160);
			string id = TileArray [rnRow, rnColumn].GetComponent<TileData> ().identifier;
			if (id != "a" && id != "b" && id != "0") {
				TileArray [rnRow, rnColumn].GetComponent<TileData> ().identifier = "0";
				TileArray [rnRow, rnColumn].GetComponent<TileData> ().colorTile();
				blockedCount++;
			}
		}
		//======================Blocked Cells======================

		//======================Start/Goal Cells======================
		setGoalStart ();
		//======================Start/Goal Cells======================
	}//MapGen Function

	public void RevertPath(List<GameObject> currentPath){
		foreach (var curr in currentPath) {
			curr.GetComponent<TileData> ().identifier = curr.GetComponent<TileData> ().prevIdentifier;
			curr.GetComponent<TileData> ().colorTile ();
		}

	}

	//=======================================================================BASIC MAP GENERATION. NOT INPUTTING FILES=======================================================================







	//=======================================================================SET THE START AND GOAL=======================================================================
	//Made into separate function since I needed to generate multiple goal starts per map for testing.
	public void setGoalStart(){
		bool goalSet = false;
		bool startSet = false;
		bool dist = false;
		int r1 = 0;
		int r2 = 0;
		int c1 = 0;
		int c2 = 0;
		while (!goalSet && !startSet && !dist) {
			while (!startSet) {
				int rn3 = Random.Range (0, 100);
				if (rn3 <= 25) {
					r1 = Random.Range (0, 20);
					c1 = Random.Range (0, 160);
					string id = TileArray [r1, c1].GetComponent<TileData> ().identifier;
					if (id != "a" && id != "b" && id != "0") {
						TileArray [r1, c1].GetComponent<TileData> ().isStart = true;
						startSet = true;
					}

				} else if (rn3 > 25 && rn3 <= 50) {
					r1 = Random.Range (0, 120);
					c1 = Random.Range (0, 20);
					string id = TileArray [r1, c1].GetComponent<TileData> ().identifier;
					if (id != "a" && id != "b" && id != "0") {
						TileArray [r1, c1].GetComponent<TileData> ().isStart = true;
						startSet = true;
					}

				} else if (rn3 > 50 && rn3 <= 75) {
					r1 = Random.Range (0, 120);
					c1 = Random.Range (139, 160);
					string id = TileArray [r1, c1].GetComponent<TileData> ().identifier;
					if (id != "a" && id != "b" && id != "0") {
						TileArray [r1, c1].GetComponent<TileData> ().isStart = true;
						startSet = true;
					}

				} else if (rn3 > 75 && rn3 <= 100) {
					r1 = Random.Range (99, 120);
					c1 = Random.Range (0, 160);
					string id = TileArray [r1, c1].GetComponent<TileData> ().identifier;
					if (id != "a" && id != "b" && id != "0") {
						TileArray [r1, c1].GetComponent<TileData> ().isStart = true;
						startSet = true;
					}

				}	
			}
			while (!goalSet) {
				int rn3 = Random.Range (0, 100);
				if (rn3 <= 25) {
					r2 = Random.Range (0, 20);
					c2 = Random.Range (0, 160);
					string id = TileArray [r2, c2].GetComponent<TileData> ().identifier;
					if (id != "a" && id != "b" && id != "0") {
						TileArray [r2, c2].GetComponent<TileData> ().isGoal = true;
						goalSet = true;
					}

				} else if (rn3 > 25 && rn3 <= 50) {
					r2 = Random.Range (0, 120);
					c2 = Random.Range (0, 20);
					string id = TileArray [r2, c2].GetComponent<TileData> ().identifier;
					if (id != "a" && id != "b" && id != "0") {
						TileArray [r2, c2].GetComponent<TileData> ().isGoal = true;
						goalSet = true;
					}

				} else if (rn3 > 50 && rn3 <= 75) {
					r2 = Random.Range (0, 120);
					c2 = Random.Range (139, 160);
					string id = TileArray [r2, c2].GetComponent<TileData> ().identifier;
					if (id != "a" && id != "b" && id != "0") {
						TileArray [r2, c2].GetComponent<TileData> ().isGoal = true;
						goalSet = true;
					}

				} else if (rn3 > 75 && rn3 <= 100) {
					r2 = Random.Range (99, 120);
					c2 = Random.Range (0, 160);
					string id = TileArray [r2, c2].GetComponent<TileData> ().identifier;
					if (id != "a" && id != "b" && id != "0") {
						TileArray [r2, c2].GetComponent<TileData> ().isGoal = true;
						goalSet = true;
					}

				}	
			}
			if (Vector2.Distance (new Vector2 (r1, c1), new Vector2 (r2, c2)) > 100) {
				dist = true;
			} else {
				r1 = 0;
				c1 = 0;
				r2 = 0; 
				c2 = 0;
				goalSet = false;
				startSet = false;
				dist = false;
			}
		}
		print ("Start: " + r1 + "," + c1);
		print ("Goal: " + r2 + "," + c2);

		TileArray [r1, c1].GetComponent<TileData> ().colorTile ("Start");
		startTile = TileArray [r1, c1];
		TileArray [r2, c2].GetComponent<TileData> ().colorTile ("Goal");
		goalTile = TileArray [r2, c2];

	}

	//=======================================================================SET THE START AND GOAL=======================================================================











	//=======================================================================OUTPUT FILE=======================================================================
	//Write the start and goal tiles and the multidimensional array into a text file
	//The text file is taken from getFileName()
	//Everytime you run getFileName you write into a different text file.
	public void outputFile(){
		string path = getFileName ();
		StreamWriter writer = new StreamWriter (path, true);
		writer.WriteLine (startTile.GetComponent<TileData>().myRow.ToString());
		writer.WriteLine (startTile.GetComponent<TileData> ().myColumn.ToString());
		writer.WriteLine (goalTile.GetComponent<TileData> ().myRow.ToString());
		writer.WriteLine (goalTile.GetComponent<TileData> ().myColumn.ToString());
		for (int i = 0; i < 120; i++) {
			for (int j = 0; j < 160; j++) {
				writer.Write (TileArray [i, j].GetComponent<TileData> ().identifier);
			}
		}
		writer.Close ();
		print ("Output Complete");
	}
	//=======================================================================OUTPUT FILE=======================================================================






	//=======================================================================INPUT FILE=======================================================================
	//Input file looks at the path for the file to read from and stores values for the map.
	//This does not do the instantiation rather just reads and stores the required data from the input file.
	//At the end it calls mapGenFromInput() which generates the map based on the input.
	public void inputFile(string newPath){

		print ("Inputting....");
		string[,] tileIdentifiers = new string[120,160];
		//string path = "Assets/map1_1.txt";
		string path = newPath;

		StreamReader reader = new StreamReader (path, true);
		startRow = int.Parse(reader.ReadLine ());
		startCol = int.Parse(reader.ReadLine ());
		goalRow = int.Parse(reader.ReadLine ());
		goalCol = int.Parse(reader.ReadLine ());

		int buffer = 0;
		for(int i = 0; i < 120; i++) {
			for (int j = 0; j < 160; j++) {
				buffer = (reader.Read());

				if (buffer == 48) {
					tileIdentifiers [i, j] = "0";
				} else if (buffer == 49) {
					tileIdentifiers [i, j] = "1";
				} else if (buffer == 50) {
					tileIdentifiers [i, j] = "2";
				} else if (buffer == 97) {
					tileIdentifiers [i, j] = "a";
				} else if (buffer == 98) {
					tileIdentifiers [i, j] = "b";
				}
			}
		}
		print ("Done Input..."); 
		GameObject parentTile = Instantiate (emptyParent, new Vector3 (0, 0, 0), Quaternion.identity);
		parentTile.name = "TileParent";
		mapGenFromInput (tileIdentifiers);
	}
	//=======================================================================INPUT FILE=======================================================================










	//=======================================================================MAP GEN FROM INPUT=======================================================================
	//Generates the map when InputFile() is called.
	//This does the actual instantiation and generation.
	public void mapGenFromInput(string[,] tileIdentifiers){
		print ("Starting MapGen Creation from Input...");

		row = 0;
		column = 0;
		GameObject newTileParent = GameObject.Find ("TileParent");
		//======================Instantiate all the tiles======================
		for (int i = 0; i < 120; i++) {
			for (int j = 0; j < 160; j++) {
				TileArray [i, j] = Instantiate (Tile, new Vector3 (row, 0, column), Quaternion.identity);
				TileArray [i, j].GetComponent<TileData> ().identifier = tileIdentifiers[i,j];
				TileArray [i, j].transform.parent = newTileParent.transform;
				TileArray [i, j].GetComponent<TileData> ().myColumn = j;
				TileArray [i, j].GetComponent<TileData> ().myRow = i;
				TileArray [i, j].GetComponent<TileData> ().myNum = assignCount;
				TileArray[i,j].GetComponent<TileData>().colorTile ();
				row += 15;
				assignCount++;
			}
			column -= 15;
			row = 0;
		}

		TileArray [startRow, startCol].GetComponent<TileData> ().colorTile ("Start");
		TileArray [startRow, startCol].GetComponent<TileData> ().isStart = true;
		startTile = TileArray [startRow, startCol];
		TileArray [goalRow, goalCol].GetComponent<TileData> ().colorTile ("Goal");
		TileArray [goalRow, goalCol].GetComponent<TileData> ().isGoal = false;
		goalTile = TileArray [goalRow, goalCol];

	}

	//=======================================================================MAP GEN FROM INPUT=======================================================================







	//=======================================================================Clear START GOAL=======================================================================
	//Simply clears Start/Goal in order to generate new Start/Goal
	//This is only used for testing.
	public void clearGoalStart(){
		TileArray [startRow, startCol].GetComponent<TileData> ().identifier = "1";
		TileArray [startRow, startCol].GetComponent<TileData> ().isStart = false;
		TileArray [startRow, startCol].GetComponent<TileData> ().colorTile ();
		TileArray [goalRow, goalCol].GetComponent<TileData> ().identifier = "1";
		TileArray [goalRow, goalCol].GetComponent<TileData> ().isGoal = false;
		TileArray [goalRow, goalCol].GetComponent<TileData> ().colorTile ();
	}
	//=======================================================================Clear START GOAL=======================================================================



	void Update(){
		if(Input.GetKeyDown("i")){
			inputFile("Assets/map3_2.txt");
		}
		if(Input.GetKeyDown("o")){
			outputFile();
		}

		if(Input.GetKeyDown("m")){
			StartCoroutine(GenerateMap());
		}

		if (Input.GetKeyDown ("c")) {
			clearGoalStart ();
		}

		if (Input.GetKeyDown ("s")) {
			setGoalStart ();
		}

		if (Input.GetKeyDown ("1")) {
			mapgen10 ();
		}



	}



	//This was used to output maps to text files nothing more.
	public void mapgen10(){
		for (int i = 0; i<9; i++) {
			clearGoalStart ();
			setGoalStart (); 
			outputFile ();
		}
		print ("10 Maps Generated");


	}

	//This was used to output maps to text files nothing more.
	public string getFileName(){
		print ("FileNumber: " + fileNumber);
		if (fileNumber > 9) {
			return "End";
			print ("No more files available...");
		}

		switch (fileNumber) {
		case 0:

			fileNumber++;
			return "Assets/map5_1.txt";
			break;

		case 1:
			fileNumber++;
			return "Assets/map5_2.txt";
			break;

		case 2:
			fileNumber++;
			return "Assets/map5_3.txt";
			break;

		case 3:
			fileNumber++;
			return "Assets/map5_4.txt";
			break;

		case 4:
			fileNumber++;
			return "Assets/map5_5.txt";
			break;

		case 5:
			fileNumber++;
			return "Assets/map5_6.txt";
			break;

		case 6:
			fileNumber++;
			return "Assets/map5_7.txt";
			break;

		case 7:
			fileNumber++;
			return "Assets/map5_8.txt";
			break;

		case 8:
			fileNumber++;
			return "Assets/map5_9.txt";
			break;

		case 9:
			fileNumber++;
			return "Assets/map5_10.txt";
			break;
		}

		return "NONE";
	}



}

