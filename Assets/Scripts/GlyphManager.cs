using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class GlyphManager : Singleton<GlyphManager>
{
    [SerializeField] public List<Glyph> glyphList;
    Board m_board;
    // Start is called before the first frame update
    void Start()
    {
        m_board = FindObjectOfType<Board>();
        EventManager.ZeroCombinations += DestroyGlyph;
        
        for (int i = 0; i < glyphList.Count; i++)
        {
            glyphList[i].globalMultiplier *= ((float)i + 1.0f);
        }
    }
    
    void DestroyGlyph()
    {
        Glyph glyphtToDestroy = glyphList.FindLast(x => x.currentGlyphState.ToString() == "active");
        if (glyphtToDestroy != null)
        {
            m_board.m_globalMultiplier -= glyphtToDestroy.globalMultiplier;
            glyphtToDestroy.UseGlyph();
            EventManager.OnGlyphUsed(glyphtToDestroy.progressGlyphBinding.ToString());
        }
    }


    private void OnDestroy()
    {
        EventManager.ZeroCombinations -= DestroyGlyph;

    }
    public void RestartGlyphs()
    {
        foreach (Glyph glyph in glyphList)
        {
            glyph.RestartGlyph();
        }
    }
}
