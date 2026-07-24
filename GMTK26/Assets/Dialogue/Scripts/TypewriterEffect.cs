using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class TypewriterEffect : MonoBehaviour
{
	TMP_Text _tmpProText;
	string writer;
	string currentText;

	[SerializeField] float delayBeforeStart = 0f;
	[SerializeField] float delayAfterEnd = 0f;
	[SerializeField] float timeBtwChars = 0.1f;
	[SerializeField] float timeBtwWords = 0.1f;
	[SerializeField] string leadingChar = "";
	[SerializeField] float forceNewlineWidth = 1110;
	[SerializeField] bool leadingCharBeforeDelay = false;
	[SerializeField] bool skippable = true;
	[Header("Speech Sounds")]
	[SerializeField] AudioClip speechSound;
	[SerializeField] float speechVolume = 1f;
	[SerializeField] float speechPitch = 1;
	[SerializeField] float speechPitchRandomizationRange = 0.25f;

	public bool fromDM = false;

	Coroutine typewriterCoroutine;
	Coroutine timeWaserCoroutine;

	private Action OnCompletedCallback;

	// Use this for initialization
	void Start()
	{
		_tmpProText = GetComponent<TMP_Text>()!;

		if (_tmpProText != null)
		{
			_tmpProText.text = "";
		}
	}
    private void Awake()
    {
		_tmpProText = GetComponent<TMP_Text>()!;
		fromDM = false;
	}

    public void NewText(string newText, Action completedCallback)
    {
	    StopAllCoroutines();
		_tmpProText = GetComponent<TMP_Text>()!;
		currentText = ManualTextWrapping(newText, _tmpProText.font, _tmpProText.fontSize, _tmpProText.fontStyle);
		OnCompletedCallback = completedCallback;
		writer = currentText;
		_tmpProText.text = "";
        // if (fromDM) { DialogueManager.newDialogueStarted = true; }
		Keyboard.current.onTextInput += SkipText;
		typewriterCoroutine = StartCoroutine("TypeWriterTMP");
	}

    private void FinishText()
    {
	    StopAllCoroutines();
	    typewriterCoroutine = null;
	    timeWaserCoroutine = null;
	    OnCompletedCallback?.Invoke();
    }

	private void OnDestroy()
	{
		Keyboard.current.onTextInput -= SkipText;
	}

	public void ChangeSoundSettings(AudioClip newSpeechSound, float newSpeechVolume, float newSpeechPitch, float newSpeechPitchRandomizationRange)
    {
		speechSound = newSpeechSound;
		speechVolume = newSpeechVolume;
		speechPitch = newSpeechPitch;
		speechPitchRandomizationRange = newSpeechPitchRandomizationRange;
	}

	public void SkipText(char ch)
    {
		if (skippable)
		{
			if (typewriterCoroutine != null)
			{
				StopCoroutine(typewriterCoroutine);
				typewriterCoroutine = null;
			}

			if (delayAfterEnd >= 0)
			{
				timeWaserCoroutine = StartCoroutine("TimeWaster");
			}
			else
			{
				Keyboard.current.onTextInput -= SkipText;
				_tmpProText.text = currentText;
				FinishText();
			}
		}
	}

	IEnumerator TypeWriterTMP()
	{
		// if (fromDM) { DialogueManager.newDialogueStarted = true; }
		_tmpProText.text = leadingCharBeforeDelay ? leadingChar : "";
		string styleMarker = "";
		bool changingStyle = false;

		yield return new WaitForSeconds(delayBeforeStart);

		foreach (char c in writer)
		{
			if (_tmpProText.text.Length > 0)
			{
				_tmpProText.text = _tmpProText.text.Substring(0, _tmpProText.text.Length - leadingChar.Length);
			}
			if(c == '<')
            {
				styleMarker += c;
				changingStyle = true;
            }
			else if (c == '>')
            {
				changingStyle = false;
				styleMarker += c;
				_tmpProText.text += styleMarker;
				styleMarker = "";
			}
			
            else if (!changingStyle)
            {
				_tmpProText.text += c;
				_tmpProText.text += leadingChar;
				if(c != ' ')
                {
					// if(speechSound != null) { GameManager.SpawnAudio(speechSound, speechVolume, speechPitch + Random.Range(-speechPitchRandomizationRange, speechPitchRandomizationRange), this.transform.position); }
					yield return new WaitForSeconds(timeBtwChars);
				}
                else
                {
					yield return new WaitForSeconds(timeBtwWords);
				}

			}
            else
            {
				styleMarker += c;
			}
		}

		if (leadingChar != "")
		{
			_tmpProText.text = _tmpProText.text.Substring(0, _tmpProText.text.Length - leadingChar.Length);
		}

		yield return new WaitForSeconds(delayAfterEnd);
		// if (fromDM) { DialogueManager.newDialogueStarted = false; }
		Keyboard.current.onTextInput -= SkipText;

		typewriterCoroutine = null;
		FinishText();
	}
	IEnumerator TimeWaster()
	{
		Keyboard.current.onTextInput -= SkipText;
		_tmpProText.text = currentText;
		yield return new WaitForSeconds(delayAfterEnd);
		// if (fromDM) { DialogueManager.newDialogueStarted = false; }

		timeWaserCoroutine = null;
		FinishText();
	}


	public string ManualTextWrapping(string text, TMP_FontAsset fontAsset, float fontSize, FontStyles style)
		{
		// Compute scale of the target point size relative to the sampling point size of the font asset.
		float pointSizeScale = fontSize / (fontAsset.faceInfo.pointSize * fontAsset.faceInfo.scale);
		float emScale = fontSize * 0.01f;

		float styleSpacingAdjustment = (style & FontStyles.Bold) == FontStyles.Bold ? fontAsset.boldSpacing : 0;
		float normalSpacingAdjustment = fontAsset.normalSpacingOffset;
		float tempNewlineWidthLimit = forceNewlineWidth;
		float width = _tmpProText.margin.x + _tmpProText.margin.z;
		bool skipCharacters = false;
		string currentVal = "";

		for (int i = 0; i < text.Length; i++)
		{
			char unicode = text[i];
			TMP_Character character;
			// Make sure the given unicode exists in the font asset.
			currentVal += unicode;
			if (fontAsset.characterLookupTable.TryGetValue(unicode, out character))
			{
				if (unicode == '<')
				{
					skipCharacters = true;
				}
				else if (unicode == '>')
				{
					skipCharacters = false;
				}
				if (!skipCharacters) { width += character.glyph.metrics.horizontalAdvance * pointSizeScale + (styleSpacingAdjustment + normalSpacingAdjustment) * emScale; }
			}

			if (width >= tempNewlineWidthLimit)
            {
				width = _tmpProText.margin.x + _tmpProText.margin.z;
				if(unicode == ' ')
                {
					currentVal = currentVal.Substring(0, currentVal.Length - 2) + "\n";
                }
                else 
				{ 
					string newBreakText = currentVal.LastIndexOf(' ') > 0 && currentVal.LastIndexOf(' ') + 1  < currentVal.Length ? currentVal.Substring(currentVal.LastIndexOf(' ') + 1) : "";
					for (int j = 0; i < newBreakText.Length; i++)
					{
						unicode = newBreakText[j];
						// Make sure the given unicode exists in the font asset.
						if (fontAsset.characterLookupTable.TryGetValue(unicode, out character))
						{
							if (unicode == '<')
							{
								skipCharacters = true;
							}
							else if (unicode == '>')
							{
								skipCharacters = false;
							}
							if (!skipCharacters) { width += character.glyph.metrics.horizontalAdvance * pointSizeScale + (styleSpacingAdjustment + normalSpacingAdjustment) * emScale; }
						}
					}
					if(currentVal.LastIndexOf(' ') > 0)
					{
						currentVal = currentVal.Substring(0, currentVal.LastIndexOf(' ') + 1) + "\n" + newBreakText;
					}
				}
			}
		}
		return currentVal;
	}
}
