using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlowMaterialPropertyBlock : MonoBehaviour
{
    [SerializeField] Vector4 noGlow;
    [SerializeField] Vector4 fullGlow;
    float Speed=0.3f;
    private Renderer _renderer;
    private MaterialPropertyBlock _propBlock;

    void Awake()
    {
        _propBlock = new MaterialPropertyBlock();
        _renderer = GetComponent<Renderer>();
    }

    public void Glow()
    {
        StartCoroutine(GlowRoutine(noGlow, fullGlow));

    }

    IEnumerator GlowRoutine(Vector4 glow1, Vector4 glow2, int timesToBlink=3)
    {
        _renderer.GetPropertyBlock(_propBlock);
        for (int i=timesToBlink-1; i>=0; i--)
        {
            bool done = false;
            float elapsedTime = 0f;
            // Assign our new value.
            while (!done)
            {
                _renderer.GetPropertyBlock(_propBlock);
                if (_propBlock.GetVector("_GlowStrength") == glow2)
                {
                    done = true;
                }
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp(elapsedTime / Speed, 0f, 1f);
                _propBlock.SetVector("_GlowStrength", Color.Lerp(glow1, glow2, t));
                _renderer.SetPropertyBlock(_propBlock);
                yield return null;
            }
            yield return new WaitForSeconds(0.1f);
            done = false;
            elapsedTime = 0f;
            // Assign our new value.
            while (!done)
            {
                _renderer.GetPropertyBlock(_propBlock);
                if (_propBlock.GetVector("_GlowStrength") == glow1)
                {
                    done = true;
                }
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp(elapsedTime / Speed, 0f, 1f);
                _propBlock.SetVector("_GlowStrength", Color.Lerp(glow2, glow1, t));
                _renderer.SetPropertyBlock(_propBlock);
                yield return null;
            }
        }

        // Apply the edited values to the renderer.

    }
    public void GlowUp()
    {
        Vector4 currentGlow = _propBlock.GetVector("_GlowStrength");
        StartCoroutine(GlowContinousRoutine(currentGlow, fullGlow));
    }
    public void GlowDown()
    {
        Vector4 currentGlow = _propBlock.GetVector("_GlowStrength");
        StartCoroutine(GlowContinousRoutine(currentGlow, noGlow));
    }
    IEnumerator GlowContinousRoutine(Vector4 glow1, Vector4 glow2)
    {
        bool done = false;
        float elapsedTime = 0f;
        // Assign our new value.
        while (!done)
        {
            _renderer.GetPropertyBlock(_propBlock);
            if (_propBlock.GetVector("_GlowStrength") == glow2)
            {
                done = true;
            }
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp(elapsedTime / Speed, 0f, 1f);
            _propBlock.SetVector("_GlowStrength", Color.Lerp(glow1, glow2, t));
            _renderer.SetPropertyBlock(_propBlock);
            yield return null;
        }
    }
}
