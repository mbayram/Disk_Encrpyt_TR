using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Management;
using System.Runtime;
using System.IO;
//using Microsoft.WindowsAPICodePack.Shell;
//using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using System.Diagnostics;
using System.Threading;
using USB_detect_service;
using System.Xml.Serialization;
using System.Runtime.InteropServices;
using MRG;
using System.Net.NetworkInformation;
using System.DirectoryServices.ActiveDirectory;

namespace WindowsFormsApp1
{

    public partial class Form1 : Form
    {
        //public readonly IUsbEventWatcher usbEventWatcher = new UsbEventWatcher();
        public string deviceid_encryting;
        public string allowed_users = null;
        static string logfile = "C:\\Windows\\Logs\\usb.log";
        static string diskpartfile = "C:\\Windows\\Logs\\diskpart.txt";
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd,
                         int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        public Form1()
        {
            InitializeComponent();

        }
        public void get_usblist()
        {
            listView2.Items.Clear();
            var driveList = DriveInfo.GetDrives();

            foreach (DriveInfo drive in driveList)
            {
                //ListViewItem lvi1 = new ListViewItem(drive.Name);
                if (drive.IsReady == false)
                {

                    File.AppendAllText(logfile, DateTime.Now.ToString() + "     " + drive + "Disk Kilitli" + Environment.NewLine);

                    listView2.Items.Add(drive.ToString() + " \r\n " + "  KİLİTLİ", 2);

                    //lvi1.SubItems.Add(drive.Name);
                    //lvi1.SubItems.Add("KİLİTLİ");
                    //lvi1.ImageIndex = 2;
                    //listView2.Items.Add(lvi1);
                }
                else

                {
                    if ((drive.DriveType == DriveType.Fixed || drive.DriveType == DriveType.Removable) && drive.Name.ToString() != "C:\\")
                    //if (drive.Name.ToString() != "C:\\")
                    {

                        //MessageBox.Show(drive.Name);
                        string a = drive.Name.Trim('\\').ToString();
                        //MessageBox.Show(a);
                        string query = "SELECT * FROM Win32_EncryptableVolume WHERE DriveLetter = " + "'" + a + "'";
                        //            //MessageBox.Show(query);
                        ManagementObjectSearcher searcher =
                       new ManagementObjectSearcher("root\\cimv2\\Security\\MicrosoftVolumeEncryption", query);

                        foreach (ManagementObject queryObj in searcher.Get())
                        {

                            string driveletter = @"Drive DriveLetter: " + queryObj["DriveLetter"].ToString();
                            File.AppendAllText(logfile, DateTime.Now.ToString() + "     " + "Drive DriveLetter: " + driveletter + Environment.NewLine);
                            //string drivestatus = @"Drive DriveLetter: " + queryObj["ProtectionStatus"].ToString();
                            string drivestatus = queryObj["ProtectionStatus"].ToString();
                            File.AppendAllText(logfile, DateTime.Now.ToString() + "     " + "ProtectionStatus: " + drivestatus + Environment.NewLine);
                            string ConversionStatus = queryObj["ConversionStatus"].ToString();
                            File.AppendAllText(logfile, DateTime.Now.ToString() + "     " + "ConversionStatus: " + ConversionStatus + Environment.NewLine);

                            string DeviceID_list = queryObj["DeviceID"].ToString();
                            File.AppendAllText(logfile, DateTime.Now.ToString() + "     " + "DeviceID: " + DeviceID_list + Environment.NewLine);


                            // 0 FullyDecrypted  For a standard hard drive (HDD), the volume is fully decrypted. For a hardware encrypted hard drive (EHDD), the volume is perpetually unlocked.
                            // 1 FullyEncrypted  For a standard hard drive (HDD), the volume is fully encrypted.For a hardware encrypted hard drive (EHDD), the volume is not perpetually unlocked.
                            // 2 EncryptionInProgress  The volume is partially encrypted.
                            // 3 DecryptionInProgress  The volume is partially encrypted.
                            // 4 EncryptionPaused  The volume has been paused during the encryption progress. The volume is partially encrypted.
                            // 5 DecryptionPaused  The volume has been paused during the decryption progress. The volume is partially encrypted.

                            if (ConversionStatus == "1")
                            {
                                listView2.Items.Add(queryObj["DriveLetter"].ToString() + " \r\n " + drive.VolumeLabel.ToString() + " \r\n BİTLOCKER AKTİF", 0);
                                //string enc_yuzde = bitlocker_status(DeviceID);
                                //lvi1.SubItems.Add("BİTLOCKER AKTİF");
                                //lvi1.ImageIndex = 0;
                                //lvi1.SubItems.Add(enc_yuzde);


                            }
                            else if (ConversionStatus == "0")
                            {
                                listView2.Items.Add(queryObj["DriveLetter"].ToString() + " \r\n " + drive.VolumeLabel.ToString() + " \r\n ŞİFRELENMEMİŞ", 1);
                                //lvi1.SubItems.Add("ŞİFRELENMEMİŞ");
                                //lvi1.ImageIndex = 1;
                            }
                            else if (ConversionStatus == "2")
                            {
                                //string enc_yuzde = bitlocker_status(DeviceID);
                                listView2.Items.Add(queryObj["DriveLetter"].ToString() + " \r\n " + drive.VolumeLabel.ToString() + " \r\n ŞİFRELENİYOR", 3);
                                deviceid_encryting = DeviceID_list;
                                //lvi1.SubItems.Add("ŞİFRELENMEMİŞ");
                                //lvi1.ImageIndex = 1;
                                timer1.Enabled = true;
                            }
                            else if (ConversionStatus == "4")
                            {
                                listView2.Items.Add(queryObj["DriveLetter"].ToString() + " \r\n " + drive.VolumeLabel.ToString() + " \r\n Şifreleme İşlemi Durdu", 4);
                                //lvi1.SubItems.Add("ŞİFRELENMEMİŞ");
                                //lvi1.ImageIndex = 1;
                            }
                            else
                            {


                                listView2.Items.Add(queryObj["DriveLetter"].ToString() + " \r\n " + drive.VolumeLabel.ToString() + " \r\n Şifreleme İptal Ediliyor", 5);
                                //lvi1.SubItems.Add("ŞİFRELENİYOR");
                                //lvi1.ImageIndex = 0;
                                //lvi1.SubItems.Add(enc_yuzde);
                                //toolStripProgressBar1.Value = Convert.ToInt32(enc_yuzde);
                                //toolStripStatusLabel1.Text = selected_drive;
                                //toolStripStatusLabel2.Text = enc_yuzde+ "%";

                            }

                            //listView2.Items.Add(lvi1);
                        }
                    }

                }

            }
        }



