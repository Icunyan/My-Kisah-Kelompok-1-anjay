# Product Requirement Document (PRD)
## Project Codename: Fantasy Life VN

---

## 1. Ringkasan Proyek & Latar Belakang (Project Overview)

* **Nama Game (Sementara):** Fantasy Life VN
* **Genre:** Visual Novel / Life Simulation / Turn-Based RPG
* **Platform:** PC (Utama)
* **Game Engine:** Unity (2D)
* **Kontrol Utama:** Full *Point-and-Click* (Menggunakan mouse sepenuhnya, penggunaan keyboard seminimal mungkin).
* **Inspirasi Gaya:** *Living with my sister: fantasy* & *Imouto! Life Monochrome* oleh Inusuku.
* [cite_start]**Premis Cerita:** Mengisahkan petualangan tiga sahabat dari panti asuhan di desa Morgendorf[cite: 2, 6]. [cite_start]Ren (MC) terpilih mendapatkan undangan ke akademi sihir bergengsi [cite: 3] [cite_start]dan harus berpisah dengan Lara dan Marco[cite: 4]. [cite_start]Lima tahun kemudian, desa mereka diserang oleh pasukan Raja Iblis[cite: 6, 9]. [cite_start]Setibanya di rumah, Ren menemukan desa hancur [cite: 14] [cite_start]dan Lara jatuh pingsan karena terkena kutukan mematikan dari sisa mana iblis saat mencoba menyembuhkan warga desa[cite: 59, 83, 86]. [cite_start]Bersama Marco dan Lucia (pendeta wanita dari akademi yang menyukai Ren), mereka harus pergi ke Benua Iblis untuk mencari bahan obat penawar kutukan tersebut[cite: 65, 89, 90, 92, 95, 99].

### Profil Karakter:
* **Ren (MC):** Karakter utama yang dikendalikan pemain. [cite_start]Seorang penyihir berbakat (*Mage*) lulusan akademi sihir[cite: 3, 51].
* [cite_start]**Lara:** Sahabat masa kecil Ren yang tekun[cite: 2]. [cite_start]Ia menderita kutukan parah yang menggerogoti sirkuit mananya, membuatnya lemas, sakit-sakitan, dan terperangkap di kamarnya[cite: 59, 87].
* [cite_start]**Marco:** Sahabat masa kecil Ren yang kini menjadi seorang Ksatria (*Knight/Tank*) bertubuh besar gagah dengan zirah, pedang, dan perisai besarnya[cite: 19, 39, 43, 44].
* [cite_start]**Lucia:** Teman sekelas Ren dari akademi sihir yang merupakan seorang pendeta wanita (*Priestess*) jenius dalam sihir penyembuhan tingkat tinggi[cite: 65, 70, 102]. [cite_start]Ikut bertualang karena mengkhawatirkan Ren dan memendam perasaan kepadanya[cite: 74].

---

## 2. Inti Permainan (Core Game Loop)

1. **Siklus Harian (Daily Cycle):** Pemain menjalani aktivitas harian yang dibagi menjadi tiga fase: Pagi, Siang, dan Malam.
2. **Eksplorasi Rumah & Manajemen Status:** Pemain berpindah ruangan untuk berinteraksi dengan NPC (Lara, Marco, Lucia) guna meningkatkan poin kedekatan (*Affection/Friendship*) atau berlatih meningkatkan status bertarung (*Train*).
3. **Ekspedisi Dungeon (Adventure):** Pada fase Siang, pemain dapat memilih opsi *Adventure* untuk masuk ke siklus pertempuran demi mencari obat penawar.
4. **Progres Cerita:** Setiap kali memenangkan pertarungan, level cerita bertambah +1. Jika kalah atau mundur (*Run*), pemain kembali ke rumah.
5. **Istirahat Harian:** Hari berganti setelah pemain mengambil opsi tidur (*Rest*) pada fase Malam yang merestorasi seluruh energi.
6. **Akhir Permainan:** Mencapai Level 30, mengalahkan Boss terakhir, dan menentukan nasib Lara (True, Good, atau Bad Ending).

---

## 3. Fitur Utama & Mekanik Game

### A. Sistem Siklus Waktu & Aktivitas Harian
Alur permainan diatur ketat berdasarkan pembagian waktu tiga fase yang membatasi aksi pemain melalui konsumsi poin Energi.

