using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : Singleton<EventManager>
{
    public delegate void GemDestroyed();
    public static event GemDestroyed GemDestroy;
    public delegate void SliderFull(string progressGlyphBinding);
    public static event SliderFull sliderFull;
    public delegate void SliderFullVoid();
    public static event SliderFullVoid sliderFullVoid;
    public delegate void AbilitySupercharged(string gaugeValue);
    public static event AbilitySupercharged AbilitySupercharge;
    public delegate void GaugeDepleted(string gaugeValue);
    public static event GaugeDepleted DepleteGauge;
    public delegate void SwitchingAllowed(bool switchingAllowed);
    public static event SwitchingAllowed SwitchAllow;
    public delegate void ZeroCombinationsReached();
    public static event ZeroCombinationsReached ZeroCombinations;
    public delegate void GlyphUsed(string progressGlyphType);
    public static event GlyphUsed GlyphUse;
    public delegate void GlyphUsedVoid();
    public static event GlyphUsedVoid GlyphUseVoid;
    public delegate void TurnDone(int currentTurn);
    public static event TurnDone Turn;
    // Start is called before the first frame update

    public static void OnGemDestroyed()
    {
        if (GemDestroy != null)
        {
            GemDestroy();
        }

    }
    public static void OnSliderFull(string progressGlyphType)
    {
        if (sliderFull != null)
        {
            sliderFull(progressGlyphType);
            
        }
        if (sliderFullVoid != null)
        {
            sliderFullVoid();
        }

    }

    public static void OnAbilitySupercharged(string gaugeValue)
    {
        if (AbilitySupercharge != null)
        {

            AbilitySupercharge(gaugeValue);
        }
    }

    public static void OnTurnDone(int currentTurn)
    {
        if (Turn != null)
        {

            Turn(currentTurn);
        }
    }

    public static void OnDepleteGauge(string gaugeValue)
    {
        if (DepleteGauge != null)
        {

            DepleteGauge(gaugeValue);
        }
    }

    public static void OnSwitchingAllowed(bool switchingAllowed)
    {
        if (SwitchAllow != null)
        {

            SwitchAllow(switchingAllowed);
        }
    }

    public static void OnZeroCombinationsReached()
    {
        if (ZeroCombinations != null)
        {
            ZeroCombinations();
        }
    }
    public static void OnGlyphUsed(string progressGlyphType)
    {
        if (GlyphUse != null)
        {
            GlyphUse(progressGlyphType);
        }
        if (GlyphUseVoid != null)
        {
            GlyphUseVoid();
        }

    }
}
