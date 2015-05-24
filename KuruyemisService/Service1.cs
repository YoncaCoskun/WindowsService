using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Migrations;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KuruyemisService
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
            Thread.Sleep(10000);
            Calis();
        }

        YilmazlarKuruyemisEntities context = new YilmazlarKuruyemisEntities();
        int count = 0;
        List<string> UrunBilgileri = new List<string>();
        
        public void Calis()
        {
            try
            {

           

            DirectoryInfo klasorler = new DirectoryInfo(@"C:\Dosyalar");
            FileInfo[] dosyalar = klasorler.GetFiles();


            foreach (FileInfo dosya in dosyalar)
            {
                StreamReader subeOkuyucu = new StreamReader(dosya.FullName, Encoding.GetEncoding("windows-1254"));
                string a = subeOkuyucu.ReadToEnd();
                string subeAdi = a.Split(':')[1];
          
                context.tblSubes.Add(new tblSube(){ SubeAd = subeAdi});
                context.SaveChanges();
                subeOkuyucu.Close();


                StreamReader okuyucu =new StreamReader(dosya.FullName,Encoding.GetEncoding("windows-1254"));
                string satir;
                while ((satir = okuyucu.ReadLine()) != null)
                {
                   
                   
                    if (satir.Split(':')[0]=="Şube")
                    {                       
                        break;
                       
                    }                      
                    else
                    {
                        if (count != 0)
                        {
                            string[] gelenler = satir.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);


                            foreach (string gelen in gelenler)
                            {
                                UrunBilgileri.Add(gelen);

                            }
                            string urunId = UrunBilgileri[0];
                            string urunAd = UrunBilgileri[1];
                            string fiyat = UrunBilgileri[2];
                            string stok = UrunBilgileri[3];

                            int urunID = Convert.ToInt32(urunId);
                            tblUrun guncellenecekUrun = context.tblUruns.FirstOrDefault(x => x.UrunId == urunID);

                            if (guncellenecekUrun != null)
                            {
                                guncellenecekUrun.UrunAd = urunAd;
                                guncellenecekUrun.Fiyat = Convert.ToDecimal(fiyat);
                                guncellenecekUrun.Stok = Convert.ToInt32(stok);


                                context.SaveChanges();
                            }
                            else
                            {
                                tblUrun NewUrun = new tblUrun();

                                NewUrun.UrunId = Convert.ToInt32(urunId);
                                NewUrun.UrunAd = urunAd;
                                NewUrun.Fiyat = Convert.ToDecimal(fiyat);
                                NewUrun.Stok = Convert.ToInt32(stok);

                                context.tblUruns.Add(NewUrun);
                                context.SaveChanges();


                            }

                            string dosyaTarihi = Path.GetFileName(dosya.Name);
                            dosyaTarihi = Path.GetFileNameWithoutExtension(dosyaTarihi.Substring(dosyaTarihi.LastIndexOf('a') + 1));
                            string islemTarihi = DateTime.Now.ToString();
                            int sonuc = context.tblSubes.FirstOrDefault(y => y.SubeAd == subeAdi).SubeId;
                            

                            tblUrunGecmisi urunGecmisi = new tblUrunGecmisi();

                            urunGecmisi.UrunId = Convert.ToInt32(urunId);
                            urunGecmisi.UrunAd = urunAd;
                            urunGecmisi.Fiyat = Convert.ToDecimal(fiyat);
                            urunGecmisi.Stok = Convert.ToInt32(stok);
                            urunGecmisi.IslemTarihi = DateTime.Parse(islemTarihi);
                            urunGecmisi.DosyaTarihi = DateTime.ParseExact(dosyaTarihi,"ddMMyyyy", CultureInfo.InvariantCulture);
                            urunGecmisi.SubeId = sonuc;


                            context.tblUrunGecmisis.Add(urunGecmisi);
                            context.SaveChanges();



                        }
                   

                    }
                    UrunBilgileri.Clear();
                    count++;


                }

                okuyucu.Close();
                count = 0;
                dosya.Delete();
                LogYaz("Dosya aktarıldı. " + dosya.Name);
                 }
            Thread.Sleep(20000);
            Calis();
            }
            catch (Exception ex)
            {
                LogYaz(ex.Message);
            }
        }
        protected override void OnStart(string[] args)
        {
            LogYaz("Servis Başlatıldı");
        }

        protected override void OnStop()
        {
            LogYaz("Servis Durduruldu");
        }
        void LogYaz(string mesaj)
        {
            string folder = @"C:\Loglar";
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string path = folder + @"\log.txt";

            StreamWriter logYazici = new StreamWriter(path, true);
            logYazici.WriteLine(DateTime.Now + " " + mesaj);
            logYazici.Close();
        }
    }
}