#### 1. Siklus Pagi (Morning Cycle)
* **Visit Lara:** Mengunjungi kamar Lara untuk interaksi.
    * *Talk to Lara:* Memicu dialog dinamis dan menambah poin kedekatan (+1/+2 *Friendship*).
    * *Upgrade Buffs:* Meningkatkan kenyamanan kamar untuk membuka bonus status pasif tertentu.
* **Train:** Melakukan latihan intensif untuk meningkatkan atribut bertarung tim (*+Stats*), mengonsumsi sejumlah poin energi (*-Energy*).
* **Rest:** Istirahat ringan untuk memulihkan sebagian kecil energi (*+Energy*).

#### 2. Siklus Siang (Afternoon Cycle)
* Memiliki opsi ruangan dan aktivitas dasar yang sama dengan pagi hari (**Visit Lara, Train, Rest**).
* **Adventure (Dungeon Entry):** Pilihan eksklusif di siang hari untuk meninggalkan area rumah dan berpindah ke layar pertempuran Benua Iblis (*Adventure Cycle*).

#### 3. Siklus Malam (Evening Cycle)
* **Visit Lara:** Kesempatan terakhir dalam satu hari untuk berinteraksi atau memeriksa kondisi Lara sebelum beristirahat.
* **Rest (Tidur Utama):** Menyelesaikan hari yang berjalan, merestorasi total poin energi kembali ke 100%, memajukan hitungan hari (*Day +1*), dan mengembalikan fase waktu ke **Siklus Pagi**.

| Aktivitas | Fase Waktu Tersedia | Efek Utama |
| :--- | :--- | :--- |
| Talk to Lara / NPC | Pagi, Siang, Malam | + Friendship Points / Membuka Dialog Baru |
| Train (Latihan Atribut) | Pagi, Siang | + Atribut Tempur (ATK/DEF/HP/MP), - Energi |
| Rest (Istirahat Sejenak) | Pagi, Siang | + Sebagian Kecil Poin Energi |
| Adventure (Combat) | Siang | Memicu Pertarungan Cerita (Level 1-30), - Energi |
| Sleep (Tidur Utama) | Malam | Reset Hari, Pulihkan 100% Energi, Kembali ke Pagi |

### B. Sistem Navigasi Ruangan & Kontrol Point-and-Click
* **Navigasi Layar:** Desain latar ruangan statis (Kamar Lara, Ruang Tengah, Tempat Latihan, Menu Dungeon) yang sepenuhnya berpindah melalui klik pada tombol pintu atau ikon transisi area.
* **Interaksi Karakter:** Sprite karakter ditempatkan pada titik tertentu di dalam ruangan. Klik pada sprite akan langsung memunculkan kotak dialog visual novel. Isi dialog dipengaruhi oleh *Story Level*, tingkat *Friendship*, dan fase waktu berjalan (Pagi/Siang/Malam).
* **Kursor Responsif:** Bentuk ikon kursor mouse akan berubah secara dinamis (misalnya ikon mata untuk memeriksa, ikon balon teks untuk mengobrol) saat berada di atas objek interaktif.

### C. Sistem Pertarungan & Progres Level Cerita (Adventure Cycle)
* **Sistem Leveling Cerita:**
    * **Level 1 - 29 (Normal Stage):** Menghadapi kawanan monster biasa dengan tingkat kesulitan yang meningkat secara linear. Setiap kemenangan (*Victory*) otomatis meningkatkan progress cerita sebanyak **+1 Story Level** dan langsung memajukan fase ke Siklus Malam.
    * **Level 30 (Boss Stage):** Pertarungan pamungkas melawan Boss penjaga bahan obat utama. Kemenangan di tahap ini langsung memicu penyelesaian game.
* **Mekanik Turn-Based Tanpa Item:**
    Pertarungan mengusung sistem giliran murni (*turn-based*) layar statis. Pemain mengendalikan barisan tim (Ren, Marco, Lucia) menggunakan GUI tombol aksi tanpa adanya manajemen inventaris barang (*No Items*). Pilihan aksi meliputi:
    1. **Attack:** Serangan dasar tanpa memakan resource/MP.
    2. **Skill:** Menggunakan sihir serang elemen Ren, keahlian perlindungan/provokasi musuh dari tank Marco, atau sihir penyembuhan (*healing*) tingkat tinggi dari Lucia dengan mengonsumsi MP.
    3. **Guard:** Mengurangi persentase kerusakan (*damage mitigation*) yang diterima karakter pada giliran aktif tersebut.
    4. **Run:** Opsi untuk melarikan diri dari pertarungan guna menyelamatkan sisa HP tim dan kembali aman ke rumah tanpa penambahan Level Cerita.

