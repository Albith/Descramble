
//Game constants. This file holds flags to 
	//keep track of player input.

static class Constants	
{
	
	public const string helpString= "Press H for help."; 
	
	//Rotate Directions
	public const int CLOCKWISE=4;
	public const int COUNTER_CLOCKWISE=5;
	
	//Letter position indexes. 
	//We start from the top index and go clockwise.
	
	public const int UP=0;
	public const int RIGHT=1;
	public const int DOWN=2;
	public const int LEFT=3;
	
	
	//Shifting var.
	public const int ShiftUP=1;
	public const int ShiftDOWN=2;
	public const int ShiftNONE=-1;
	
	
	public const int UP_ANGLE=90;
	public const int RIGHT_ANGLE=0;
	public const int DOWN_ANGLE=-90;
	public const int LEFT_ANGLE=-180;
	
}