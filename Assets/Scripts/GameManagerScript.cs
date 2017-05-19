using UnityEngine;
using System.Collections;

//This extra library is used for Text File Parsing.
using System.Text;
using System.IO; 

public class GameManagerScript: MonoBehaviour {
	
	//This class handles the game logic in the game.
		//It changes and verifies the game state.

	//Game Logic variables. current Level starts at 1.
		public int currentLevel;
		public int MaxLevels;
		bool isGameWaitingToBeRestarted;
	
	//These variables hold one set (3-word, 4-word or 5-word)
		//of words and definitions at a time.
		public ArrayList wordList;
		public ArrayList definitionList;
	
	//Each puzzle is verified by having two multi-dimensional character arrays.
		//(Strings are tough to alter, so we're using chars,
		//(as we are shuffling characters around when the player rotates a row.)
		//One variable contains the puzzle's words+letters in correct order.
		//The other variable contains the letters in a scrambled configuration.
			//This last variable is updated along with the player's choices.
		 public char[][] solvedWords;
		 public char[][] scrambledWords;
	
	//Keeps track of the indexes of the current puzzle's 4 words within the text files.
	public int [] current4WordIndexes; 
		
	//Keeping track of the previous rotation the player performed.
		//To do in the future: this variable is insufficient 
		//to check if a word has been scrambled enough.

		//There is a bug in this operation, in which the resulting letter shifts
		//end up presenting an ordered solution (therefore there is no puzzle to solve).
		int previousShiftDirection;
		//To do: This variable should be a dictionary, 
			//detailing both the rows and amount of rotations performed.
		ArrayList rotatedRows; 	//already Rotated Rows. Used in the scrambleWords();

	
	//Keeping track of the words+definitions that have already been used
		//in a puzzle.
		ArrayList usedWordIndexes;
	
	//Source files for the words and their definitions.
	TextAsset myWordFile;
	TextAsset myDefinitionsFile;
		
	public AudioClip[] gameStateAudioClips;
	//index 0 is for Level Clear.
	//index 1 is for Hint sound.
	//index 2 is for Game Clear Sound.
	

	void Update()
	{
		
		//This update function is for resetting purposes.
			//Pressing the R key will result in a level reset,
			//or a game reset, if pressed at the end of the game.		

		if(Input.GetKeyDown(KeyCode.R))
		{	
			if(!isGameWaitingToBeRestarted)
				StartCoroutine(resetLevelRoutine());
	
			else
				StartCoroutine(resetGameRoutine());
		}
			
	}	
		
	//Initializing the class's variables in the beginning.
	void Start () {
	
		isGameWaitingToBeRestarted=false;
		currentLevel=1;
		MaxLevels=7;
			
		current4WordIndexes= new int[4];		
		usedWordIndexes= new ArrayList();
		
		//handling the other vars.
		previousShiftDirection=Constants.ShiftNONE;
		
		//-1.Loading the text files.
		fetchWordFile();
		fetchWordDefinitionsFile();
		
		//0.Creating word lists.
		wordList= new ArrayList();
		wordList.AddRange( myWordFile.text.Split('\n') );
		
		
		definitionList= new ArrayList();
		definitionList.AddRange( myDefinitionsFile.text.Split('\n') );
		
		//Adding newlinecharacters to the definitions file
		prepareDefinitionsNewline();
		
		
			//0a. Initializing the char arrays.
			//The char arrays are initialized to {'@', '@', '@', '@', '@' }.
			solvedWords= new char[4][];
			scrambledWords= new char[4][];

				for(int i=0; i< 4; i++)
				{
			
					solvedWords[i]= new char[] {'@', '@', '@', '@', '@' };
					scrambledWords[i]= new char[] {'@', '@', '@', '@', '@' };

				}
		
		
		
		//1.Fetching the solved words.
		fetchWords();
		
		//printWordArrays();
		
		//2.Scrambling the words depending on the current level.
		scrambleWords();
		
		
		//printWordArrays();
		
		
		//3.Feeding the scrambled words to the SpiralUIScript.
		gameObject.GetComponent<SpiralUIScript>().loadWords();
		
		
		
		//4. Hiding the first two rows and letters.
		gameObject.GetComponent<SpiralUIScript>().hide2Rows();

		
	}
	