        private void Form1_Load(object sender, EventArgs e)
        {
            lblexit.Visible = false;
            button1.Select();

            loadingCircle1.Visible = false;

            font_set(1, label2);
            font_set(1, label3);
            font_set(1, label4);
            toolStripProgressBar1.Visible = false;

            get_usblist();


        }

        private void button1_Click(object sender, EventArgs e)
        {

            get_usblist();
            //listenusb();

        }
        public string selected_drive;
        public string DeviceID_selected;
        private void button2_Click(object sender, EventArgs e)
        {
            toolStripProgressBar1.Value = 0;
            toolStripProgressBar1.Visible = true;

            if (nls_check() == true)
            {

                       get_user();
            //MessageBox.Show(allowed_users.ToString());
                selected_drive = listView2.SelectedItems[0].Text.Substring(0, 2);
                DriveInfo driver = new DriveInfo(selected_drive);
                if (driver.IsReady == false)
            {

                MessageBox.Show(" DİSK FARKLI BİR UYGULAMA İLE KORUMA ALTINA ALINMIŞTIR. \n KORUMAYI DEVRE DIŞI BIRAKIP YADA FORMATLAYARAK TEKRAR DENEYİNİZ. ");
            }
                else
            {

                string query = "SELECT * FROM Win32_EncryptableVolume WHERE DriveLetter = " + "'" + selected_drive + "'";
                //            //MessageBox.Show(query);
                ManagementObjectSearcher searcher =
               new ManagementObjectSearcher("root\\cimv2\\Security\\MicrosoftVolumeEncryption", query);

                foreach (ManagementObject queryObj in searcher.Get())
                {

                    string driveletter = @"Drive DriveLetter: " + queryObj["DriveLetter"].ToString();
                    //string drivestatus = @"Drive DriveLetter: " + queryObj["ProtectionStatus"].ToString();
                    string drivestatus = queryObj["ProtectionStatus"].ToString();
                    File.AppendAllText(logfile, DateTime.Now.ToString() + "     " + driveletter + "   " + "ProtectionStatus: " + drivestatus + Environment.NewLine);
                    string ConversionStatus = queryObj["ConversionStatus"].ToString();
                    File.AppendAllText(logfile, DateTime.Now.ToString() + "     " + driveletter + "   " + "ConversionStatus: " + ConversionStatus + Environment.NewLine);

                    DeviceID_selected = queryObj["DeviceID"].ToString();
                    //textBox1.AppendText(DeviceID_selected + "\r\n");
                    File.AppendAllText(logfile, DateTime.Now.ToString() + "     " + driveletter + "   " + "DeviceID: " + DeviceID_selected + Environment.NewLine);

                    toolStripStatusLabel1.Text = selected_drive;
                    if (ConversionStatus == "1")
                    {
                        //listView2.Items.Add(queryObj["DriveLetter"].ToString() + " \r\n " + drive.VolumeLabel.ToString() +" \r\n BİTLOCKER AKTİF", 0);

                        MessageBox.Show(" SEÇİLEN DİSK ŞİFRELENMİŞ OLDUĞUNDAN ŞİFRELEME YAPILAMAZ");
                        //toolStripProgressBar1.Value = Convert.ToInt32(enc_yuzde);
                        //toolStripStatusLabel1.Text = selected_drive;
                        //toolStripStatusLabel2.Text = enc_yuzde;
                    }
                    else if (ConversionStatus == "0")
                    {


                       

                        DialogResult dialog = new DialogResult();
                        dialog = MessageBox.Show("Seçilen Disk " + selected_drive + " için  Şifreleme işlemi Başlayacaktır.", "Disk Şifreleniyor", MessageBoxButtons.YesNo);
                        if (dialog == DialogResult.Yes)
                        {
                            font_set(2, label2);
                            font_set(1, label3);
                            font_set(1, label4);
                            usb_encrypt();

                            

                        }
                        else
                        {
                                MessageBox.Show("Disk Şifreleme işlemini İPTAL ettiniz. Uygulama kapatılacaktır.");
                                this.Close();
                        }


                    }
                    else
                    {

                        MessageBox.Show(" DİSK ŞİFRELENMİŞ OLDUĞUNDAN ŞİFRELENEMEZ. ");

                    }

                }

            }
            }
            else
            {
                MessageBox.Show("ŞİRKET AĞINA BAĞLI DEĞİLSİNİZ. ŞİFRELEME YAPILAMAZ");
                this.Close();
            }
        }




