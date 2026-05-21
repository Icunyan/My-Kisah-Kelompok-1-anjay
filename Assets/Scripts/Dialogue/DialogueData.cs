using UnityEngine;
using System.Collections.Generic;
using FantasyLifeVN.Core;

namespace FantasyLifeVN.Dialogue
{
    [System.Serializable]
    public struct DialogueLine
    {
        public string speakerName;
        [TextArea(3, 5)]
        public string text;
        public string expression; // Normal, Senang, Tsundere, Malu, Serius, Cemas, Lemah, Terkejut
    }

    [System.Serializable]
    public struct DialogueChoice
    {
        public string choiceText;
        public string targetNodeID;
        public int affectionGain;
        public int energyCost;
        public string consequenceText;
    }

    [System.Serializable]
    public class DialogueSequence
    {
        public string sequenceID;
        public List<DialogueLine> lines = new List<DialogueLine>();
        public List<DialogueChoice> choices = new List<DialogueChoice>();
    }

    public static class DialogueDatabase
    {
        /// <summary>
        /// Daily dialogue database based on characters, phase, affection and story progression.
        /// </summary>
        public static DialogueSequence GetDailyDialogue(string npcId, string phase, int affection, bool isSick)
        {
            DialogueSequence ds = new DialogueSequence();
            ds.sequenceID = npcId;

            // =====================================================================
            // LARA - Edit teks dialog di sini
            // =====================================================================
            if (npcId.ToLower() == "lara")
            {
                ds.lines.Add(new DialogueLine { speakerName = "Lara", text = "Ah! Ren! Kamu butuh apa hari ini?", expression = "Senang" });
                ds.lines.Add(new DialogueLine { speakerName = "Lara", text = "Hmmm? ah! Maaf ya, Dzaky masih bingung cara nambah dialog! ♥", expression = "Malu" });
                ds.lines.Add(new DialogueLine { speakerName = "Lara", text = "Benar kak, cuman segini aja dulu", expression = "Senang" });
            }
            // =====================================================================
            // LUCIA - Edit teks dialog di sini
            // =====================================================================
            else if (npcId.ToLower() == "lucia")
            {
                ds.lines.Add(new DialogueLine { speakerName = "Lucia", text = "Hmph, ada perlu apa datang-datang ke sini?", expression = "Tsundere" });
                ds.lines.Add(new DialogueLine { speakerName = "Lucia", text = "...Yah, kalau kau sudah di sini, duduklah sebentar.", expression = "Malu" });
            }
            // =====================================================================
            // MARCO - Edit teks dialog di sini
            // =====================================================================
            else
            {
                ds.lines.Add(new DialogueLine { speakerName = "Marco", text = "Hahaha! Ren! Siap untuk petualangan hari ini, kawan?", expression = "Senang" });
                ds.lines.Add(new DialogueLine { speakerName = "Marco", text = "Percayakan perlindungan pada Marco si benteng berjalan!", expression = "Senang" });
            }

            return ds;
        }