	void prepareDefinitionsNewline()
	{
		
		//Check every array entry.
		int i=0;
		int howManyNewlines=0;
		
		while( i< definitionList.Count)
		{
			string tempString= (string) definitionList[i];
			int newlineIndex= tempString.IndexOf('|');
			
			if(newlineIndex!=-1)
			{
				
				string subStr1= tempString.Substring(0, newlineIndex);
				string subStr2= tempString.Substring((newlineIndex+1));
				
				definitionList[i]= subStr1 + "\n" + subStr2;
				
				howManyNewlines++;
				
			}
			
			i++;
			
		}	
		
		//print("prepareDefinitionsNewLine(): added "+howManyNewlines+" newlines.");
		
	}
	
	
	void prepareNextLevel()
	{
		
		
		//Only on level 3 or level 6, or 1 do we switch text files.
		//Only then should we clear the ArrayList.
		if((currentLevel==3)||(currentLevel==6)||(currentLevel==1))
		usedWordIndexes= new ArrayList();
		
		
		//handling the other vars.
		previousShiftDirection=Constants.ShiftNONE;
		
		

		//Only doing a new ArrayList
		if((currentLevel==3)||(currentLevel==6)||(currentLevel==1))
		{
			
			
			//-1.Loading the text files.
			fetchWordFile();
			fetchWordDefinitionsFile();
		
			//0.Creating word lists.
			wordList= new ArrayList();
			wordList.AddRange( myWordFile.text.Split('\n') );
		
		
			definitionList= new ArrayList();
			definitionList.AddRange( myDefinitionsFile.text.Split('\n') );
		
			//Adding newlinecharacters to the definitions file
			prepareDefinitionsNewline();
		
		}
		
		
		//1.Fetching the solved words.
		fetchWords();
		
		//printWordArrays();
		
		//2.Scrambling the words depending on the current level.
		scrambleWords();
		
		
		//printWordArrays();
		
		
		//3.Feeding the scrambled words to the SpiralUIScript.
		gameObject.GetComponent<SpiralUIScript>().loadWords();
		

	}
	
	
	void resetLevel()
	{
			
		
		//1.Fetching the solved words.
		fetchWordsForReset();
		
		//printWordArrays();
		
		//2.Scrambling the words depending on the current level.
		scrambleWords();
		
		
		//printWordArrays();
		
		
		//3.Feeding the scrambled words to the SpiralUIScript.
		gameObject.GetComponent<SpiralUIScript>().loadWords();
		

	}
	
	
//------Level Setup Functions.	
	int startIndexByLevel()
	{

		
		switch(currentLevel)
		{
		case 1:
			return 2;
			
		case 2:
			return 2;
			
		case 3:
			return 1;
			
		case 4:
			return 1;
			
		case 5:
			return 1;
			
		case 6:
			return 0;
			
		case 7:
			return 0;
			
		default:
		print ("GameManagerScript(), lettersByLevel(): invalid number.");
			return -1;
			
		}
		
		
	}
	int getNextWordByIndex(int currentWordIndex)
	{
		//Used to shift between words in the word wheel.
		
		int nextWordIndex=-1;
		
		switch(currentWordIndex)
		{
		case 0:
		nextWordIndex=1;
		break;
			
		case 1:
		nextWordIndex=2;
		break;
			
		case 2:
		nextWordIndex=3;
		break;
			
		case 3:
		nextWordIndex=0;
		break;
			
		default:
		print ("GameManagerScript(), getNextWordByIndex: invalid Index.");	
			
		break;
			
		}	
		
		return nextWordIndex;
		
	}
	
