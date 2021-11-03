using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using DirectShowLib;


namespace CamApp
{


    public partial class Form1 : Form
    {
        // Contadores
        int cont1 = 0;
        int cont2 = 0;
        int cont3 = 0;
        // Fin contadores
        VideoCapture capture;
        Mat _frame;
        VideoWriter vidw;
        Boolean flagRec = false;
        int camId = 0;
        int fps;
        int line = 8;
        List<string> notes = new List<string>();
        Mat smoothedFrame;
        Boolean DFlag = false;
        private int trackBarValX = 1;
        private int trackBarValY = 1;
        private int sigmaX = 0;
        private Boolean cameraFlag = false;
        private int frameWidth = 640;
        private int frameHeight = 480;


        private void ProcessFrame(object sender, EventArgs e)
        {
            
            if (capture != null && capture.Ptr != IntPtr.Zero)
            {
                try
                {
                    Mat recImg = new Mat();
                    _frame = new Mat();
                    smoothedFrame = new Mat(); 
                    capture.Retrieve(_frame, 0);
                    //CvInvoke.GaussianBlur(_frame, smoothedFrame, new Size(trackBarValX, trackBarValY), sigmaX, 0, Emgu.CV.CvEnum.BorderType.Replicate);

                    drawDate();

                    if (DFlag)
                    {

                        pictureBox1.Image = _frame.ToImage<Bgr, byte>().ToBitmap();
                        //pictureBox1.Image = smoothedFrame.ToImage<Bgr, byte>().ToBitmap();

                        smoothedFrame.CopyTo(recImg);
                        CvInvoke.Resize(smoothedFrame, recImg, new Size(frameWidth, frameHeight), 0, 0, Emgu.CV.CvEnum.Inter.Linear);
                    }
                    else if (!DFlag)
                    {
                        pictureBox1.Image = _frame.ToImage<Bgr, byte>().ToBitmap();
                        _frame.CopyTo(recImg);
                        CvInvoke.Resize(_frame, recImg, new Size(frameWidth, frameHeight), 0, 0, Emgu.CV.CvEnum.Inter.Linear);
                    }
                    else
                        return;

                    if (flagRec)
                    {
                        
                        vidw.Write(recImg);
                    }
                }
                catch(Exception ex)
                {
                   // MessageBox.Show(ex.Message);
                }
           
            }
            else
            {
                return;
            }
        }


        public Form1()
        {
            InitializeComponent();
            //try to create the capture

            if (capture == null)
            {
                try
                {
                    //capture = new VideoCapture(camId, VideoCapture.API.Ffmpeg);
                    capture = new VideoCapture("rtsp://admin:admin@192.168.1.88:554");
                    // Obtiene todos los dispositivos de cámara disponibles
                    DsDevice[] _SystemCamereas = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
                    var dataSource = new List<Camera>();
                    capture.ImageGrabbed += ProcessFrame;

                    for (int i = 0; i < _SystemCamereas.Length; i++)
                    {
                        dataSource.Add(new Camera() { Name = _SystemCamereas[i].Name.ToString(), Id = i });
                        //MessageBox.Show(_SystemCamereas[i].Name.ToString());

                    }
                    this.comboBox1.DataSource = dataSource;
                    this.comboBox1.DisplayMember = "Name";
                    this.comboBox1.ValueMember = "Id";

                    //Solo lectura
                    this.comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;

                    var dimDs = new List<Dimension>();
                    dimDs.Add(new Dimension() { Des = "320x240", Id = 0 });
                    //dimDs.Add(new Dimension() { Des = "800x600", Id = 1 });
                    dimDs.Add(new Dimension() { Des = "640x480", Id = 1 });
                    dimDs.Add(new Dimension() { Des = "720x480", Id = 2 });
                    dimDs.Add(new Dimension() { Des = "1280x720", Id = 3 });
                    dimDs.Add(new Dimension() { Des = "1920x1080", Id = 4 });
                    this.comboBox2.DataSource = dimDs;
                    this.comboBox2.DisplayMember = "Des";
                    this.comboBox2.ValueMember = "Id";
                    this.comboBox2.DropDownStyle = ComboBoxStyle.DropDownList;

                }
                catch (NullReferenceException excpt)
                {   //show errors if there is any
                    MessageBox.Show(excpt.Message);
                }
            }

            if (capture != null) //if camera capture has been successfully created
            {
                capture.Start();
            }
        }