        private delegate void setTextLblInvoker(string item, TextBox textBox);
        private void SetTextLbl(string item, TextBox textBox)
        {



            textBox.AppendText(item);

        }


        private async void usb_encrypt()
        {
            loadingCircle1.Visible = true;
            loading();
            button2.Enabled = false;
            listView2.Enabled = false;
    IntPtr wow64Value = IntPtr.Zero;
            try
            {
                await Task.Run(() =>
                {

                    //MessageBox.Show(allowed_users);
                    Application.DoEvents();

                    //if (listView2.SelectedItems.Count > 0)
                    //{
                    string surucu = selected_drive;
                    //textBox1.Invoke(new setTextLblInvoker(SetTextLbl), "--------------------------------------------------------------------" + Environment.NewLine, textBox1);
                    //textBox1.AppendText("1--------------------------------------------------------------------" + Environment.NewLine);
                    //textBox1.AppendText(DateTime.Now.ToString() + "   " + surucu + " Seçildi." + Environment.NewLine);
                    //textBox1.Invoke(new setTextLblInvoker(SetTextLbl), DateTime.Now.ToString() + "   " + surucu + " Seçildi." + Environment.NewLine, textBox1);

                    //textBox1.AppendText(DateTime.Now.ToString() + "   C:\\windows\\System32\\manage-bde.exe " + " -on " + surucu + " -rp -used" + "     Komutu Çalıştırılıyor.\n" + Environment.NewLine);
                    //textBox1.Invoke(new setTextLblInvoker(SetTextLbl), DateTime.Now.ToString() + "   C:\\windows\\System32\\manage-bde.exe " + " -on " + surucu + " -rp -used" + "     Komutu Çalıştırılıyor.\n" + Environment.NewLine, textBox1);

                    File.AppendAllText(logfile, DateTime.Now.ToString() + "\t" + surucu + " icin attributes readonly degeri kaldırılıyor " + "\n");
                    string[] lines = { "select volume " + surucu, "attributes disk clear readonly" };
                    // WriteAllLines creates a file, writes a collection of strings to the file,
                    // and then closes the file.  You do NOT need to call Flush() or Close().
                    System.IO.File.WriteAllLines(diskpartfile, lines);

                    File.AppendAllText(logfile, DateTime.Now.ToString() + "\t" + "added diskpart.txt file : select volume" + surucu + "\n");
                    File.AppendAllText(logfile, DateTime.Now.ToString() + "\t" + "added diskpart.txt file : attributes disk clear readonly" + surucu);


                    Process dskcmd1 = new Process();
                    dskcmd1.StartInfo.FileName = @"C:\windows\System32\diskpart.exe";
                    dskcmd1.StartInfo.Arguments = " /s  " + diskpartfile;
                    dskcmd1.StartInfo.UseShellExecute = false;
                    dskcmd1.StartInfo.CreateNoWindow = true;
                    dskcmd1.StartInfo.RedirectStandardOutput = true;
                    dskcmd1.StartInfo.RedirectStandardError = true;
                    dskcmd1.Start();
                    dskcmd1.WaitForExit(3000);
                    dskcmd1.Close();

                    File.AppendAllText(logfile, DateTime.Now.ToString() + "     " + "--------------------------------------------------------------------" + Environment.NewLine);
                    File.AppendAllText(logfile, DateTime.Now.ToString() + "     " + surucu + " Selected." + Environment.NewLine);
                    File.AppendAllText(logfile, DateTime.Now.ToString() + "     " + "   C:\\windows\\System32\\manage-bde.exe " + " -on " + surucu + " -rp -used" + "     Command started.\n" + Environment.NewLine);
                    Wow64Interop.DisableWow64FSRedirection(ref wow64Value);
                    Process bit_cmd1 = new Process();
                    bit_cmd1.StartInfo.FileName = @"C:\\windows\System32\manage-bde.exe";
                    bit_cmd1.StartInfo.Arguments = " -on " + surucu + " -rp -used";
                    bit_cmd1.StartInfo.UseShellExecute = false;
                    bit_cmd1.StartInfo.CreateNoWindow = true;
                    bit_cmd1.StartInfo.RedirectStandardOutput = true;
                    bit_cmd1.StartInfo.RedirectStandardError = true;
                    bit_cmd1.Start();
                    bit_cmd1.WaitForExit(3000);
                    string sonuc = bit_cmd1.StandardOutput.ReadToEnd();
                    //textBox1.Text = sonuc.ToString();
                    bit_cmd1.Close();
                    //textBox1.AppendText(DateTime.Now.ToString() + "   C:\\windows\\System32\\manage-bde.exe " + " -on " + surucu + " -rp -used" + "     Komutu Tamamlandı.\n" + Environment.NewLine);
                    //textBox1.Invoke(new setTextLblInvoker(SetTextLbl), DateTime.Now.ToString() + "   C:\\windows\\System32\\manage-bde.exe " + " -on " + surucu + " -rp -used" + "     Komutu Tamamlandı.\n" + Environment.NewLine, textBox1);
                    File.AppendAllText(logfile, DateTime.Now.ToString() + "     " + "   C:\\windows\\System32\\manage-bde.exe " + " -on " + surucu + " -rp -used" + "     Komutu Tamamlandi.\n" + Environment.NewLine);
                    File.AppendAllText(logfile, DateTime.Now.ToString() + "     " + "   C:\\windows\\System32\\manage-bde.exe " + " -protectors -add -SID " + "\"" + allowed_users + "\" " + surucu + "     Command started.\n" + Environment.NewLine);
                    //textBox1.Invoke(new setTextLblInvoker(SetTextLbl), DateTime.Now.ToString() + "   C:\\windows\\System32\\manage-bde.exe " + " -protectors -add -SID " + "\"" + allowed_users + "\" " + surucu + "     Komutu Çalıştırılıyor.\n" + Environment.NewLine, textBox1);
                    int pform = sonuc.IndexOf("{") + "{".Length;
                    int pto = sonuc.LastIndexOf("}");
                    string USB_id = sonuc.Substring(pform, pto - pform);
                    //MessageBox.Show(USB_id);

                    Process bit_cmd2 = new Process();
                    bit_cmd2.StartInfo.FileName = @"C:\\windows\System32\manage-bde.exe";
                    bit_cmd2.StartInfo.Arguments = " -protectors -add -SID " + "\"" + allowed_users + "\" " + surucu;
                    bit_cmd2.StartInfo.UseShellExecute = false;
                    bit_cmd2.StartInfo.CreateNoWindow = true;
                    bit_cmd2.StartInfo.RedirectStandardOutput = true;
                    bit_cmd2.StartInfo.RedirectStandardError = true;
                    bit_cmd2.Start();
                    bit_cmd2.WaitForExit(3000);
                    //textBox1.AppendText(bit_cmd2.StandardOutput.ReadToEnd());
                    File.AppendAllText(logfile, DateTime.Now.ToString() + "     " + "   C:\\windows\\System32\\manage-bde.exe " + " -protectors -add -SID " + "\"" + allowed_users + "\" " + surucu + "     Command completed.\n" + Environment.NewLine);
                    File.AppendAllText(logfile, DateTime.Now.ToString() + "     " + "   C:\\windows\\System32\\manage-bde.exe " + "-protectors - adbackup " + surucu + " - id " + "{ " + USB_id + "}" + "     Command started.\n" + Environment.NewLine);


                    bit_cmd2.Close();

                    Process bit_cmd3 = new Process();
                    bit_cmd3.StartInfo.FileName = @"C:\\windows\System32\manage-bde.exe";
                    bit_cmd3.StartInfo.Arguments = " -protectors -adbackup " + surucu + " -id " + "{" + USB_id + "}";
                    bit_cmd3.StartInfo.UseShellExecute = false;
                    bit_cmd3.StartInfo.CreateNoWindow = true;
                    bit_cmd3.StartInfo.RedirectStandardOutput = true;
                    bit_cmd3.StartInfo.RedirectStandardError = true;
                    bit_cmd3.Start();
                    bit_cmd3.WaitForExit(3000);
                    //textBox1.AppendText(bit_cmd3.StandardOutput.ReadToEnd());
                    bit_cmd3.Close();
                    File.AppendAllText(logfile, DateTime.Now.ToString() + "     " + "   C:\\windows\\System32\\manage-bde.exe " + "-protectors - adbackup " + surucu + " - id " + "{ " + USB_id + "}" + "     Command completed.\n" + Environment.NewLine);
                    File.AppendAllText(logfile, DateTime.Now.ToString() + "     " + "{ " + USB_id + "}" + "     Diskin recovery Sifresi AD'ye yedeklendi.\n" + Environment.NewLine);
                    File.AppendAllText(logfile, DateTime.Now.ToString() + "     " + "--------------------------------------------------------------------\n" + Environment.NewLine);
                    //
                    //MessageBox.Show("Diskin Şifreleme Aktif Edildi. \r\n Diskinizin şifrelenmesi arkaplanda devam etmektedir. ");

                });
                listView2.Enabled = true;
                get_usblist();
                font_set(0, label2);
                font_set(2, label3);
                font_set(1, label4);
                //}
                //else
                //{
                //    MessageBox.Show(" Disk Seçilmedi");
                //}
            }
            catch (Exception hata)
            {
                MessageBox.Show(hata.Message);
                File.AppendAllText(logfile, DateTime.Now.ToString() + "Unabled to disable/enable WOW64 File System Redirection");
                File.AppendAllText(logfile, DateTime.Now.ToString() + hata.Message);
            }
            finally
            {
                Wow64Interop.Wow64RevertWow64FsRedirection(wow64Value);
            }
        }

