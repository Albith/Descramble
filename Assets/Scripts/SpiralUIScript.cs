using UnityEngine;
using System.Collections;

public class SpiralUIScript : MonoBehaviour {

//This class manages the game's display, 
	//in particular, the display and rotation of words on a spiral.
	
//0.Hint text variables on the right side of the screen.
	public TextMesh hintText;
	public GameObject hintBox;
	bool isHintBeingDisplayed;
	string hintMessage= "Press H for Help,\nR for Reset.";
	
//1.Rotation speed variables.
	public int speedAngleChange=15;
	public float frequencyRotationCall=0.06f;
	
//2.Letter Display Variables, movement and angle data.	
	public Vector3 circleCenter;
	
	//An array that keeps track of the radii for each of the 5 circles
		//used for the 5 letters.
	float [] rowRadius;
	
	//5 arrays that display the letters of the latest puzzle the player is solving.
	public GameObject[] row0LettersArray;
	public GameObject[] row1LettersArray;
	public GameObject[] row2LettersArray;
	public GameObject[] row3LettersArray;
	public GameObject[] row4LettersArray;
	
	//Variables used when shifting letter rows; 
		//it contains the 'goal angle' the letters will shift towards.
	public int [] targetAngles;   //one target Angle for every row.
	public int [][] letterAngles;

//3.Row variables, including a state check
	//to determine when the rows are moving 
	//(and during those times, the player can't change the display).	
	public int currentRow;
	bool areRowsMoving;
	int currentMaxRow;
	
//4. Color data for each of the 5 circles associated with the 5 letters.
	public Color32[] originalRowColors;

//5. Sound data.
	//in Index: 
		//0: wind-up sound.
		//1: change row sound.
	public AudioClip[] spiralAudioClips;
	
	
	// Awake method starts up as soon as the game starts running.
		//Sets up the display variables in the class.
	void Awake () {
	
		//-1. Hide the hintBox and other hint-related stuff.
		isHintBeingDisplayed=false;
		hintBox.renderer.enabled=false;
		
		//0. Circle Center.
		circleCenter= new Vector3( -0.10922f, 0f, 0.19449f);
		
		//1. Radius for each row.
		rowRadius= new float[5];
		
		//Setting the radius for each of the circles (stacked on top of each other)
			//associated with each letter.  The array starts from the largest circle,
			//or the outermost row.
		rowRadius[4]=0.2f;
		rowRadius[3]=0.51465f;
		rowRadius[2]=0.8513f;
		rowRadius[1]=1.19638f;
		rowRadius[0]=1.52858f;
	
			
		//2. Initializing the Target Angles to 0. These will change during gameplay.
			targetAngles= new int[5];
			
			for(int i=0; i< 5; i++)
			{
				targetAngles[i]=0; 
			}
				
			areRowsMoving=false;
		
		
		//3. Initializing the letter angles arrays.		
			//These contain the relative angles of each of the words
			//in the puzzle. Possible values are 0,90,-90, and -180.
			//shifting a row in the puzzle will shift all letters in the row
			//by 90 or -90 degrees.
		letterAngles= new int[5][];
		
		for(int i=0; i< 5; i++)
		{
       		letterAngles[i] = new int[4];			
		}
				
	}
	
//	A method for hiding the first two rows on game start.
	//The game begins with 3 letter words (not 5 letter ones)
	public void hide2Rows(){
		
		currentMaxRow=2;

		currentRow=4;
		highlightRow(currentRow);
		
		
		//Hide all letters in the Row0 and Row1 Letter Arrays.
		for(int i=0; i<4; i++)
		{
			row0LettersArray[i].renderer.enabled= false;
			row1LettersArray[i].renderer.enabled= false;
		
		}	
		
		
		//Hide the 2 circles behind the game.
		GameObject.FindGameObjectWithTag("row0").renderer.enabled=false;
		GameObject.FindGameObjectWithTag("row1").renderer.enabled=false;

		
		//Scaling the hint Box to its original shape.
		hintBox.transform.localScale=new Vector3( hintBox.transform.localScale.x,
												  -0.494f,
  												  hintBox.transform.localScale.z);
		
	}
	
	
	//Function called when the player reaches 4 letter words in the game.
	public void showRow1(){
		
		currentMaxRow=1;
		
		//Show all letters in the Row1 Letter Array.
		for(int i=0; i<4; i++)
		{
			row1LettersArray[i].renderer.enabled= true;
		
		}	
		
		
		//Hide the  circle behind the game.
		GameObject.FindGameObjectWithTag("row1").renderer.enabled=true;
	
		//Scale the hintBox to fit 4 words.
		hintBox.transform.localScale=new Vector3( hintBox.transform.localScale.x,
												  -0.67f,
  												  hintBox.transform.localScale.z);
	
		
	}	
	
