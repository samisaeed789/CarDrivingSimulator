using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TypingEffect : MonoBehaviour
{
    public Text textComponent;  // Reference to the Text component
    public string fullText;     // The full text to display
    public float typingSpeed = 0.05f;  // Typing speed (in seconds per character)

    private void OnEnable()
    {
        // Start the typing effect when the GameObject is enabled
        if (textComponent != null)
        {
            textComponent.text = "";  // Clear the text initially
            StartCoroutine(TypeText());
        }
    }

    private IEnumerator TypeText()
    {
        foreach (char letter in fullText)
        {
            textComponent.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
    }
}