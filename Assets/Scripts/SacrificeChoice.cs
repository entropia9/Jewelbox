using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using System.Linq;

[RequireComponent(typeof(RectXformMover))]
public class SacrificeChoice : MonoBehaviour
{
    [Serializable]
    public struct SpecialAbilityImage
    {
        public Gauge gaugeToDisable;
        public Sprite image;
    }
    public SpecialAbilityImage[] pictures;
    List<Glyph> glyphs;
    List<Gauge> disabledGauges;
    [SerializeField] Button SpecialAbilityChoice;
    [SerializeField] Button GlyphChoice;
    [SerializeField] Button TileDiscardChoice;
    Gauge gaugeToDisable;
    Glyph glyphToDisable;
    MessageWindow messageWindow;
    string initialMessageText;
    [Serializable]
    public struct DiscardPattern
    {
        public Sprite image;
        public int[] x;
        public int[] y;
    }
    public DiscardPattern[] patterns;
    List<DiscardPattern> usedPatterns;
    DiscardPattern currentDiscardPattern;
    // Start is called before the first frame update
    void Start()
    {
        messageWindow = GetComponent<MessageWindow>();
        disabledGauges = new List<Gauge>();
        usedPatterns = new List<DiscardPattern>();
        glyphs = FindObjectOfType<GlyphManager>().glyphList;
    }

    public void FindOptionsToChooseFrom()
    {
        initialMessageText = this.GetComponent<MessageWindow>().messageText.text;
        int indexForGauge;
        int indexForDP;
        if (disabledGauges.Count == pictures.Length)
        {
            SpecialAbilityChoice.gameObject.SetActive(false);
        }
        else
        {
            do
            {
                indexForGauge = UnityEngine.Random.Range(0, pictures.Length);
                gaugeToDisable = pictures[indexForGauge].gaugeToDisable;
                SpecialAbilityChoice.image.sprite = pictures[indexForGauge].image;
            } while (disabledGauges.Contains(gaugeToDisable));
        }
        if (glyphs.Count == 0)
        {
            GlyphChoice.gameObject.SetActive(false);
        }
        else
        {
            glyphToDisable = glyphs[glyphs.Count - 1];
        }

        if (usedPatterns.Count == patterns.Length)
        {
            TileDiscardChoice.gameObject.SetActive(false);

        }
        else
        {
            do
            {
                indexForDP = UnityEngine.Random.Range(0, patterns.Length);
                currentDiscardPattern = patterns[indexForDP];
                TileDiscardChoice.image.sprite = patterns[indexForDP].image;
            } while (usedPatterns.Contains(currentDiscardPattern));
        }

    }

    public void ChooseAbility()
    {   
        disabledGauges.Add(gaugeToDisable);
        gaugeToDisable.DisableGauge();
        this.GetComponent<RectXformMover>().MoveOff();
        Time.timeScale = 1;
    }

    public void ChooseGlyph()
    {
        glyphs.Remove(glyphToDisable);
        glyphToDisable.DisableGlyph();
        this.GetComponent<RectXformMover>().MoveOff();
        Time.timeScale = 1;
    }

    public void ChooseDiscardPattern()
    {
        usedPatterns.Add(currentDiscardPattern);
        this.GetComponent<RectXformMover>().MoveOff();
        Time.timeScale = 1;
    }

    public void HoverAbility()
    {
        if (gaugeToDisable != null)
        {
            messageWindow.messageText.text = "Disable " + gaugeToDisable.gaugeValue + "ability";
        }
       
    }

    public void HoverGlyph()
    {
        messageWindow.messageText.text = "Disable last Save Glyph";
    }
    public void HoverDisardPattern()
    {
       messageWindow.messageText.text = "Disable Tiles shown on the picture";
    }

    public void StopHovering()
    {
        messageWindow.messageText.text = initialMessageText;
    }
}