	//Function called when the player reaches 5 letter word puzzles in the game.
	public void showRow0(){
		
		currentMaxRow=0;
		
		//Show all letters in the Row1 Letter Array.
		for(int i=0; i<4; i++)
		{
			row0LettersArray[i].renderer.enabled= true;
		
		}	
		
		
		//Hide the  circle behind the game.
		GameObject.FindGameObjectWithTag("row0").renderer.enabled=true;
	
		//Scale the hintBox to fit 5 words.
		hintBox.transform.localScale=new Vector3( hintBox.transform.localScale.x,
												  -0.83f,
  												  hintBox.transform.localScale.z);	
	
	}
	


	void setLetterPositions(){
	
		//This method assigns an on-screen position to each of the letters 
			//in the words of the puzzle.
		//This position depends on the letter's relative direction from the circle centers
			//(up, down, left, or right from it), as well as
			//each circle's radius.
		
		//Row4.
			row4LettersArray[Constants.UP].transform.position=
				 new Vector3(circleCenter.x, circleCenter.y, circleCenter.z + rowRadius[4]);
		
		
			row4LettersArray[Constants.RIGHT].transform.position=
				new Vector3(circleCenter.x + rowRadius[4], circleCenter.y, circleCenter.z);
		
			row4LettersArray[Constants.DOWN].transform.position=
				new Vector3(circleCenter.x, circleCenter.y, circleCenter.z - rowRadius[4]);
		
			row4LettersArray[Constants.LEFT].transform.position=
				new Vector3(circleCenter.x - rowRadius[4], circleCenter.y, circleCenter.z);
		
		//Row3.
			row3LettersArray[Constants.UP].transform.position=
				 new Vector3(circleCenter.x, circleCenter.y, circleCenter.z + rowRadius[3]);
		
		
			row3LettersArray[Constants.RIGHT].transform.position=
				new Vector3(circleCenter.x + rowRadius[3], circleCenter.y, circleCenter.z);
		
			row3LettersArray[Constants.DOWN].transform.position=
				new Vector3(circleCenter.x, circleCenter.y, circleCenter.z - rowRadius[3]);
		
			row3LettersArray[Constants.LEFT].transform.position=
				new Vector3(circleCenter.x - rowRadius[3], circleCenter.y, circleCenter.z);
		
		//Row2.
			row2LettersArray[Constants.UP].transform.position=
				 new Vector3(circleCenter.x, circleCenter.y, circleCenter.z + rowRadius[2]);
		
		
			row2LettersArray[Constants.RIGHT].transform.position=
				new Vector3(circleCenter.x + rowRadius[2], circleCenter.y, circleCenter.z);
		
			row2LettersArray[Constants.DOWN].transform.position=
				new Vector3(circleCenter.x, circleCenter.y, circleCenter.z - rowRadius[2]);
		
			row2LettersArray[Constants.LEFT].transform.position=
				new Vector3(circleCenter.x - rowRadius[2], circleCenter.y, circleCenter.z);

		//Row1.
			row1LettersArray[Constants.UP].transform.position=
				 new Vector3(circleCenter.x, circleCenter.y, circleCenter.z + rowRadius[1]);
		
		
			row1LettersArray[Constants.RIGHT].transform.position=
				new Vector3(circleCenter.x + rowRadius[1], circleCenter.y, circleCenter.z);
		
			row1LettersArray[Constants.DOWN].transform.position=
				new Vector3(circleCenter.x, circleCenter.y, circleCenter.z - rowRadius[1]);
		
			row1LettersArray[Constants.LEFT].transform.position=
				new Vector3(circleCenter.x - rowRadius[1], circleCenter.y, circleCenter.z);
		
		//Row0.
			row0LettersArray[Constants.UP].transform.position=
				 new Vector3(circleCenter.x, circleCenter.y, circleCenter.z + rowRadius[0]);
		
		
			row0LettersArray[Constants.RIGHT].transform.position=
				new Vector3(circleCenter.x + rowRadius[0], circleCenter.y, circleCenter.z);
		
			row0LettersArray[Constants.DOWN].transform.position=
				new Vector3(circleCenter.x, circleCenter.y, circleCenter.z - rowRadius[0]);
		
			row0LettersArray[Constants.LEFT].transform.position=
				new Vector3(circleCenter.x - rowRadius[0], circleCenter.y, circleCenter.z);
			
	}
	
//Fetching data from the Game Manager.
	public void loadWords()
	{
		
		//This method loads the words for the current puzzle, 
			//stored by the game manager.		

		//Since a new puzzle is being loaded,
			//The position of the letters (and their angles)
			//go back to default. (up=90, right=0, down=-90, left=180 or -180)
		for(int i=0; i< 5; i++)
		{	
			letterAngles[i] = new int[4];
			
			letterAngles[i][Constants.UP]=90;
			letterAngles[i][Constants.RIGHT]=0;
			letterAngles[i][Constants.DOWN]=-90;
			letterAngles[i][Constants.LEFT]=-180;
		
		}
		
		//resetting Letter Positions.
		setLetterPositions();

		
		
		//1.Fetching the scrambledWords array.
		
		char[][] scrambledWords= new char[4][];
		scrambledWords= gameObject.GetComponent<GameManagerScript>().scrambledWords;
		
		//2.Assigning the text to each word.
		for(int i=0; i<4; i++)
		{
			
		  row0LettersArray[i].GetComponent<TextMesh>().text = scrambledWords[i][0].ToString();
		  row1LettersArray[i].GetComponent<TextMesh>().text = scrambledWords[i][1].ToString();
          row2LettersArray[i].GetComponent<TextMesh>().text = scrambledWords[i][2].ToString();
		  row3LettersArray[i].GetComponent<TextMesh>().text = scrambledWords[i][3].ToString();	
		  row4LettersArray[i].GetComponent<TextMesh>().text = scrambledWords[i][4].ToString();	
		
			
		}
		
		
		//3. Invoking the checkForInput() function.
		InvokeRepeating("checkForInput", 0, 0.017f);
		
	}	
	
// Update is called once per frame
	//void Update () {
	
