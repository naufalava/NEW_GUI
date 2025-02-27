using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.IO;
using System.Windows.Forms.DataVisualization.Charting;
using System.Net.Sockets;
using GMap.NET.MapProviders;
using GMap.NET;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using System.Threading;
using static GMap.NET.Entity.OpenStreetMapGraphHopperGeocodeEntity;


namespace Penelitian_Push_Up_Counter
{
    public partial class Form1 : Form
    {
        string dataOUT;
        string dataIN;
        string Address;
        int i = 0;
        int j = 0;
        int k = 0;
        string kecepatan_setpoint = "0";
        string kp = "0";
        string ki = "0";
        string kd = "0";
        string setpoint = "0" ;
        string kode = "0";
        string dir = "0";
        string function_code = "0";
        char character_pisah_data = ',';

        double[] lat_gps_click;
        double[] lng_gps_click;
        int index_point = 0;
        bool flag_marker = false;
        TextBox[] textbox_lat_gps;
        TextBox[] textbox_lon_gps;

        private double currentAngle = 0;
        private double setpoint_heading = 0;
        private double mobil_heading = 0;

        private GMapOverlay markers_titik;
        private GMapOverlay markers_mobil;



        public Form1()
        {
            InitializeComponent();

        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string[] ports = SerialPort.GetPortNames();
            cBoxCOMPORT.Items.AddRange(ports);

            checkBox1.Checked = true;
            checkBox2.Checked = false;

            textbox_lat_gps = new TextBox[] { lat_gps_mouse, textBox2, textBox4, textBox7};
            textbox_lon_gps = new TextBox[] { long_gps_mouse, textBox1, textBox3, textBox6};

            lat_gps_click = new double[5];
            lng_gps_click = new double[5];

            markers_titik = new GMapOverlay("markers");
            MapUtama.Overlays.Add(markers_titik);

            markers_mobil = new GMapOverlay("markers");
            MapUtama.Overlays.Add(markers_mobil);
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            try
            {
                serialPort1.PortName = cBoxCOMPORT.Text;
                serialPort1.BaudRate = Convert.ToInt32(cBoxBaudrate.Text);
                serialPort1.DataBits = Convert.ToInt32("8");
                serialPort1.StopBits = (StopBits)Enum.Parse(typeof(StopBits), "One");
                serialPort1.Parity = (Parity)Enum.Parse(typeof(Parity), "None");

                serialPort1.Open();
                progressBar1.Value = 100;
            }

            catch(Exception err)
            {
                MessageBox.Show(err.Message,"Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                serialPort1.Close();
                progressBar1.Value = 0;
            }
        }

        private void btnSendData_Click(object sender, EventArgs e)
        {
            if(serialPort1.IsOpen)
            {
                dataOUT = tBoxDataOut.Text;
                serialPort1.Write(dataOUT);
            }
        }

        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            dataIN = serialPort1.ReadLine(); // mengambil data dengan pembatas enter
            this.Invoke(new EventHandler(ShowData));
        }
        private Bitmap RotateImage(Bitmap img, float angle)
        {
            Bitmap rotated = new Bitmap(img.Width, img.Height);
            using (Graphics g = Graphics.FromImage(rotated))
            {
                g.Clear(Color.Transparent); // Ensures background transparency
                g.TranslateTransform(img.Width / 2, img.Height / 2); // Move the origin to the center
                g.RotateTransform(angle); // Apply rotation
                g.TranslateTransform(-img.Width / 2, -img.Height / 2); // Move back
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

                // Corrected method: Draw using Rectangle to prevent errors
                g.DrawImage(img, new Rectangle(0, 0, img.Width, img.Height));
            }
            return rotated;
        }
  
