using UnityEngine;
using System.Collections;

// a Collectible is just a GamePiece with no match value

// it could be either cleared by a Bomb and/or cleared at the bottom of the screen

public class Collectible : GamePiece 
{
	public bool clearedByBomb = false;
	public bool clearedAtBottom = true;


	void Start () 
	{
		matchValue = MatchValue.None;
	}

}