        private void ThreadFunction()
        {

            capture.ImageGrabbed += ProcessFrame;

        }

        //Botones de control para grabar video
        private void button1_Click(object sender, EventArgs e)
        {
            if (flagRec)
            {
                flagRec = false;
                button1.Text = "Grabar";
                vidw.Dispose();
                MessageBox.Show("Grabación Finalizada.");
                label4.Text = "";
            }
            else
            {
                setRecord();
                flagRec = true;
                button1.Text = "Detener";
                label4.ForeColor = System.Drawing.Color.Red;
                label4.Text = "Grabando";
            }

        }


        // Captura frame desde la camara y guarda imagen
        private void button2_Click(object sender, EventArgs e)
        {

            string path = @".\Images";
            try
            {
                if (!Directory.Exists(path))
                {
                    DirectoryInfo di = Directory.CreateDirectory(path);
                }
            }
            catch (Exception dirExcp)
            {
                MessageBox.Show(dirExcp.ToString());
            }
          
            if (!smoothedFrame.IsContinuous)
            {
                string imgName = DateTime.Now.ToString("ddMMyyhhmmss");
                pictureBox1.Image.Save(path + "\\" + imgName + ".png");

            }

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
           /* if (!cameraFlag) 
            {
                SetupCapture(camId);
                cameraFlag = true;
            }*/
            
           /* if(camId != comboBox1.SelectedIndex) 
            {*/
                camId = comboBox1.SelectedIndex;
                SetupCapture(camId);
            //}
        }