	public void stopInputCheck()
	{
	
		CancelInvoke("checkForInput");
		
	}
	
	void checkForInput()
	{
		
		if(!areRowsMoving && !isHintBeingDisplayed)
		{

		//Checking for H key pressed, for help string display.
		if(Input.GetKeyDown(KeyCode.H))
 			showHintBoxAndText();
				
		//Checking if the row has been changed
				
				
			if(Input.GetKeyDown(KeyCode.UpArrow))
				changeRow(Constants.UP);
			
			else if(Input.GetKeyDown(KeyCode.DownArrow))
				changeRow(Constants.DOWN);
		
		
		//Checking whether to rotate.
		if(Input.GetKeyDown(KeyCode.RightArrow))
			rotateWords(Constants.CLOCKWISE);
		
		else if(Input.GetKeyDown(KeyCode.LeftArrow))
			rotateWords(Constants.COUNTER_CLOCKWISE);
		
			
		}		
				
		

	}
	
//Showing the hint box that appears over one of the 4 words.
	void showHintBoxAndText()
	{
	
		isHintBeingDisplayed=true;
		
		//1. Select a random Hint from the 4 words in the wheel. 
		int hintLocation= Random.Range(0,8)/2;	
		
		//Move the Hint Box to that location, to highlight one of the words.
		moveHintBoxToLocation(hintLocation);
		
		//1. Fetch Helper Text found in the definitions.
		hintText.text= gameObject.GetComponent<GameManagerScript>().fetchWordDefinitionByIndex(hintLocation);
		
		//2. Run a Coroutine that , after a delay, 
			//hides the string and hitbox.
		StartCoroutine(displayHintRoutine());
		
		
	}	
	
	void moveHintBoxToLocation(int hintBoxLocation)
	{
		
		switch(hintBoxLocation)
		{
			case 0:   //Initial location, top word.
			hintBox.transform.position= new Vector3 (0.01f, hintBox.transform.position.y, 0.03f);
			hintBox.transform.eulerAngles= new Vector3 (hintBox.transform.eulerAngles.x, 0,0);
			break;
		
			case 1:   //Right word.
			hintBox.transform.position= new Vector3 (0.04f, hintBox.transform.position.y, 0.01f);
			hintBox.transform.eulerAngles= new Vector3 (hintBox.transform.eulerAngles.x, 90,0);
			break;
			
			case 2:   //Bottom word.
			hintBox.transform.position= new Vector3 (0.01f, hintBox.transform.position.y, -0.04f);
			hintBox.transform.eulerAngles= new Vector3 (hintBox.transform.eulerAngles.x, 180,0);
			break;
		
			case 3:   //Left word.
			hintBox.transform.position= new Vector3 (-0.03f, hintBox.transform.position.y, 0);
			hintBox.transform.eulerAngles= new Vector3 (hintBox.transform.eulerAngles.x, 270,0);
			break;
			
		
			default:
			print("SpiralUIScript, moveHintBoxToLocation(): invalid location index given.");	
			break;
		}
	}
		
	
//HintBox routine.
	IEnumerator displayHintRoutine()
	{
			
		//1. Display Helper Text and TextBox.
		hintText.renderer.enabled= true;
		
		int hintSoundCounter=0;
		
		//2. Do the following 3 times:
		while (hintSoundCounter<3)
		{
					
			//2a. Play Hint Box sound and Show Hint Box.
			hintBox.renderer.enabled=true;

			
			audio.clip=spiralAudioClips[2];
			audio.Play();
		
			//2b. Delay for some time.
			yield return new WaitForSeconds(0.5f);
	
			//2c. Hide the hint box.
			hintBox.renderer.enabled=false;
			
			//2d. Delay for some time.
			yield return new WaitForSeconds(0.5f);
			
			hintSoundCounter++;
			
		}	
		
		//3. Hide the hint Box.
		hintBox.renderer.enabled=false;
		
		//4. Hide the Helper Text, and show the default message.
			//hintMessage tells the user how to view a hint.
		hintText.text= hintMessage;
		isHintBeingDisplayed=false;
		
		
	}	
	
//Method that changes the active row that players can shift.
	//Players can only move up or down between the 5 rows.	
	void changeRow(int whichDirection)
	{	
		//Going into the center of the circle, towards row 4.
		if(whichDirection== Constants.UP)
		{
			if(currentRow!=4)
			{
				
				//play the change row audio (button press)
				audio.clip=spiralAudioClips[1];
				audio.Play();

				//remove the highlight of the row you were in prior to the button press.		
				dehighlightRow(currentRow);
				
				//incrementing counter and re-highlighting.
				currentRow++;	
				highlightRow(currentRow);
				
			}
				
		}
	
		else{

			//Going down, to the outside circles, to row 0.			
			if(currentRow!=currentMaxRow)	
			{
				
				//play the change row audio (button press)
				audio.clip=spiralAudioClips[1];
				audio.Play();
							
				//remove the highlight of the row you were in prior to the button press.		
				dehighlightRow(currentRow);
				
				//incrementing counter and re-highlighting.
				currentRow--;			
				highlightRow(currentRow);
				
			}	
				
		}
		
	}	
	