	void fetchWordDefinitionsFile()
	{
		//The words and their definitions are kept in pairs, in two text files.
		//These files are kept in the Resources folder.
		//There are 3 pairs of files: files for 3, 4 and 5 letter words and their definitions.

		if(currentLevel<3)
		myDefinitionsFile= (TextAsset)Resources.Load("3LetterWordDefinitions", typeof(TextAsset));
		
		else if(currentLevel<6)
		myDefinitionsFile= (TextAsset)Resources.Load("4LetterWordDefinitions", typeof(TextAsset));
		
		else
		myDefinitionsFile= (TextAsset)Resources.Load("5LetterWordDefinitions", typeof(TextAsset));	
		
	}
	
	public string fetchWordDefinitionByIndex(int definitionIndex)
	{
		
		return (string) definitionList[current4WordIndexes[definitionIndex]];
		
	}
	
	void fetchWordFile()
	{
	
		if(currentLevel<3)
		myWordFile= (TextAsset)Resources.Load("3LetterWordList", typeof(TextAsset));
		
		else if(currentLevel<6)
		myWordFile= (TextAsset)Resources.Load("4LetterWordList", typeof(TextAsset));
		
		else
		myWordFile= (TextAsset)Resources.Load("5LetterWordList", typeof(TextAsset));
		
		
	}	
	
	void fetchWords()
	{
	
		//
		
		bool are4IndexesReady= false;
		int tempIndex;
		
		int numberOfIndexesAdded=0;
		
		//Get 4 random words from wordList, that havent been used before.
		
		while(!are4IndexesReady)
		{
			
			
			tempIndex= Random.Range(0,wordList.Count);
		
			
			
			//check if the index has already been used.
			//if not, add it to the indexes.
			if(!usedWordIndexes.Contains(tempIndex))
			{	
				//print ("Word index "+ tempIndex);
				
				usedWordIndexes.Add(tempIndex);
				
				//adding index to the IndexArray.
				current4WordIndexes[3-numberOfIndexesAdded]= tempIndex;
				
				
				numberOfIndexesAdded++;
				
				
				if(numberOfIndexesAdded>= 4)
				are4IndexesReady=true;
					
			}
			
		}
		
		print("Fetched new indexes.");
		
		//Convert the words to Char arrays, and populate both scrambledWords and solvedWords.
		
		for(int i=0; i<4; i++)
		{
			
			tempIndex=(int) usedWordIndexes[(usedWordIndexes.Count-1-i)];
			
			//print("tempIndex is "+ tempIndex);
			
			string tempString= (string) wordList[tempIndex];
			
			//These strings must be concatenated with @ characters
			//for the beginning levels
			
			//if we need to concatenate, we do so before 
			//tempString is converted to char.
			
			if(startIndexByLevel()==2)
				tempString= "  "+tempString;
			
			else if(startIndexByLevel()==1)
				tempString= " "+tempString;
	
					
				solvedWords[i]= tempString.ToCharArray();
				scrambledWords[i]= tempString.ToCharArray();
				
			//scrambledWords[i]=solvedWords[i];
			//^ For some reason, this  copies the data.
						
		}	
		
	}	
	