        //private void eventUsbDriveInserted(string path)
        //{

        //    //Console.WriteLine($"Inserted: {path}");
        //    textBox1.AppendText(path.ToString());
        //    get_usblist();




        //}
        //private void eventUsbDriveRemoved(string path)
        //{

        //    textBox1.AppendText(path.ToString());
        //    get_usblist();

        //}
        //private void eventUsbDeviceInserted(UsbDevice device)
        //{

        //    textBox1.AppendText(device.DeviceName.ToString());

        //}
        //private void eventUsbDeviceRemoved(UsbDevice device)
        //{

        //    textBox1.AppendText(device.DeviceName.ToString());

        //}

        private string bitlocker_status(string deviceid)
        {
            //try
            //{
            string deviceid_query = "Win32_EncryptableVolume.DeviceID=" + "'" + deviceid + "'";
            //MessageBox.Show(deviceid_query);
            ManagementObject classInstance =
                new ManagementObject("root\\cimv2\\Security\\MicrosoftVolumeEncryption", deviceid_query, null);

            // Obtain in-parameters for the method
            ManagementBaseObject inParams =
                classInstance.GetMethodParameters("GetConversionStatus");

            // Add the input parameters.

            // Execute the method and obtain the return values.
            ManagementBaseObject outParams =
                classInstance.InvokeMethod("GetConversionStatus", inParams, null);

            // List outParams
            Console.WriteLine("Out parameters:");
            string ConversionStatus = outParams["ConversionStatus"].ToString();
            File.AppendAllText(logfile, DateTime.Now.ToString() + "     " + deviceid + "ConversionStatus: " + ConversionStatus + "\r\n");
            string EncryptionFlags = outParams["EncryptionFlags"].ToString();
            File.AppendAllText(logfile, DateTime.Now.ToString() + "     " + deviceid + "EncryptionFlags: " + EncryptionFlags + "\r\n");
            string EncryptionPercentage = outParams["EncryptionPercentage"].ToString();
            File.AppendAllText(logfile, DateTime.Now.ToString() + "     " + deviceid + "EncryptionPercentage: " + EncryptionPercentage + "\r\n");
            toolStripProgressBar1.Visible = true;
            toolStripStatusLabel1.Text = selected_drive;
            toolStripProgressBar1.Value = Convert.ToInt32(EncryptionPercentage);
            toolStripStatusLabel2.Text = EncryptionPercentage + "%";
            if (ConversionStatus == "1")
            {
                timer1.Enabled = false;
                get_usblist();
                File.AppendAllText(logfile, DateTime.Now.ToString() + "     " + deviceid + " tammalandi. " + "\r\n");
                font_set(0, label3);
                font_set(0, label4);
                font_set(0, label2);
                lblexit.Visible = true;
                lblexit.Text = "";
                tmr_sn = tmr_exit.Interval / 1000 + 9;
                tmr_exit.Enabled = true;
                loadingCircle1.Visible = false;
                button2.Enabled = true;
                loadingCircle1.Active = false;
                loadingCircle1.Visible = false;
            }
            return EncryptionPercentage;

            //}
            //catch (ManagementException err)
            //{
            //    MessageBox.Show("An error occurred while trying to execute the WMI method: " + err.Message);
            //    return err.Message;
            //}
        }

