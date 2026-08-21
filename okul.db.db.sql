BEGIN TRANSACTION;
CREATE TABLE IF NOT EXISTS "GrupSohbeti" (
	"Id"	INTEGER NOT NULL UNIQUE,
	"GonderenIsmi"	TEXT,
	"Mesaj"	TEXT,
	"Saat"	TEXT,
	"OgretmenGrubu"	TEXT,
	PRIMARY KEY("Id" AUTOINCREMENT)
);
CREATE TABLE IF NOT EXISTS "Mesajlar" (
	"Id"	INTEGER NOT NULL,
	"Gonderen"	TEXT,
	"AlanOgrenciNo"	TEXT,
	"MesajIcerigi"	TEXT,
	PRIMARY KEY("Id" AUTOINCREMENT)
);
CREATE TABLE IF NOT EXISTS "OgrenciLoglari" (
	"Id"	INTEGER NOT NULL UNIQUE,
	"OgrenciNo"	TEXT,
	"GirisZamani"	TEXT,
	"CikisZamani"	TEXT,
	"SureDakika"	TEXT,
	PRIMARY KEY("Id" AUTOINCREMENT)
);
CREATE TABLE IF NOT EXISTS "Ogrenciler" (
	"Id"	INTEGER,
	"OgrenciIsmi"	TEXT,
	"Numara"	TEXT,
	"Sifre"	TEXT,
	"Ogretmen"	TEXT,
	PRIMARY KEY("Id")
);
CREATE TABLE IF NOT EXISTS "Ogretmenler" (
	"Id"	INTEGER NOT NULL,
	"KullaniciAdi"	TEXT,
	"Sifre"	TEXT,
	PRIMARY KEY("Id" AUTOINCREMENT)
);
INSERT INTO "GrupSohbeti" VALUES (0,'Ahmet','Hello','11:10',NULL);
INSERT INTO "GrupSohbeti" VALUES (1,'Berke','Deneme ','21:56','admin');
INSERT INTO "GrupSohbeti" VALUES (2,'Zeynep','Başarılı','21:57','admin');
INSERT INTO "GrupSohbeti" VALUES (4,'Ahmet','hello','17:05','Belkıs');
INSERT INTO "GrupSohbeti" VALUES (5,'Ali','zaaa','17:07','Belkıs');
INSERT INTO "GrupSohbeti" VALUES (6,'Berke','Naber','13:15','admin');
INSERT INTO "GrupSohbeti" VALUES (7,'Zeynep','İyi senden','13:15','admin');
INSERT INTO "GrupSohbeti" VALUES (8,'Berke','Ders nasıl','13:18','admin');
INSERT INTO "Mesajlar" VALUES (1,'Öğretmen','148','deneme');
INSERT INTO "Mesajlar" VALUES (2,'Öğretmen','149','deneme2');
INSERT INTO "Mesajlar" VALUES (3,'Öğretmen','111','Bitirdin mi?');
INSERT INTO "Ogrenciler" VALUES (1,'Berke','148','qwe','admin');
INSERT INTO "Ogrenciler" VALUES (2,'Zeynep','149','rty','admin');
INSERT INTO "Ogrenciler" VALUES (3,'Ahmet','111','zxc','Belkıs');
INSERT INTO "Ogrenciler" VALUES (4,'Ali','159','cvb','Belkıs');
INSERT INTO "Ogrenciler" VALUES (5,'Can','199','4321','Belkıs');
INSERT INTO "Ogrenciler" VALUES (6,'deneme','456','456','');
INSERT INTO "Ogrenciler" VALUES (7,'Osman','150','berr','admin');
INSERT INTO "Ogrenciler" VALUES (8,'Enis','753','cvb','admin');
INSERT INTO "Ogrenciler" VALUES (9,'Betül','951','bnm','Belkıs');
INSERT INTO "Ogretmenler" VALUES (1,'admin','1234');
INSERT INTO "Ogretmenler" VALUES (2,'Belkıs','9876');
INSERT INTO "Ogretmenler" VALUES (3,'Eftal','1111');
COMMIT;