        /// <summary>
        /// Returns the exact Story Campaign sequences transcribed from GIM.pdf
        /// </summary>
        public static DialogueSequence GetCampaignDialogue(int sectionNumber)
        {
            DialogueSequence ds = new DialogueSequence();
            ds.sequenceID = $"campaign_section_{sectionNumber}";

            switch (sectionNumber)
            {
                case 1:
                    ds.lines.Add(new DialogueLine { speakerName = "Lara", text = "Wah!", expression = "Terkejut" });
                    ds.choices.Add(new DialogueChoice { choiceText = "Lanjutkan", targetNodeID = "trigger_section_2", affectionGain = 0, energyCost = 0, consequenceText = "" });
                    break;

                case 2:
                    ds.lines.Add(new DialogueLine { speakerName = "Ren", text = "(Terengah-engah, menatap ngeri sekeliling) Tidak... ini tidak mungkin! (Berlari menerobos runtuhan panti asuhan) Lara! Marco! Kumohon jawab aku... Di mana kalian?!", expression = "Cemas" });
                    ds.lines.Add(new DialogueLine { speakerName = "Ren", text = "(Saat berlari, Ren tidak sengaja menabrak seorang Knight) Aduh... Maaf, aku tidak melihat tunggu. Marco?! Kau... tinggi sekali sekarang.", expression = "Terkejut" });
                    
                    ds.lines.Add(new DialogueLine { speakerName = "Marco", text = "(Terkejut, lalu tertawa lebar hingga suaranya menggelegar) Ren?! Astaga, si penyihir kecil kita akhirnya pulang! (Menepuk pundak Ren kencang) Baguslah kau selamat.", expression = "Senang" });
                    ds.lines.Add(new DialogueLine { speakerName = "Marco", text = "Sapaannya nanti saja, bantu aku bawa kotak-kotak pasokan medis ini. Panti asuhan kita sekarang jadi tempat evakuasi warga!", expression = "Serius" });
                    
                    ds.lines.Add(new DialogueLine { speakerName = "Narasi", text = "Ren Langsung menggunakan sihir angin untuk meringankan beban kotak obat.", expression = "Normal" });
                    ds.lines.Add(new DialogueLine { speakerName = "Ren", text = "Oke, serahkan padaku! Ayo cepat!", expression = "Senang" });
                    
                    ds.lines.Add(new DialogueLine { speakerName = "Narasi", text = "Ren dan Marco bergegas mendorong pintu aula panti asuhan yang berat. Di dalam, suasana tampak penuh sesak oleh warga yang terluka.", expression = "Cemas" });
                    ds.lines.Add(new DialogueLine { speakerName = "Narasi", text = "Di tengah kerumunan itu, tampak Lara terduduk lemas di sebuah kursi kayu, wajahnya pucat dan nafasnya terengah-engah akibat terlalu banyak menguras energi sihir untuk menyembuhkan warga.", expression = "Lemah" });
                    
                    ds.choices.Add(new DialogueChoice { choiceText = "Hampiri Lara", targetNodeID = "trigger_section_3", affectionGain = 0, energyCost = 0, consequenceText = "Kamu bergegas mendekati kursi kayu tempat Lara beristirahat." });
                    break;

                case 3:
                    ds.lines.Add(new DialogueLine { speakerName = "Marco", text = "(Berjalan cepat mendekat dengan raut cemas) Lara! Kau memaksakan dirimu lagi? Istirahatlah dulu, pasokan medisnya sudah datang.", expression = "Cemas" });
                    
                    ds.lines.Add(new DialogueLine { speakerName = "Lara", text = "(Menoleh lemah, menyeka keringat dingin di dahinya) Aku tidak apa-apa, Marco... warga masih banyak yang— (Matanya tiba-tiba tertuju pada sosok di belakang Marco, seketika perban di tangannya terjatuh) ...Ren? Ini... benar-benar kau?", expression = "Terkejut" });
                    
                    ds.lines.Add(new DialogueLine { speakerName = "Ren", text = "(Berjalan mendekat dengan senyum lega sekaligus prihatin, lalu berlutut di depan kursi Lara) Lara Maaf aku terlambat. Jangan paksakan sihirmu lagi, biar aku yang bantu sisa warga di sini.", expression = "Malu" });
                    
                    ds.choices.Add(new DialogueChoice { choiceText = "Bantu obati warga bersama Marco", targetNodeID = "trigger_section_4", affectionGain = 10, energyCost = 0, consequenceText = "Kamu merapalkan sihir pemulihan akademi untuk menyembuhkan warga." });
                    break;

                case 4:
                    ds.lines.Add(new DialogueLine { speakerName = "Narasi", text = "Ren segera merapalkan sihir pemulihan dari akademinya untuk mengobati luka warga yang tersisa.", expression = "Normal" });
                    ds.lines.Add(new DialogueLine { speakerName = "Narasi", text = "Sementara Marco dengan tubuh besarnya cekatan memindahkan puing-puing dan menata tempat istirahat yang layak.", expression = "Normal" });
                    ds.lines.Add(new DialogueLine { speakerName = "Narasi", text = "Setelah semua warga tertangani dan kondisi panti asuhan mulai kondusif, ketiganya akhirnya bisa duduk bersama di sudut ruangan yang tenang.", expression = "Normal" });
                    
                    ds.lines.Add(new DialogueLine { speakerName = "Marco", text = "(Menyandarkan perisai besarnya ke dinding, lalu menghela napas lega) Akhirnya... Kerja bagus, Ren. Sihir akademi memang beda, warga bisa tenang sekarang.", expression = "Senang" });
                    
                    ds.lines.Add(new DialogueLine { speakerName = "Ren", text = "(Tersenyum, menatap cangkirnya) Ini belum seberapa dibanding apa yang kalian lakukan di sini selama aku pergi. Terutama kau, Marco... (melihat zirah dan tubuh kekar Marco).", expression = "Senang" });
                    ds.lines.Add(new DialogueLine { speakerName = "Ren", text = "Sial, waktu aku berangkat dulu, kau itu kurus kering. Sekarang badanmu sudah seperti benteng berjalan. Gagah sekali, kawan.", expression = "Senang" });
                    
                    ds.lines.Add(new DialogueLine { speakerName = "Marco", text = "(Tertawa terbahak-bahak sambil menepuk dadanya yang berbaju besi) Hahaha! Seorang Knight harus bisa jadi tameng, Ren! Tapi bicara soal perubahan... (menyenggol lengan Lara) lihat si penyihir kita ini, Lara.", expression = "Senang" });
                    
                    ds.lines.Add(new DialogueLine { speakerName = "Lara", text = "(Tertawa kecil, menopang dagunya sambil menatap Ren dengan pandangan mengejek namun hangat) Benar juga. Padahal dulu waktu di panti asuhan, Ren ini yang paling pendek di antara kita bertiga.", expression = "Senang" });
                    ds.lines.Add(new DialogueLine { speakerName = "Lara", text = "Selalu protes kalau disuruh mengambil barang di rak atas. Tapi sekarang? Kenapa kau bisa lebih tinggi dari aku, huh? Curang sekali.", expression = "Senang" });
                    
                    ds.lines.Add(new DialogueLine { speakerName = "Ren", text = "(Tertawa kecil sambil menggaruk tengkuknya yang tidak gatal) Hei, pertumbuhan di akademi itu nyata, Lara! Lagipula, tidak mungkin kan seorang master sihir tingginya segini-gini saja?", expression = "Malu" });
                    
                    ds.lines.Add(new DialogueLine { speakerName = "Lara", text = "(Tersenyum lembut, tatapannya beralih menatap langit-langit aula) Tapi melihat kita bertiga duduk seperti ini... rasanya seperti Dejavu. Mengingatkan aku pada malam-malam di mana kita sering berbisik di tempat tidur, bermimpi tentang dunia luar.", expression = "Senang" });
                    
                    ds.lines.Add(new DialogueLine { speakerName = "Marco", text = "(Sorot matanya melembut, mengangguk setuju) Benar. Kita dulu selalu berteriak, 'Suatu hari nanti, kita akan jadi petualang hebat dan menjelajahi ujung dunia!'", expression = "Senang" });
                    
                    ds.lines.Add(new DialogueLine { speakerName = "Narasi", text = "Malam semakin larut, namun kehangatan obrolan mereka bertiga seolah menepis hawa dingin di aula panti asuhan. Mereka terus bercerita, tertawa, dan mengenang banyak hal yang telah mereka lalui terpisah selama lima tahun ini.", expression = "Normal" });
                    ds.lines.Add(new DialogueLine { speakerName = "Narasi", text = "Namun, sadar bahwa tugas belum selesai, ketiganya pun berdiri dan bersiap kembali untuk lanjut membantu warga desa yang membutuhkan.", expression = "Normal" });
                    ds.lines.Add(new DialogueLine { speakerName = "Narasi", text = "Namun baru beberapa langkah berjalan, tubuh Lara tiba-tiba lemas. Pandangannya mengabur, dan sedetik kemudian, ia jatuh pingsan ke lantai.", expression = "Cemas" });
                    
                    ds.choices.Add(new DialogueChoice { choiceText = "Tangkap tubuh Lara!", targetNodeID = "trigger_section_5", affectionGain = 15, energyCost = 0, consequenceText = "Kamu langsung menahan tubuh Lara sebelum membentur lantai." });
                    break;

                case 5:
                    ds.lines.Add(new DialogueLine { speakerName = "Ren", text = "(Terkejut, langsung menahan tubuh Lara sebelum membentur lantai) Lara?!", expression = "Cemas" });
                    
                    ds.lines.Add(new DialogueLine { speakerName = "Marco", text = "(Seketika panik, ikut berlutut di samping mereka) Ada apa dengannya?! Lara! Sial, dia pasti benar-benar kehabisan energi sihirnya karena memaksakan diri sejak tadi!", expression = "Cemas" });
                    
                    ds.lines.Add(new DialogueLine { speakerName = "Narasi", text = "(Tiba-tiba, pintu aula panti asuhan terbuka lebar. Derap langkah kaki yang tergesa-gesa terdengar mendekat, disusul oleh sebuah suara yang sangat familier bagi Ren.)", expression = "Terkejut" });
                    
                    ds.lines.Add(new DialogueLine { speakerName = "Lucia", text = "'Ren! Akhirnya aku menemukanmu! Kenapa kau hobi sekali pergi duluan tanpa menunggu, sih?!'", expression = "Tsundere" });
                    
                    ds.lines.Add(new DialogueLine { speakerName = "Ren", text = "(Menoleh kaget) Lu... Lucia?! Kenapa kau bisa ada di sini? Kau menyusulku dari akademi?", expression = "Terkejut" });
                    
                    ds.lines.Add(new DialogueLine { speakerName = "Lucia", text = "(Napasnya terengah-engah, dengan jubah penyihir akademinya yang sedikit kotor) Tentu saja! Berita serangan pasukan iblis ini sudah menyebar luas, aku tidak mungkin membiarkanmu menghadapi ini sendirian!", expression = "Tsundere" });
                    
                    ds.lines.Add(new DialogueLine { speakerName = "Ren", text = "(Wajahnya berubah penuh harap) Lucia, kamu datang di waktu yang tepat! Kau jauh lebih jago sihir penyembuhan tingkat tinggi daripadaku. Periksa keadaan Lara!", expression = "Cemas" });
                    
                    ds.lines.Add(new DialogueLine { speakerName = "Lucia", text = "(Seketika terdiam, matanya menatap gadis rambut panjang yang sedang didekap Ren) ...Tunggu. Jadi, dia ini... Lara? Teman masa kecil yang selalu, selalu kau ceritakan di akademi itu?", expression = "Serius" });
                    
                    ds.lines.Add(new DialogueLine { speakerName = "Ren", text = "(Panik) Iya! Kumohon, Lucia, nanti saja tanyanya. Dia pingsan karena kehabisan mana!", expression = "Cemas" });
                    
                    ds.lines.Add(new DialogueLine { speakerName = "Lucia", text = "(Menghela napas panjang, mengerucutkan bibirnya sedikit dengan tatapan yang agak cemburu) Huh... jadi ini orangnya. Baiklah, baiklah! Minggir sedikit, biar kuperiksa.", expression = "Tsundere" });
                    ds.lines.Add(new DialogueLine { speakerName = "Lucia", text = "Jauh-jauh aku menyusulmu ke sini, malah langsung disuruh mengobati saingan... maksudku, temanmu!", expression = "Tsundere" });
                    
                    ds.lines.Add(new DialogueLine { speakerName = "Narasi", text = "Lucia langsung merapalkan mantra sihir penyembuh tingkat tinggi, sementara tangannya mulai memancarkan cahaya hijau yang menenangkan.", expression = "Normal" });
                    ds.lines.Add(new DialogueLine { speakerName = "Narasi", text = "Cahaya hijau dari sihir penyembuhan Lucia perlahan meredup, raut wajahnya berubah menjadi semakin serius setelah memeriksa aliran mana di tubuh Lara. Ren dan Marco menunggu dengan cemas.", expression = "Cemas" });
                    
                    // Directly transitions to Section 6
                    ds.choices.Add(new DialogueChoice { choiceText = "Tanyakan hasil diagnosa Lucia", targetNodeID = "trigger_section_6", affectionGain = 5, energyCost = 0, consequenceText = "Lucia menarik napas dalam dan menatap kalian berdua dengan serius." });
                    break;

                case 6:
                    ds.lines.Add(new DialogueLine { speakerName = "Ren", text = "Bagaimana, Lucia? Apa dia baik-baik saja?", expression = "Cemas" });
                    
                    ds.lines.Add(new DialogueLine { speakerName = "Lucia", text = "Ini buruk, Ren. Dia pingsan bukan hanya karena kelelahan, tapi karena tubuhnya terkena kutukan dari sihir para iblis.", expression = "Serius" });
                    
                    ds.lines.Add(new DialogueLine { speakerName = "Marco", text = "Kutukan?! Bagaimana bisa?", expression = "Cemas" });
                    
                    ds.lines.Add(new DialogueLine { speakerName = "Lucia", text = "Karena dia terlalu banyak mengobati warga desa yang terluka akibat senjata atau sihir monster.", expression = "Serius" });
                    ds.lines.Add(new DialogueLine { speakerName = "Lucia", text = "Tanpa disadari, residu mana iblis dari luka-luka warga itu masuk dan mengontaminasi sirkulasi sihirnya sendiri saat dia merapalkan mantra penyembuh. Kutukan ini perlahan menggerogoti sirkuit mananya.", expression = "Serius" });
                    
                    ds.lines.Add(new DialogueLine { speakerName = "Ren", text = "Lalu apa ada obatnya? Sihir pemurnian mu tidak bisa menghapusnya?", expression = "Cemas" });
                    
                    ds.lines.Add(new DialogueLine { speakerName = "Lucia", text = "Sihirku hanya bisa menahan penyebarannya untuk sementara waktu.", expression = "Serius" });
                    ds.lines.Add(new DialogueLine { speakerName = "Lucia", text = "Untuk menyembuhkannya secara total, kita butuh obat khusus. Dan sayangnya, satu-satunya bahan utama untuk obat itu hanya tumbuh di tempat asalnya, Benua Iblis.", expression = "Serius" });
                    
                    ds.lines.Add(new DialogueLine { speakerName = "Marco", text = "Benua Iblis... wilayah paling berbahaya yang dikuasai penuh oleh pasukan Raja Iblis.", expression = "Cemas" });
                    
                    ds.lines.Add(new DialogueLine { speakerName = "Ren", text = "Kalau begitu aku tau apa yang harus ku lakukan. Aku akan pergi ke Benua Iblis sekarang juga, sendirian! Aku yang akan bawa obat itu kembali!", expression = "Serius" });
                    
                    ds.lines.Add(new DialogueLine { speakerName = "Marco", text = "Hei, tunggu dulu! Jangan main-main, Ren! Kau tidak bisa pergi sendirian ke tempat menyeramkan seperti itu. Ajak aku dan kita bisa melawan mereka bersama!", expression = "Serius" });
                    
                    ds.lines.Add(new DialogueLine { speakerName = "Ren", text = "Marco, ini bukan perjalanan biasa. Ini Benua Iblis! Tempat itu sangat berbahaya, nyawamu bisa terancam!", expression = "Cemas" });
                    
                    ds.lines.Add(new DialogueLine { speakerName = "Marco", text = "Aku tidak masalah dengan hal itu, Ren. Lagipula, menghadapi bahaya dan melindungi orang lain adalah tujuan utamaku menjadi seorang Ksatria.", expression = "Senang" });
                    ds.lines.Add(new DialogueLine { speakerName = "Marco", text = "Aku tidak akan membiarkan sahabatku pergi sendirian ke sarang musuh.", expression = "Serius" });
                    
                    ds.lines.Add(new DialogueLine { speakerName = "Lucia", text = "Dan jangan lupakan aku! Aku juga harus ikut!", expression = "Tsundere" });
                    
                    ds.lines.Add(new DialogueLine { speakerName = "Ren", text = "T-tapi… Lucia…", expression = "Terkejut" });
                    
                    ds.lines.Add(new DialogueLine { speakerName = "Lucia", text = "Tidak ada tapi-tapi! Aku tidak akan pernah membiarkan sahabatku di akademi pergi ke tempat berbahaya seperti itu sendirian untuk mati konyol.", expression = "Tsundere" });
                    ds.lines.Add(new DialogueLine { speakerName = "Lucia", text = "Lagipula, kalian berdua butuh penyihir jenius sepertiku untuk mendeteksi sihir iblis di sana, kan?", expression = "Senang" });

                    ds.choices.Add(new DialogueChoice { choiceText = "Sambut tekad mereka dan bersiap menuju Benua Iblis", targetNodeID = "trigger_dungeon_prep", affectionGain = 10, energyCost = 0, consequenceText = "Petualangan sesungguhnya menuju Benua Iblis pun dimulai..." });
                    break;
            }

            return ds;
        }