        private void SetupCapture(int Camera_Identifier)
        {
            //update the selected device
            int CameraDevice = Camera_Identifier;

            //Dispose of Capture if it was created before
            if (capture != null)
            {
                capture.Dispose();
            }
            try
            {
                //Set up capture device
                capture = new VideoCapture(CameraDevice);
                    
                switch(comboBox2.SelectedIndex)
                {
                    case 0:
                        frameWidth = 320;
                        frameHeight = 240;
                        break;
                    case 1:
                        frameWidth = 640;
                        frameHeight = 480;
                        break;
                    case 2:
                        frameWidth = 720;
                        frameHeight = 480;
                        break;
                    case 3:
                        frameWidth = 1280;
                        frameHeight = 720;
                        break;
                    case 4:
                        frameWidth = 1920;
                        frameHeight = 1080;
                        break;
                }
                capture.ImageGrabbed += ProcessFrame;
                capture.Start();

            }
            catch (NullReferenceException excpt)
            {
                MessageBox.Show(excpt.Message);
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
                int resId = comboBox2.SelectedIndex;
            if (resId == 0)
            {
                /*pictureBox1.Size = new Size(320, 240);
                ClientSize = new Size(960, 580);*/

                /*frameHeight = Convert.ToInt32(capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameWidth, 320));
                frameWidth = Convert.ToInt32(capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameWidth, 240));*/
                frameWidth = 320;
                frameHeight = 240;
                /*capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameWidth, 320);
                capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameHeight, 240);        
                pictureBox1.Width = 640;
                pictureBox1.Height = 480;
                pictureBox1.Refresh();*/

            }
            else if (resId == 1) {
                /*pictureBox1.Size = new Size(640, 480);
                ClientSize = new Size(960, 580);*/
                frameWidth = 640;
                frameHeight = 480;
                /*frameWidth = Convert.ToInt32(capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameWidth, 640));
                frameHeight = Convert.ToInt32(capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameHeight, 480));
                capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameWidth, 640);
                capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameHeight, 480);
                pictureBox1.Size = new Size(640, 480);
                pictureBox1.Refresh();*/
            }
            else if (resId == 2)
            {
                /* pictureBox1.Size = new Size(720, 480);
                 ClientSize = new Size(960, 580);*/
                frameWidth = 720;
                frameHeight = 480;
               /*frameWidth = Convert.ToInt32(capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameWidth, 720));
                frameHeight = Convert.ToInt32(capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameHeight, 480));
                capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameWidth, 640);
                capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameHeight, 480);
                pictureBox1.Refresh();*/
            }
            else if (resId == 3)
            {
                /*pictureBox1.Size = new Size(1280, 720);
                ClientSize = new Size(960, 580);*/
                frameWidth = 1280;
                frameHeight = 720;
               /* capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameWidth, 1280);
                capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameHeight, 720);*/
                //pictureBox1.Refresh();
            }
            else if (resId == 4)
            {
                /*pictureBox1.Size = new Size(1920, 1080);
                ClientSize = new Size(936, 517);*/
                frameWidth = 1920;
                frameHeight = 1080;
               /* capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameWidth, 1920);
                capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameHeight, 1080);*/
                //pictureBox1.Refresh();
            }

        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            fps = (int)numericUpDown1.Value;
        }

        // Dibuja fecha en el frame
        private void drawDate() {

            try
            {
                // Deinterlaced frame
                string txt = DateTime.Now.ToString("dd-MM-yy hh:mm:ss");
                Font font = new Font("Arial", 12, FontStyle.Bold);
                Graphics g = Graphics.FromImage(smoothedFrame.ToBitmap());
                // Frame deinterlaced
                g.DrawString(txt, font, Brushes.Black, new Point(Convert.ToInt32(smoothedFrame.Size.Width - 150), 12)); // Sombra de texto
                g.DrawString(txt, font, Brushes.White, new Point(Convert.ToInt32(smoothedFrame.Size.Width - 151), 10)); // Texto original
                                                                                                                        //Contador 1
                g.DrawString((cont1++).ToString(), font, Brushes.Black, new Point(12, Convert.ToInt32(smoothedFrame.Size.Height - 50)));
                g.DrawString((cont1++).ToString(), font, Brushes.White, new Point(13, Convert.ToInt32(smoothedFrame.Size.Height - 48)));
                //Contador 2
                g.DrawString((cont2++).ToString(), font, Brushes.Black, new Point(112, Convert.ToInt32(smoothedFrame.Size.Height - 50)));
                g.DrawString((cont2++).ToString(), font, Brushes.White, new Point(113, Convert.ToInt32(smoothedFrame.Size.Height - 48)));
                // Contador 3
                g.DrawString((cont3++).ToString(), font, Brushes.Black, new Point(212, Convert.ToInt32(smoothedFrame.Size.Height - 50)));
                g.DrawString((cont3++).ToString(), font, Brushes.White, new Point(213, Convert.ToInt32(smoothedFrame.Size.Height - 48)));

                g.Dispose();

                Graphics gr = Graphics.FromImage(smoothedFrame.ToBitmap());

                int aux = 1;
                foreach (var note in notes)
                {
                    gr.DrawString(note, font, Brushes.Black, new Point(40, (line * aux * 2) + 2));
                    gr.DrawString(note, font, Brushes.White, new Point(40, line * aux * 2));
                    aux++;
                }

                // Interlaced frame
                Graphics dg = Graphics.FromImage(_frame.ToBitmap());
                // Frame deinterlaced
                dg.DrawString(txt, font, Brushes.Black, new Point(Convert.ToInt32(_frame.Size.Width - 150), 12)); // Sombra de texto
                dg.DrawString(txt, font, Brushes.White, new Point(Convert.ToInt32(_frame.Size.Width - 151), 10)); // Texto original
                                                                                                                  //Contador 1
                dg.DrawString((cont1++).ToString(), font, Brushes.Black, new Point(12, Convert.ToInt32(_frame.Size.Height - 50)));
                dg.DrawString((cont1++).ToString(), font, Brushes.White, new Point(13, Convert.ToInt32(_frame.Size.Height - 48)));
                //Contador 2
                dg.DrawString((cont2++).ToString(), font, Brushes.Black, new Point(112, Convert.ToInt32(_frame.Size.Height - 50)));
                dg.DrawString((cont2++).ToString(), font, Brushes.White, new Point(113, Convert.ToInt32(_frame.Size.Height - 48)));
                // Contador 3
                dg.DrawString((cont3++).ToString(), font, Brushes.Black, new Point(212, Convert.ToInt32(_frame.Size.Height - 50)));
                dg.DrawString((cont3++).ToString(), font, Brushes.White, new Point(213, Convert.ToInt32(_frame.Size.Height - 48)));

                dg.Dispose();

                Graphics dgr = Graphics.FromImage(_frame.ToBitmap());

                int daux = 1;
                foreach (var note in notes)
                {
                    dgr.DrawString(note, font, Brushes.Black, new Point(40, (line * daux * 2) + 2));
                    dgr.DrawString(note, font, Brushes.White, new Point(40, line * daux * 2));
                    daux++;
                }
            }
            catch
            {

            }
        }
        // Funcion para agregar notas al video
        private void textBox1_KeyDown(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {

                if (textBox1.Text.Length > 0)
                {
                    notes.Add(textBox1.Text);
                    textBox1.Clear();
                }
                else
                {
                    MessageBox.Show("El campo NO debe estar vacío.");
                }
            }

        }

        // Modo "Smoothframe" (deinterlace) activado o desactivado 
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked) {

                DFlag = true;
            }
            else {

                DFlag = false;
            }

        }