	void fetchWordsForReset()
	{
	
		//
		
		bool are4IndexesReady= false;
		int tempIndex;
		
		int numberOfIndexesAdded=0;
		
		//Make arrayList of words for only this round.
		
		//Get 4 random words from wordList, that havent been used before.
		
		while(!are4IndexesReady)
		{
			
			
			tempIndex= Random.Range(0,wordList.Count);
		
			
			
			//check if the index has already been used.
			//if not, add it to the indexes.
			if(!usedWordIndexes.Contains(tempIndex))
			{	
				//print ("Word index "+ tempIndex);
				
				usedWordIndexes.Add(tempIndex);
				
				//adding index to the IndexArray.
				current4WordIndexes[3-numberOfIndexesAdded]= tempIndex;
				
				numberOfIndexesAdded++;
				
				if(numberOfIndexesAdded>= 4)
				are4IndexesReady=true;
					
			}
			
		}
		
		print("Fetched new indexes.");
		
		//Convert the words to Char arrays, and populate both scrambledWords and solvedWords.
		
		for(int i=0; i<4; i++)
		{
			
			tempIndex=(int) usedWordIndexes[(usedWordIndexes.Count-1-i)];
			
			//print("tempIndex is "+ tempIndex);
			
			string tempString= (string) wordList[tempIndex];
			
			//These strings must be concatenated with @ characters
			//for the beginning levels
			
			//if we need to concatenate, we do so before 
			//tempString is converted to char.
			
			if(startIndexByLevel()==2)
				tempString= "  "+tempString;
			
			else if(startIndexByLevel()==1)
				tempString= " "+tempString;
	
					
				solvedWords[i]= tempString.ToCharArray();
				scrambledWords[i]= tempString.ToCharArray();
				
			//scrambledWords[i]=solvedWords[i];
			//^ For some reason, this  copies the data.
						
		}	
		
	}
	
	
	void scrambleWords()
	{
		
		//Scramble different rows.
		//How often they're scrambled depends on the current level.
		
		//also, the current level determines the length of the words.
		
		//Make sure you don't scramble the same row twice.
		
		int randomRow=0;
		bool areWordsAllScrambled=false;
		
		int numberOfScrambles=1;
		
		
	  switch(currentLevel)
		{
		
			case 1:
			//3 letters, 1 row scramble.
			
			randomRow= Random.Range(2,5);
			
			scrambleRow(randomRow);
			
			
			break;
			
			case 2:
			//3 letters, 2 row scramble.
			
			rotatedRows= new ArrayList();
			
			
			while(!areWordsAllScrambled)
			{	
				
				randomRow= Random.Range(2,5);
				
				if(!rotatedRows.Contains(randomRow))
				{	
	
					rotatedRows.Add(randomRow);
					scrambleRow(randomRow);
				
					numberOfScrambles++;
					
					if(numberOfScrambles > 2)
						areWordsAllScrambled=true;
	
				}	
					
			}
			
			break;
			
			case 3:
			//4 letters, 2 row scramble.
			
			//setting other vars.
			gameObject.GetComponent<SpiralUIScript>().showRow1();

			
			rotatedRows= new ArrayList();
			
			
			while(!areWordsAllScrambled)
			{	
				
				randomRow= Random.Range(1,5);
				
				if(!rotatedRows.Contains(randomRow))
				{	
	
					rotatedRows.Add(randomRow);
					scrambleRow(randomRow);
				
					numberOfScrambles++;
				
				
					if(numberOfScrambles > 2)
						areWordsAllScrambled=true;
				
				}	
					
			}
			
			
			break;
			
			case 4:
			//4 letters, 3 row scramble.
			
			rotatedRows= new ArrayList();
			
			
			while(!areWordsAllScrambled)
			{	
				
				randomRow= Random.Range(1,5);
				
				if(!rotatedRows.Contains(randomRow))
				{	
	
					rotatedRows.Add(randomRow);
					scrambleRow(randomRow);
				
					numberOfScrambles++;
				
					if(numberOfScrambles > 3)
						areWordsAllScrambled=true;
			
				}	
					
			}

			
			
			break;
			
			case 5:
			//4 letters, 4 row scramble.
			
			rotatedRows= new ArrayList();
	
			
			while(!areWordsAllScrambled)
			{	
				
				randomRow= Random.Range(1,5);
				
				if(!rotatedRows.Contains(randomRow))
				{	
	
					rotatedRows.Add(randomRow);
					scrambleRow(randomRow);
				
					numberOfScrambles++;
					
					if(numberOfScrambles > 4)
						areWordsAllScrambled=true;
				
				}	
					
			}
			
			
			break;
			
			case 6:
			//5 letters, 4 row scramble.
			
			//setting other vars.
			gameObject.GetComponent<SpiralUIScript>().showRow0();
			
			
			rotatedRows= new ArrayList();

			
			while(!areWordsAllScrambled)
			{	
				
				randomRow= Random.Range(0,5);
				
				if(!rotatedRows.Contains(randomRow))
				{	
	
					rotatedRows.Add(randomRow);
					scrambleRow(randomRow);
				
					numberOfScrambles++;
				
					if(numberOfScrambles > 4)
						areWordsAllScrambled=true;
			
				}	
					
			}
			
			
			break;
			
			
			case 7:
			//5 letters, 5 row scramble.
		
			rotatedRows= new ArrayList();

			
			while(!areWordsAllScrambled)
			{	
				
				randomRow= Random.Range(0,5);
				
				if(!rotatedRows.Contains(randomRow))
				{	
					
					rotatedRows.Add(randomRow);
					scrambleRow(randomRow);
				
					numberOfScrambles++;
				
					if(numberOfScrambles > 5)
						areWordsAllScrambled=true;
				
				}	
					
			}
			
			
			break;
			
			
			default:
			print("GameManagerScript.cs, scrambleWords(): invalid currentLevel.");		
			break;
			
		}
	}	
	
	
	//Shift a given row 1 or 2 times in a direction.
	void scrambleRow(int whichRow)
	{
		
		//Note: This function will also rotate the 4 Word Indexes as well.
			
		
		int howManyShifts= Random.Range(1,3);	
	
		//Update: This will shift the row in the opposite direction.
		//to the one previously given.
		
		if(previousShiftDirection==Constants.ShiftNONE)
			previousShiftDirection= Random.Range(1,3);
		
		//shifting Down.
		if(previousShiftDirection==Constants.ShiftUP)
		{
						
			shiftRowDown(whichRow);
			
			if(howManyShifts==2)
				shiftRowDown(whichRow);
			
		//print("Scrambling down row "+ whichRow +", "+ howManyShifts+ " times");

			
			previousShiftDirection=Constants.ShiftDOWN;
			
		}
		
		//shifting Up.
		else
		{
			
			shiftRowUp(whichRow);
			
			if(howManyShifts==2)
				shiftRowUp(whichRow);
			
			
		//print("Scrambling up row "+ whichRow +", "+ howManyShifts+ " times");

			
			previousShiftDirection=Constants.ShiftUP;
			
		}	
			
	}	
	
	
	
//-----Word modification functions.	
	
