using UnityEngine;
using UnityEngine.SceneManagement;

public static class GriefExpertSystem
{
    private const string DefaultAngerSceneName = "ES2";
    private const string DefaultAcceptanceSceneName = "ES5";

    private static GriefState sharedState;

    public static GriefState ResolveState(GriefState preferredState = null)
    {
        if (preferredState != null)
        {
            if (sharedState == null)
            {
                sharedState = preferredState;
            }
            else if (sharedState != preferredState)
            {
                CopyState(sharedState, preferredState);
                sharedState = preferredState;
            }

            return sharedState;
        }

        if (sharedState != null)
        {
            return sharedState;
        }

        sharedState = ScriptableObject.CreateInstance<GriefState>();
        sharedState.ResetData();

        return sharedState;
    }

    public static void ResetSharedState()
    {
        ResolveState(null).ResetData();
    }

    public static bool ProcessPlayerChoice(
        GriefState state,
        Choice selectedChoice,
        string angerSceneName = DefaultAngerSceneName,
        string acceptanceSceneName = DefaultAcceptanceSceneName
    )
    {
        GriefState currentState = ResolveState(state);

        if (currentState == null || selectedChoice == null)
        {
            Debug.LogWarning("Expert system did not receive a valid choice.");
            return false;
        }

        currentState.ModifyEmotion(
            ref currentState.distress,
            selectedChoice.addDistress
        );
        currentState.ModifyEmotion(
            ref currentState.denial,
            selectedChoice.addDenial
        );
        currentState.ModifyEmotion(
            ref currentState.rumination,
            selectedChoice.addRumination
        );
        currentState.ModifyEmotion(
            ref currentState.hope,
            selectedChoice.addHope
        );

        currentState.questionCount++;

        bool sceneLoaded = UpdateCategoryCounters(
            currentState,
            selectedChoice.category,
            acceptanceSceneName
        );

        if (sceneLoaded)
        {
            return true;
        }

        sceneLoaded = EvaluateRules(
            currentState,
            selectedChoice,
            angerSceneName
        );

        return sceneLoaded;
    }

    public static bool ProcessShoppingOutcome(
        GriefState state,
        ShoppingOutcome outcome,
        int purchasedItemCount,
        int requiredItemCount,
        string angerSceneName = DefaultAngerSceneName,
        string acceptanceSceneName = DefaultAcceptanceSceneName
    )
    {
        Choice shoppingChoice =
            CreateShoppingOutcomeChoice(
                outcome,
                purchasedItemCount,
                requiredItemCount
            );

        return ProcessPlayerChoice(
            state,
            shoppingChoice,
            angerSceneName,
            acceptanceSceneName
        );
    }

    private static Choice CreateShoppingOutcomeChoice(
        ShoppingOutcome outcome,
        int purchasedItemCount,
        int requiredItemCount
    )
    {
        Choice choice = new Choice();

        switch (outcome)
        {
            case ShoppingOutcome.LeftWithoutBuying:
                choice.choiceText = "Langsung keluar toko";
                choice.systemResponse =
                    "Kamu memilih pergi tanpa membawa apa-apa.";
                choice.addDistress = 12f;
                choice.addDenial = 8f;
                choice.addRumination = 10f;
                choice.addHope = -10f;
                choice.category = ChoiceCategory.Avoidance;
                break;

            case ShoppingOutcome.BoughtAllRequiredItems:
                choice.choiceText = "Membeli semua barang yang diperlukan";
                choice.systemResponse =
                    "Kamu menyelesaikan belanja dengan lebih tenang.";
                choice.addDistress = -8f;
                choice.addDenial = -6f;
                choice.addRumination = -4f;
                choice.addHope = 12f;
                choice.category = ChoiceCategory.CleanUpAction;
                break;

            case ShoppingOutcome.BoughtSomeItems:
            default:
            {
                float completionRatio = 0.5f;

                if (requiredItemCount > 0)
                {
                    completionRatio =
                        Mathf.Clamp01(
                            (float) purchasedItemCount /
                            requiredItemCount
                        );
                }

                choice.choiceText = "Membeli beberapa barang";
                choice.systemResponse =
                    "Kamu masih ragu-ragu, tapi setidaknya tidak pulang kosong.";
                choice.addDistress = Mathf.Lerp(4f, -2f, completionRatio);
                choice.addDenial = Mathf.Lerp(4f, -1f, completionRatio);
                choice.addRumination = Mathf.Lerp(5f, 0f, completionRatio);
                choice.addHope = Mathf.Lerp(4f, 10f, completionRatio);
                choice.category = ChoiceCategory.Rational;
                break;
            }
        }

        return choice;
    }

