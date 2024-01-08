using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AdjacentLetters;
using UnityEngine;

/// <summary>
/// On the Subject of Not Adjacent Letters
/// Original Mod Created by lumbud84, implemented by Timwi
/// New Mod by coolpapabell2282
/// </summary>
public class notAdjacentLettersModule : MonoBehaviour
{
    public KMBombInfo Bomb;
    public KMBombModule Module;
    public KMAudio Audio;
    public KMSelectable[] Buttons;
    public KMSelectable SubmitButton;
    public Material FontMaterial;
    public Material UnpushedButtonMaterial;
    public Material PushedButtonMaterial;
    public GameObject Label;
    public Light[] Lights;
    
    private bool _puzzleSet = false;
    private GameObject[] _labels;
    private System.Random _rnd;
    private char[] _letters;
    private char[] _grid;
    private int _selected;
    private char[] _origGrid;
    private bool _isSolved;
    private bool _submitButtonCoroutineActive = false;

    private static int _moduleIdCounter = 1;
    private int _moduleId;

    private static readonly string _alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private static readonly string[] _leftRight = new[] {
        "GJMOY",
        "IKLRT",
        "BHIJW",
        "IKOPQ",
        "ACGIJ",
        "CERVY",
        "ACFNS",
        "LRTUX",
        "DLOWZ",
        "BQTUW",
        "AFPXY",
        "GKPTZ",
        "EILQT",
        "PQRSV",
        "HJLUZ",
        "DMNOX",
        "CEOPV",
        "AEGSU",
        "ABEKQ",
        "GVXYZ",
        "FMVXZ",
        "DHMNW",
        "DFHMN",
        "BDFKW",
        "BCHSU",
        "JNRSY"
    };
    // rearrangment of table from original Adj Letters. EGKRS are the letters that must be pressed if left/right of A, etc.
    private static readonly string[] _leftRightOk = {  
	"EGKRS",
	"CJSXY",
	"EFGQY",
	"IPVWX",
	"FMQRS",
	"GKUWX",
	"AELRT",
	"COVWY",
	"BCDEM",
	"ACEOZ",
	"BDLSX",
	"BHIMO",
	"APUVW",
	"GPVWZ",
	"ADIPQ",
	"DKLNQ",
	"DJMNS",
	"BFHNZ",
	"GNRYZ",
	"BHJLM",
	"HJORY",
	"FNQTU",
	"CIJVX",
	"HKPTU",
	"AFKTZ",
	"ILOTU"
    };
    // Same but for above\below data
    private static readonly string[] _aboveBelowOk = new[] {  
	"FLNVX",
	"LMQRU",
	"BDPTZ",
	"BCHKQ",
	"CIJVY",
	"BIMNP",
	"FNSWY",
	"AGJNP",
	"JKOQU",
	"DFTVX",
	"AEHPQ",
	"NTUWZ",
	"CHSYZ",
	"IQRUX",
	"GJKRX",
	"AFHMZ",
	"FGOVW",
	"AKLPW",
	"EHJOT",
	"CDOWY",
	"CDEIT",
	"ILSXZ",
	"ADEMY",
	"LMORV",
	"BGRSU",
	"BEGKS",
    };
    private static readonly string[] _aboveBelow = new[] {
        "HKPRW",
        "CDFYZ",
        "DEMTU",
        "CJTUW",
        "KSUWZ",
        "AGJPQ",
        "HOQYZ",
        "DKMPS",
        "EFNUV",
        "EHIOS",
        "DIORZ",
        "ABRVX",
        "BFPWX",
        "AFGHL",
        "IQSTX",
        "CFHKR",
        "BDIKN",
        "BNOXY",
        "GMVYZ",
        "CJLSU",
        "BILNY",
        "AEJQX",
        "GLQRT",
        "AJNOV",
        "EGMTW",
        "CLMPV"
    };


