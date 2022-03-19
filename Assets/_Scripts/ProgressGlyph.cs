using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ProgressGlyph : MonoBehaviour
{

    public int loopIndex=0;
    List<ProgressGlyphPart> progressGlyphParts;

    // Start is called before the first frame update
    void Start()
    {
        progressGlyphParts = GetComponentsInChildren<ProgressGlyphPart>().ToList();
        BindProgressPartsToGlyphs();
    }

    // Update is called once per frame
    void BindProgressPartsToGlyphs()
    {
        if (loopIndex == GameManager.Instance.m_currentLoop)
        {
            foreach (ProgressGlyphPart part in progressGlyphParts)
            {
                part.SubscribeToEvents();
            }
        }
    }
}