        //private void bw_encrypt_DoWork(object sender, DoWorkEventArgs e)
        //{
        //    usb_encrypt();
        //}

        //private void bw_encrypt_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        //{
        //    get_usblist();

        //}

        private void timer1_Tick(object sender, EventArgs e)
        {
            bitlocker_status(deviceid_encryting);
        }

        private void font_set(int durum, Label lb)
        {
            System.Drawing.Font aktif_font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            System.Drawing.Font pasif_font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            System.Drawing.Font tamamlanmis_font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));

            switch (durum)
            {
                case 1:  // Normal
                    lb.Font = pasif_font;
                    lb.ForeColor = System.Drawing.Color.Gray;
                    break;
                case 2:  // aktif 
                    lb.Font = aktif_font;
                    lb.ForeColor = System.Drawing.Color.Red;
                    break;
                default: // Bitti
                    lb.Font = tamamlanmis_font;
                    lb.ForeColor = System.Drawing.Color.Green;
                    break;
            }


        }

        private void get_user()
        {
            string user = null;
            ManagementObjectSearcher searcher =
                     new ManagementObjectSearcher("root\\CIMV2",
                     "SELECT * FROM Win32_ComputerSystem");

            foreach (ManagementObject queryObj in searcher.Get())
            {
                File.AppendAllText(logfile, DateTime.Now.ToString() + "     " + "-----------------------------------" + Environment.NewLine);
                File.AppendAllText(logfile, DateTime.Now.ToString() + "     " + "Win32_ComputerSystem instance");


                //textBox1.AppendText(queryObj["UserName"] + "\r\n");

                user = queryObj["UserName"].ToString();
                File.AppendAllText(logfile, DateTime.Now.ToString() + "     " + "UserName:" + user + Environment.NewLine);
            }

            if (radioButton1.Checked == true)
            {
                int p1to = user.LastIndexOf("\\");
                allowed_users = user.Substring(0, p1to) + "\\Domain Users";
                File.AppendAllText(logfile, DateTime.Now.ToString() + "     " + allowed_users + Environment.NewLine);
            }
            else
            {
                allowed_users = user;
                File.AppendAllText(logfile, DateTime.Now.ToString() + "     " + allowed_users + Environment.NewLine);
            }
        }




        public int tmr_sn;
        private void tmr_exit_Tick(object sender, EventArgs e)
        {

            app_close();


        }


        private void app_close()
        {

            if (tmr_sn > 0)
            {
                lblexit.Text = tmr_sn.ToString() + " saniye sonra kapatılacak";
                tmr_sn--;

            }
            else
            {
                tmr_exit.Enabled = false;

                Close();

            }
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            Close();
        }



        private void pctbx_info_Click(object sender, EventArgs e)
        {

        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void statusStrip1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void panel2_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void pctrbx_mini_MouseHover(object sender, EventArgs e)
        {
           
        }

        private void statusStrip1_MouseDown_1(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void btnmini_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void btn_close_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            info fr_info = new info();
            fr_info.ShowDialog();
        }

        private void label1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void loading ()
        {
            
            loadingCircle1.InnerCircleRadius = 10;

            loadingCircle1.NumberSpoke = 18;
            loadingCircle1.SpokeThickness = 2;
            loadingCircle1.OuterCircleRadius = 20;

            loadingCircle1.Color = Color.FromKnownColor((System.Drawing.KnownColor)Enum.Parse(typeof(System.Drawing.KnownColor), "Red"));
            loadingCircle1.RotationSpeed = 80;
            loadingCircle1.Active = true;
        }

        private void toolTip1_Popup(object sender, PopupEventArgs e)
        {

        }



        public bool nls_check()
        {
            var myDomain = Domain.GetComputerDomain();
            //foreach (DomainController dc in myDomain.DomainControllers)
            //{
            //    Console.WriteLine("{0} - {1}", dc.IPAddress, dc.Name);
            //}
            //Console.Read();

            string host = myDomain.Name;
            //MessageBox.Show(host);
            bool result = false;
            Ping p = new Ping();
            try
            {
                PingReply reply = p.Send(host, 3000);
               // MessageBox.Show(reply.Status.ToString());
                if (reply.Status == IPStatus.Success)
                    
                    return true;
                
            }
            catch { }
            return result;
        }








    }
}