    string Intersect(string _short, string _long) // returns a string consisting of exactly the letters in both input strings
    {
	var output = new List<char>();
	for(var i=0; i < _short.Length; i++)
	{
		if(_long.Contains(_short[i]))
		{
			output.Add(_short[i]);
		}
	}
	return new String(output.ToArray());
    }
    void Start()
    {
	for(var i =0; i<12; i++)
	{
	        Lights[i].enabled=false;
	}
	_rnd = new System.Random();
        _moduleId = _moduleIdCounter++;
        _selected = 13;
	_labels = new GameObject[12];
        _isSolved = false;
	var choice = 0;
	var curAlpha = _alphabet;
        FontMaterial.mainTexture.wrapMode = TextureWrapMode.Clamp; // Timwi had this in original Adj Letters so I left it.
	_grid = new Char[12];
	_origGrid = new Char[12];
	var _gridSet = new bool[12];
	var first = _rnd.Next(17);  // choose where first entries in grid will go
	var second = 0;
	// Debug.LogFormat("first" + first.ToString());
	choice = _rnd.Next(26);
	curAlpha = _alphabet.Remove(choice,1);
	if(first < 9)  // Second letter goes right of first, chooses it so it is pressed as a result
	{
		first = (first/3)*4 + first % 3;
		_grid[first] = _alphabet[choice];
		_grid[first+1] = (_leftRightOk[choice])[_rnd.Next(5)];
		_gridSet[first]=true;
		_gridSet[first+1]=true;
		second = first+1;
		curAlpha = curAlpha.Remove(curAlpha.IndexOf(_grid[first+1]),1);
	} else {    // Second letter goes below first, chooses it so it is pressed as a result.
		first -= 9;
		_grid[first] = _alphabet[choice];
		_grid[first+4] = (_aboveBelowOk[choice])[_rnd.Next(5)];
		_gridSet[first]=true;
		_gridSet[first+4]=true;
		second = first+4;
		curAlpha = curAlpha.Remove(curAlpha.IndexOf(_grid[first+4]),1);
	}
	
	var _gridLoop = new int[]{0,1,2,3,4,5,6,7,8,9,10,11};
	_gridLoop = _gridLoop.Shuffle();   //randomize the order in which the grid tries to fill
	var debug =0;
	var options = "";
	var done = false;
	while(!_puzzleSet && debug<500)
	{
		for(var i=0; i<12;i++)
		{
			var temp = _gridLoop[i];
			if(_gridSet[temp])
			{
				continue;   //continue if this cell is already set
			}
			switch(_rnd.Next(4))  // Direction of adjacency that this cell looks for a filled cell: right,left,up,down
			{
				case 0:
					if(temp % 4 ==3)  // If cell to the right is not set
					{
						break;
					} else if(_gridSet[temp+1]) //if it is set
					{
						options = Intersect(_leftRightOk[_alphabet.IndexOf(_grid[temp+1])],curAlpha); //find options that would make cell temp be pressed and haven't been used
						if(options.Length==0) // If there are none, flag that we're stuck(ish) and break
						{
							done = true;
							break;
						}
						_grid[temp] = options[_rnd.Next(options.Length)];  //randomly choose from the options
						_gridSet[temp] = true;
						curAlpha = curAlpha.Remove(curAlpha.IndexOf(_grid[temp]),1); // update curAlpha to remove the letter we just used
					}
					break;
				case 1:
					if(temp % 4 ==0)
					{
						break;
					} else if(_gridSet[temp-1])
					{
						options = Intersect(_leftRightOk[_alphabet.IndexOf(_grid[temp-1])],curAlpha);
						if(options.Length==0)
						{
							done = true;
							break;
						}
						_grid[temp] = options[_rnd.Next(options.Length)];
						_gridSet[temp] = true;
						curAlpha = curAlpha.Remove(curAlpha.IndexOf(_grid[temp]),1);
					}
					break;
				case 2:
					if(temp < 4)
					{
						break;
					} else if(_gridSet[temp-4])
					{
						options = Intersect(_aboveBelowOk[_alphabet.IndexOf(_grid[temp-4])],curAlpha);
						if(options.Length==0)
						{
							done = true;
							break;
						}
						_grid[temp] = options[_rnd.Next(options.Length)];
						_gridSet[temp] = true;
						curAlpha = curAlpha.Remove(curAlpha.IndexOf(_grid[temp]),1);
					}
					break;
				case 3:
					if(temp >7)
					{
						break;
					} else if(_gridSet[temp+4])
					{
						options = Intersect(_aboveBelowOk[_alphabet.IndexOf(_grid[temp+4])],curAlpha);
						if(options.Length==0)
						{
							done = true;
							break;
						}
						_grid[temp] = options[_rnd.Next(options.Length)];
						_gridSet[temp] = true;
						curAlpha = curAlpha.Remove(curAlpha.IndexOf(_grid[temp]),1);
					}
					break;	
			}
		}
		if(_gridSet.All(x => x) || done) //if every single cell has a letter or if we flagged a problem
		{
			var x = first % 4;
          		var y = first / 4;
			//These check whether we got lucky and the first cell should be pressed - if so, flag the puzzle as fully set
            		if ((x > 0 && _leftRight[_alphabet.IndexOf(_grid[first])].Contains(_grid[first - 1]) || (x < 3 && _leftRight[_alphabet.IndexOf(_grid[first])].Contains(_grid[first + 1]))))
			{
                		_puzzleSet = true;
			}
		        if ((y > 0 && _aboveBelow[_alphabet.IndexOf(_grid[first])].Contains(_grid[first - 4]) || (y < 2 && _aboveBelow[_alphabet.IndexOf(_grid[first])].Contains(_grid[first + 4]))))
			{
		                _puzzleSet = true;
			}
			if(!_puzzleSet)   // If not, reset puzzle generation and try again
			{
				done = false;
				curAlpha = _alphabet.Remove(_alphabet.IndexOf(_grid[first]),1);
				curAlpha = curAlpha.Remove(curAlpha.IndexOf(_grid[second]),1);
				// Debug.LogFormat(curAlpha);
				debug++;
				for(var i=0;i<12;i++)
				{
					if(i == first || i == second)
					{
						continue;
					} else {
						_gridSet[i]=false;
						_grid[i] = ' ';
					}
				}
			}
		}
	}
	if(debug>= 500 || _grid.Contains(' ')) // Failsafe to just fill the grid with ABCDEFGHIJKL - unlikely to be reached
	{
		for(var i=0;i<12;i++)
		{
			_grid[i] = _alphabet[i];
		}
	}
        Debug.LogFormat("Initial grid:" + new String(_grid));
	// Debug.LogFormat("It took " + debug.ToString() + " loops");
	for(var i=0; i<12; i++) //remember original grid for autosolving
	{
		_origGrid[i] = _grid[i];
	}
	_grid = _grid.Shuffle(); // shuffle letters
        for (int i = 0; i < Buttons.Length; i++)  // Assign letters to labels on buttons
        {
            if (i == 0)
	    {
                Label.GetComponent<TextMesh>().text = _grid[i].ToString();
		_labels[0]=Label;
	    }
            else
            {
                var label = Instantiate(Label);    //great work Timwi
                label.name = "Label"+i;
                label.transform.parent = Buttons[i].transform;
                label.transform.localPosition = new Vector3(0, 0.0401f, 0);
                label.transform.localEulerAngles = new Vector3(90, 0, 0);
                label.transform.localScale = new Vector3(.01f, .01f, .01f);
                label.GetComponent<TextMesh>().text = _grid[i].ToString();
		_labels[i]=label;
            }

            var j = i;
            Buttons[i].OnInteract += delegate { Click(j); return false; };
            Buttons[i].GetComponent<MeshRenderer>().material = UnpushedButtonMaterial;
        }
        SubmitButton.OnInteract += delegate { Submit(); return false; };
    }