    private static bool UpdateCategoryCounters(
        GriefState currentState,
        ChoiceCategory category,
        string acceptanceSceneName
    )
    {
        switch (category)
        {
            case ChoiceCategory.HardDenial:
                currentState.consecutiveHardDenial++;
                break;

            case ChoiceCategory.Aggressive:
                currentState.aggressiveChoiceCount++;
                break;

            case ChoiceCategory.Denial:
                currentState.denialChoiceCount++;
                break;

            case ChoiceCategory.InternalizedAnger:
                currentState.internalizedAngerCount++;
                break;

            case ChoiceCategory.FalseHope:
                currentState.falseHopeChoiceCount++;
                break;

            case ChoiceCategory.Rumination:
                currentState.ruminationChoiceCount++;
                break;

            case ChoiceCategory.Begging:
                currentState.beggingChoiceCount++;
                break;

            case ChoiceCategory.Avoidance:
                currentState.avoidanceChoiceCount++;
                break;

            case ChoiceCategory.PassiveSuicidal:
                currentState.passiveSuicidalChoiceCount++;
                break;

            case ChoiceCategory.Rational:
                currentState.rationalChoiceCount++;
                break;

            case ChoiceCategory.CleanUpAction:
                Debug.Log("Pemain memilih Clean Up. Pindah ke Acceptance.");
                return TryLoadScene(acceptanceSceneName);

            case ChoiceCategory.Neutral:
                return false;
        }

        return false;
    }

