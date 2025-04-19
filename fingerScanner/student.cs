using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Threading;

namespace fingerScanner
{
    public partial class main : Form
    {
        private student st;
        FingerPrintManager FPM = new FingerPrintManager();
        private string appPath = Application.StartupPath;
        private bool getfirst = false;
        private bool getsecond = false;
        private string image1 = "output1.bmp";
        private string image2 = "output2.bmp";
        public main()
        {
            InitializeComponent();
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://localhost:4000/");
        }

        private HttpClient _httpClient;

        private void main_Load(object sender, EventArgs e)
        {
            username.Text = Form1.LoginRes.User.Username;
        }

        private void search_Click(object sender, EventArgs e)
        {
            string id = code.Text.Trim();
            getStudent(id);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            student_list ls = new student_list(this);
            ls.Show();
        }

        public async void getStudent(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    MessageBox.Show("اول کد دانشجویی را وارد کنید");
                    return;
                }
                if (pictureBox1.Image != null)
                {
                    pictureBox1.Image.Dispose();
                    pictureBox1.Image = null;
                }
                if (pictureBox2.Image != null)
                {
                    pictureBox2.Image.Dispose();
                    pictureBox2.Image = null;
                }
                if (pictureBox3.Image != null)
                {
                    pictureBox3.Image.Dispose();
                    pictureBox3.Image = null;
                }
                var request = new HttpRequestMessage(HttpMethod.Get, "reports/student/" + id);
                request.Headers.Add("Authorization", $"Bearer {Form1.LoginRes.Token}");
                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();
                JObject obj = JObject.Parse(responseBody);
                st = new student();
                if (obj["student"] != null)
                {
                    if (obj["student"]["id"] != null)
                        st.id = (string)obj["student"]["id"];
                    if (obj["student"]["firstname"] != null)
                        st.firstname = (string)obj["student"]["firstname"];
                    if (obj["student"]["lastname"] != null)
                        st.lastname = (string)obj["student"]["lastname"];
                    if (obj["student"]["picture"] != null)
                        st.picture = (string)obj["student"]["picture"];
                    if (obj["student"]["nationCode"] != null)
                        st.nationCode = (string)obj["student"]["nationCode"];
                    if (string.IsNullOrEmpty(obj["student"]["fingerprint"].ToString()))
                    {
                        st.fingerprint = "ثبت نشده";
                    }
                    else
                    {
                        st.fingerprint = "ثبت شده";
                    }
                    firstFinger.Enabled = true;
                    secendFinger.Enabled = true;
                }
                else
                {
                    MessageBox.Show("این دانشجو یافت نشد");
                }
                //MessageBox.Show(st.ToString());
                label11.Text = st.id;
                label3.Text = st.firstname;
                label4.Text = st.lastname;
                label5.Text = st.nationCode;
                label6.Text = st.fingerprint;
                if (st.picture != "")
                {
                    var imagereq = new HttpRequestMessage(HttpMethod.Get, "reports/image/" + st.picture);
                    imagereq.Headers.Add("Authorization", $"Bearer {Form1.LoginRes.Token}");
                    var imageres = await _httpClient.SendAsync(imagereq);
                    imageres.EnsureSuccessStatusCode();
                    var imagebyte = await imageres.Content.ReadAsByteArrayAsync();
                    using (MemoryStream ms = new MemoryStream(imagebyte))
                    {
                        Image loadedImage = Image.FromStream(ms);
                        Image resizedImage = ResizeImage(loadedImage, 151, 151);
                        pictureBox1.Image = resizedImage;
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                // Log the status code and message
                MessageBox.Show($"Message: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Log other exceptions
                MessageBox.Show($"Exception: {ex.Message}");
            }
        }

        private async void sumbitfingerprint_Click(object sender, EventArgs e)
        {
            try
            {
                if (st == null)
                {
                    MessageBox.Show("اول دانشجو را تایین کنید.");
                    return;
                }
                Int32 ConfirmationCode1;
                ConfirmationCode1 = FPM.RegModel();
                switch (ConfirmationCode1)
                {
                    case 0:
                        MessageBox.Show("Reg Model successfully");
                        break;
                    case 1:
                        MessageBox.Show("Error Reciving package");
                        break;
                    case 0x0a:
                        MessageBox.Show("Two finger print dont blong to a finger");
                        break;
                    default:
                        MessageBox.Show("خطا در دریافت اطلاعات");
                        break;
                }
                // upchar
                Int32 ConfirmationCodeupchar1;
                ConfirmationCodeupchar1 = FPM.UpChar(1);
                Byte[] buffer = new Byte[0];
                switch (ConfirmationCodeupchar1)
                {
                    case 0:
                        MessageBox.Show("UpChar successfully");
                        while (USBData.Length >= 139)
                        {
                            Byte[] packet = USBData.ReciveData(139);
                            buffer = ByteArrays.AppendBytes(buffer, packet, 9, 128);
                            Thread.Sleep(18);
                        }
                        // Save buffer(512 bytes) in database (fingerprint pattern)
                        // MessageBox.Show(buffer.Length.ToString());
                        // MessageBox.Show(ByteArrays.ByteArrayToHexString(buffer));
                        break;
                    case 1:
                        MessageBox.Show("Error Reciving package 1");
                        break;
                    case 0x0D:
                        MessageBox.Show("Command Execution Error");
                        break;
                    default:
                        MessageBox.Show("خطا در دریافت اطلاعات");
                        break;
                }
                // send to server information/fingerprint post
                var content = new MultipartFormDataContent();
                postfingerprint f = new postfingerprint();
                // add data to post body
                f.fingerprint = buffer;
                f.studentId = st.id;
                var json = JsonSerializer.Serialize(f);
                var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");
                content.Add(jsonContent,"body");

                // image 1
                string path1 = Path.Combine(appPath, image1);
                var imageBytes1 = File.ReadAllBytes(path1);
                var imageContent1 = new ByteArrayContent(imageBytes1);
                imageContent1.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("image/bmp");
                content.Add(imageContent1, "file", image1);
                // image 2
                string path2 = Path.Combine(appPath,image1);
                var imageBytes2 = File.ReadAllBytes(path2);
                var imageContent2 = new ByteArrayContent(imageBytes2);
                imageContent1.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("image/bmp");
                content.Add(imageContent2, "file", image2);

                var request = new HttpRequestMessage(HttpMethod.Post, "information/fingerprint/")
                {
                    Content = content
                };
                request.Headers.Add("Authorization", $"Bearer {Form1.LoginRes.Token}");
                var response = await _httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("درخواست با موفقیت ارسال شد.");
                }
                else
                {
                    MessageBox.Show($"خطا: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void firstFinger_Click(object sender, EventArgs e)
        {
            try
            {
                progressBar1.Visible = true;
                Int32 ConfirmationCode1;
                ConfirmationCode1 = FPM.GenImg();
                ///MessageBox.Show(ConfirmationCode1.ToString());
                switch (ConfirmationCode1)
                {
                    case 0:
                        Int32 ConfirmationCode2 = FPM.UpImage();
                        switch (ConfirmationCode2)
                        {
                            case 0:
                                try
                                {
                                    progressBar1.Value = 10;
                                    byte[] ImgData = ReadImageData();
                                    progressBar1.Value = 60;
                                    string fullPath = Path.Combine(appPath, image1);
                                    WriteBMP(fullPath, ImgData, 256, 48);
                                    //CreateBmpFile(ImgData, "f:\\output.bmp", 256, 48);
                                    Image loadedImage = Image.FromFile(fullPath);
                                    Image resizedImage = ResizeImage(loadedImage,768, 144);
                                    pictureBox3.Image = resizedImage;
                                    getfirst = true;
                                    if (getfirst && getsecond)
                                    {
                                        sumbitfingerprint.Enabled = true;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show(ex.ToString());
                                }
                                break;
                            case 1:
                                MessageBox.Show("خطا در دریافت عکس اثر انگشت");
                                break;
                            case 15:
                                MessageBox.Show("خطا در دریافت عکس اثر انگشت");
                                break;
                            default:
                                MessageBox.Show("خطا در دریافت اطلاعات");
                                break;
                        }
                        break;
                    case 1:
                        MessageBox.Show("خطا در دریافت اطلاعات");
                        break;
                    case 2:
                        MessageBox.Show("انگشت روی سنسور قرار ندارد");
                        break;
                    case 3:
                        MessageBox.Show("خطا در دریافت اطلاعات");
                        break;
                    default:
                        MessageBox.Show("خطا در دریافت اطلاعات");
                        break;
                }
                progressBar1.Value = 70;
                Int32 ConfirmationCode3;
                ConfirmationCode3 = FPM.Img2Tz(1);
                switch (ConfirmationCode3)
                {
                    case 0:
                        progressBar1.Value = 90;
                        //MessageBox.Show("Image 1 Tz Successfully (buffer 1)");
                        break;
                    case 1:
                        MessageBox.Show("Error Reciving package");
                        break;

                    case 6:
                        MessageBox.Show("Image too messy");
                        break;
                    case 7:
                        MessageBox.Show("Too few feature points");
                        break;
                    case 0x15:
                        MessageBox.Show("No valid Image in Image buffer");
                        break;
                    default:
                        MessageBox.Show("خطا در نمایش اطلاعات");
                        break;
                }
                progressBar1.Visible = false;
                progressBar1.Value = 0;
            }catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public Image ResizeImage(Image image, int maxWidth, int maxHeight)
        {
            double ratioX = (double)maxWidth / image.Width;
            double ratioY = (double)maxHeight / image.Height;
            double ratio = Math.Min(ratioX, ratioY);
            int newWidth = (int)(image.Width * ratio);
            int newHeight = (int)(image.Height * ratio);
            // ایجاد یک Bitmap با اندازه جدید
            var resizedImage = new Bitmap(newWidth, newHeight);

            // ایجاد یک گرافیک از Bitmap جدید
            using (var graphics = Graphics.FromImage(resizedImage))
            {
                // تنظیمات کیفیت برای گرافیک
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.CompositingQuality = CompositingQuality.HighQuality;

                // رسم تصویر اصلی با اندازه جدید
                graphics.DrawImage(image, 0, 0, newWidth, newHeight);
            }

            return resizedImage;
        }

        private byte[] ReadImageData()
        {
            Byte[] ImgData = new Byte[0];
            while (USBData.Length >= 139)
            {
                Byte[] packet = USBData.ReciveData(139);
                ImgData = ByteArrays.AppendBytes(ImgData, packet, 9, 128);
                Thread.Sleep(18);
            }
            return ImgData;
        }

        private void WriteBMP(string fileName, byte[] imageData, int width, int height)
        {
            try
            {
                using (Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb))
                {
                    BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, bitmap.PixelFormat);
                    IntPtr ptr = bmpData.Scan0;
                    System.Runtime.InteropServices.Marshal.Copy(imageData, 0, ptr, imageData.Length);

                    bitmap.UnlockBits(bmpData);
                    bitmap.Save(fileName, ImageFormat.Bmp);
                    bitmap.Dispose();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void secendFinger_Click(object sender, EventArgs e)
        {
            try
            {
                progressBar2.Visible = true;
                Int32 ConfirmationCode1;
                ConfirmationCode1 = FPM.GenImg();
                //MessageBox.Show(ConfirmationCode1.ToString());
                switch (ConfirmationCode1)
                {
                    case 0:
                        Int32 ConfirmationCode2 = FPM.UpImage();
                        switch (ConfirmationCode2)
                        {
                            case 0:
                                progressBar2.Value = 10;
                                byte[] ImgData = ReadImageData();
                                string fullPath = Path.Combine(appPath, image2);
                                progressBar2.Value = 60;
                                WriteBMP(fullPath, ImgData, 256, 48);
                                Image loadedImage = Image.FromFile(fullPath);
                                Image resizedImage = ResizeImage(loadedImage, 768, 144);
                                //CreateBmpFile(ImgData, "f:\\output.bmp", 256, 48);
                                pictureBox2.Image = resizedImage;
                                getsecond = true;
                                if(getfirst&& getsecond)
                                {
                                    sumbitfingerprint.Enabled = true;
                                }
                                break;
                            case 1:
                                MessageBox.Show("خطا در دریافت عکس اثر انگشت");
                                break;
                            case 15:
                                MessageBox.Show("خطا در دریافت عکس اثر انگشت");
                                break;
                            default:
                                MessageBox.Show("خطا در دریافت اطلاعات");
                                break;
                        }
                        break;
                    case 1:
                        MessageBox.Show("خطا در دریافت اطلاعات");
                        break;
                    case 2:
                        MessageBox.Show("انگشت روی سنسور قرار ندارد");
                        break;
                    case 3:
                        MessageBox.Show("خطا در دریافت اطلاعات");
                        break;
                    default:
                        MessageBox.Show("خطا در دریافت اطلاعات");
                        break;
                }
                progressBar2.Value = 70;
                Int32 ConfirmationCode3;
                ConfirmationCode3 = FPM.Img2Tz(2);
                switch (ConfirmationCode3)
                {
                    case 0:
                        progressBar2.Value = 90;
                        //MessageBox.Show("Image 2 Tz Successfully (buffer 2)");
                        break;
                    case 1:
                        MessageBox.Show("Error Reciving package");
                        break;

                    case 6:
                        MessageBox.Show("Image too messy");
                        break;
                    case 7:
                        MessageBox.Show("Too few feature points");
                        break;
                    case 0x15:
                        MessageBox.Show("No valid Image in Image buffer");
                        break;
                    default:
                        MessageBox.Show("خطا در دریافت اطلاعات");
                        break;
                }
                progressBar2.Visible = false;
                progressBar2.Value = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void CreateBmpFile(byte[] imageData, string outputFileName, int width, int height)
        {
            try
            {
                using (FileStream fileStream = new FileStream(outputFileName, FileMode.Create))
                {
                    int headerSize = 14; // BMP Header size
                    int dibHeaderSize = 40; // DIB Header size
                    byte[] header = new byte[headerSize];
                    byte[] dibHeader = new byte[dibHeaderSize];

                    // BMP Header
                    header[0] = (byte)'B';
                    header[1] = (byte)'M';
                    int fileSize = headerSize + dibHeaderSize + imageData.Length;
                    header[2] = (byte)(fileSize & 0xFF);
                    header[3] = (byte)((fileSize >> 8) & 0xFF);
                    header[4] = (byte)((fileSize >> 16) & 0xFF);
                    header[5] = (byte)((fileSize >> 24) & 0xFF);
                    header[10] = (byte)(headerSize + dibHeaderSize); // Pixel array offset

                    // DIB Header
                    dibHeader[0] = (byte)dibHeaderSize;
                    dibHeader[4] = (byte)(width & 0xFF);
                    dibHeader[5] = (byte)((width >> 8) & 0xFF);
                    dibHeader[6] = (byte)((width >> 16) & 0xFF);
                    dibHeader[7] = (byte)((width >> 24) & 0xFF);
                    dibHeader[8] = (byte)(height & 0xFF);
                    dibHeader[9] = (byte)((height >> 8) & 0xFF);
                    dibHeader[10] = (byte)((height >> 16) & 0xFF);
                    dibHeader[11] = (byte)((height >> 24) & 0xFF);
                    dibHeader[12] = 1; // Number of color planes
                    dibHeader[14] = 24; // Bits per pixel

                    fileStream.Write(header, 0, headerSize);
                    fileStream.Write(dibHeader, 0, dibHeaderSize);
                    fileStream.Write(imageData, 0, imageData.Length);
                    fileStream.Close();
                    // MessageBox.Show("Success");
                }
            }
            catch (IOException e)
            {
                MessageBox.Show("خطایی در ایجاد فایل BMP رخ داده است: " + e.Message);
            }
        }
    }
}