	void highlightRow(int whichRow)
	{
		//The highlight Color is yellow.
		
		GameObject rowCircle;
        rowCircle = GameObject.FindWithTag("row"+ whichRow);
		
		rowCircle.renderer.material.color= originalRowColors[5];	

	
	}
	
	void dehighlightRow(int whichRow)
	{
	
		GameObject rowCircle;
        rowCircle = GameObject.FindWithTag("row"+ whichRow);
		
		rowCircle.renderer.material.color= originalRowColors[currentRow];		
		
	}
	
	
//Methods that rotate the row contents when player presses a button. 
	//Rows can only be shifted in two ways: clockwise(down) or counter-clockwise (up).
	//Each letter in the row is moved, and their angles are updated.
	void rotateWords(int direction)
	{
		//play the rotate row audio (wind up sound)
				audio.clip=spiralAudioClips[0];
				audio.Play();
		
		//Initiate the row moving sequence.
		areRowsMoving=true;

		//Performing a clockwise or counter-clockwise rotation.
		if(direction== Constants.CLOCKWISE)
		{	targetAngles[currentRow]= letterAngles[currentRow][0]- 90;
			InvokeRepeating( "moveClockwise", frequencyRotationCall, frequencyRotationCall);
		}

		else
		{	targetAngles[currentRow]= letterAngles[currentRow][0] +90;
			InvokeRepeating( "moveCounterClockwise", frequencyRotationCall, frequencyRotationCall);
		}
		
	}	
	