    private static bool EvaluateRules(
        GriefState currentState,
        Choice lastChoice,
        string angerSceneName
    )
    {
        string currentScene = SceneManager.GetActiveScene().name;

        // BAB 1: DENIAL
        if (currentScene.Contains("ES1"))
        {
            if (currentState.distress >= 80 ||
                currentState.rumination >= 80 ||
                currentState.questionCount > 10)
            {
                Debug.Log("Pindah ke Anger.");
                return TryLoadScene(angerSceneName);
            }

            if (currentState.hope >= 80 || currentState.denial >= 80)
            {
                Debug.Log("Notifikasi teman chat melihat mantan!");
                currentState.hope = 0;
                currentState.distress = 100;
                return TryLoadScene(angerSceneName);
            }

            if (currentState.denial > 50)
            {
                Debug.Log("Sembunyikan opsi jawaban C (logis).");
            }

            if (currentState.consecutiveHardDenial >= 3)
            {
                Debug.Log("SISTEM RESPONSE: 'Kamu percaya itu? Benar-benar percaya?'");
            }

            if (lastChoice.category == ChoiceCategory.Logical &&
                currentState.distress > 50)
            {
                Debug.Log(
                    "SISTEM RESPONSE: 'Logikamu berkata tidak apa-apa, tapi tanganmu gemetar hebat.'"
                );
            }

            if (currentState.rumination > 50)
            {
                Debug.Log(
                    "AUDIO: suara degdegan & 'jangan pilih itu, itu semua salahmu, pilihanmu selalu salah' "
                );
            }

            if (currentState.hope > 60)
            {
                Debug.Log("VISUAL: efek bloom & blur.");
            }

            if (currentState.distress > 70)
            {
                Debug.Log("VISUAL: kurangi saturasi warna dan aktifkan Vignette.");
            }

            if (currentState.rumination > 60)
            {
                Debug.Log("VISUAL: efek Film Grain.");
            }

            if (currentState.denial > 60)
            {
                Debug.Log("AUDIO: suara teredam seperti di air.");
            }

            if (currentState.distress > 80)
            {
                Debug.Log("AUDIO: efek detak jantung lambat.");
            }
        }
        // BAB 2: ANGER
        else if (currentScene.Contains("ES2"))
        {
            if ((currentState.rumination >= 80 && currentState.distress < 80) ||
                currentState.questionCount > 9)
            {
                Debug.Log("SISTEM: Transisi ke Bargaining.");
                return TryLoadScene("ES3");
            }

            if (currentState.aggressiveChoiceCount >= 3)
            {
                Debug.Log(
                    "SISTEM: Boomerang Effect aktif! Distress turun lalu akan naik 2x lipat nanti."
                );
            }

            if (currentState.denialChoiceCount > 2)
            {
                currentState.hope += 10;
                currentState.distress -= 5;
                Debug.Log("SISTEM WARNING: 'Kamu masih membela mantanmu'.");
            }

            if (currentState.internalizedAngerCount >= 3)
            {
                Debug.Log("SISTEM: Memicu Pertanyaan Bonus.");
            }

            if (currentState.distress > 70)
            {
                Debug.Log("VISUAL: Aktifkan Red Vignette.");
                Debug.Log("UI: Tombol 'Bijak' bergetar saat diklik.");
            }

            if (currentState.rumination > 60)
            {
                Debug.Log("AUDIO: Pitch Musik 1.2x + Nada Sumbang (Dissonance).");
            }

            if (lastChoice.category == ChoiceCategory.Aggressive)
            {
                Debug.Log("AUDIO: Play SFX Boom / Dentuman Keras.");
            }
        }
        // BAB 3: BARGAINING
        else if (currentScene.Contains("ES3"))
        {
            if (currentState.distress >= 80)
            {
                Debug.Log("TRANSISI: Pindah ke Depression.");
                return TryLoadScene("ES4");
            }

            if (currentState.hope >= 80)
            {
                Debug.Log(
                    "EVENT REALITY CRASH: Pemain melihat mantan bergandengan dengan orang lain!"
                );
                currentState.hope = 0;
                currentState.distress = 100;
                return TryLoadScene("ES4");
            }

            if (currentState.questionCount > 9)
            {
                Debug.Log(
                    "TRANSISI: Batas waktu habis. Paksa transisi ke Depression."
                );
                return TryLoadScene("ES4");
            }

            if (currentState.falseHopeChoiceCount > 2)
            {
                Debug.Log("SISTEM RESPONSE: 'Apakah itu realitanya? Sadarlah!!!'");
            }

            if (currentState.ruminationChoiceCount >= 4)
            {
                Debug.Log(
                    "SISTEM: Ruminasi ekstrem! Ulangi pertanyaan yang sama (maksimal 2x)."
                );
            }

            if (currentState.beggingChoiceCount >= 3)
            {
                Debug.Log(
                    "VISUAL & UI: Perkecil skala karakter/UI pemain dan kurangi saturasi warna."
                );
            }

            if (currentState.hope > 60)
            {
                Debug.Log(
                    "VISUAL: Aktifkan Bloom intensitas tinggi & Filter Warm/Gold (Indah tapi rapuh)."
                );
                Debug.Log("AUDIO: Putar musik nada minor/sedih (Disonansi kognitif aktif).");
            }
        }
        // BAB 4: DEPRESSION
        else if (currentScene.Contains("ES4"))
        {
            if (currentState.distress >= 90 && currentState.rationalChoiceCount >= 3)
            {
                Debug.Log(
                    "EVENT: Syarat Acceptance terpenuhi! Munculkan Pertanyaan Kunci (Membersihkan Sampah)."
                );
            }
            else if (currentState.distress >= 90)
            {
                Debug.Log(
                    "SISTEM RESPONSE: 'Kamu menatap dinding kosong itu selama berjam-jam. Tidak ada yang berubah... Sampai kapan kamu mau begini?'"
                );
            }

            if (currentState.questionCount > 15 && currentState.distress < 90)
            {
                Debug.Log("SISTEM: Mencegah stagnansi. Memaksa penambahan Distress +5.");
                currentState.ModifyEmotion(ref currentState.distress, 5f);
            }

            if (currentState.avoidanceChoiceCount == 1)
            {
                currentState.ModifyEmotion(ref currentState.distress, -5f);
                Debug.Log(
                    "SISTEM RESPONSE: 'Tidak apa-apa menutup pintu hari ini. Terkadang pelarian sejenak adalah satu-satunya cara otakmu beristirahat.'"
                );
            }
            else if (currentState.avoidanceChoiceCount == 3)
            {
                currentState.ModifyEmotion(ref currentState.distress, 10f);
                currentState.ModifyEmotion(ref currentState.rumination, 15f);
                Debug.Log(
                    "SISTEM RESPONSE: 'Kamu tidak bisa terus-terusan bersembunyi. Monster yang kamu hindari tidak pergi, dia hanya menunggu di luar pintumu dan tumbuh semakin besar.'"
                );
            }

            if (currentState.passiveSuicidalChoiceCount > 2)
            {
                currentState.ModifyEmotion(ref currentState.rumination, 10f);
                Debug.Log(
                    "SISTEM RESPONSE: 'Kamu lelah, bukan ingin berakhir. Istirahatlah, tapi berjanjilah untuk bangun lagi.'"
                );
            }

            Debug.Log("UI GLOBAL: Terapkan Delay input 1.5 - 2.5 detik pada tombol.");

            if (lastChoice.category == ChoiceCategory.Rational ||
                lastChoice.category == ChoiceCategory.Logical)
            {
                Debug.Log(
                    "VISUAL: Kilatan cahaya (Flash/Bloom) sesaat, warna kembali normal selama 5 detik."
                );
            }

            if (currentState.avoidanceChoiceCount >= 3)
            {
                Debug.Log(
                    $"VISUAL: Layar bergetar ringan (Camera Shake). Skala: {currentState.avoidanceChoiceCount}"
                );
                Debug.Log("AUDIO: Suara napas pendek atau detak jantung yang tidak beraturan.");
            }

            if (currentState.distress > 80)
            {
                Debug.Log(
                    "VISUAL: Layar Monochrome (Hitam Putih total) + Vignette hitam tebal (Tunnel Vision)."
                );
                Debug.Log(
                    "AUDIO: Hentikan musik latar (Silence) / ganti dengan White Noise (dengung AC)."
                );
            }
            else if (currentState.distress > 50)
            {
                Debug.Log(
                    "VISUAL: Color Grading Desaturated (pucat) + Temperature dingin (biru)."
                );
            }
        }
        // BAB 5: ACCEPTANCE
        else if (currentScene.Contains("ES5"))
        {
            Debug.Log("VISUAL: Color Grading normal dengan sentuhan hangat (Golden Hour).");
            Debug.Log("VISUAL: Hilangkan efek Grain dan Vignette.");
            Debug.Log("AUDIO: Putar musik piano akustik nada Mayor (Uplifting).");

            if (currentState.hope > 80)
            {
                Debug.Log("VISUAL: Munculkan partikel cahaya halus (Floating Dust).");
            }

            if (!currentState.isEndingTriggered)
            {
                DetermineFinalEnding(currentState);
            }
        }

        return false;
    }

