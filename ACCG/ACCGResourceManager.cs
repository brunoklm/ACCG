﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ACCG
{
    public class ACCGResourceManager
    {

        private static ACCGResourceManager instance = null;

        private ACCGResourceManager() { }

        public static ACCGResourceManager GetInstance()
        {
            if (instance == null)
            {
                instance = new ACCGResourceManager();
            }

            return instance;
        }

        public void LoadSettings(string settings_path, EventArgs e)
        {
            try
            {
                if (File.Exists(settings_path))
                {
                    using (StreamReader sr = new StreamReader(settings_path))
                    {
                        while (sr.Peek() >= 0)
                        {
                            string tmp = sr.ReadLine();

                            switch (tmp)
                            {
                                case "[AC_PATH]":
                                    ACCGMainForm.ac_path = sr.ReadLine();
                                    break;                                    

                                default:
                                    break;
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Missing file \"" + settings_path + "\"!");                    
                    Application.Exit();
                }
            }
            catch
            {
                Console.WriteLine("The process failed: {0}", e.ToString());
            }
        }
       
        public List<Series> LoadACCGSeries(string accg_series_path, EventArgs e)
        {
            List<Series> series_list = new List<Series>();

            try
            {
                if (File.Exists(accg_series_path))
                {

                    using (Stream stream = File.Open(accg_series_path, FileMode.Open))
                    {
                        var bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                        series_list = (List<Series>)bformatter.Deserialize(stream);
                    }
                }
                else 
                {
                    Console.WriteLine("DEBUG: File not exist, will be create later");
                    series_list = new List<Series>();
                }
            }
            catch
            {
                Console.WriteLine("The process failed: {0}", e.ToString());
            }

            return series_list;
        }

        public void SaveACCGSeries(string accg_series_path, List<Series> accg_series_list, EventArgs e)
        {
            try
            {
                using (Stream stream = File.Open(accg_series_path, FileMode.Create))
                {
                    var bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                    bformatter.Serialize(stream, accg_series_list);
                }
            }
            catch
            {
                Console.WriteLine("The process failed: {0}", e.ToString());
            }
        }

        public List<Car> LoadCars(string file_path, EventArgs e)
        {
            List<Car> cars_list = new List<Car>();
            string car_name;
            string trimmed_car_name;

            try
            {
                if (File.Exists(file_path))
                {
                    using (StreamReader sr = new StreamReader(file_path))
                    {
                        while (sr.Peek() >= 0)
                        {
                            car_name = sr.ReadLine();

                            if (car_name.Contains("[") && car_name != "[END_SKINS]")
                            {
                                trimmed_car_name = car_name.Trim(new Char[] { '[', ']' });

                                Car tmp_car = new Car();
                                tmp_car.model = trimmed_car_name;
                                cars_list.Add(tmp_car);

                                Skin tmp_skin = new Skin();
                                tmp_skin.skin_name = sr.ReadLine();
                                string skin_image_path = ACCGMainForm.ac_path + @"\content\cars\" + tmp_car.model + @"\skins\" + tmp_skin.skin_name + @"\preview.jpg";
                                Console.WriteLine(skin_image_path);
                                
                                if (File.Exists(skin_image_path))
                                {
                                    using (var tempImage = Image.FromFile(skin_image_path))
                                    {
                                        Bitmap bmp = new Bitmap(170, 96);
                                        using (Graphics g = Graphics.FromImage(bmp))
                                        {
                                            g.DrawImage(tempImage, new Rectangle(0, 0, bmp.Width, bmp.Height));
                                        }
                                        tmp_skin.skin_preview = bmp;
                                    }                                    
                                    
                                }
                                else
                                {
                                    tmp_skin.skin_preview = ACCG.Properties.Resources.placeholder;
                                }
                                
                                while (tmp_skin.skin_name != "[END_SKINS]")
                                {
                                    tmp_car.skins.Add(tmp_skin);
                                    tmp_skin = new Skin();
                                    tmp_skin.skin_name = sr.ReadLine();
                                    skin_image_path = ACCGMainForm.ac_path + @"\content\cars\" + tmp_car.model + @"\skins\" + tmp_skin.skin_name + @"\preview.jpg";

                                    if (File.Exists(skin_image_path))
                                    {
                                        using (var tempImage = Image.FromFile(skin_image_path))
                                        {
                                            Bitmap bmp = new Bitmap(170, 96);
                                            using (Graphics g = Graphics.FromImage(bmp))
                                            {
                                                g.DrawImage(tempImage, new Rectangle(0, 0, bmp.Width, bmp.Height));
                                            }
                                            tmp_skin.skin_preview = bmp;
                                        }
                                        
                                    }
                                    else
                                    {
                                        tmp_skin.skin_preview = ACCG.Properties.Resources.placeholder;
                                    }

                                }
                            }
                        }
                    }
                }
                else 
                {
                    MessageBox.Show("Missing file \"" + file_path + "\"!");                    
                    Application.Exit();
                }
            }
            catch
            {
                Console.WriteLine("The process failed: {0}", e.ToString());
            }

            return cars_list;
        }

        public void SaveCars(string file_path, List<Car> cars_list, EventArgs e) 
        {
            try
            {
                StringBuilder new_file = new StringBuilder();

                if (File.Exists(file_path))
                {
                    for (int i = 0; i < cars_list.Count; i++)
                    {
                        Car temp_car = cars_list[i];

                        new_file.Append("[" + temp_car.model + "]");
                        new_file.Append("\r\n");

                        for (int j = 0; j < temp_car.skins.Count; j++)
                        {
                            Skin temp_skin = new Skin();
                            temp_skin.skin_name = temp_car.skins[j].skin_name;

                            new_file.Append(temp_skin.skin_name);
                            new_file.Append("\r\n");
                        }

                        new_file.Append("[END_SKINS]");
                        new_file.Append("\r\n");
                        new_file.Append("\r\n");
                    }

                    File.WriteAllText(file_path, new_file.ToString());
                }
                else
                {
                    MessageBox.Show("Missing file \"" + file_path + "\"!");
                }
            }
            catch 
            {
                Console.WriteLine("The process failed: {0}", e.ToString());
            }
        
        }

        public List<string> LoadTracks(string file_path, EventArgs e)
        {
            List<string> track_list = new List<string>();

            try
            {
                if (File.Exists(file_path))
                {
                    using (StreamReader sr = new StreamReader(file_path))
                    {
                        while (sr.Peek() >= 0)
                        {
                            track_list.Add(sr.ReadLine());
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Missing file \"" + file_path + "\"!");
                    Application.Exit();                    
                }
            }
            catch
            {
                Console.WriteLine("The process failed: {0}", e.ToString());
            }

            return track_list;
        }

        private void SaveTracks(string file_path, List<string> tracks_list, EventArgs e) 
        {
            try
            {
                StringBuilder new_file = new StringBuilder();

                if (File.Exists(file_path))
                {
                    
                    for(int i = 0; i < tracks_list.Count; i++){
                                                
                        new_file.Append(tracks_list[i] + "\r\n");
                    }

                    File.WriteAllText(file_path, new_file.ToString());
                }
                else
                {
                    MessageBox.Show("Missing file \"" + file_path + "\"!");
                }
            }
            catch
            {
                Console.WriteLine("The process failed: {0}", e.ToString());
            }
        }

        public void Sync(string cars_file_name, string tracks_file_name, EventArgs e) 
        {
            string ac_path = ACCGMainForm.ac_path;
            foreach(Car car in ACCGMainForm.ac_cars_list){
                foreach(Skin skin in car.skins){
                    skin.skin_preview.Dispose();
                }
            }

            try
            {
                // Sync the cars
                string ac_cars_path = ac_path + @"\content\cars";
                string[] ac_cars = Directory.GetDirectories(ac_cars_path, "*", SearchOption.TopDirectoryOnly);
                List<Car> temp_cars_list = new List<Car>();

                Car temp_car;

                foreach (string car in ac_cars)
                {
                    temp_car = new Car();
                    temp_car.model = car.Substring(car.LastIndexOf(@"\") + 1);

                    string car_skins_path = ac_cars_path + @"\" + temp_car.model + @"\skins";
                    string[] car_skins = Directory.GetDirectories(car_skins_path, "*", SearchOption.TopDirectoryOnly);

                    foreach (string skin in car_skins)
                    {
                        Skin temp_skin = new Skin();
                        temp_skin.skin_name = skin.Substring(skin.LastIndexOf(@"\") + 1);

                        if(File.Exists(skin + @"\preview.jpg")){
                            using (var tempImage = Image.FromFile(skin + @"\preview.jpg"))
                            {
                                Bitmap bmp = new Bitmap(170, 96);
                                using (Graphics g = Graphics.FromImage(bmp))
                                {
                                    g.DrawImage(tempImage, new Rectangle(0, 0, bmp.Width, bmp.Height));
                                }
                                temp_skin.skin_preview = bmp;
                            }
                            
                        }else{
                            temp_skin.skin_preview = ACCG.Properties.Resources.placeholder;
                        }
                        
                        temp_car.skins.Add(temp_skin);
                    }

                    temp_cars_list.Add(temp_car);
                }

                SaveCars(cars_file_name, temp_cars_list, e);

                // Sync the tracks
                string ac_tracks_path = ac_path + @"\content\tracks";
                string[] ac_tracks = Directory.GetDirectories(ac_tracks_path, "*", SearchOption.TopDirectoryOnly);
                List<string> temp_tracks_list = new List<string>();

                foreach (string track in ac_tracks)
                {
                    temp_tracks_list.Add(track.Substring(track.LastIndexOf(@"\") + 1));
                }

                SaveTracks(tracks_file_name, temp_tracks_list, e);
            }
            finally { }
            /*catch
            {
                Console.WriteLine("The process failed: {0}", e.ToString());
            }
            */               

            }
        

        public Image ScaleImage(Image image, int maxWidth, int maxHeight)
        {

                var ratioX = (double)maxWidth / image.Width;
                var ratioY = (double)maxHeight / image.Height;
                var ratio = Math.Min(ratioX, ratioY);

                var newWidth = (int)(image.Width * ratio);
                var newHeight = (int)(image.Height * ratio);

                var newImage = new Bitmap(newWidth, newHeight);
                Graphics.FromImage(newImage).DrawImage(image, 0, 0, newWidth, newHeight);

                return newImage;
        }
    }
}
