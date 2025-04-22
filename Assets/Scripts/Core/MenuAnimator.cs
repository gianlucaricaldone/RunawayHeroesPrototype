using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MenuAnimator : MonoBehaviour
{
    public CanvasGroup mainMenuGroup;
    public RectTransform titleRect;
    public Button[] menuButtons;
    public float animationDelay = 0.1f;
    public float fadeInTime = 0.5f;
    
    private void Start()
    {
        // Inizializza il menu come invisibile
        if (mainMenuGroup != null)
        {
            mainMenuGroup.alpha = 0f;
        }
        
        // Nascondi i pulsanti
        foreach (Button button in menuButtons)
        {
            CanvasGroup buttonGroup = button.GetComponent<CanvasGroup>();
            if (buttonGroup == null)
            {
                buttonGroup = button.gameObject.AddComponent<CanvasGroup>();
            }
            buttonGroup.alpha = 0f;
        }
        
        // Avvia l'animazione di entrata
        StartCoroutine(AnimateMenuIn());
    }
    
    private IEnumerator AnimateMenuIn()
    {
        // Fade in del pannello principale
        float time = 0;
        while (time < fadeInTime)
        {
            time += Time.deltaTime;
            if (mainMenuGroup != null)
            {
                mainMenuGroup.alpha = Mathf.Lerp(0, 1, time / fadeInTime);
            }
            yield return null;
        }
        
        // Anima il titolo
        if (titleRect != null)
        {
            Vector3 originalScale = titleRect.localScale;
            titleRect.localScale = Vector3.zero;
            time = 0;
            while (time < fadeInTime)
            {
                time += Time.deltaTime;
                titleRect.localScale = Vector3.Lerp(Vector3.zero, originalScale, time / fadeInTime);
                yield return null;
            }
        }
        
        // Fai apparire i pulsanti uno alla volta
        foreach (Button button in menuButtons)
        {
            CanvasGroup buttonGroup = button.GetComponent<CanvasGroup>();
            time = 0;
            while (time < fadeInTime)
            {
                time += Time.deltaTime;
                if (buttonGroup != null)
                {
                    buttonGroup.alpha = Mathf.Lerp(0, 1, time / fadeInTime);
                }
                yield return null;
            }
            yield return new WaitForSeconds(animationDelay);
        }
        
        // Avvia la pulsazione del titolo
        StartTitlePulsing();
    }
    
    // Metodo per animare l'uscita dal menu
    public void AnimateMenuOut(System.Action onComplete = null)
    {
        StartCoroutine(FadeOutMenu(onComplete));
    }
    
    private IEnumerator FadeOutMenu(System.Action onComplete)
    {
        float time = 0;
        while (time < fadeInTime)
        {
            time += Time.deltaTime;
            if (mainMenuGroup != null)
            {
                mainMenuGroup.alpha = Mathf.Lerp(1, 0, time / fadeInTime);
            }
            yield return null;
        }
        
        if (onComplete != null)
        {
            onComplete();
        }
    }
    
    // Metodo per far pulsare leggermente il titolo
    public void StartTitlePulsing()
    {
        if (titleRect != null)
        {
            StartCoroutine(PulseTitle());
        }
    }
    
    private IEnumerator PulseTitle()
    {
        Vector3 originalScale = titleRect.localScale;
        Vector3 targetScale = originalScale * 1.05f;
        
        while (true)
        {
            // Aumenta
            float time = 0;
            while (time < 1f)
            {
                time += Time.deltaTime;
                titleRect.localScale = Vector3.Lerp(originalScale, targetScale, Mathf.SmoothStep(0, 1, time));
                yield return null;
            }
            
            // Diminuisci
            time = 0;
            while (time < 1f)
            {
                time += Time.deltaTime;
                titleRect.localScale = Vector3.Lerp(targetScale, originalScale, Mathf.SmoothStep(0, 1, time));
                yield return null;
            }
        }
    }
}