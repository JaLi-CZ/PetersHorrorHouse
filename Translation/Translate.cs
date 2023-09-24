using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Translate : MonoBehaviour
{
    public static List<Translate> elements = new List<Translate>();

    public int translationId;
    public int[] optionTranslationIds = null;

    void Start()
    {
        elements.Add(this);
        UpdateElementLanguage();
    }

    private void UpdateElementLanguage()
    {
        if (optionTranslationIds == null || optionTranslationIds.Length == 0)
            GetComponent<Text>().text = Translator.GetText(translationId);
        else
        {
            Dropdown dropdown = GetComponent<Dropdown>();
            var options = dropdown.options;
            for (int i = 0; i < options.Count; i++) options[i].text = Translator.GetText(optionTranslationIds[i]);
            dropdown.RefreshShownValue();
        }
    }

    public static void UpdateAllElements()
    {
        List<Translate> elementsToRemove = new List<Translate>();
        foreach (Translate element in elements)
        {
            if (element == null) elementsToRemove.Add(element);
            else element.UpdateElementLanguage();
        }
        foreach (Translate element in elementsToRemove) elements.Remove(element);
    }
}