        private void ShowData(object sender, EventArgs e)
        {

            try
            {
                string[] pisah_data = dataIN.Split(character_pisah_data);

                jarak_mobil.Text = pisah_data[3];

                if (checkBox3.Checked)
                {
                    i++;
                    this.chart1.Series[0].Points.AddXY(i, pisah_data[Convert.ToInt16(index_data1.Text)]);
                    this.chart1.Series[1].Points.AddXY(i, pisah_data[Convert.ToInt16(index_data2.Text)]);
                }

                if (gps_serial.Checked)
                {
                    Latitude.Text = pisah_data[Convert.ToInt16(index_lat_var.Text)];
                    Longitude.Text = pisah_data[Convert.ToInt16(index_long_var.Text)];
                    
                }

                if (auto_load.Checked)
                {
                    MapUtama.DragButton = MouseButtons.Left;
                    MapUtama.MapProvider = GMapProviders.GoogleMap;
                    double lat = Convert.ToDouble(Latitude.Text);
                    double lon = Convert.ToDouble(Longitude.Text);
                    var titik_mobil = new PointLatLng(lat, lon);
                    MapUtama.Position = titik_mobil;
                    double heading = Convert.ToDouble(value_car_heading.Text);
                    Bitmap originalIcon = new Bitmap("C:\\Users\\ASUS\\Documents\\GitHub\\Code\\GUI_Autonomous_Car\\car.png");  // Ensure this file exists in your project directory
                    Bitmap rotatedIcon = RotateImage(originalIcon, (float)heading);


                    if (markers_mobil.Markers.Count > 0)
                    {
                        var markerPertama = markers_mobil.Markers[0];
                        markers_mobil.Markers.Remove(markerPertama);

                    }

                    //var marker = new GMarkerGoogle(titik_mobil, GMarkerGoogleType.red);
                    var marker = new GMarkerGoogle(new PointLatLng(lat, lon), rotatedIcon);
                    marker.Offset = new System.Drawing.Point(-rotatedIcon.Width / 2, -rotatedIcon.Height / 2);
                    marker.ToolTipText = $"{index_point}";

                    markers_mobil.Markers.Add(marker);

                    MapUtama.MinZoom = 1;
                    MapUtama.MaxZoom = 100;
                    MapUtama.Zoom = 20;


                }

                if (kompas_serial.Checked)
                {
                    value_sp_heading.Text = pisah_data[Convert.ToInt16(index_sp_heading.Text)];
                    value_car_heading.Text = pisah_data[Convert.ToInt16(index_heading.Text)];

                }

                if (auto_load_kompas.Checked)
                {
                    pictureBox2.Image = RotateNeedle(0, -Convert.ToDouble(value_sp_heading.Text));
                    pictureBox3.Image = RotateNeedle(1, Convert.ToDouble(value_car_heading.Text) - Convert.ToDouble(value_sp_heading.Text));
                }


            }
            catch (Exception err)
            {

                MessageBox.Show(err.Message, "Error String Input Tidak Sesuai:\n" + "\"" + dataIN + "\"", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            if (checkBox1.Checked)
            {
                tBoxDataIn.Text = dataIN;

            }
            else if (checkBox2.Checked)
            {
                tBoxDataIn.AppendText(dataIN+"\n");
                
            }
            
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                checkBox1.Checked = true;
                checkBox2.Checked = false;
            }
            else
            {
                checkBox2.Checked = true;
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                checkBox1.Checked = false;
                checkBox2.Checked = true;
            }
            else
            {
                checkBox1.Checked = true;
            }
        }

        private void exportToTxtButton_Click(object sender, EventArgs e)
        {
            // Create a SaveFileDialog to choose the export file location.
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text Files|*.txt";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                // Get the selected file path.
                string filePath = saveFileDialog.FileName;

                // Get chart data from the chart control.
                Chart chart = chart1; // Replace "yourChartControl" with your chart control's name
                Series series = chart.Series[0]; // Assuming you have only one series
                Series series_set_point = chart.Series[1];

                // Create a StreamWriter to write data to the text file.
                using (StreamWriter sw = new StreamWriter(filePath))
                {
                    // Write the data from the chart to the text file.
                    sw.WriteLine("X-Value\tY-Value");

                    foreach (var point in series.Points)
                    {
                        foreach(var point_setpoint in series_set_point.Points)
                        {
                            if(point.XValue == point_setpoint.XValue)
                            {
                                sw.WriteLine($"{point.XValue}\t{point.YValues[0]}\t{point_setpoint.YValues[0]}");
                            }
                        }
                        
                    }
                }

                MessageBox.Show("Data exported to TXT successfully!", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            chart1.Series[0].Points.Clear();
            chart1.Series[1].Points.Clear();
            i = 0;
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void tBoxDataIn_TextChanged(object sender, EventArgs e)
        {

        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void button15_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                setpoint = textBox5.Text;
                kp = kp_textbox.Text;
                ki = ki_textbox.Text;
                kd = kd_textbox.Text;
                //serialPort1.Write(kp + "," + ki + "," + kd + "," + setpoint);
                serialPort1.Write(kp + "," + ki + "," + kd + "," + setpoint);
                
            }
        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            MapUtama.DragButton = MouseButtons.Left;
            MapUtama.MapProvider = GMapProviders.GoogleMap;
            double lat = Convert.ToDouble(Latitude.Text);
            double lon = Convert.ToDouble(Longitude.Text);
           
            MapUtama.Position = new PointLatLng(lat, lon);
            MapUtama.MinZoom = 1;
            MapUtama.MaxZoom = 100;
            MapUtama.Zoom = 20;
            double heading = Convert.ToDouble(value_car_heading.Text);
            //GMapOverlay markersOverlay = new GMapOverlay("markers");
            //GMarkerGoogle marker = new GMarkerGoogle(new PointLatLng(lat, lon), GMarkerGoogleType.arrow);
            //marker.ToolTipText = $"Lat: {lat}, Lon: {lon}, Heading: {heading}°";
            //marker.ToolTipMode = MarkerTooltipMode.Always;
            //markersOverlay.Markers.Add(marker);

            //// Clear old overlays and add the new one
            //MapUtama.Overlays.Clear();
            //MapUtama.Overlays.Add(markersOverlay);
            //MapUtama.Refresh();

        }

        private void MapUtama_Click(object sender, EventArgs e)
        {
        }


        
        private void MapUtama_MouseClick(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Right)
            {
                if (index_point >= 4)
                {
                    index_point = 0;
                    flag_marker = true;
                }

                if (index_point < 0)
                {
                    index_point = 0;
                    flag_marker = false;
                }

                if (markers_titik.Markers.Count > 0 && flag_marker == true)
                {
                    var markerPertama = markers_titik.Markers[0];
                    markers_titik.Markers.Remove(markerPertama);
                    MapUtama.Refresh();
                }

                var point = MapUtama.FromLocalToLatLng(e.X,e.Y);

                lat_gps_click[index_point] = point.Lat;
                lng_gps_click[index_point]= point.Lng;

                textbox_lat_gps[index_point].Text = lat_gps_click[index_point] + "";
                textbox_lon_gps[index_point].Text = lng_gps_click[index_point] + "";

                MapUtama.Position = point;

                var marker = new GMarkerGoogle(point, GMarkerGoogleType.blue);
                marker.ToolTipText = $"{index_point}";

                markers_titik.Markers.Add(marker);
                
                index_point = index_point + 1;

            }
        }
        private void button4_Click(object sender, EventArgs e)
        {
            string[] ports = SerialPort.GetPortNames();
            cBoxCOMPORT.Items.AddRange(ports);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            pictureBox2.Image = RotateNeedle(0, -Convert.ToDouble(value_sp_heading.Text));
            pictureBox3.Image = RotateNeedle(1,  Convert.ToDouble(value_car_heading.Text) - Convert.ToDouble(value_sp_heading.Text));
        }

        private Bitmap RotateNeedle(int index_geser, double angle)
        {
            currentAngle = angle;
            Bitmap needleBitmap = new Bitmap(imageList1.Images[index_geser]);
            needleBitmap = RotateImage(needleBitmap, currentAngle);

            return needleBitmap;
        }

        private Bitmap RotateImage(Bitmap image, double angle)
        {
            Bitmap rotatedImage = new Bitmap(image.Width, image.Height);
            using (Graphics g = Graphics.FromImage(rotatedImage))
            {
                g.TranslateTransform((float)image.Width / 2, (float)image.Height / 2);
                g.RotateTransform((float)angle);
                g.TranslateTransform(-(float)image.Width / 2, -(float)image.Height / 2);
                g.DrawImage(image, new System.Drawing.Point(0, 0));
            }
            return rotatedImage;
        }

        private void change_character_split_Click(object sender, EventArgs e)
        {
            character_pisah_data = character_split.Text[0];
        }

        private void remove_Click(object sender, EventArgs e)
        {
            if(MapUtama.Overlays.Count > 0)
            {
                MapUtama.Overlays.RemoveAt(0);
                MapUtama.Refresh();
                flag_marker = false;
                index_point = index_point - 1;
            }
            
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                serialPort1.Write(lat_gps_mouse.Text + "," + long_gps_mouse.Text + "," + "0,0");

            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                serialPort1.Write(textbox_lat_gps[1].Text + "," + textbox_lon_gps[1].Text + "," + "0,0");

            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                serialPort1.Write(textbox_lat_gps[2].Text + "," + textbox_lon_gps[2].Text + "," + "0,0");

            }
        }

