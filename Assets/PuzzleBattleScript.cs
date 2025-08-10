using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class PuzzleBattleScript : MonoBehaviour
{
    static int _moduleIdCounter = 1;
    int _moduleID = 0;

    public KMBombModule Module;
    public KMBombInfo Bomb;
    public KMAudio Audio;
    public KMSelectable[] CardSelectables;
    public KMSelectable SubmitButton;
    public GameObject[] Highlights;
    public SpriteRenderer SubmitButtonRend;
    public SpriteRenderer[] CardRends;
    public Sprite[] AllCardBacks;
    public Sprite[] AllSubmitButtonSprites;
    public GameObject[] Fingers;
    public SpriteRenderer[] SpeechBubbles;
    public Sprite[] AllSpeechBubbleSprites;
    public GameObject FindTheSpade;

    private Puzzle Puzzle;
    private int SelectionIx = -1;
    private List<float> CardInitXs = new List<float>();
    private bool CannotPress = true;
    private bool Suspense;
    private static readonly string[] Ordinals = new[] { "first", "second", "third", "fourth" };

    private Sprite FindCardBack(int id)
    {
        foreach (var back in AllCardBacks)
            if (back.name == "card sheet " + id)
                return back;
        return null;
    }

    void Awake()
    {
        _moduleID = _moduleIdCounter++;

        var puzzleGenerator = new PuzzleGenerator();
        Puzzle = puzzleGenerator.Puzzle;

        Debug.LogFormat("[Puzzle Battle #{0}] Selected rules:\n{1}", _moduleID, Puzzle.Rules.Select((x, ix) => (ix + 1) + ". " + x.Name).Join("\n"));
        Debug.LogFormat("[Puzzle Battle #{0}] The cards are as follows: {1}.", _moduleID, Puzzle.Cards.Join(", "));
        Debug.LogFormat("[Puzzle Battle #{0}] Select the {1} card.", _moduleID, Ordinals[Array.IndexOf(Puzzle.Cards, Suit.Spade)]);

        for (int i = 0; i < CardSelectables.Length; i++)
        {
            int x = i;
            CardSelectables[x].OnInteract += delegate { if (!CannotPress) CardSelect(x); return false; };
            CardSelectables[x].OnHighlight += delegate { Highlights[x].SetActive(true); };
            CardSelectables[x].OnHighlightEnded += delegate { Highlights[x].SetActive(false); };
            Highlights[x].SetActive(false);
            CardRends[x].sprite = FindCardBack(Puzzle.Rules[x].ID);
            Fingers[x].SetActive(false);
            SpeechBubbles[x].gameObject.SetActive(false);
            CardInitXs.Add(CardSelectables[x].transform.localPosition.x);
        }

        SubmitButton.OnInteract += delegate { if (!CannotPress) SubmitButtonPress(); return false; };
        SubmitButton.OnHighlight += delegate { if (!Suspense) SubmitButtonRend.sprite = AllSubmitButtonSprites[1]; };
        SubmitButton.OnHighlightEnded += delegate { if (!Suspense) SubmitButtonRend.sprite = AllSubmitButtonSprites[0]; };

        for (int i = 0; i < CardSelectables.Length; i++)
            CardSelectables[i].transform.localPosition = new Vector3(CardInitXs.Last(), CardSelectables[i].transform.localPosition.y, CardSelectables[i].transform.localPosition.z);

        Module.OnActivate += delegate { StartCoroutine(IntroAnim()); };
    }

    void Start()
    {
        SubmitButton.transform.localScale = Vector3.zero;
    }

    private void CardSelect(int pos)
    {
        CardSelectables[pos].AddInteractionPunch();
        Audio.PlaySoundAtTransform("select", CardSelectables[pos].transform);
        for (int i = 0; i < Fingers.Length; i++)
            Fingers[i].SetActive(false);
        if (SelectionIx != pos)
        {
            SelectionIx = pos;
            Fingers[pos].SetActive(true);
            SubmitButton.transform.localScale = Vector3.one;
        }
        else
        {
            SelectionIx = -1;
            SubmitButton.transform.localScale = Vector3.zero;
        }
    }

    private void SubmitButtonPress()
    {
        SubmitButton.AddInteractionPunch();
        CannotPress = true;
        Suspense = true;
        SubmitButtonRend.sprite = AllSubmitButtonSprites[2];
        StartCoroutine(HandleSubmit());
    }

    private IEnumerator HandleSubmit()
    {
        var correct = SelectionIx == Array.IndexOf(Puzzle.Cards, Suit.Spade);
        if (correct)
            Debug.LogFormat("[Puzzle Battle #{0}] You selected the {1} card, which was correct. Module solved!", _moduleID, Ordinals[SelectionIx]);
        else
            Debug.LogFormat("[Puzzle Battle #{0}] You selected the {1} card, which was incorrect. Strike!", _moduleID, Ordinals[SelectionIx]);

        Audio.PlaySoundAtTransform("submit", SubmitButton.transform);
        yield return new WaitForSeconds(0.1f);
        SubmitButtonRend.sprite = AllSubmitButtonSprites[0];
        yield return new WaitForSeconds(0.3f);
        SubmitButton.transform.localScale = Vector3.zero;
        yield return new WaitForSeconds(0.6f);

        for (int i = 0; i < Fingers.Length; i++)
            Fingers[i].SetActive(false);
        SelectionIx = -1;

        Suspense = false;

        if (correct)
        {
            Module.HandlePass();
            Audio.PlaySoundAtTransform("solve", transform);
            FindTheSpade.SetActive(false);
            yield return "solve";

            yield return new WaitForSeconds(0.25f);
            for (int i = 0; i < SpeechBubbles.Length; i++)
            {
                yield return new WaitForSeconds(0.5f);
                SpeechBubbles[i].sprite = AllSpeechBubbleSprites[(int)Puzzle.Cards[i]];
                SpeechBubbles[i].gameObject.SetActive(true);
                Audio.PlaySoundAtTransform("speech bubble", SpeechBubbles[i].transform);
            }
        }
        else
        {
            Module.HandleStrike();
            Audio.PlaySoundAtTransform("strike", transform);
            yield return "strike";
            CannotPress = false;
        }
    }

    private IEnumerator IntroAnim(float duration = 0.25f)
    {
        yield return new WaitForSeconds(0.1f);
        StartCoroutine(PlayDealSounds());
        float timer = 0;
        while (timer < duration)
        {
            yield return null;
            timer += Time.deltaTime;
            CardSelectables[0].transform.localPosition = new Vector3(Mathf.Lerp(CardInitXs.Last(), CardInitXs[0], timer / duration), CardSelectables[0].transform.localPosition.y, CardSelectables[0].transform.localPosition.z);
            CardSelectables[1].transform.localPosition = new Vector3(Mathf.Lerp(CardInitXs.Last(), CardInitXs[1], Mathf.Clamp01(timer / (duration * (2f / 3)))), CardSelectables[1].transform.localPosition.y, CardSelectables[1].transform.localPosition.z);
            CardSelectables[2].transform.localPosition = new Vector3(Mathf.Lerp(CardInitXs.Last(), CardInitXs[2], Mathf.Clamp01(timer / (duration * (1f / 3)))), CardSelectables[2].transform.localPosition.y, CardSelectables[2].transform.localPosition.z);
        }

        CardSelectables[0].transform.localPosition = new Vector3(CardInitXs[0], CardSelectables[0].transform.localPosition.y, CardSelectables[0].transform.localPosition.z);
        CardSelectables[1].transform.localPosition = new Vector3(CardInitXs[1], CardSelectables[1].transform.localPosition.y, CardSelectables[1].transform.localPosition.z);
        CardSelectables[2].transform.localPosition = new Vector3(CardInitXs[2], CardSelectables[2].transform.localPosition.y, CardSelectables[2].transform.localPosition.z);

        CannotPress = false;
    }

    private IEnumerator PlayDealSounds()
    {
        Audio.PlaySoundAtTransform("deal", CardSelectables[2].transform);
        yield return new WaitForSeconds(0.1f);
        Audio.PlaySoundAtTransform("deal", CardSelectables[1].transform);
        yield return new WaitForSeconds(0.1f);
        Audio.PlaySoundAtTransform("deal", CardSelectables[0].transform);
    }

#pragma warning disable 414
    private string TwitchHelpMessage = "Use '!{0} 3s' to select the third card, then press \"OK!\".";
#pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.ToLowerInvariant();
        string validcmds = "1234s";

        for (int i = 0; i < command.Length; i++)
        {
            if (!validcmds.Contains(command[i]))
            {
                yield return "sendtochaterror Invalid command.";
                yield break;
            }
        }
        yield return null;

        for (int i = 0; i < command.Length; i++)
        {
            if (!CannotPress && !Suspense)
            {
                if (command[i] == 's')
                    SubmitButton.OnInteract();
                else
                    CardSelectables[int.Parse(command[i].ToString()) - 1].OnInteract();
                yield return new WaitForSeconds(0.2f);
            }
        }
    }
    IEnumerator TwitchHandleForcedSolve()
    {
        var answer = Array.IndexOf(Puzzle.Cards, Suit.Spade);
        if (SelectionIx != answer)
            CardSelectables[answer].OnInteract();
        yield return null;
        SubmitButton.OnInteract();
    }
}
