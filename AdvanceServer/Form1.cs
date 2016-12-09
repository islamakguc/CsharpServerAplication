using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NetworksApi.TCP.SERVER; // referens olarak eklediğimiz networksApi kütüphansini tanımlıyoruz Sunucu sayfamız oldugu içim .SERVER yazıyoruz İstemcide .CLİENT olacak 
using System.Collections;
using System.Net;//bilgisayarın İp sini almak için kullanıalcak kütüphane
using System.Net.Sockets;

namespace AdvanceServer
{
    //delegate metod referansını tutar. formlar arasındaki işlemleeri tetiklemek için kullanılır
    public delegate void TextGuncelle(string txt); //Mesaj guncelleme kullanacagımız fonksiyon tanımı
    public delegate void ListGuncelle(ListBox box, string value ,bool remove); //listbox içindekileri guncelleme için kullanacagımız fonksiyon tanımı
    public delegate void SayacGuncelle(int count);//online kullancıları guncelleme kullanacagımız fonksiyon tanımı

    public partial class Form1 : Form
    {
        Server server;

        public Form1()
        {
            InitializeComponent();
        }

        private void Textdegis(string txt)//gelen mesajları ekrana yazdırır
        {
            if (textBox1.InvokeRequired)//istemciden istek gelirse if e gir
            {
                Invoke(new TextGuncelle(Textdegis), new object[] { txt });//invoke metodu nesneye delegate i yönlendiriyor(İstemciden gelen mesajı yazdır)
            }
            else
            {
                if(txt=="")//gelen txt bos ise if'e gir
                {
                    textBox1.Clear();//ekranı temızle
                    textBox1.Text = "Sunucu Hizmeti Durduruldu..\r\n"; //mesaj alanına uyarı verır
                }
                else
                {
                    textBox1.Text += txt + "\r\n"; // mesajları görüntülediğimiz TextBox'a mesajı yazar ve bir alt satıra geçer. 
                    textBox1.SelectionStart = textBox1.Text.Length;// sürekli textbox'ın sonunu gösterir
                    textBox1.ScrollToCaret();// sürekli textbox'ın sonunu gösterir
                }
               
            }
        }

        private void ListDegis(ListBox box, string value, bool remove)//online olan kullanıcılar ekrana yazdırır
        {
            if (box.InvokeRequired)//istemciden istek gelirse if e gir
            {
                Invoke(new ListGuncelle(ListDegis), new object[] { box, value, remove }); //invoke metodu nesneye delegate i yönlendiriyor
            }
            else
            {
                if (remove)// true ise if'e gir
                {
                    box.Items.Remove(value); // value'daki index numaralı kullanıcıyı ListBoxdan sil
                }
                else // false ise
                {
                    box.Items.Add(value); //value'daki index numaralı kullanıcıyı ListBoxa ekle
                }
            }
        }

        private void SayacDegis(int count) // online Kullancı sayısnı güncelle
        {
            if (statusStrip1.InvokeRequired) //eğer yeni kullanıcı girmişse
            {
                Invoke(new SayacGuncelle(SayacDegis), new object[] { count }); //sayıyı güncelle
            }
            else
            {
                toolStripStatusLabel2.Text = count.ToString(); //  sayıyı ekrana yazdır
            }
        }

        private void server_OnServerError(object Sender, ErrorArguments R)//servera bağlanılamdıgında çalışacak fonksiyon
        {
          MessageBox.Show("Server Bağlantısı Kesildi!", "Sunucu Bilgilendirme Penceresi", MessageBoxButtons.OK, MessageBoxIcon.Information);//ekrana hata mesajı verir

        }

        private void server_OnDataReceived(object Sender, ReceivedArguments R)//mesaj alma fonksiyonu
        {
            string yazi = R.Name + " : " + R.ReceivedData; //istemciden gelen mesajı sunucu  ekranına yazar
            Textdegis(yazi); //mesaj ekranını güncelleme fonksiyonu çalışır
            server.BroadCast(R.Name + " : " + R.ReceivedData); // istemcinin sunucuya gönderdiği mesajı kendi mesaj penceresindede yazmaya yarar
        }

        private void server_OnClientDisconnected(object Sender, DisconnectedArguments R)//disconnect durumnda çalışacak fonksiyon
        {
            server.BroadCast(R.Name + " Ayrıldı."); //Ayrılan kullanıcının adını istemci ekranına yazar
            ListDegis(listBox1, R.Name, true); // ListBox1 den kulllanıcı adını siler(true da siler fonk özelliği)
            ListDegis(listBox2, R.Ip, true);    // ListBox2 den kullanıcın ip'sini siler(true da siler fonk özelliği)
            Textdegis(R.Name + " Ayrıldı.");//Kullanıcı sunucudan ayrıldığında ekrana durumunu yazar
            SayacDegis(server.NumberOfConnections);// online kullanıcı sayısını güncceller
            Textdegis("");// textdeğişe boş mesaj atıyoruz boş mesaj geldiğinde Sunucunun kapatıldığını anlıcak ve uyarı mesajı verecek
        }