	public void wordWheelShiftedUp(int whichRow)
	{
		
		shiftRowUp(whichRow);
		checkForLevelSolved();
		
	}
	
	public void wordWheelShiftedDown(int whichRow)
	{
		
		shiftRowDown(whichRow);
		checkForLevelSolved();
		
	}
	
	//Shifting wordIndexes up or down.
	void shiftWordIndexesUp()
	{
		print("shifting Hint Indexes up.");
		
		int temp= current4WordIndexes[3];
		
		current4WordIndexes[3]=current4WordIndexes[0];
		current4WordIndexes[0]=current4WordIndexes[1];
		current4WordIndexes[1]=current4WordIndexes[2];
		current4WordIndexes[2]=temp;

		
	}
	
	void shiftWordIndexesDown()
	{
		print("shifting Hint Indexes down.");
		
		int temp= current4WordIndexes[0];
		
		current4WordIndexes[0]=current4WordIndexes[3];
		current4WordIndexes[3]=current4WordIndexes[2];
		current4WordIndexes[2]=current4WordIndexes[1];
		current4WordIndexes[1]=temp;
		
		
	}	
	
	
	//Shifts word list up on specified row.
	void shiftRowUp(int whichRow)
	{
	 	//Note: This function will rotate the 4 Word Indexes as well.
		print("shifting Row "+whichRow+" up.");
		
		if(whichRow==startIndexByLevel())
			shiftWordIndexesUp();
		
		//---
		
		char tempString= scrambledWords[0][whichRow];
		
		scrambledWords[0][whichRow]= scrambledWords[1][whichRow];
		scrambledWords[1][whichRow]= scrambledWords[2][whichRow];
		scrambledWords[2][whichRow]= scrambledWords[3][whichRow];
		scrambledWords[3][whichRow]= tempString;

		
	}
	
	
	//Shifts word list down on specified row.
	void shiftRowDown(int whichRow)
	{
		
		//Note: This function will rotate the 4 Word Indexes as well.
		print("shifting Row "+whichRow+" down.");
		
		
		if(whichRow==startIndexByLevel())
			shiftWordIndexesDown();
		
		//----
		
		char tempString= scrambledWords[3][whichRow];
		
		scrambledWords[3][whichRow]= scrambledWords[2][whichRow];
		scrambledWords[2][whichRow]= scrambledWords[1][whichRow];
		scrambledWords[1][whichRow]= scrambledWords[0][whichRow];
		scrambledWords[0][whichRow]= tempString;
	
		
	}
	