        private void button11_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                serialPort1.Write(textbox_lat_gps[3].Text + "," + textbox_lon_gps[3].Text + "," + "0,0");

            }
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            await Task.Run(() => kirim_serial_step());
        }

        private void kirim_serial_step()
        {
            int index_step = 0;
            while (true)
            {
                if (index_step >= 4)
                {
                    return;
                }

                // Pastikan akses UI dengan Invoke jika diperlukan
                this.Invoke(new Action(() =>
                {
                    kirim_serial_gps(textbox_lat_gps[index_step], textbox_lon_gps[index_step]);
                }));

                Thread.Sleep(100);

                this.Invoke(new Action(() =>
                {
                    if (Convert.ToDouble(jarak_mobil.Text) < 5)
                    {
                        index_step++;
                    }
                }));
            }
        }

        private void kirim_serial_gps(TextBox textbox_latitude, TextBox textbox_longitude)
        {
            if (serialPort1.IsOpen)
            {
                serialPort1.Write(textbox_latitude.Text + "," + textbox_longitude.Text + "," + "0,0");

            }
        }

        private void MapUtama_Load(object sender, EventArgs e)
        {

        }

        private void groupBox4_Enter(object sender, EventArgs e)
        {

        }

        private void groupBox3_Enter(object sender, EventArgs e)
        {

        }

        private void gps_serial_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {

        }

        private void auto_load_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void kompas_serial_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void value_car_heading_TextChanged(object sender, EventArgs e)
        {

        }

        private void index_sp_heading_TextChanged(object sender, EventArgs e)
        {

        }

        private void text1_Click(object sender, EventArgs e)
        {

        }

        private void lat_gps_mouse_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
