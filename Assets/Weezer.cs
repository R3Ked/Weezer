using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;
using Math = ExMath;
using System.Security.Cryptography;

public class Weezer : MonoBehaviour {

    public KMBombInfo Bomb;
    public KMAudio Audio;

    static int ModuleIdCounter = 1;
    int ModuleId;
    private bool ModuleSolved = false;

    public KMSelectable[] buttons;

    int[] soundsChosen = { -1, -1, -1, -1 };
    int[] order = { -1, -1, -1, -1 };
    int randomSound;
    bool soundValid = false;

    int firstButtonNotAssigned = 1; //hey, i could've named this variable bibblybeebly. it could always be worse

    bool inSubmissionMode = false;
    int stage = 1;

    public SpriteRenderer[] bandHighlights;
    public Sprite highlighted;
    public Sprite notHighlighted;

    void Awake()
    { //Avoid doing calculations in here regarding edgework. Just use this for setting up buttons for simplicity.
        ModuleId = ModuleIdCounter++;

        GetComponent<KMBombModule>().OnActivate += Activate;
        foreach (KMSelectable Button in buttons)
        {
            Button.OnInteract += delegate () { ButtonPress(Button); return false; };

            //workaround for having highlights on the band members
            //this hack sucks and is bad and terrible i hate this
            //thanks to redpenguin for telling me how to do this but also why do i even have to do this in the first place
            if (!(Button == buttons[4]))
            {
                Button.OnHighlight += delegate () { Button.GetComponent<SpriteRenderer>().sprite = highlighted; };
                Button.OnHighlightEnded += delegate () { Button.GetComponent<SpriteRenderer>().sprite = notHighlighted; };
            }
        }
    }
    void ButtonPress(KMSelectable Button)
    {
        Button.AddInteractionPunch();

        //check if it's the submit button or not
        if (Button != buttons[4])
        {
            //make sure we don't play the audio clips if in submission mode or module solved (souvenir stuff i guess)
            if (inSubmissionMode || ModuleSolved)
            {
                Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Button.transform);
                if (!ModuleSolved)
                {
                    //check what button you pressed
                    for (int i = 0; i < 4; i++)
                    {
                        if (Button == buttons[i])
                        {
                            Debug.LogFormat("[Weezer #{0}] You pressed button {1}.", ModuleId, i + 1);
                            //check if the button is right
                            if (stage == order[i])
                            {
                                Debug.LogFormat("[Weezer #{0}] That's correct.", ModuleId);
                                stage++;
                                //check if we've pressed everything right
                                if (stage == 5)
                                {
                                    //solve stuff
                                    Audio.PlaySoundAtTransform("riff", Button.transform);
                                    Solve();
                                    Debug.LogFormat("[Weezer #{0}] Module solved.", ModuleId);
                                    ModuleSolved = true;
                                }
                            }
                            else
                            {
                                //strike stuff
                                Strike();
                                Debug.LogFormat("[Weezer #{0}] That's incorrect. Strike.", ModuleId);
                                inSubmissionMode = false;
                                stage = 1;
                            }
                        }
                    }
                }
            } //that was pretty spaghetti but who cares tbh
            else
            {
                //find which button you pressed and play the corresponding sound
                for (int i = 0; i < 4; i++)
                {
                    if (Button == buttons[i])
                    {
                        Audio.PlaySoundAtTransform(soundsChosen[i].ToString(), Button.transform);
                    }
                }
            }
        }
        else
        {
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Button.transform);
            //check if we're in submission mode or not
            if (!inSubmissionMode)
            {
                inSubmissionMode = true;
                Debug.LogFormat("[Weezer #{0}] Entering submission mode.", ModuleId);
            }
            else
            {
                Debug.LogFormat("[Weezer #{0}] You're already in submission mode lol", ModuleId);
            }
        }
    }

    void OnDestroy () { //Shit you need to do when the bomb ends
      
    }

    void Activate () { //Shit that should happen when the bomb arrives (factory)/Lights turn on

    }

    void Start () { //Shit that you calculate, usually a majority if not all of the module

        //pick four different random sounds to play
        for (int i = 0; i < 4; i++)
        {
            soundValid = false;
            while (!soundValid) {
                randomSound = Rnd.Range(1, 23);
                if (!soundsChosen.Contains(randomSound))
                {
                    soundsChosen[i] = randomSound;
                    soundValid = true;
                }
            }
        }
        Debug.LogFormat("[Weezer #{0}] Using sounds {1}, {2}, {3}, and {4}.", ModuleId, soundsChosen[0], soundsChosen[1], soundsChosen[2], soundsChosen[3]);

        //figure out what order to press the buttons in
        for (int i = 1; i < 23; i++)
        {
            if (soundsChosen.Contains(i))
            {
                order[Array.IndexOf(soundsChosen, i)] = firstButtonNotAssigned;
                firstButtonNotAssigned++;
            }
        }
        Debug.LogFormat("[Weezer #{0}] The order you have to press the buttons in is {1} {2} {3} {4}", ModuleId, order[0], order[1], order[2], order[3]);

        //check to make sure that we didn't mess something up somewhere because i'm afraid of this breaking
        if (order.Contains(-1))
        {
            Debug.LogFormat("[Weezer #{0}] Well, that shouldn't have happened. Please report this to me. Automatically solving module.", ModuleId);
            Solve();
            ModuleSolved = true;
        }
    }

    void Update () { //Shit that happens at any point after initialization

    }

    void Solve () {
       GetComponent<KMBombModule>().HandlePass();
    }

    void Strike () {
       GetComponent<KMBombModule>().HandleStrike();
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Use !{0} play 1234 to play the sounds from left to right. Use !{0} submit 4231 to submit your answer.";
#pragma warning restore 414

    IEnumerator ProcessTwitchCommand (string Command) {
       yield return null;
    }

    IEnumerator TwitchHandleForcedSolve () {
      yield return null;
        Solve(); // don't feel like making an autosolver for a module probably not worth enough points for it
    }
}