	//Method that checks if the scrambled letters have been properly sorted.	
	void checkForLevelSolved()
	{
		bool are3WordsMatching=false;
		bool are2WordsMatching=false;
		bool isAWordMatching=false;
		
		int matchingWordIndex=-1;
		bool WordFound=false;
		
		//We are checking 3 words.
			//The reason being, there are duplicate letters in the puzzle.
			//One word may be correctly ordered, but this doesn't mean the other words are.

			//Just to be safe, we check that at least 3 of the scrambled words
			//are equal to the correcly sorted words,
			//before deciding that the current puzzle is solved.	

		//First Word.		
		for(int i=0; i<4; i++)
		{
			//Check each word of solvedWords...
			//with all the words of scrambledWords.
						
			if(scrambledWords[i][startIndexByLevel()] == 
				solvedWords[0][startIndexByLevel()]) 
			{	

				//print ("First Letters of Solved Words 0 DO match with Scrambled Word "+i+ ", " +scrambledWords[i][startIndexByLevel()]);
				
				//Checking words letter by letter.
				for(int j=(startIndexByLevel()+1); j<5; j++)
				{
			
					if(scrambledWords[i][j] == solvedWords[0][j])
					{
					
						//print ("Letters "+j+" of Solved Word 0 DO MATCH Scrambled Word "+i+ ", " + scrambledWords[i][j]);
						
						//if all letters, up to the last letter in the scrambled word,
							//match one of the solved word's letters, the puzzle is solved.
						if(j==4)
						{
							//print ("Solved Word 0 matches Scrambled Word "+i+".");
											
							isAWordMatching=true;
							matchingWordIndex=i;
						
							WordFound=true;			
						}	
					}	
					
					//Else break out of the loop, the words don't match.
					else 
					{	
						//print ("Letters "+j+" of Solved Word 0 DO NOT MATCH Scrambled Word "+i+ ", " + scrambledWords[i][j]+ " .  Breaking.");
						break;
					}
				
				
				}	
			
				if(WordFound)
					break;
			
			}	
			
			//else print ("Solved Words 0 DOESN'T match with Scrambled Word "+i);
			
			
		}	
			

		//Second word.
		if(isAWordMatching)
		{
			//trying the word that is 2 indexes after the matching word.
			
			int y= getNextWordByIndex(getNextWordByIndex(matchingWordIndex));
			//print("Second word: y-index is "+y);
			
			if(scrambledWords[y][startIndexByLevel()] == solvedWords[2][startIndexByLevel()]) 
			{	
			
				//print ("checkForLevelSolved(): ");
							
				for(int j=startIndexByLevel()+ 1; j<5; j++)
				{
			
					if(scrambledWords[y][j] == solvedWords[2][j])
					{
					
						if(j==4)
						{
							
							//print ("Second word: Solved Word "+y+ " matches Scrambled Word "+y);

							are2WordsMatching=true;
						}	
					}	
					
					//Else break out of the loop.
					else 
					{
						//print ("Word 2 doesn't match.");
						break;
					}
				}	
			
			}	
		
			//else print ("Word 2 doesn't match.");
			
		}	
			
		
		//Third word.
		if(are2WordsMatching)
		{
			//Testing the word that is 2 indexes after the matching word.		
			int y= getNextWordByIndex(matchingWordIndex);
			//print("Second word: y-index is "+y);
			
			if(scrambledWords[y][startIndexByLevel()] == solvedWords[1][startIndexByLevel()]) 
			{	
			
				//print ("checkForLevelSolved(): ");
							
				for(int j=startIndexByLevel()+ 1; j<5; j++)
				{
			
					if(scrambledWords[y][j] == solvedWords[1][j])
					{
					
						if(j==4)
						{
							
							//print ("Second word: Solved Word "+y+ " matches Scrambled Word "+y);
							are3WordsMatching=true;
						}	
					}	
					
					//Else break out of the loop.
					else 
					{
						//print ("Word 3 doesn't match.");
						break;
					}
				}	
			
			}	
		
			//else print ("Word 3 doesn't match.");
		
		
		}
			
		//If 3 words ore matching, you cleared the level.
		if(are3WordsMatching)
		{
			
			//print ("LEVEL CLEAR.");
			StartCoroutine(toNextLevelRoutine());
			
		}	
	
	}	
	
//The rest of the methods are game state checking and level management routines.
	//There is also some display logic here.
	