    private void Click(int i)  //interacts with button i
    {
	if (_selected == 13) //13 is nothing selected - select this button
	{
		_selected = i;
	        Lights[i].enabled=true;
	} else if(_selected == i) {  //unselect by clicking again
		_selected = 13;
		Lights[i].enabled=false;
	} else {     // click two different buttons to swap them
		Lights[i].enabled = true; 
		var temp = _labels[i].GetComponent<TextMesh>().text;
		_labels[i].GetComponent<TextMesh>().text = _labels[_selected].GetComponent<TextMesh>().text;
		_labels[_selected].GetComponent<TextMesh>().text = temp;
		var let = _grid[i];
		_grid[i]=_grid[_selected];
		_grid[_selected] = let;
		Lights[i].enabled = false;
		Lights[_selected].enabled=false;
		_selected=13;
	}
		
        Buttons[i].AddInteractionPunch(.1f);
        Audio.PlaySoundAtTransform("ClickIn", Buttons[i].transform);
    }

    private IEnumerator SubmitButtonCoroutine()   //Submit button animation
    {
        var origLocation = SubmitButton.transform.localPosition;
        for (int j = 0; j <= 2; j++)
        {
            SubmitButton.transform.localPosition = new Vector3(origLocation.x, origLocation.y - j / 400f, origLocation.z);
            yield return null;
        }
        yield return new WaitForSeconds(.05f);
        for (int j = 5; j >= 0; j--)
        {
            SubmitButton.transform.localPosition = new Vector3(origLocation.x, origLocation.y - j / 1000f, origLocation.z);
            yield return null;
        }
        _submitButtonCoroutineActive = false;
    }