        // Funcion para seterar la grabación de video
        private void setRecord (){

            // Crea el directorio de videos, si no existe.
            string path = @".\Videos";
            try
            {
                if (!Directory.Exists(path))
                {
                    DirectoryInfo di = Directory.CreateDirectory(path);
                }
            }
            catch (Exception dirExcp)
            {
                MessageBox.Show(dirExcp.ToString());
            }
            // Setea todos los parametros para instanciar videoWriter
            //double fps = capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Fps);

            Backend[] backends = CvInvoke.WriterBackends;
            int backend_idx = 0; //any backend;
            foreach (Backend be in backends)
            {
                if (be.Name.Equals("MSMF"))
                {
                    backend_idx = be.ID;
                    break;
                }
            }

            int fourcc = Convert.ToInt32(VideoWriter.Fourcc('H', '2', '6', '4'));
            int _frameHeight = Convert.ToInt32(capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameHeight));
            int _frameWidth = Convert.ToInt32(capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameWidth));
            string destination = path + "\\" +  String.Format(DateTime.Now.ToString("ddMMyyhhmmss") + ".mp4"); //"C:\\Users\\ITNOA\\Desktop\\savedVideoDHS\\" + i + ".avi";
            fps = (int)numericUpDown1.Value;
            vidw = new VideoWriter(destination, backend_idx, fourcc, fps, new Size(frameWidth, frameHeight), true);         

        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            if ((int)trackBar1.Value >= 0)
                trackBarValX = (2 * trackBar1.Value) + 1;
            Console.WriteLine(trackBar1.Value + " = (" + trackBarValX + "," + trackBarValY + ") " + sigmaX);
            label6.Text = "X = " + trackBarValX.ToString();
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            if ((int)trackBar2.Value >= 0)
                trackBarValY = (2 * trackBar2.Value) + 1;
            Console.WriteLine(trackBar2.Value + " = (" + trackBarValX + "," + trackBarValY + ") " + sigmaX);
            label7.Text = "Y = " + trackBarValY.ToString();
        }

        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            /*if ((int)trackBar2.Value >= 0)
                trackBarValY = (2 * trackBar2.Value) + 1;*/
            sigmaX = trackBar3.Value;
            Console.WriteLine(trackBar3.Value + " = (" + trackBarValX + "," + trackBarValY + ") " + sigmaX);
            label8.Text = "Sigma = " + sigmaX.ToString();
        }

        private void trackBar4_Scroll(object sender, EventArgs e)
        {
            capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Brightness, trackBar4.Value);
            label9.Text = "Brillo = " + trackBar4.Value + "%";
        }

        private void trackBar5_Scroll(object sender, EventArgs e)
        {
            capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Contrast, trackBar5.Value);
            label10.Text = "Contraste = " + trackBar5.Value; 
        }

        private void Form1_FormClossing(object sender, FormClosingEventArgs e)
        {
            if (string.Equals((sender as Button).Name, @"CloseButton"))
            {
                capture.Dispose();
            }
            // Do something proper to CloseButton.
            else 
            { 
            }
                // Then assume that X has been clicked and act accordingly.
        }
    }
    // Clase para Listar camaras
    public class Camera 
    { 
       public string Name { get; set; }
       public int Id { get; set; }
    }
    // Clase para listar resolución de video
    public class Dimension
    { 
        public string Des { get; set; }
        public int Id { get; set; }
    }
}
