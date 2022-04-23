using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Web.UI;
using System.Web.UI.WebControls;
using Image = System.Drawing.Image;
using Newtonsoft.Json.Linq;

namespace PredictorWeb
{
    public partial class Predictor : System.Web.UI.Page
    {
        static List<Bitmap> display = new List<Bitmap>();
        List<String> images = new List<string>();
        static string newPath = "", newName = "";
        string folderPath;
        static JObject result;

        protected void Page_Load(object sender, EventArgs e)
        {
            ArrayList a = new ArrayList();
            folderPath = Server.MapPath("~/imgTemp/");
        }
        protected void Button1_Click(object sender, EventArgs e)
        {
            //Check wether the --------------------------
            if (!FileUpload1.HasFiles)
            {
                string script = " <script type=\"text/javascript\"> myalert('請選擇圖片！');  function myalert(content) {$('<div></div>').kendoAlert({title: 'Warning!',content: content,messages: {okText: 'Ok'}}).data('kendoAlert').open();} </script> ";
                ScriptManager.RegisterStartupScript(this, typeof(Page), "callfnc", script, false);
                return;
            }
            //Unable the button to wait the prediction proccess--------------------------
            FileUpload1.Enabled = false;
            PredictButton.Enabled = false;
            Label1.Text = "分析中...";

            //Get the current directory and create igTemp folder--------------------------
            string path = Directory.GetCurrentDirectory();
            string target = @"C:\Users\Hsulab32\Downloads\PredictorWeb-main\PredictorWeb\imgTemp";
            if (!Directory.Exists(target))
            {
                Directory.CreateDirectory(target);
            }

            DateTime today = DateTime.Now;
            newName = Path.GetFileName(FileUpload1.FileName) + today.ToString("yyMMddhhmmss");
            newPath = folderPath + newName;

            //Save the File to the Directory (Folder)--------------------------
            FileUpload1.SaveAs(newPath + ".JPG");
            Image myImg = Image.FromFile(newPath + ".JPG");

            folderPath = "https://203.72.73.18/imgTemp/"; //Path for retrieve image from IIS
            result = StartClient(newPath + ".JPG");

            //Draw image 200x200 --------------------------
            int picWidth = 200;
            int picHeight = 200;
            int imgCount = 0;
            int idxY = 0;
            int idxX = 0;
            display.Clear();
            for (idxY = 0; idxY < 6; idxY++)//Y軸迴圈
            {
                for (idxX = 0; idxX < 8; idxX++)//X軸迴圈
                {
                    //BitmapImage pic = new BitmapImage(picWidth, picHeight);
                    Bitmap pic = new Bitmap(picWidth, picHeight), picLabel = new Bitmap(picWidth, picHeight), picDisplay = new Bitmap(picWidth, picHeight);
                    //建立圖片
                    Graphics graphic = Graphics.FromImage(pic);
                    graphic.DrawImage(myImg,
                                     //將被切割的圖片畫在新圖片上面，第一個參數是被切割的原圖片
                                     new Rectangle(0, 0, picWidth, picHeight),
                                     //指定繪製影像的位置和大小，基本上是同pic大小
                                     new Rectangle(picWidth * idxX, picHeight * idxY, picWidth, picHeight),
                                     //指定被切割的圖片要繪製的部分
                                     GraphicsUnit.Pixel);
                    display.Add(new Bitmap(pic));

                    string drawString = "";
                    if (Convert.ToInt32(result["GNB"][imgCount]) == 1)
                        drawString = "GNB\n";
                    if (Convert.ToInt32(result["GPB"][imgCount]) == 1)
                        drawString += "GPB\n";
                    if (Convert.ToInt32(result["GPC"][imgCount]) == 1)
                        drawString += "GPC\n";
                    if (Convert.ToInt32(result["GPCinChain"][imgCount]) == 1)
                        drawString += "GPCinChain\n";
                    if (Convert.ToInt32(result["Yeast"][imgCount]) == 1)
                        drawString += "yeast\n";
                    picLabel = writeonimage(pic, drawString, imgCount);
                    picLabel.Save(newPath + "Label-" + imgCount.ToString() + ".JPG");
                    images.Add(folderPath + newName + "Label-" + imgCount.ToString() + ".JPG");
                    imgCount++;
                }
            }
            ListView1.DataSource = images;
            ListView1.DataBind();

            //Set dropdownList value --------------------------
            string[] preName = { "GNB", "GPB", "GPC", "GPCinChain", "Yeast" };
            var count = new List<int>();
            DropDownList1.Items.Clear();
            DropDownList1.Items.Insert(0, new ListItem("All", "All"));
            for (int i = 0; i < 5; i++)
            {
                var preCount = result[preName[i]].Where(num =>
                {
                    if (num is null)
                    {
                        throw new ArgumentNullException(nameof(num));
                    }
                    return Convert.ToInt32(num) == 1;
                });
                count.Add(preCount.Count());
                DropDownList1.Items.Insert(i + 1, new ListItem(preName[i] + ": " + preCount.Count(), preName[i]));
            }

            FileUpload1.Enabled = true;
            PredictButton.Enabled = true;
            Label1.Text = "影像:" + newName + " Finish Predict!";
        }
        ///<summary>
        /// To start client TCP to connect TCP server
        ///</summary>
        private JObject StartClient(string imgPath)
        {
            //Uses the IP address and port number to establish a socket connection
            IPAddress ipaddress = IPAddress.Parse("203.72.73.18");
            Console.WriteLine("Connection established");
            using (TcpClient tcpClient = new TcpClient())
            {
                tcpClient.Connect(ipaddress, 10000);
                NetworkStream stream = tcpClient.GetStream();
                Label1.Text = "Connected...";
                byte[] image = File.ReadAllBytes(imgPath);
                stream.Write(image, 0, image.Length); // Send image to the server

                // Receive some data from the peer.
                byte[] receiveBuffer = new byte[1024];
                int bytesReceived = stream.Read(receiveBuffer, 0, receiveBuffer.Length);

                //// Read the first batch of the TcpServer response bytes
                string responseData = System.Text.Encoding.ASCII.GetString(receiveBuffer, 0, bytesReceived);
                return JObject.Parse(responseData);
            }
        }
        ///<summary>
        /// Display image result based on the dropdownlist
        ///</summary>
        protected void DropDownList1_SelectedIndexChanged(object sender, EventArgs e)
        {
            folderPath = "https://203.72.73.18/imgTemp/";
            images.Clear();
            Bitmap pic = new Bitmap(200, 200);
            for (int i = 0; i < 48; i++)
            {
                string drawString = "";
                if (DropDownList1.SelectedValue == "All")
                {
                    string[] preName = { "GNB", "GPB", "GPC", "GPCinChain", "Yeast" };
                    for (int j = 0; j < preName.Length; j++)
                    {
                        if (Convert.ToInt32(result[preName[j]][i]) == 1)
                            drawString += preName[j] + "\n";
                    }
                }
                else
                {
                    if (Convert.ToInt32(result[DropDownList1.SelectedValue][i]) == 1)
                        drawString = DropDownList1.SelectedValue + "\n";
                }
                if (drawString != "")
                {
                    pic = writeonimage(display[i], drawString, i);
                }
                else
                {
                    pic = display[i];
                }
                pic.Save(newPath + "Label-" + i.ToString() + ".JPG");
                images.Add(folderPath + newName + "Label-" + i.ToString() + ".JPG");
            }
            ListView1.DataSource = images;
            ListView1.DataBind();
        }
        ///<summary>
        ///Draw text on img
        ///</summary>
        private Bitmap writeonimage(Bitmap img, string classN, int i)
        {
            Bitmap IMG = new Bitmap(img);
            Graphics gImg = Graphics.FromImage(IMG);
            gImg.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            gImg.DrawString(classN, new Font("TimesNewRoman", 18, FontStyle.Bold), SystemBrushes.WindowText, new Point(5, 5));
            return IMG;
        }
    }
}