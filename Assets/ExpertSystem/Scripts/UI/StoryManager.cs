using UnityEngine;
using System.Collections.Generic;

public class StoryManager : MonoBehaviour
{
    [Header("Data Utama")]
    public GriefState globalState; 

    [Header("Daftar Pertanyaan (Satu Bab)")]
    public List<QuestionData> chapterQuestions; 

    private int currentQuestionIndex = 0;

    void Start()
    {
        if (globalState != null)
        {
            globalState.ResetData();
        }

        //set indeks ke 0 dan muat soal pertama
        currentQuestionIndex = 0;
        LoadCurrentQuestion();
    }

    //menyuruh UIManager memutar kaset
    public void LoadCurrentQuestion()
    {
        //cek apakah soal masih ada
        if (currentQuestionIndex < chapterQuestions.Count)
        {
            QuestionData soalSekarang = chapterQuestions[currentQuestionIndex];
            UIManager.Instance.DisplayQuestion(soalSekarang);
        }
        else
        {
            //jika soal di list sudah habis
            Debug.Log("STORY MANAGER: Pertanyaan di bab ini sudah habis!");
        }
    }

    public void NextQuestion()
    {
        currentQuestionIndex++; //soal berikutnya
        LoadCurrentQuestion();  //muat ke layar
    }
}