	void moveClockwise()
	{
		//When the rotation ends, check if the current puzzle is solved.		
		if(letterAngles[currentRow][0]<= targetAngles[currentRow])
		{
			CancelInvoke("moveClockwise");			
			areRowsMoving=false;
			resetAngles();
			
			
			//Call the Game Manager Script to verify the game logic.
			//Move the scrambledWords array, check if level won.
			gameObject.GetComponent<GameManagerScript>().wordWheelShiftedDown(currentRow);
		
		}	

		//if rotation is not over, rotate each letter in row by 
			//a small increment.
		else
		{	
			//angles for letters in same index but different row are the same.	
			letterAngles[currentRow][0]-=speedAngleChange;
			letterAngles[currentRow][1]-=speedAngleChange;
			letterAngles[currentRow][2]-=speedAngleChange;
			letterAngles[currentRow][3]-=speedAngleChange;
			

			//now assigning position based on the different rows and indexes.
			switch(currentRow){
			
			case 0:

			//Rotating along a circle by using a circle equation:		
				//point along circle's circumference (x,z; y is constant): p(c.x + cos(angle)*r, y + sin(angle)*r)
			row0LettersArray[0].transform.position= 
			new Vector3(
						circleCenter.x + Mathf.Cos(letterAngles[currentRow][0]*Mathf.Deg2Rad)*rowRadius[currentRow],
						row0LettersArray[0].transform.position.y,
						circleCenter.z + Mathf.Sin(letterAngles[currentRow][0]*Mathf.Deg2Rad)*rowRadius[currentRow]
				
				);
		
			row0LettersArray[1].transform.position= 
			new Vector3(
						circleCenter.x + Mathf.Cos(letterAngles[currentRow][1]*Mathf.Deg2Rad)*rowRadius[currentRow],
						row0LettersArray[1].transform.position.y,
						circleCenter.z + Mathf.Sin(letterAngles[currentRow][1]*Mathf.Deg2Rad)*rowRadius[currentRow]
				
				);
			
			row0LettersArray[2].transform.position= 
			new Vector3(
						circleCenter.x + Mathf.Cos(letterAngles[currentRow][2]*Mathf.Deg2Rad)*rowRadius[currentRow],
						row0LettersArray[2].transform.position.y,
						circleCenter.z + Mathf.Sin(letterAngles[currentRow][2]*Mathf.Deg2Rad)*rowRadius[currentRow]
				
				);
			
			row0LettersArray[3].transform.position= 
			new Vector3(
						circleCenter.x + Mathf.Cos(letterAngles[currentRow][3]*Mathf.Deg2Rad)*rowRadius[currentRow],
						row0LettersArray[3].transform.position.y,
						circleCenter.z + Mathf.Sin(letterAngles[currentRow][3]*Mathf.Deg2Rad)*rowRadius[currentRow]
				
				);
			
			break;
				
			case 1:
				
			//circle: p(c.x + cos(angle)*r, y + sin(angle)*r)
			row1LettersArray[0].transform.position= 
			new Vector3(
						circleCenter.x + Mathf.Cos(letterAngles[currentRow][0]*Mathf.Deg2Rad)*rowRadius[currentRow],
						row1LettersArray[0].transform.position.y,
						circleCenter.z + Mathf.Sin(letterAngles[currentRow][0]*Mathf.Deg2Rad)*rowRadius[currentRow]
				
				);
		
			row1LettersArray[1].transform.position= 
			new Vector3(
						circleCenter.x + Mathf.Cos(letterAngles[currentRow][1]*Mathf.Deg2Rad)*rowRadius[currentRow],
						row1LettersArray[1].transform.position.y,
						circleCenter.z + Mathf.Sin(letterAngles[currentRow][1]*Mathf.Deg2Rad)*rowRadius[currentRow]
				
				);
			
			row1LettersArray[2].transform.position= 
			new Vector3(
						circleCenter.x + Mathf.Cos(letterAngles[currentRow][2]*Mathf.Deg2Rad)*rowRadius[currentRow],
						row1LettersArray[2].transform.position.y,
						circleCenter.z + Mathf.Sin(letterAngles[currentRow][2]*Mathf.Deg2Rad)*rowRadius[currentRow]
				
				);
			
			row1LettersArray[3].transform.position= 
			new Vector3(
						circleCenter.x + Mathf.Cos(letterAngles[currentRow][3]*Mathf.Deg2Rad)*rowRadius[currentRow],
						row1LettersArray[3].transform.position.y,
						circleCenter.z + Mathf.Sin(letterAngles[currentRow][3]*Mathf.Deg2Rad)*rowRadius[currentRow]
				
				);
			
			break;
				
			case 2:
				
			//circle: p(c.x + cos(angle)*r, y + sin(angle)*r)
			row2LettersArray[0].transform.position= 
			new Vector3(
						circleCenter.x + Mathf.Cos(letterAngles[currentRow][0]*Mathf.Deg2Rad)*rowRadius[currentRow],
						row2LettersArray[0].transform.position.y,
						circleCenter.z + Mathf.Sin(letterAngles[currentRow][0]*Mathf.Deg2Rad)*rowRadius[currentRow]
				
				);
		
			row2LettersArray[1].transform.position= 
			new Vector3(
						circleCenter.x + Mathf.Cos(letterAngles[currentRow][1]*Mathf.Deg2Rad)*rowRadius[currentRow],
						row2LettersArray[1].transform.position.y,
						circleCenter.z + Mathf.Sin(letterAngles[currentRow][1]*Mathf.Deg2Rad)*rowRadius[currentRow]
				
				);
			
			row2LettersArray[2].transform.position= 
			new Vector3(
						circleCenter.x + Mathf.Cos(letterAngles[currentRow][2]*Mathf.Deg2Rad)*rowRadius[currentRow],
						row2LettersArray[2].transform.position.y,
						circleCenter.z + Mathf.Sin(letterAngles[currentRow][2]*Mathf.Deg2Rad)*rowRadius[currentRow]
				
				);
			
			row2LettersArray[3].transform.position= 
			new Vector3(
						circleCenter.x + Mathf.Cos(letterAngles[currentRow][3]*Mathf.Deg2Rad)*rowRadius[currentRow],
						row2LettersArray[3].transform.position.y,
						circleCenter.z + Mathf.Sin(letterAngles[currentRow][3]*Mathf.Deg2Rad)*rowRadius[currentRow]
				
				);
			
			break;	
				
				
			case 3:
				
			//circle: p(c.x + cos(angle)*r, y + sin(angle)*r)
			row3LettersArray[0].transform.position= 
			new Vector3(
						circleCenter.x + Mathf.Cos(letterAngles[currentRow][0]*Mathf.Deg2Rad)*rowRadius[currentRow],
						row3LettersArray[0].transform.position.y,
						circleCenter.z + Mathf.Sin(letterAngles[currentRow][0]*Mathf.Deg2Rad)*rowRadius[currentRow]
				
				);
		
			row3LettersArray[1].transform.position= 
			new Vector3(
						circleCenter.x + Mathf.Cos(letterAngles[currentRow][1]*Mathf.Deg2Rad)*rowRadius[currentRow],
						row3LettersArray[1].transform.position.y,
						circleCenter.z + Mathf.Sin(letterAngles[currentRow][1]*Mathf.Deg2Rad)*rowRadius[currentRow]
				
				);
			
			row3LettersArray[2].transform.position= 
			new Vector3(
						circleCenter.x + Mathf.Cos(letterAngles[currentRow][2]*Mathf.Deg2Rad)*rowRadius[currentRow],
						row3LettersArray[2].transform.position.y,
						circleCenter.z + Mathf.Sin(letterAngles[currentRow][2]*Mathf.Deg2Rad)*rowRadius[currentRow]
				
				);
			
			row3LettersArray[3].transform.position= 
			new Vector3(
						circleCenter.x + Mathf.Cos(letterAngles[currentRow][3]*Mathf.Deg2Rad)*rowRadius[currentRow],
						row3LettersArray[3].transform.position.y,
						circleCenter.z + Mathf.Sin(letterAngles[currentRow][3]*Mathf.Deg2Rad)*rowRadius[currentRow]
				
				);
			
			break;	
				
			case 4:
				
			//circle: p(c.x + cos(angle)*r, y + sin(angle)*r)
			row4LettersArray[0].transform.position= 
			new Vector3(
						circleCenter.x + Mathf.Cos(letterAngles[currentRow][0]*Mathf.Deg2Rad)*rowRadius[currentRow],
						row4LettersArray[0].transform.position.y,
						circleCenter.z + Mathf.Sin(letterAngles[currentRow][0]*Mathf.Deg2Rad)*rowRadius[currentRow]
				
				);
		
			row4LettersArray[1].transform.position= 
			new Vector3(
						circleCenter.x + Mathf.Cos(letterAngles[currentRow][1]*Mathf.Deg2Rad)*rowRadius[currentRow],
						row4LettersArray[1].transform.position.y,
						circleCenter.z + Mathf.Sin(letterAngles[currentRow][1]*Mathf.Deg2Rad)*rowRadius[currentRow]
				
				);
			
			row4LettersArray[2].transform.position= 
			new Vector3(
						circleCenter.x + Mathf.Cos(letterAngles[currentRow][2]*Mathf.Deg2Rad)*rowRadius[currentRow],
						row4LettersArray[2].transform.position.y,
						circleCenter.z + Mathf.Sin(letterAngles[currentRow][2]*Mathf.Deg2Rad)*rowRadius[currentRow]
				
				);
			
			row4LettersArray[3].transform.position= 
			new Vector3(
						circleCenter.x + Mathf.Cos(letterAngles[currentRow][3]*Mathf.Deg2Rad)*rowRadius[currentRow],
						row4LettersArray[3].transform.position.y,
						circleCenter.z + Mathf.Sin(letterAngles[currentRow][3]*Mathf.Deg2Rad)*rowRadius[currentRow]
				
				);
			
			break;
				
			
			default:
			print("SpiralUIScript.cs, moveClockwise(): Invalid currentRow.");
			break;
				
							
			}	
				
		}
		
	}	
	