    private void Submit()
    {
	var correct = true;   //assume correct to begin with
	for (int i = 0; i < 12; i++)
        {
            var x = i % 4;
            var y = i / 4;
            if (!((x > 0 && _leftRight[_alphabet.IndexOf(_grid[i])].Contains(_grid[i - 1])) || (x < 3 && _leftRight[_alphabet.IndexOf(_grid[i])].Contains(_grid[i + 1])) || (y > 0 && _aboveBelow[_alphabet.IndexOf(_grid[i])].Contains(_grid[i - 4])) || (y < 2 && _aboveBelow[_alphabet.IndexOf(_grid[i])].Contains(_grid[i + 4]))))
		{
                correct = false;    //If any button is in a state where it shouldn't be pressed, flag as false.
		Debug.LogFormat("Problem in button" + i);
		}
        }
        SubmitButton.AddInteractionPunch();
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, SubmitButton.transform);

        if (_isSolved)
            return;

        if (!_submitButtonCoroutineActive)
        {
            _submitButtonCoroutineActive = true;
            StartCoroutine(SubmitButtonCoroutine());
        }

        // Debug.LogFormat("[AdjacentLetters #{1}] You submitted:{0}", string.Join("", _pushed.Select((b, i) => (i % 4 == 0 ? "\n" : "") + string.Format(b ? "[{0}]" : " {0} ", _letters[i])).ToArray()), _moduleId);

        if (correct)
        {
            Module.HandlePass();
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
            _isSolved = true;
        }
        else
        {
            Module.HandleStrike();
        }
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} click 0 1 2 to click buttons 0 then 1 then 2, 0-indexed in reading order,  !{0} submit to submit the current configuration";
#pragma warning restore 414


IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.ToUpperInvariant().Trim();
	if (command == "SUBMIT")
	{
		yield return null;
		SubmitButton.OnInteract();
		yield return null;
	}
	if (command.StartsWith("CLICK "))
	{
		List<string> parameters = command.Split(new String[] { " " }, StringSplitOptions.RemoveEmptyEntries).ToList();
		parameters.RemoveAt(0);
		var junk = 0;
		if(parameters.All(x => Int32.TryParse(x, out junk) && Int32.Parse(x) < 12 && Int32.Parse(x) >= 0))
		{
			for(var i =0; i<parameters.Count; i++)
			{
				yield return null;
				Buttons[Int32.Parse(parameters[i])].OnInteract();
				yield return new WaitForSeconds(.3f);
			}
		} else {
			yield break;
		}
	} else {
		yield break;
	}
    }    


    IEnumerator TwitchHandleForcedSolve ()
    {
	yield return null;
        if(!_isSolved)
	{
		_selected=13;
		for(var i = 0; i<12; i++)
		{
			if(_grid[i] != _origGrid[i])
			{	
				Buttons[i].OnInteract();
				yield return new WaitForSeconds(.1f);
				Buttons[new String(_grid).IndexOf(_origGrid[i])].OnInteract();
			}
		}
		SubmitButton.OnInteract();
		yield return null;
	}
    }

}