        public static DialogueSequence GetKamarLaraMenu()
        {
            DialogueSequence ds = new DialogueSequence();
            ds.sequenceID = "kamar_lara_menu";
            ds.lines.Add(new DialogueLine { speakerName = "Lara", text = "Ren! Ada apa? Apakah kamu butuh sesuatu?", expression = "Pose1_Senyum" });
            ds.choices.Add(new DialogueChoice { choiceText = "Talk to Lara", targetNodeID = "lara_talk", affectionGain = 0, energyCost = 0, consequenceText = "" });
            ds.choices.Add(new DialogueChoice { choiceText = "Upgrade Skills", targetNodeID = "lara_upgrade", affectionGain = 0, energyCost = 15, consequenceText = "" });
            ds.choices.Add(new DialogueChoice { choiceText = "Kembali ke Kamar Ren", targetNodeID = "lara_return", affectionGain = 0, energyCost = 0, consequenceText = "" });
            return ds;
        }

        public static DialogueSequence GetRandomLaraDialogue(int index)
        {
            DialogueSequence ds = new DialogueSequence();
            ds.sequenceID = $"lara_random_{index}";

            switch (index)
            {
                case 1:
                    ds.lines.Add(new DialogueLine { speakerName = "Lara", text = "Ren! Apakah petualanganmu hari ini lancar? Aku mengkhawatirkanmu...", expression = "Pose1_tutupmulut" });
                    ds.lines.Add(new DialogueLine { speakerName = "Lara", text = "Tapi melihatmu sehat-sehat saja, aku merasa lega.", expression = "Pose1_Senyum" });
                    ds.lines.Add(new DialogueLine { speakerName = "Lara", text = "Jangan memaksakan dirimu terlalu keras ya.", expression = "Pose1_Tutupmata_senyum" });
                    break;
                case 2:
                    ds.lines.Add(new DialogueLine { speakerName = "Lara", text = "Kamar ini terasa sepi sekali saat kamu pergi.", expression = "Pose2_TutupMulut" });
                    ds.lines.Add(new DialogueLine { speakerName = "Lara", text = "Tapi aku tahu kamu sedang berjuang mencari obat untukku.", expression = "Pose1_Ngomong" });
                    ds.lines.Add(new DialogueLine { speakerName = "Lara", text = "Aku akan selalu menunggumu di sini.", expression = "Pose1_Senyum" });
                    break;
                case 3:
                    ds.lines.Add(new DialogueLine { speakerName = "Lara", text = "Ren, ingat tidak dulu waktu kita di panti asuhan?", expression = "Pose2_tutupmata_Ngomong" });
                    ds.lines.Add(new DialogueLine { speakerName = "Lara", text = "Kamu sering sekali mencuri apel di kebun belakang.", expression = "Pose1_Tutupmata_grin" });
                    ds.lines.Add(new DialogueLine { speakerName = "Lara", text = "Lalu aku dan Marco yang harus kena hukumannya... Hehe.", expression = "Pose2_tutupmata_senyum" });
                    break;
                case 4:
                    ds.lines.Add(new DialogueLine { speakerName = "Lara", text = "Bagaimana keadaan Lucia? Dia sangat baik ya menyusulmu sampai ke sini.", expression = "Pose1_Ngomong" });
                    ds.lines.Add(new DialogueLine { speakerName = "Lara", text = "Kuharap kalian berdua bisa akur saat berpetualang.", expression = "Pose1_Senyum" });
                    ds.lines.Add(new DialogueLine { speakerName = "Lara", text = "Aku tidak sabar untuk bisa sembuh dan ikut bersama kalian lagi.", expression = "Pose2_Senyum" });
                    break;
                case 5:
                    ds.lines.Add(new DialogueLine { speakerName = "Lara", text = "Ren... terima kasih ya sudah merawatku.", expression = "Pose1_tutupmulut" });
                    ds.lines.Add(new DialogueLine { speakerName = "Lara", text = "Sentuhan tanganmu saat memulihkan manaku selalu terasa hangat.", expression = "Pose2_tutupmata_senyum" });
                    ds.lines.Add(new DialogueLine { speakerName = "Lara", text = "Aku berjanji akan bertahan sampai obatnya terkumpul.", expression = "Pose1_Senyum" });
                    break;
            }

            return ds;
        }
    }
}
