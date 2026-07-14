using UnityEngine;
using UnityEngine.SceneManagement; 

public class GriefInferenceEngine : MonoBehaviour
{

    [Header("Referensi Data")]
    public GriefState currentState;
    //dipanggil saat pemain klik tombol jawaban A/B/C
    public void ProcessPlayerChoice(Choice selectedChoice)
    {
        currentState.ModifyEmotion(ref currentState.distress, selectedChoice.addDistress);
        currentState.ModifyEmotion(ref currentState.denial, selectedChoice.addDenial);
        currentState.ModifyEmotion(ref currentState.rumination, selectedChoice.addRumination);
        currentState.ModifyEmotion(ref currentState.hope, selectedChoice.addHope);

        currentState.questionCount++;

        //update kategori khusus
        UpdateCategoryCounters(selectedChoice.category);
        EvaluateRules(selectedChoice);
    }

    //menambah counter spesifik berdasarkan pilihan pemain
    private void UpdateCategoryCounters(ChoiceCategory category)
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
                SceneManager.LoadScene("ES5"); //SESUAIIN NAMANYA SAMA YG ASLI GIMANA
                break;

            case ChoiceCategory.Neutral:
                break;
        }
    }

    private void EvaluateRules(Choice lastChoice)
    {
        string currentScene = SceneManager.GetActiveScene().name;
        //BAB 1 DENIAL - PERPINDAHAN BAB
        if (currentScene.Contains("ES1"))
        {
            if (currentState.distress >= 80 || currentState.rumination >= 80 || currentState.questionCount > 10)
            {
                Debug.Log("Pindah ke Anger.");
                UnityEngine.SceneManagement.SceneManager.LoadScene("ES2"); //JANGAN LUPA GANTI YA NAMANYA !!!!!!!
                return;
            }
            if (currentState.hope >= 80 || currentState.denial >= 80)
            {
                Debug.Log("Notifikasi teman chat melihat mantan!");
                currentState.hope = 0;
                currentState.distress = 100;
                UnityEngine.SceneManagement.SceneManager.LoadScene("ES2");
                return;
            }

            //BAB 1 DENIAL - KONSEKUENSI
            if (currentState.denial > 50)
            {
                Debug.Log("Sembunyikan opsi jawaban C (logis)."); //jadi ngga ya ini???
            }
            if (currentState.consecutiveHardDenial >= 3)
            {
                Debug.Log("SISTEM RESPONSE: 'Kamu percaya itu? Benar-benar percaya?'");
            }
            if (lastChoice.category == ChoiceCategory.Logical && currentState.distress > 50)
            {
                Debug.Log("SISTEM RESPONSE: 'Logikamu berkata tidak apa-apa, tapi tanganmu gemetar hebat.'");
            }

            //BAB 1 DENIAL - VISUAL & AUDIO
            if (currentState.rumination > 50)
            {
                Debug.Log("AUDIO: suara degdegan & 'jangan pilih itu, itu semua salahmu, pilihanmu selalu salah' ");
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
        //BAB 2: ANGER 
        else if (currentScene.Contains("ES2"))
        {
            //PERPINDAHAN BAB
            if ((currentState.rumination >= 80 && currentState.distress < 80) || currentState.questionCount > 9)
            {
                Debug.Log("SISTEM: Transisi ke Bargaining.");
                SceneManager.LoadScene("ES3");
                return;
            }
            //KONSEKUENSI
            if (currentState.aggressiveChoiceCount >= 3)
            {
                Debug.Log("SISTEM: Boomerang Effect aktif! Distress turun lalu akan naik 2x lipat nanti.");
                // Catatan: Logika penggandaan poin bisa diimplementasikan di ProcessPlayerChoice turn berikutnya
            }
            if (currentState.denialChoiceCount > 2)
            {
                currentState.hope += 10;
                currentState.distress -= 5;
                Debug.Log("SISTEM WARNING: 'Kamu masih membela mantanmu'."); //INI RESPON SISTEM
            }
            if (currentState.internalizedAngerCount >= 3)
            {
                Debug.Log("SISTEM: Memicu Pertanyaan Bonus.");
                // Panggil fungsi untuk memunculkan UI Pertanyaan Khusus
            }
            //VISUAL & AUDIO
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
            //PERPINDAHAN BAB (TRANSISI)
            if (currentState.distress >= 80)
            {
                Debug.Log("TRANSISI: Pindah ke Depression.");
                UnityEngine.SceneManagement.SceneManager.LoadScene("ES4");
                return;
            }
            if (currentState.hope >= 80)
            {
                Debug.Log("EVENT REALITY CRASH: Pemain melihat mantan bergandengan dengan orang lain!");
                currentState.hope = 0;
                currentState.distress = 100; // Hancur lebur
                UnityEngine.SceneManagement.SceneManager.LoadScene("ES4");
                return;
            }
            if (currentState.questionCount > 9)
            {
                Debug.Log("TRANSISI: Batas waktu habis. Paksa transisi ke Depression.");
                UnityEngine.SceneManagement.SceneManager.LoadScene("ES4");
                return;
            }
            //KONSEKUENSI 
            if (currentState.falseHopeChoiceCount > 2)
            {
                Debug.Log("SISTEM RESPONSE: 'Apakah itu realitanya? Sadarlah!!!'");
            }
            if (currentState.ruminationChoiceCount >= 4)
            {
                Debug.Log("SISTEM: Ruminasi ekstrem! Ulangi pertanyaan yang sama (maksimal 2x).");
            }
            //VISUAL & AUDIO 
            if (currentState.beggingChoiceCount >= 3)
            {
                Debug.Log("VISUAL & UI: Perkecil skala karakter/UI pemain dan kurangi saturasi warna.");
            }
            if (currentState.hope > 60)
            {
                Debug.Log("VISUAL: Aktifkan Bloom intensitas tinggi & Filter Warm/Gold (Indah tapi rapuh).");
                Debug.Log("AUDIO: Putar musik nada minor/sedih (Disonansi kognitif aktif).");
            }
        }
        //BAB 4: DEPRESSION
        else if (currentScene.Contains("ES4"))
        {
            //PERPINDAHAN BAB
            if (currentState.distress >= 90 && currentState.rationalChoiceCount >= 3)
            {
                Debug.Log("EVENT: Syarat Acceptance terpenuhi! Munculkan Pertanyaan Kunci (Membersihkan Sampah).");
                // Catatan: Nanti UIManager akan membaca ini dan menimpa pertanyaan reguler 
                // dengan pertanyaan final tentang membuang sampah.

                // (Jika di pertanyaan sampah pemain memilih 'CleanUpAction', 
                // kode otomatis pindah ke Bab 5 karena sudah kita atur di fungsi UpdateCategoryCounters).
            }
            else if (currentState.distress >= 90)
            {
                Debug.Log("SISTEM RESPONSE: 'Kamu menatap dinding kosong itu selama berjam-jam. Tidak ada yang berubah... Sampai kapan kamu mau begini?'");
            }
            //KONSEKUENSI GAMEPLAY 
            if (currentState.questionCount > 15 && currentState.distress < 90)
            {
                Debug.Log("SISTEM: Mencegah stagnansi. Memaksa penambahan Distress +5.");
                currentState.ModifyEmotion(ref currentState.distress, 5f);
            }
            if (currentState.avoidanceChoiceCount == 1)
            {
                currentState.ModifyEmotion(ref currentState.distress, -5f);
                Debug.Log("SISTEM RESPONSE: 'Tidak apa-apa menutup pintu hari ini. Terkadang pelarian sejenak adalah satu-satunya cara otakmu beristirahat.'");
            }
            else if (currentState.avoidanceChoiceCount == 3)
            {
                currentState.ModifyEmotion(ref currentState.distress, 10f);
                currentState.ModifyEmotion(ref currentState.rumination, 15f);
                Debug.Log("SISTEM RESPONSE: 'Kamu tidak bisa terus-terusan bersembunyi. Monster yang kamu hindari tidak pergi, dia hanya menunggu di luar pintumu dan tumbuh semakin besar.'");
            }
            if (currentState.passiveSuicidalChoiceCount > 2)
            {
                currentState.ModifyEmotion(ref currentState.rumination, 10f);
                Debug.Log("SISTEM RESPONSE: 'Kamu lelah, bukan ingin berakhir. Istirahatlah, tapi berjanjilah untuk bangun lagi.'");
            }
            //VISUAL & AUDIO
            Debug.Log("UI GLOBAL: Terapkan Delay input 1.5 - 2.5 detik pada tombol.");
            if (lastChoice.category == ChoiceCategory.Rational || lastChoice.category == ChoiceCategory.Logical)
            {
                Debug.Log("VISUAL: Kilatan cahaya (Flash/Bloom) sesaat, warna kembali normal selama 5 detik.");
            }
            if (currentState.avoidanceChoiceCount >= 3)
            {
                Debug.Log($"VISUAL: Layar bergetar ringan (Camera Shake). Skala: {currentState.avoidanceChoiceCount}");
                Debug.Log("AUDIO: Suara napas pendek atau detak jantung yang tidak beraturan.");
            }
            if (currentState.distress > 80)
            {
                Debug.Log("VISUAL: Layar Monochrome (Hitam Putih total) + Vignette hitam tebal (Tunnel Vision).");
                Debug.Log("AUDIO: Hentikan musik latar (Silence) / ganti dengan White Noise (dengung AC).");
            }
            else if (currentState.distress > 50)
            {
                Debug.Log("VISUAL: Color Grading Desaturated (pucat) + Temperature dingin (biru).");
            }
        }
        //BAB 5: ACCEPTANCE
        else if (currentScene.Contains("ES5"))
        {
            //VISUAL & AUDIO GLOBAL
            Debug.Log("VISUAL: Color Grading normal dengan sentuhan hangat (Golden Hour).");
            Debug.Log("VISUAL: Hilangkan efek Grain dan Vignette.");
            Debug.Log("AUDIO: Putar musik piano akustik nada Mayor (Uplifting).");
            if (currentState.hope > 80)
            {
                Debug.Log("VISUAL: Munculkan partikel cahaya halus (Floating Dust).");
            }
            //LOGIKA PENENTUAN ENDING
            if (currentState.isEndingTriggered == false)
            {
                DetermineFinalEnding();
            }
        }

        //BATAS BAB
    }

    private void DetermineFinalEnding()
    {
        currentState.isEndingTriggered = true;

        //True Ending (Harapan tinggi, ruminasi rendah)
        if (currentState.hope > 80 && currentState.rumination < 20)
        {
            currentState.finalEndingName = "True Ending: Rebirth";
            Debug.Log("ENDING: Bangkit sepenuhnya & masa depan cerah.");
            Debug.Log("AUDIO: Fade out bising, fokus pada musik menenangkan.");
        }
        //The Scarred / Resilience (Selamat meski terluka)
        else if (currentState.hope > 50 && currentState.rumination > 20)
        {
            currentState.finalEndingName = "The Scarred / Resilience";
            Debug.Log("ENDING: Selamat meski masih membawa luka.");
        }
        //Numb (Mati rasa/Netral)
        else if (currentState.distress < 20 && currentState.hope < 50)
        {
            currentState.finalEndingName = "Numb";
            Debug.Log("ENDING: Mati rasa / Netral.");
        }
        //The Survivor (Default - Hidup terus berjalan)
        else
        {
            currentState.finalEndingName = "The Survivor";
            Debug.Log("ENDING: Life goes on. Hidup berjalan biasa.");
        }

        //fITUR PENUTUP ---
        Debug.Log("SISTEM: Memanggil fitur Journaling 'Letter to Myself' sebagai closure.");
        //munculkan panel surat sebelum ke Credit Title.
    }
}