        private void server_OnClientConnected(object Sender, ConnectedArguments R)//server'a bağlantı durumunda çalışaca fonksiyon
        {
            server.BroadCast(R.Name + " Bağlandı."); // servera bağlanan Kullanıcın adını istemci mesaj panelinde gösterir
            ListDegis(listBox1, R.Name, false); // bağlantı durumunda Kullanıcı adını listBoxa yaz(false da ekler fonk özelliği)
            ListDegis(listBox2, R.Ip, false);   // bağlantı durumunda İP adını listBoxa yaz(false da ekler fonk özelliği)
            Textdegis(R.Name + " Bağlandı.");//Kullanıcı sunucuya bağlandığında Ekrana durumunu yazar
            SayacDegis(server.NumberOfConnections); // online kullanıcı sayısını güncceller 
        }

        private void button1_Click(object sender, EventArgs e)  // tek istemciye mesaj atma butonuna basıldığında çalışacak fonksiyon
        {
            if (listBox1.SelectedIndex != -1) // Listboxda isim seçili ise if'e gir
            {
                if (textBox2.Text != "") //mesaj paneli boş değilse
                {
                    string yazi = "Sunucu (Private) : " + textBox2.Text;
                    server.SendTo((string)listBox1.SelectedItem, yazi);// sunucun yazdığı mesajı istemciye gönder
                    Textdegis(yazi);//mesaj ekranını güncelleme fonksiyonu çalışır
                    textBox2.Clear(); //mesaj ekranımızı temizler
                    textBox1.SelectionStart = textBox1.Text.Length;// sürekli textbox'ın sonunu gösterir
                    textBox1.ScrollToCaret();// sürekli textbox'ın sonunu gösterir
                }
                else//mesaj yazmamışsak çalışır
                {
                    MessageBox.Show("Lütfen Mesajınızı yazınız!", "Sunucu Bilgilendirme Penceresi", MessageBoxButtons.OK, MessageBoxIcon.Information);//mesaj yazmamışsak ekrana uyarı verir
                }
           }
           else
           {
                MessageBox.Show("Lütfen Kullanıcı Seciniz!", "Sunucu Bilgilendirme Penceresi", MessageBoxButtons.OK, MessageBoxIcon.Information);//Kullanıcı seçmememişsek ekrana uyarı verir
           }

        }

        private void button2_Click(object sender, EventArgs e)//herkese mesaj gönder butonuna basıldığında çalışacak fonksiyonu
        {
            if (textBox2.Text != "")//mesaj paneli boş değilse if'e gir
            {
                string yazi = "Sunucu (Public) : " + textBox2.Text;
                server.BroadCast(yazi); // tüm istemcilere yazdığımız mesaj gider
                Textdegis(yazi);//mesaj ekranını güncelleme fonksiyonu çalışır
                textBox2.Clear();//mesaj ekranımızı temizler
                textBox1.SelectionStart = textBox1.Text.Length;// sürekli textbox'ın sonunu gösterir
                textBox1.ScrollToCaret();// sürekli textbox'ın sonunu gösterir
            }
            else
            {
                MessageBox.Show("Lütfen Mesajınızı yazınız!", "Sunucu Bilgilendirme Penceresi", MessageBoxButtons.OK, MessageBoxIcon.Information); //mesaj panalinde bişey yazmıyorsa ekrana uyarı ver
            }
        }

        private void button3_Click(object sender, EventArgs e)// kullanıcı atma butonuna basıldığında çalışacak fonksiyonu
        {

            if (listBox1.SelectedIndex != -1) // Listbox1 de elaman seçili ise bu if'e girer
            {
                server.DisconnectClient((string)listBox1.SelectedItem);//listBox 1 de kim seçili ise onu server dan atmaya yarar
                server.DisconnectClient((string)listBox2.SelectedItem); //listBox 2 de hangi ip seçili ise onu server dan atmaya yarar
            }
            else
            {
                MessageBox.Show("Lütfen Kullanıcı Seciniz!", "Sunucu Bilgilendirme Penceresi", MessageBoxButtons.OK, MessageBoxIcon.Information); //seçili değilse ekrana uyarı ver 
            }

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)//Form kapatıltığında çalışacak fonksiyon
        {
            System.Environment.Exit(System.Environment.ExitCode);//uygulamadan çıkmak için Kullanılır
        }
      