    private static void DetermineFinalEnding(GriefState currentState)
    {
        currentState.isEndingTriggered = true;

        if (currentState.hope > 80 && currentState.rumination < 20)
        {
            currentState.finalEndingName = "True Ending: Rebirth";
            Debug.Log("ENDING: Bangkit sepenuhnya & masa depan cerah.");
            Debug.Log("AUDIO: Fade out bising, fokus pada musik menenangkan.");
        }
        else if (currentState.hope > 50 && currentState.rumination > 20)
        {
            currentState.finalEndingName = "The Scarred / Resilience";
            Debug.Log("ENDING: Selamat meski masih membawa luka.");
        }
        else if (currentState.distress < 20 && currentState.hope < 50)
        {
            currentState.finalEndingName = "Numb";
            Debug.Log("ENDING: Mati rasa / Netral.");
        }
        else
        {
            currentState.finalEndingName = "The Survivor";
            Debug.Log("ENDING: Life goes on. Hidup berjalan biasa.");
        }

        Debug.Log("SISTEM: Memanggil fitur Journaling 'Letter to Myself' sebagai closure.");
    }

    private static bool TryLoadScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogWarning("Scene target belum diatur untuk expert system.");
            return false;
        }

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogWarning(
                $"Scene '{sceneName}' belum tersedia di build settings."
            );
            return false;
        }

        SceneManager.LoadScene(sceneName);
        return true;
    }

    private static void CopyState(GriefState source, GriefState target)
    {
        if (source == null || target == null)
        {
            return;
        }

        target.distress = source.distress;
        target.denial = source.denial;
        target.rumination = source.rumination;
        target.hope = source.hope;
        target.questionCount = source.questionCount;
        target.consecutiveHardDenial = source.consecutiveHardDenial;
        target.aggressiveChoiceCount = source.aggressiveChoiceCount;
        target.denialChoiceCount = source.denialChoiceCount;
        target.internalizedAngerCount = source.internalizedAngerCount;
        target.falseHopeChoiceCount = source.falseHopeChoiceCount;
        target.ruminationChoiceCount = source.ruminationChoiceCount;
        target.beggingChoiceCount = source.beggingChoiceCount;
        target.avoidanceChoiceCount = source.avoidanceChoiceCount;
        target.passiveSuicidalChoiceCount = source.passiveSuicidalChoiceCount;
        target.rationalChoiceCount = source.rationalChoiceCount;
        target.finalEndingName = source.finalEndingName;
        target.isEndingTriggered = source.isEndingTriggered;
    }
}