### D. Penentuan Akhir Cerita (Ending Branching)
Setelah memenangkan Boss Fight di Level 30, game akan mengevaluasi seluruh riwayat variabel pemain untuk menentukan percabangan akhir:
* **True Ending:** Berhasil mengalahkan Boss dengan akumulasi poin kedekatan (*Friendship*) Lara yang maksimal serta pemenuhan parameter tertentu.
* **Good Ending:** Berhasil mengalahkan Boss dengan status standar namun berhasil membawa obat penawar pulang.
* **Bad Ending:** Terjadi apabila tim kalah mati dalam pertempuran melawan Boss, atau kehabisan batas waktu hari tertentu (jika diterapkan batas hari), atau jika status kedekatan dengan Lara terlalu rendah sehingga penawar tidak bekerja optimal.

---

## 4. Kebutuhan Antarmuka (UI/UX) & Menu Navigasi

* **HUD Utama (Layar Eksplorasi):**
    * Bar Indikator Sisa Energi (0 - 100).
    * Penunjuk Fase Waktu Aktif (Pagi / Siang / Malam).
    * Teks Info Hari (Contoh: *Hari ke-12*).
    * Teks Progress Cerita (Contoh: *Progress: Level 14/30*).
* **Menu Navigasi Ringkas (Sidebar/Pop-up):**
    * **Status:** Panel informasi untuk melihat statistik pertarungan (HP, MP, ATK, DEF) milik Ren, Marco, Lucia, serta grafik/angka nilai kedekatan (*Friendship*) Lara.
    * **Save/Load:** Antarmuka penyimpanan data permainan yang menyediakan slot bagi pemain untuk menyimpan atau memuat kembali progres permainan di luar mode pertempuran.

---

## 5. Rekomendasi Teknis & Arsitektur Unity

* **Data Persistence dengan ScriptableObjects:**
    Disarankan memisahkan penyimpanan data dinamis ke dalam file *ScriptableObject* agar mudah diakses di seluruh Scene Unity (Scene Rumah, Scene Dialog, Scene Combat):
    * `TimeSystemSO`: Mengatur variabel hari (int) dan fase waktu (enum).
    * `EnergySystemSO`: Mengatur nilai integer energi sisa dan fungsi pemulihannya.
    * `StoryProgressSO`: Menyimpan integer tingkat level cerita saat ini (1-30).
    * `PartyStatsSO`: Menyimpan array data stat bertarung masing-masing karakter beserta poin *Friendship* Lara.
* **Sistem Dialog:** Menggunakan integrasi plugin **Yarn Spinner** atau **Ink** untuk menyusun naskah percabangan cerita berdasarkan kondisi variabel dari *ScriptableObjects* tanpa membebani performa engine.

---

## 6. Kriteria Penerimaan (Acceptance Criteria)

1. Mekanik kendali game berjalan penuh 100% menggunakan klik mouse (*Point-and-Click*) dari awal menu utama hingga layar credit akhir game.
2. Transisi fase waktu dari Pagi -> Siang -> Malam terpicu dengan benar sesuai aktivitas yang dilakukan, dan opsi "Tidur" untuk meriset hari hanya dapat di-klik pada fase Malam.
3. Kemenangan di fase pertarungan berhasil menaikkan variabel level cerita sebanyak tepat +1 angka, sedangkan aksi melarikan diri (*Run*) mengembalikan pemain ke rumah dengan selamat tanpa penambahan level cerita.
4. Menu pertarungan berfungsi secara turn-based murni dengan 4 tombol perintah (Attack, Skill, Guard, Run) dan terbukti stabil beroperasi tanpa sistem item.
5. Sistem mampu membaca nilai akhir level cerita dan status kedekatan karakter untuk memicu layar percabangan akhir (True/Good/Bad Ending) secara akurat setelah Boss Level 30 dikalahkan.