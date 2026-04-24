using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using System.Collections; 
using UnityEngine.Events;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("UI Teks & Tombol")]
    public TextMeshProUGUI questionText;
    public Button[] choiceButtons;
    public TextMeshProUGUI[] choiceTexts;
    
    [Header("UI Respon Sistem")]
    public GameObject responsePanel;
    public TextMeshProUGUI responseText;

    [Header("Referensi Sistem Pakar")]
    public GriefInferenceEngine inferenceEngine;

    [Header("Event Penghubung")]
    public UnityEvent OnQuestionFinished; 

    [HideInInspector] public float buttonDelayModifier = 0f;

    private bool isProcessingChoice = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void DisplayQuestion(QuestionData data)
    {
        isProcessingChoice = false; 
        responsePanel.SetActive(false); 
        
        questionText.text = data.mainQuestion; 

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (i < data.choices.Length)
            {
                choiceButtons[i].gameObject.SetActive(true); 
                choiceTexts[i].text = data.choices[i].choiceText; 

                choiceButtons[i].onClick.RemoveAllListeners();
                
                int index = i; 
                choiceButtons[i].onClick.AddListener(() => OnButtonClick(data.choices[index]));

                //tombol tidak bisa langsung diklik jika ada delay (bab 4)
                StartCoroutine(EnableButtonWithDelay(choiceButtons[i], buttonDelayModifier));
            }
            else
            {
                choiceButtons[i].gameObject.SetActive(false);
            }
        }
    }

    //buat bab 4
    private IEnumerator EnableButtonWithDelay(Button btn, float delay)
    {
        btn.interactable = false; //kunci tombol
        yield return new WaitForSeconds(delay); //tunggu (0 detik jika normal, 2 detik jika Bab 4)
        btn.interactable = true; //buka kunci
    }

    private void OnButtonClick(Choice selectedChoice)
    {
        if (isProcessingChoice) return;
        isProcessingChoice = true;

        //kirim pilihan ke Otak
        inferenceEngine.ProcessPlayerChoice(selectedChoice);

        //munculkan respon dari tombol yang dipilih
        StartCoroutine(ShowSystemResponseRoutine(selectedChoice.systemResponse));
    }

    private IEnumerator ShowSystemResponseRoutine(string responseMsg)
    {
        foreach (Button btn in choiceButtons) btn.interactable = false;

        if (!string.IsNullOrEmpty(responseMsg))
        {
            responsePanel.SetActive(true);
            responseText.text = responseMsg;
            yield return new WaitForSeconds(5f); 
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
        }

        //memuat soal berikutnya
        if (OnQuestionFinished != null)
        {
            OnQuestionFinished.Invoke();
        }
    }

    public void ShowSystemIntervention(string msg, float duration = 3.5f)
    {
        StartCoroutine(InterventionRoutine(msg, duration));
    }

    private IEnumerator InterventionRoutine(string msg, float duration)
    {
        isProcessingChoice = true;    
        responsePanel.SetActive(true);
        responseText.text = msg;
        yield return new WaitForSeconds(duration);
        responsePanel.SetActive(false);
        isProcessingChoice = false;
    }
}