using Newtonsoft.Json.Linq;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace FingerScannerWPF
{
    public partial class Student : Window
    {
        private student st;
        FingerPrintManager FPM = new FingerPrintManager();

        private bool getfirst = false;
        private bool getsecond = false;
        private string appPath = AppDomain.CurrentDomain.BaseDirectory;
        private string image1 = "output1.bmp";
        private string image2 = "output2.bmp";

        private HttpClient _httpClient;
        public Student()
        {
            InitializeComponent();
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://localhost:4000/");
        }

        private void firstFinger_Click(object sender, RoutedEventArgs e)
        {
            ScanFinger(progressBar1, pictureBox3, image1, 1, () => getfirst = true);
        }

        private void secendFinger_Click(object sender, RoutedEventArgs e)
        {
            ScanFinger(progressBar2, pictureBox2, image2, 2, () => getsecond = true);
        }

        private void ScanFinger(System.Windows.Controls.ProgressBar bar, System.Windows.Controls.Image imgControl, string imgFile, int bufferId, Action onSuccess)
        {
            try
            {
                bar.Visibility = Visibility.Visible;
                bar.Value = 10;

                int code = FPM.GenImg();
                if (code == 0)
                {
                    int upCode = FPM.UpImage();
                    if (upCode == 0)
                    {
                        bar.Value = 30;
                        byte[] data = ReadImageData();
                        bar.Value = 60;
                        string path = Path.Combine(appPath, imgFile);
                        WriteBMP(path, data, 256, 48);

                        Dispatcher.Invoke(() =>
                        {
                            imgControl.Source = new BitmapImage(new Uri(path));
                            bar.Value = 80;
                        }, DispatcherPriority.Render);

                        if (bufferId == 1) getfirst = true;
                        else if (bufferId == 2) getsecond = true;

                        Int32 convertCode = FPM.Img2Tz(1);
                        bar.Value = 100;

                        if (convertCode == 0)
                        {
                            onSuccess();
                            if (getfirst && getsecond)
                                sumbitfingerprint.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            MessageBox.Show("خطا در تبدیل اثر انگشت.");
                        }
                    }
                    else MessageBox.Show("خطا در دریافت عکس اثر انگشت");
                }
                else
                {
                    MessageBox.Show("انگشت روی سنسور قرار ندارد یا خطا در دریافت تصویر");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطا: " + ex.Message);
            }
            finally
            {
                bar.Visibility = Visibility.Collapsed;
                bar.Value = 0;
            }
        }

        private byte[] ReadImageData()
        {
            byte[] result = Array.Empty<byte>();
            while (USBData.Length >= 139)
            {
                byte[] packet = USBData.ReciveData(139);
                result = ByteArrays.AppendBytes(result, packet, 9, 128);
                Thread.Sleep(18);
            }
            return result;
        }

        private void WriteBMP(string filename, byte[] imageData, int width, int height)
        {
            using Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, bitmap.PixelFormat);
            Marshal.Copy(imageData, 0, bmpData.Scan0, imageData.Length);
            bitmap.UnlockBits(bmpData);
            bitmap.Save(filename, ImageFormat.Bmp);
        }

        private void search_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(code.Text))
            {
                MessageBox.Show("لطفاً کد دانشجویی را وارد کنید.");
                return;
            }

            string id = code.Text.Trim();
            getStudent(id);
        }

        private void StudentList_Click(object sender, RoutedEventArgs e)
        {
            var listWindow = new StudentList(this);
            listWindow.ShowDialog();
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

                pictureBox1.Source = null;
                pictureBox2.Source = null;
                pictureBox3.Source = null;

                var request = new HttpRequestMessage(HttpMethod.Get, "reports/student/" + id);
                request.Headers.Add("Authorization", $"Bearer {LoginWindow.LoginRes.Token}");
                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();
                JObject obj = JObject.Parse(responseBody);

                st = new student();

                if (obj["student"] != null)
                {
                    st.id = (string?)obj["student"]["id"];
                    st.firstname = (string?)obj["student"]["firstname"];
                    st.lastname = (string?)obj["student"]["lastname"];
                    st.picture = (string?)obj["student"]["picture"];
                    st.nationCode = (string?)obj["student"]["nationCode"];
                    st.fingerprint = string.IsNullOrEmpty((string?)obj["student"]["fingerprint"]) ? "ثبت نشده" : "ثبت شده";

                    firstFinger.IsEnabled = true;
                    secendFinger.IsEnabled = true;
                }
                else
                {
                    MessageBox.Show("این دانشجو یافت نشد");
                    return;
                }

                label11.Text = st.id;
                label3.Text = st.firstname;
                label4.Text = st.lastname;
                label5.Text = st.nationCode;
                label6.Text = st.fingerprint;

                if (!string.IsNullOrEmpty(st.picture))
                {
                    var imagereq = new HttpRequestMessage(HttpMethod.Get, "reports/image/" + st.picture);
                    imagereq.Headers.Add("Authorization", $"Bearer {LoginWindow.LoginRes.Token}");
                    var imageres = await _httpClient.SendAsync(imagereq);
                    imageres.EnsureSuccessStatusCode();
                    var imagebyte = await imageres.Content.ReadAsByteArrayAsync();

                    using (var ms = new MemoryStream(imagebyte))
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.StreamSource = ms;
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        bitmap.Freeze();
                        pictureBox1.Source = bitmap;
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Message: {ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception: {ex.Message}");
            }
        }

        private async Task sumbitfingerprint_ClickAsync(object sender, RoutedEventArgs e)
        {
            try
            {
                if (st == null)
                {
                    MessageBox.Show("اول دانشجو را تایین کنید.");
                    return;
                }

                Int32 ConfirmationCode1 = FPM.RegModel();
                switch (ConfirmationCode1)
                {
                    case 0:
                        MessageBox.Show("Reg Model successfully");
                        break;
                    case 1:
                        MessageBox.Show("Error Reciving package");
                        return;
                    case 0x0a:
                        MessageBox.Show("دو اثر انگشت با هم تطابق ندارند");
                        return;
                    default:
                        MessageBox.Show("خطا در دریافت اطلاعات");
                        return;
                }

                Int32 ConfirmationCodeupchar1 = FPM.UpChar(1);
                Byte[] buffer = new Byte[0];
                if (ConfirmationCodeupchar1 == 0)
                {
                    while (USBData.Length >= 139)
                    {
                        Byte[] packet = USBData.ReciveData(139);
                        buffer = ByteArrays.AppendBytes(buffer, packet, 9, 128);
                        Thread.Sleep(18);
                    }
                }
                else
                {
                    MessageBox.Show("خطا در دریافت اطلاعات اثر انگشت");
                    return;
                }

                var content = new MultipartFormDataContent();
                postfingerprint f = new postfingerprint
                {
                    fingerprint = buffer,
                    studentId = st.id
                };
                var json = JsonSerializer.Serialize(f);
                var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");
                content.Add(jsonContent, "body");

                string path1 = Path.Combine(appPath, image1);
                string path2 = Path.Combine(appPath, image2);

                var imageBytes1 = File.ReadAllBytes(path1);
                var imageBytes2 = File.ReadAllBytes(path2);

                var imageContent1 = new ByteArrayContent(imageBytes1);
                imageContent1.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("image/bmp");
                content.Add(imageContent1, "file", image1);

                var imageContent2 = new ByteArrayContent(imageBytes2);
                imageContent2.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("image/bmp");
                content.Add(imageContent2, "file", image2);

                var request = new HttpRequestMessage(HttpMethod.Post, "information/fingerprint/")
                {
                    Content = content
                };
                request.Headers.Add("Authorization", $"Bearer {LoginWindow.LoginRes.Token}");
                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("درخواست با موفقیت ارسال شد.");
                    this.Close();
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
    }
}