	void moveCounterClockwise()
	{
	
		//When the rotation ends, check if the current puzzle is solved.		
		if(letterAngles[currentRow][0]>= targetAngles[currentRow])
		{
			CancelInvoke("moveCounterClockwise");			
			areRowsMoving=false;
			resetAngles();
			
			//Call the Game Manager Script to verify the game logic.
			//Move the scrambledWords array, check if level won.
			gameObject.GetComponent<GameManagerScript>().wordWheelShiftedUp(currentRow);
			
		}	
		else
		{
			
			letterAngles[currentRow][0]+=speedAngleChange;
			letterAngles[currentRow][1]+=speedAngleChange;
			letterAngles[currentRow][2]+=speedAngleChange;
			letterAngles[currentRow][3]+=speedAngleChange;
			
			
		switch(currentRow)
			{
			
			case 0:
			
			//circle: p(c.x + cos(angle)*r, y + sin(angle)*r)
			row0LettersArray[0].transform.position= 
			new Vector3(
						circleCenter.x + Mathf.Cos(letterAngles[currentRow][0]*Mathf.Deg2Rad)*rowRadius[currentRow],
						row0LettersArray[0].transform.position.y,
						circleCenter.z + Mathf.Sin(letterAngles[currentRow][0]*Mathf.Deg2Rad)*rowRadius[currentRow]
				
				);
		
			row0LettersArray[1].transform.position= 
			new Vector3(
						circleCenter.x + Mathf.Cos(letterAngles[currentRow][1]*Mathf.Deg2Rad)*rowRadius[currentRow],
						row0LettersArray[1].transform.position.y,
						circleCenter.z + Mathf.Sin(letterAngles[currentRow][1]*Mathf.Deg2Rad)*rowRadius[currentRow]
				
				);
			
			row0LettersArray[2].transform.position= 
			new Vector3(
						circleCenter.x + Mathf.Cos(letterAngles[currentRow][2]*Mathf.Deg2Rad)*rowRadius[currentRow],
						row0LettersArray[2].transform.position.y,
						circleCenter.z + Mathf.Sin(letterAngles[currentRow][2]*Mathf.Deg2Rad)*rowRadius[currentRow]
				
				);
			
			row0LettersArray[3].transform.position= 
			new Vector3(
						circleCenter.x + Mathf.Cos(letterAngles[currentRow][3]*Mathf.Deg2Rad)*rowRadius[currentRow],
						row0LettersArray[3].transform.position.y,
						circleCenter.z + Mathf.Sin(letterAngles[currentRow][3]*Mathf.Deg2Rad)*rowRadius[currentRow]
				
				);
			
			break;
			
			
			case 1:
			
			//circle: p(c.x + cos(angle)*r, y + sin(angle)*r)
			row1LettersArray[0].transform.position= 
			new Vector3(
						circleCenter.x + Mathf.Cos(letterAngles[currentRow][0]*Mathf.Deg2Rad)*rowRadius[currentRow],
						row1LettersArray[0].transform.position.y,
						circleCenter.z + Mathf.Sin(letterAngles[currentRow][0]*Mathf.Deg2Rad)*rowRadius[currentRow]
				
				);
		
			row1LettersArray[1].transform.position= 
			new Vector3(
						circleCenter.x + Mathf.Cos(letterAngles[currentRow][1]*Mathf.Deg2Rad)*rowRadius[currentRow],
						row1LettersArray[1].transform.position.y,
						circleCenter.z + Mathf.Sin(letterAngles[currentRow][1]*Mathf.Deg2Rad)*rowRadius[currentRow]
				
				);
			
			row1LettersArray[2].transform.position= 
			new Vector3(
						circleCenter.x + Mathf.Cos(letterAngles[currentRow][2]*Mathf.Deg2Rad)*rowRadius[currentRow],
						row1LettersArray[2].transform.position.y,
						circleCenter.z + Mathf.Sin(letterAngles[currentRow][2]*Mathf.Deg2Rad)*rowRadius[currentRow]
				
				);
			
			row1LettersArray[3].transform.position= 
			new Vector3(
						circleCenter.x + Mathf.Cos(letterAngles[currentRow][3]*Mathf.Deg2Rad)*rowRadius[currentRow],
						row1LettersArray[3].transform.position.y,
						circleCenter.z + Mathf.Sin(letterAngles[currentRow][3]*Mathf.Deg2Rad)*rowRadius[currentRow]
				
				);
			
			break;
			
			case 2:
			
			//circle: p(c.x + cos(angle)*r, y + sin(angle)*r)
			row2LettersArray[0].transform.position= 
			new Vector3(
						circleCenter.x + Mathf.Cos(letterAngles[currentRow][0]*Mathf.Deg2Rad)*rowRadius[currentRow],
						row2LettersArray[0].transform.position.y,
						circleCenter.z + Mathf.Sin(letterAngles[currentRow][0]*Mathf.Deg2Rad)*rowRadius[currentRow]
				
				);
		
			row2LettersArray[1].transform.position= 
			new Vector3(
						circleCenter.x + Mathf.Cos(letterAngles[currentRow][1]*Mathf.Deg2Rad)*rowRadius[currentRow],
						row2LettersArray[1].transform.position.y,
						circleCenter.z + Mathf.Sin(letterAngles[currentRow][1]*Mathf.Deg2Rad)*rowRadius[currentRow]
				
				);
			
			row2LettersArray[2].transform.position= 
			new Vector3(
						circleCenter.x + Mathf.Cos(letterAngles[currentRow][2]*Mathf.Deg2Rad)*rowRadius[currentRow],
						row2LettersArray[2].transform.position.y,
						circleCenter.z + Mathf.Sin(letterAngles[currentRow][2]*Mathf.Deg2Rad)*rowRadius[currentRow]
				
				);
			
			row2LettersArray[3].transform.position= 
			new Vector3(
						circleCenter.x + Mathf.Cos(letterAngles[currentRow][3]*Mathf.Deg2Rad)*rowRadius[currentRow],
						row2LettersArray[3].transform.position.y,
						circleCenter.z + Mathf.Sin(letterAngles[currentRow][3]*Mathf.Deg2Rad)*rowRadius[currentRow]
				
				);
			
			break;
				
			
			case 3:
			
			//circle: p(c.x + cos(angle)*r, y + sin(angle)*r)
			row3LettersArray[0].transform.position= 
			new Vector3(
						circleCenter.x + Mathf.Cos(letterAngles[currentRow][0]*Mathf.Deg2Rad)*rowRadius[currentRow],
						row3LettersArray[0].transform.position.y,
						circleCenter.z + Mathf.Sin(letterAngles[currentRow][0]*Mathf.Deg2Rad)*rowRadius[currentRow]
				
				);
		
			row3LettersArray[1].transform.position= 
			new Vector3(
						circleCenter.x + Mathf.Cos(letterAngles[currentRow][1]*Mathf.Deg2Rad)*rowRadius[currentRow],
						row3LettersArray[1].transform.position.y,
						circleCenter.z + Mathf.Sin(letterAngles[currentRow][1]*Mathf.Deg2Rad)*rowRadius[currentRow]
				
				);
			
			row3LettersArray[2].transform.position= 
			new Vector3(
						circleCenter.x + Mathf.Cos(letterAngles[currentRow][2]*Mathf.Deg2Rad)*rowRadius[currentRow],
						row3LettersArray[2].transform.position.y,
						circleCenter.z + Mathf.Sin(letterAngles[currentRow][2]*Mathf.Deg2Rad)*rowRadius[currentRow]
				
				);
			
			row3LettersArray[3].transform.position= 
			new Vector3(
						circleCenter.x + Mathf.Cos(letterAngles[currentRow][3]*Mathf.Deg2Rad)*rowRadius[currentRow],
						row3LettersArray[3].transform.position.y,
						circleCenter.z + Mathf.Sin(letterAngles[currentRow][3]*Mathf.Deg2Rad)*rowRadius[currentRow]
				
				);
			
			break;	
				
			case 4:
			
			//circle: p(c.x + cos(angle)*r, y + sin(angle)*r)
			row4LettersArray[0].transform.position= 
			new Vector3(
						circleCenter.x + Mathf.Cos(letterAngles[currentRow][0]*Mathf.Deg2Rad)*rowRadius[currentRow],
						row4LettersArray[0].transform.position.y,
						circleCenter.z + Mathf.Sin(letterAngles[currentRow][0]*Mathf.Deg2Rad)*rowRadius[currentRow]
				
				);
		
			row4LettersArray[1].transform.position= 
			new Vector3(
						circleCenter.x + Mathf.Cos(letterAngles[currentRow][1]*Mathf.Deg2Rad)*rowRadius[currentRow],
						row4LettersArray[1].transform.position.y,
						circleCenter.z + Mathf.Sin(letterAngles[currentRow][1]*Mathf.Deg2Rad)*rowRadius[currentRow]
				
				);
			
			row4LettersArray[2].transform.position= 
			new Vector3(
						circleCenter.x + Mathf.Cos(letterAngles[currentRow][2]*Mathf.Deg2Rad)*rowRadius[currentRow],
						row4LettersArray[2].transform.position.y,
						circleCenter.z + Mathf.Sin(letterAngles[currentRow][2]*Mathf.Deg2Rad)*rowRadius[currentRow]
				
				);
			
			row4LettersArray[3].transform.position= 
			new Vector3(
						circleCenter.x + Mathf.Cos(letterAngles[currentRow][3]*Mathf.Deg2Rad)*rowRadius[currentRow],
						row4LettersArray[3].transform.position.y,
						circleCenter.z + Mathf.Sin(letterAngles[currentRow][3]*Mathf.Deg2Rad)*rowRadius[currentRow]
				
				);
			
			break;	
			
			default:
			print("SpiralUIScript.cs, moveCounterClockwise(): Invalid currentRow.");
			break;
				
				
			}
		
		
		}
	
		
		
	}
	

	
	void resetAngles()
	{
		
		//This method ensures that letter & target angles 
			//are not going over 360 degrees, or less than -360 degrees.
		for(int i=0; i<4; i++)
		{
			if ( (letterAngles[currentRow][i]==360) || (letterAngles[currentRow][i]==-360) )
				letterAngles[currentRow][i]=0;
			
		}
		
		if ( (targetAngles[currentRow]==360) || (targetAngles[currentRow]==-360) )
				targetAngles[currentRow]=0;
		
		
	}
	
	
	public void gameFinished()
	{
		//The hint text is used to display the winning text.
			//It is all that is shown as a reward to the player.
		hintText.renderer.enabled=true;

		isHintBeingDisplayed=true;
		
		CancelInvoke();
		
		hintText.text= "You Win!";
		
		StartCoroutine(showRestartOption());
		
	}
	
	//Function called by the GameManager after the hint display sequence ends.
	public void giveControlBackToPlayer()
	{
		
		isHintBeingDisplayed=false;		
	
	}
	
	//This simple function displays a message after the game ends.
	IEnumerator showRestartOption()
	{
		yield return new WaitForSeconds(1.3f);		
		hintText.text= "Press R to\nRestart Game.";
		
	}
	
}