	IEnumerator toNextLevelRoutine()
	{
				
		if(currentLevel== MaxLevels)
			gameCleared();
		
		else
		{
			
			//Play the change row audio (level clear.)
			audio.clip=gameStateAudioClips[0];
			audio.Play();
			
			gameObject.GetComponent<SpiralUIScript>().stopInputCheck();

			
			yield return new WaitForSeconds(0.72f);
			
			currentLevel++;
			prepareNextLevel();
		}
	
	}
	
	IEnumerator resetLevelRoutine()
	{
			
			//play the change row audio (level clear.)
			audio.clip=gameStateAudioClips[3];
			audio.Play();
			
			gameObject.GetComponent<SpiralUIScript>().stopInputCheck();

			
			yield return new WaitForSeconds(0.72f);
			
			resetLevel();
		
	
	}
	
	IEnumerator resetGameRoutine()
	{
			
			//Play the change row audio (level clear.)
			audio.clip=gameStateAudioClips[3];
			audio.Play();
			
			//Temporarily take control away from the player.
			gameObject.GetComponent<SpiralUIScript>().stopInputCheck();
			
			yield return new WaitForSeconds(0.72f);	
		
			resetGame();

			//Wait a little bit.	
			yield return new WaitForSeconds(0.3f);
		
			//The player can now start playing.
			gameObject.GetComponent<SpiralUIScript>().giveControlBackToPlayer();
	
	}
	
	void resetGame()
	{
		
		//Hide the two outer word rows.
		gameObject.GetComponent<SpiralUIScript>().hide2Rows();
		
		//Set the current Level to 1.
		currentLevel=1;
				
		//Reload the word definitions and wordList.
		prepareNextLevel();
					
		//We're done with the restarting method.
		isGameWaitingToBeRestarted=false;
		
	}
	
	void gameCleared()
	{	
	
		audio.clip=gameStateAudioClips[2];
		audio.Play();
		
		//Put a winning Text on the HelpText.
		gameObject.GetComponent<SpiralUIScript>().gameFinished();
		
		//It's up to the player to restart.
		isGameWaitingToBeRestarted=true;
		
	}
	
	//Helper function used to print the state of the current puzzle.
	void printWordArrays()
	{		
		print("Scrambled	Solved  index 0, index 2");
			
		for(int i=startIndexByLevel(); i<5; i++)
			print(scrambledWords[0][i]+"          "+solvedWords[0][i]);		
		
	}	
		
	
}
