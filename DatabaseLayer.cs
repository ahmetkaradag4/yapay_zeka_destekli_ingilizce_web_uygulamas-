using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite; // SQLite Kütüphanesi
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsApp2
{
    public static class DatabaseLayer
    {
        // MERKEZİ BAĞLANTI YOLU (Pooling=False ile kilitlenmeyi önledik)
        static string baglantiYolu = @"Data Source=okul.db.db;Version=3;Pooling=False;Journal Mode=WAL;";
        // =============================================================
        // BÖLÜM 1: GİRİŞ İŞLEMLERİ (FORM 1)
        // =============================================================

        public static string OgretmenGirisYap(string kadi, string sifre)
        {
            using (SQLiteConnection baglanti = new SQLiteConnection(baglantiYolu))
            {
                try
                {
                    baglanti.Open();
                    string sql = "SELECT KullaniciAdi FROM Ogretmenler WHERE KullaniciAdi=@kadi AND Sifre=@sifre";
                    using (SQLiteCommand komut = new SQLiteCommand(sql, baglanti))
                    {
                        komut.Parameters.AddWithValue("@kadi", kadi);
                        komut.Parameters.AddWithValue("@sifre", sifre);
                        object sonuc = komut.ExecuteScalar();
                        return (sonuc != null) ? sonuc.ToString() : null;
                    }
                }
                catch { return null; }
            }
        }

        public static string OgrenciGirisYap(string numara, string sifre)
        {
            using (SQLiteConnection baglanti = new SQLiteConnection(baglantiYolu))
            {
                try
                {
                    baglanti.Open();
                    string sql = "SELECT OgrenciIsmi FROM Ogrenciler WHERE Numara=@no AND Sifre=@sifre";
                    using (SQLiteCommand komut = new SQLiteCommand(sql, baglanti))
                    {
                        komut.Parameters.AddWithValue("@no", numara);
                        komut.Parameters.AddWithValue("@sifre", sifre);
                        object sonuc = komut.ExecuteScalar();
                        return (sonuc != null) ? sonuc.ToString() : null;
                    }
                }
                catch { return null; }
            }
        }

        // =============================================================
        // BÖLÜM 2: ÖĞRETMEN YÖNETİMİ (FORM 2)
        // =============================================================

        public static DataTable OgrencileriGetir(string ogretmenAdi)
        {
            using (SQLiteConnection baglanti = new SQLiteConnection(baglantiYolu))
            {
                try
                {
                    baglanti.Open();
                    string sql = "SELECT OgrenciIsmi 'Öğrenci İsmi', Numara, Sifre 'Şifre', Ogretmen FROM Ogrenciler WHERE Ogretmen=@ogr";

                    using (SQLiteCommand komut = new SQLiteCommand(sql, baglanti))
                    {
                        komut.Parameters.AddWithValue("@ogr", ogretmenAdi);
                        using (SQLiteDataAdapter adaptor = new SQLiteDataAdapter(komut))
                        {
                            DataTable tablo = new DataTable();
                            adaptor.Fill(tablo);
                            return tablo;
                        }
                    }
                }
                catch { return null; }
            }
        }

        public static bool OgrenciEkle(string isim, string numara, string sifre, string ogretmenAdi)
        {
            using (SQLiteConnection baglanti = new SQLiteConnection(baglantiYolu))
            {
                try
                {
                    baglanti.Open();
                    string sql = "INSERT INTO Ogrenciler (OgrenciIsmi, Numara, Sifre, Ogretmen) VALUES (@isim, @no, @sifre, @ogr)";

                    using (SQLiteCommand komut = new SQLiteCommand(sql, baglanti))
                    {
                        komut.Parameters.AddWithValue("@isim", isim);
                        komut.Parameters.AddWithValue("@no", numara);
                        komut.Parameters.AddWithValue("@sifre", sifre);
                        komut.Parameters.AddWithValue("@ogr", ogretmenAdi);
                        komut.ExecuteNonQuery();
                        return true;
                    }
                }
                catch { return false; }
            }
        }

        // Öğrenci Sil
        public static bool OgrenciSil(string numara)
        {
            using (SQLiteConnection baglanti = new SQLiteConnection(baglantiYolu))
            {
                try
                {
                    baglanti.Open();
                    string sql = "DELETE FROM Ogrenciler WHERE Numara=@no";
                    using (SQLiteCommand komut = new SQLiteCommand(sql, baglanti))
                    {
                        komut.Parameters.AddWithValue("@no", numara);
                        komut.ExecuteNonQuery();
                        return true;
                    }
                }
                catch { return false; }
            }
        }

        // Öğretmenin öğrenciye attığı özel mesajları kaydeder
        public static bool MesajGonder(string gonderen, string aliciNo, string mesaj)
        {
            using (SQLiteConnection baglanti = new SQLiteConnection(baglantiYolu))
            {
                try
                {
                    baglanti.Open();
                    string sql = "INSERT INTO Mesajlar (Gonderen, AlanOgrenciNo, MesajIcerigi) VALUES (@gonderen, @aliciNo, @mesaj)";
                    using (SQLiteCommand komut = new SQLiteCommand(sql, baglanti))
                    {
                        komut.Parameters.AddWithValue("@gonderen", gonderen);
                        komut.Parameters.AddWithValue("@aliciNo", aliciNo);
                        komut.Parameters.AddWithValue("@mesaj", mesaj);
                        komut.ExecuteNonQuery();
                        return true;
                    }
                }
                catch { return false; }
            }
        }

        // =============================================================
        // BÖLÜM 3: ÖĞRENCİ ANA SAYFASI (FORM 3) - YENİ EKLENEN
        // =============================================================

        // Öğrenciye gelen özel mesajları getirir
        public static DataTable OgrenciOzelMesajlariGetir(string ogrenciNo)
        {
            using (SQLiteConnection baglanti = new SQLiteConnection(baglantiYolu))
            {
                try
                {
                    baglanti.Open();
                    string sql = "SELECT Gonderen, MesajIcerigi FROM Mesajlar WHERE AlanOgrenciNo = @numara";

                    using (SQLiteCommand komut = new SQLiteCommand(sql, baglanti))
                    {
                        komut.Parameters.AddWithValue("@numara", ogrenciNo);
                        using (SQLiteDataAdapter adaptor = new SQLiteDataAdapter(komut))
                        {
                            DataTable tablo = new DataTable();
                            adaptor.Fill(tablo);
                            return tablo;
                        }
                    }
                }
                catch { return null; }
            }
        }

        #region Sohbet Modülü
        // =============================================================
        // BÖLÜM 4: SOHBET MODÜLÜ (FORM 4)
        // =============================================================

        public static DataRow OgrenciBilgisiGetir(string numara)
        {
            using (SQLiteConnection baglanti = new SQLiteConnection(baglantiYolu))
            {
                try
                {
                    baglanti.Open();
                    string sql = "SELECT OgrenciIsmi, Ogretmen FROM Ogrenciler WHERE Numara=@no";
                    using (SQLiteCommand komut = new SQLiteCommand(sql, baglanti))
                    {
                        komut.Parameters.AddWithValue("@no", numara);
                        using (SQLiteDataAdapter da = new SQLiteDataAdapter(komut))
                        {
                            DataTable dt = new DataTable();
                            da.Fill(dt);
                            if (dt.Rows.Count > 0) return dt.Rows[0];
                            else return null;
                        }
                    }
                }
                catch { return null; }
            }
        }

        public static DataTable SohbetMesajlariniGetir(string ogretmenGrubu)
        {
            using (SQLiteConnection baglanti = new SQLiteConnection(baglantiYolu))
            {
                try
                {
                    baglanti.Open();
                    string sql = "SELECT * FROM GrupSohbeti WHERE OgretmenGrubu=@grup ORDER BY Id ASC";
                    using (SQLiteCommand komut = new SQLiteCommand(sql, baglanti))
                    {
                        komut.Parameters.AddWithValue("@grup", ogretmenGrubu);
                        using (SQLiteDataAdapter da = new SQLiteDataAdapter(komut))
                        {
                            DataTable dt = new DataTable();
                            da.Fill(dt);
                            return dt;
                        }
                    }
                }
                catch { return null; }
            }
        }

        public static bool SohbetMesajiEkle(string isim, string mesaj, string ogretmenGrubu)
        {
            using (SQLiteConnection baglanti = new SQLiteConnection(baglantiYolu))
            {
                try
                {
                    baglanti.Open();
                    string sql = "INSERT INTO GrupSohbeti (GonderenIsmi, Mesaj, Saat, OgretmenGrubu) VALUES (@isim, @mesaj, @saat, @grup)";
                    using (SQLiteCommand komut = new SQLiteCommand(sql, baglanti))
                    {
                        komut.Parameters.AddWithValue("@isim", isim);
                        komut.Parameters.AddWithValue("@mesaj", mesaj);
                        komut.Parameters.AddWithValue("@saat", DateTime.Now.ToString("HH:mm"));
                        komut.Parameters.AddWithValue("@grup", ogretmenGrubu);
                        komut.ExecuteNonQuery();
                        return true;
                    }
                }
                catch { return false; }
            }
        }
        #endregion
        // Sınav bitince puanı kaydetmek için
        public static bool PuanKaydet(string no, string kur, double testPuani, double konusmaPuani)
        {
            string baglantiYolu = @"Data Source=okul.db.db;Version=3;Pooling=False;";
            using (System.Data.SQLite.SQLiteConnection baglanti = new System.Data.SQLite.SQLiteConnection(baglantiYolu))
            {
                try
                {
                    baglanti.Open();
                    // Hem test puanını (testPuani) hem de Konuşma Puanını kaydediyoruz
                    string sql = "INSERT INTO SinavSonuclari (OgrenciNo, Kur, Puan, KonusmaPuani, Tarih) VALUES (@no, @kur, @testPuani, @konusmaPuani, @tarih)";
                    using (System.Data.SQLite.SQLiteCommand komut = new System.Data.SQLite.SQLiteCommand(sql, baglanti))
                    {
                        komut.Parameters.AddWithValue("@no", no);
                        komut.Parameters.AddWithValue("@kur", kur);
                        komut.Parameters.AddWithValue("@testPuani", testPuani);
                        komut.Parameters.AddWithValue("@konusmaPuani", konusmaPuani);
                        komut.Parameters.AddWithValue("@tarih", DateTime.Now.ToString("dd.MM.yyyy HH:mm"));
                        komut.ExecuteNonQuery();
                        return true;
                    }
                }
                catch { return false; }
            }
        }

        // Öğrencinin güncel kurunu hesaplayan metot
        public static string OgrencininKurunuGetir(string ogrenciNo)
        {
            using (SQLiteConnection baglanti = new SQLiteConnection(baglantiYolu))
            {
                try
                {
                    baglanti.Open();
                    // Bu öğrencinin 60 ve üzeri puan aldığı kurları getiriyoruz
                    string sql = "SELECT Kur FROM SinavSonuclari WHERE OgrenciNo=@no AND Puan >= 60";

                    using (SQLiteCommand komut = new SQLiteCommand(sql, baglanti))
                    {
                        komut.Parameters.AddWithValue("@no", ogrenciNo);

                        using (SQLiteDataReader okuyucu = komut.ExecuteReader())
                        {
                            List<string> gecilenKurlar = new List<string>();
                            while (okuyucu.Read())
                            {
                                gecilenKurlar.Add(okuyucu["Kur"].ToString());
                            }

                            // Hangi kurları geçmiş kontrol edip sıradakini veriyoruz
                            if (gecilenKurlar.Contains("B2")) return "MEZUN"; // Hepsini bitirmiş
                            if (gecilenKurlar.Contains("B1")) return "B2";    // B1'i geçmiş, B2'de
                            if (gecilenKurlar.Contains("A2")) return "B1";    // A2'yi geçmiş, B1'de
                            if (gecilenKurlar.Contains("A1")) return "A2";    // A1'i geçmiş, A2'de

                            return "A1"; // Hiçbirini geçemediyse veya kaydı yoksa A1'den başlar
                        }
                    }
                }
                catch { return "A1"; } // Hata olursa mecburen A1 döner
            }
        }
        // =============================================================
        // BÖLÜM 5: YAPAY ZEKA (CHATBOT) HAFIZA İŞLEMLERİ
        // =============================================================

        // Chatbot mesajını veritabanına kaydetme metodu
        public static void BotMesajKaydet(string ogrenciNo, string gonderen, string mesaj)
        {
            using (SQLiteConnection baglanti = new SQLiteConnection(baglantiYolu))
            {
                try
                {
                    baglanti.Open();
                    string sql = "INSERT INTO BotSohbetleri (OgrenciNo, Gonderen, Mesaj) VALUES (@no, @gonderen, @mesaj)";
                    using (SQLiteCommand komut = new SQLiteCommand(sql, baglanti))
                    {
                        komut.Parameters.AddWithValue("@no", ogrenciNo);
                        komut.Parameters.AddWithValue("@gonderen", gonderen); // "user" veya "assistant"
                        komut.Parameters.AddWithValue("@mesaj", mesaj);
                        komut.ExecuteNonQuery();
                    }
                }
                catch { /* Hata olursa sessizce geçebilir veya loglanabilir */ }
            }
        }

        // Öğrenciye özel chatbot sohbet geçmişini getirme metodu (Botun hafızası)
        public static DataTable BotGecmisiniGetir(string ogrenciNo)
        {
            using (SQLiteConnection baglanti = new SQLiteConnection(baglantiYolu))
            {
                try
                {
                    baglanti.Open();
                    // Son 15 mesajı alıp, API'ye sırayla (eskiden yeniye) göndermek için alt sorgu kullanıyoruz
                    string sql = "SELECT Gonderen, Mesaj FROM (SELECT * FROM BotSohbetleri WHERE OgrenciNo = @no ORDER BY Id DESC LIMIT 15) ORDER BY Id ASC";
                    using (SQLiteCommand komut = new SQLiteCommand(sql, baglanti))
                    {
                        komut.Parameters.AddWithValue("@no", ogrenciNo);
                        using (SQLiteDataAdapter adaptor = new SQLiteDataAdapter(komut))
                        {
                            DataTable dt = new DataTable();
                            adaptor.Fill(dt);
                            return dt;
                        }
                    }
                }
                catch { return null; }
            }
        }
        // Öğretmen sayfasında (Form2) puanları listelemek için
        // Öğretmen sayfasında (Form2) puanları listelemek için (Güncellendi!)
        public static DataTable TumPuanlariGetir(string ogretmenAdi)
        {
            using (SQLiteConnection baglanti = new SQLiteConnection(baglantiYolu))
            {
                try
                {
                    baglanti.Open();
                    // Öğrencinin adını, numarasını, GİRDİĞİ KURU, puanını ve tarihi çekiyoruz.
                    // Sadece sisteme giriş yapan öğretmenin (O.Ogretmen = @ogr) öğrencilerini getiririz.
                    string sql = @"SELECT O.OgrenciIsmi AS 'Öğrenci Adı', S.OgrenciNo AS 'Numara', 
                                  S.Kur AS 'Girdiği Kur', S.Puan, S.Tarih 
                           FROM SinavSonuclari S 
                           INNER JOIN Ogrenciler O ON S.OgrenciNo = O.Numara
                           WHERE O.Ogretmen = @ogr
                           ORDER BY S.Tarih DESC";

                    using (SQLiteCommand komut = new SQLiteCommand(sql, baglanti))
                    {
                        komut.Parameters.AddWithValue("@ogr", ogretmenAdi);
                        using (SQLiteDataAdapter adaptor = new SQLiteDataAdapter(komut))
                        {
                            DataTable tablo = new DataTable();
                            adaptor.Fill(tablo);
                            return tablo;
                        }
                    }
                }
                catch { return null; }
            }
        }
    }

}