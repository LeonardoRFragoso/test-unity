using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour {


	protected float maxValue;
    protected float minValue;
    protected float currentValue;
    protected RectTransform fillTransform;
    protected float fillMaxWidth;
    public Text barText;
    public bool reversed = false;


	public void SetInitialValues(float max,float min, float initial){
		fillTransform= transform.GetChild(0).GetComponent<RectTransform>();
		maxValue= max;
		minValue= min;
		currentValue= initial;
		fillMaxWidth= fillTransform.sizeDelta.x;

        if (barText != null)
        {
            barText.text = initial + "/" + maxValue;
        }
    }


    public void UpdateMaxValue(float val)
    {
        maxValue = val;
        UpdateBar(currentValue);
    }

	public virtual void UpdateBar(float newVal){

		currentValue= newVal;
		if(newVal<minValue){
			currentValue=minValue;
		}
        //		Debug.Log("Updte Lifebar "+((currentValue-minValue)/(maxValue-minValue)));
        if (!reversed)
        {
            fillTransform.sizeDelta = new Vector2(((currentValue - minValue) / (maxValue - minValue)) * fillMaxWidth, fillTransform.sizeDelta.y);

        }
        else
        {
            fillTransform.sizeDelta = new Vector2(fillMaxWidth - (((currentValue - minValue) / (maxValue - minValue)) * fillMaxWidth), fillTransform.sizeDelta.y);
        }


        if (fillTransform.sizeDelta.x < 0)
        {
            fillTransform.sizeDelta = new Vector2(0.01f, fillTransform.sizeDelta.y);
        }
        if (barText != null)
        {
            barText.text = currentValue + "/" + maxValue;
        }
	}

    #region TAREFA 2: Feedback Visual
    
    // Cache para flash effect
    private Image fillImage;
    private Color originalFillColor;
    private Coroutine flashCoroutine;
    
    /// <summary>
    /// TAREFA 2: Flash visual na barra para feedback (ex: energia insuficiente).
    /// </summary>
    /// <param name="flashColor">Cor do flash</param>
    /// <param name="duration">Duração do flash</param>
    public void Flash(Color flashColor, float duration)
    {
        // Cache do Image component se necessário
        if (fillImage == null && fillTransform != null)
        {
            fillImage = fillTransform.GetComponent<Image>();
            if (fillImage != null)
            {
                originalFillColor = fillImage.color;
            }
        }
        
        if (fillImage == null) return;
        
        // Cancela flash anterior se existir
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }
        
        flashCoroutine = StartCoroutine(FlashRoutine(flashColor, duration));
    }
    
    private IEnumerator FlashRoutine(Color flashColor, float duration)
    {
        if (fillImage == null) yield break;
        
        // Flash para a cor
        fillImage.color = flashColor;
        
        // Aguarda metade da duração
        yield return new WaitForSeconds(duration * 0.5f);
        
        // Interpola de volta para cor original
        float elapsed = 0f;
        float fadeTime = duration * 0.5f;
        
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            fillImage.color = Color.Lerp(flashColor, originalFillColor, elapsed / fadeTime);
            yield return null;
        }
        
        fillImage.color = originalFillColor;
        flashCoroutine = null;
    }
    
    #endregion
}