        private void button4_Click(object sender, EventArgs e) // server başlat butonuna basıldığında çalışan fonksiyonu
        {
            if (textBox3.Text != "" && textBox4.Text != "") //ip ve port boş değilse if'e gir
            {
                try
                {
                    TcpClient tcp = new TcpClient();//tcpClient değişkenı olusturuyoruz
                    tcp.Connect(textBox3.Text, int.Parse(textBox4.Text)); //tcp yi çalıştırıp ip ve port kontrolu yapıyoruz
                    MessageBox.Show("Bu Port Numarası Kullanılamaz!", "Sunucu Bilgilendirme Penceresi", MessageBoxButtons.OK, MessageBoxIcon.Information);   // ekrana uyarı verir
                }
                catch (Exception)//Port Kullanımda değilse burası calışır
                {
                    server = new Server(textBox3.Text, textBox4.Text); // gelen ip ve port le server tanımlar
                    server.OnClientConnected += new OnConnectedDelegate(server_OnClientConnected); //bağlantı fonk tanımlar
                    server.OnClientDisconnected += new OnDisconnectedDelegate(server_OnClientDisconnected);//çıkış fonk tanımlar
                    server.OnDataReceived += new OnReceivedDelegate(server_OnDataReceived);//mesaj alma fonk tanımlar
                    server.OnServerError += new OnErrorDelegate(server_OnServerError); //hata durumnda çalışacak fonk tanımlar
                    server.Start(); //server'ı başlatır
                    textBox1.Text = "Sunucu Hizmeti Aktifleştirildi..\r\n"; //mesaj ekranına server'ın başladığını yazar
                    button5.Enabled = true; //server durdur butonunu aktif eder
                    button1.Enabled = true;// tek kişiye gönder butonunu aktif eder
                    button2.Enabled = true;//herkes e gönder butonunu aktif eder
                    button3.Enabled = true;// kullanıcı at butonunu aktif eder
                    button4.Enabled = false; // server başlat butonuny pasif eder
                }
            }
            else //ip ve port boş ise çalışır
            {
                MessageBox.Show("Lütfen TextBox'ları doldurunuz!", "Sunucu Bilgilendirme Penceresi", MessageBoxButtons.OK, MessageBoxIcon.Information);   // ekrana uyarı verir
            }
       }

        private void button5_Click(object sender, EventArgs e) //server durdur butonuna basıldığında çalışacak fonksiyonu
        {
            server.Stop();// server durdur
            button4.Enabled = true; //başlat butonunu aktif eder
            button5.Enabled = false; //durdur butonunu pasif eder
            button1.Enabled = false;// tek kişiye gönder butonunu pasif eder
            button2.Enabled = false;//herkes e gönder butonunu pasif eder
            button3.Enabled = false;// Kullancı at butonunu pasif eder
        }

        private void listBox1_MouseDoubleClick_1(object sender, MouseEventArgs e)// ListBox1 deki isime çift tıklandığında çalışacak fonksiyon
        {
            if(listBox1.SelectedIndex != -1 )//listbox1 de isim secili ise if'e gir
            {
                string ip = listBox2.SelectedItem.ToString(); // secili ip yi ip değişkenine at
                string name = listBox1.SelectedItem.ToString(); //secili isimi yi name değişkenine at
                MessageBox.Show("' "+name+ " '"+ " ip Adresi : " + ip, "Sunucu Bilgilendirme Penceresi", MessageBoxButtons.OK, MessageBoxIcon.Information);// ekrana uyarı olarak isim ve ip ' i yazdır
            }
            else//listbox1 de isim secili değil ise 
            {
                MessageBox.Show("Aktif kullanıcı bulunamadı!", "Sunucu Bilgilendirme Penceresi", MessageBoxButtons.OK, MessageBoxIcon.Information);//ekrana uyarı ver
            }
            
        }

        private void listBox1_SelectedIndexChanged_1(object sender, EventArgs e)// listbox1'de isimlere tek tıklandıgında çalışacak fonksiyon
        {
            listBox2.SelectedIndex = listBox1.SelectedIndex;
            //listbox 1 de seçtiğimiz ismin index'ini alır. aldığımız index numarasını kacsa listbox 2'de de o numaralı satırı seçmemize yarar
            //listBox 1'in 10 satırında islam olduğunu düşünelim listbox2'nin 10 satırında ise islamın ip numarası Listbox1 de islamı secersek listbox 2 de 10 numaralı
            //ip yi yani islamın ip sini seçer
        }

        private void button6_Click(object sender, EventArgs e)//temizle butonuna basıldığında çalışacak fonksiyon
        {
            textBox1.Clear();//gelen giden mesajların gösterildigi paneli temizler
        }

        private void Form1_Load(object sender, EventArgs e)//Uygulama başladığında çalışacak fonksiyon
        {
            string ipAdresi = Dns.GetHostByName(Dns.GetHostName()).AddressList[1].ToString();//ağ içindeki İp adresimizi bulur

            DialogResult secenek = MessageBox.Show("İp adresinizi kullanmak istiyor musunuz?\n\n\n İp Adresiniz : "+ipAdresi, " Sunucu Bilgilendirme Penceresi", MessageBoxButtons.YesNo, MessageBoxIcon.Information);//ekrana Onay mesajı verir

            if (secenek == DialogResult.Yes)//onay mesajına yes dersek if'e girer
            {
                textBox3.Text =ipAdresi;//İp mizi İp textine yazar
            }
        }
    }     
}
