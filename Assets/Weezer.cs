using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Rnd = UnityEngine.Random;

public class Weezer : MonoBehaviour
{

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

    void OnDestroy()
    { //Shit you need to do when the bomb ends

    }

    void Activate()
    { //Shit that should happen when the bomb arrives (factory)/Lights turn on

    }

    void Start()
    { //Shit that you calculate, usually a majority if not all of the module

        //pick four different random sounds to play
        for (int i = 0; i < 4; i++)
        {
            soundValid = false;
            while (!soundValid)
            {
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

    void Update()
    { //Shit that happens at any point after initialization

    }

    void Solve()
    {
        GetComponent<KMBombModule>().HandlePass();
    }

    void Strike()
    {
        GetComponent<KMBombModule>().HandleStrike();
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} play 1 2 3 4 [Presses the buttons to play those sounds.] !{0} submit 4 2 3 1 [Submit the buttons in the order 4 3 2 1.]";
#pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string Command)
    {
        Command = Command.Trim().ToLowerInvariant();
        var list = new List<int>();
        var cmds = Command.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (cmds[0] == "play")
        {
            if (cmds.Length < 2)
                yield break;
            for (int i = 1; i < cmds.Length; i++)
            {
                if (!"1234".Contains(cmds[i]))
                    yield break;
                list.Add(cmds[i][0] - '1');
            }
            yield return null;
            foreach (var btn in list)
            {
                buttons[btn].OnInteract();
                yield return new WaitForSeconds(3f);
            }
            yield break;
        }
        if (cmds.Length != 5 || cmds[0] != "submit")
            yield break;
        list.Add(4);
        for (int i = 1; i < 5; i++)
        {
            if (!"1234".Contains(cmds[i]))
                yield break;
            list.Add(cmds[i][0] - '1');
        }
        if (list.Distinct().Count() != 5)
        {
            yield return "sendtochaterror There are duplicate button presses. Command ignored.";
            yield break;
        }
        yield return null;
        foreach (var btn in list)
        {
            buttons[btn].OnInteract();
            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        yield return null;
        Solve(); // don't feel like making an autosolver for a module probably not worth enough points for it
    }
}
