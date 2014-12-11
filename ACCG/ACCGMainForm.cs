﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Collections.Concurrent;

namespace ACCG
{
    public partial class ACCGMainForm : Form
    {
        public ACCGMainForm()
        {
            InitializeComponent();
            accg_generator = ACCGGenerator.GetInstance();
            accg_resource = ACCGResourceManager.GetInstance();
            accg_log = ACCGLogManager.GetInstance();
        }
        
        private void ACCGMainForm_Load(object sender, EventArgs e)
        {
            // Delete existing log files
            accg_log.DeleteLogFiles();
            DateTime time_stamp;

            // Load settings.ini        
            time_stamp = DateTime.Now;
            accg_log.WriteLog("SYSTEM", time_stamp + ": Load settings.ini");            
            accg_resource.LoadSettings(accg_settings_file_name);
            this.openSeriesFileDialog.InitialDirectory = ac_path + @"\content\career";
                    
            // ACCG Series list (list with only user created series)   
            time_stamp = DateTime.Now;
            accg_log.WriteLog("SYSTEM", time_stamp + ": Load ACCG Series list");            
            accg_series_list = accg_resource.LoadACCGSeries(accg_series_file_name);

            // Sync resources
            time_stamp = DateTime.Now;
            accg_log.WriteLog("SYSTEM", time_stamp + ": Sync resources");            
            accg_resource.Sync(accg_cars_file_name, accg_tracks_file_name);
                                                                   
            // Populating Cars list   
            time_stamp = DateTime.Now;
            accg_log.WriteLog("SYSTEM", time_stamp + ": Load Cars");            
            ac_cars_list = accg_resource.LoadCars(accg_cars_file_name);
            
            // Populating Tracks list 
            time_stamp = DateTime.Now;
            accg_log.WriteLog("SYSTEM", time_stamp + ": Load Tracks");             
            ac_tracks_list = accg_resource.LoadTracks(accg_tracks_file_name);            

            // Creating the time table             
            time_table = new Dictionary<string, int>();
      
            time_table.Add("08:00", -80);
            time_table.Add("08:30", -72);
            time_table.Add("09:00", -64);
            time_table.Add("09:30", -56);
            time_table.Add("10:00", -48);
            time_table.Add("10:30", -40);
            time_table.Add("11:00", -32);
            time_table.Add("11:30", -24);
            time_table.Add("12:00", -16);
            time_table.Add("12:30", -8);
            time_table.Add("13:00", 0);
            time_table.Add("13:30", 8);
            time_table.Add("14:00", 16);
            time_table.Add("14:30", 24);
            time_table.Add("15:00", 32);
            time_table.Add("15:30", 40);
            time_table.Add("16:00", 48);
            time_table.Add("16:30", 56);
            time_table.Add("17:00", 64);
            time_table.Add("17:30", 72);
            time_table.Add("18:00", 80);

            ShowData();

            SplashScreen.CloseSplash();
            
        }

        private void lbSeries_SelectedIndexChanged(object sender, EventArgs e)
        {
            current_selected_series = (Series)lbSeries.SelectedItem;
            
            if (current_selected_series != null)
            {
                if (!current_selected_series.isGenerated)
                {
                    lblCurrentSeries.ForeColor = Color.Red;
                }
                else 
                {
                    lblCurrentSeries.ForeColor = Color.Green;
                }
                
                if (current_selected_series.isEdited)
                {
                    lblCurrentSeries.ForeColor = Color.DarkOrange;
                }

                lblCurrentSeries.Text = "Current series: " + current_selected_series.name;

                Car tmp_car = current_selected_series.car;

                rtbSeriesInfo.ResetText();
                rtbSeriesInfo.AppendText("Code: " + current_selected_series.code + "\n");
                rtbSeriesInfo.AppendText("Name: " + current_selected_series.name + "\n");
                rtbSeriesInfo.AppendText("Description: " + current_selected_series.description + "\n");
                rtbSeriesInfo.AppendText("Requires: " + current_selected_series.requires + "\n");

                if (current_selected_series.isChampionship) 
                {
                    rtbSeriesInfo.AppendText("Points: " + current_selected_series.points + "\n");
                    rtbSeriesInfo.AppendText("Car: " + tmp_car.model + "\n");
                    rtbSeriesInfo.AppendText("Skin: " + current_selected_series.skin.skin_name + "\n");
                    rtbSeriesInfo.AppendText("Opponents: " + current_selected_series.opponents_list.Count + "\n");
                    
                    if (current_selected_series.series_goals.points == "")
                    {
                        rtbSeriesInfo.AppendText("Goals: " + current_selected_series.series_goals.ranking + " position\n");
                    }
                    else
                    {
                        rtbSeriesInfo.AppendText("Goals: " + current_selected_series.series_goals.points + " points\n");
                    }
                    
                }
                else
                {
                    rtbSeriesInfo.AppendText("Goals:\n");
                    rtbSeriesInfo.AppendText("Gold medals X " + current_selected_series.series_goals.tier_3 + "\n");
                    rtbSeriesInfo.AppendText("Silver medals X " + current_selected_series.series_goals.tier_2 + "\n");
                    rtbSeriesInfo.AppendText("Bronze medals X " + current_selected_series.series_goals.tier_1 + "\n");
                }
                                
                rtbSeriesInfo.AppendText("Events: " + current_selected_series.events_list.Count + "\n");
                
            }
                                                                    
        }

        private void creditsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ACCGCredits credits = new ACCGCredits();
            credits.Show();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnLoadSeries_Click(object sender, EventArgs e)
        {
            if (openSeriesFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string filename = openSeriesFileDialog.FileName;
                accg_series_list.Add(accg_resource.loadSeries(filename));
            }

            bs_series_datasource.ResetBindings(false);
            ShowData();

            rtbSeriesInfo.ResetText();
        }

        private void btnNewSeries_Click(object sender, EventArgs e)
        {
            ACCGNewSeriesForm newSeriesForm = new ACCGNewSeriesForm();
            newSeriesForm.Text = "New Series";
            newSeriesForm.ShowDialog();
        }

        private void btnEditSeries_Click(object sender, EventArgs e)
        {
            if (current_selected_series == null)
            {
                MessageBox.Show("You have to select a series!");
            }
            else
            {
                ACCGNewSeriesForm editSeriesForm = new ACCGNewSeriesForm(current_selected_series);
                editSeriesForm.Text = "Edit Series " + current_selected_series.name;
                editSeriesForm.ShowDialog();
                
            }
            
        }

        private void btnDeleteSeries_Click(object sender, EventArgs e)
        {

            GC.Collect();

            if (current_selected_series == null)
            {
                MessageBox.Show("You have to select a series!");
            }
            else
            {
                DialogResult are_you_sure = MessageBox.Show("Are you sure?","Delete series", MessageBoxButtons.YesNo);

                if (are_you_sure == DialogResult.Yes)
                {
                    if (current_selected_series.isGenerated)
                    {
                        string series_path = String.Format(ac_path + @"\content\career\series{0}", current_selected_series.ID);

                        try
                        {
                            if (Directory.Exists(series_path))
                            {
                                Directory.Delete(series_path, true);
                            }

                            accg_series_list.Remove(current_selected_series);
                            bs_series_datasource.ResetBindings(false);
                            rtbSeriesInfo.ResetText();

                            // Saving the accg series list
                            string accg_series_path = @"data\accg_series_list.dat";
                            accg_resource.SaveACCGSeries(accg_series_path, accg_series_list);

                            lblCurrentSeries.Text = "Current series: ";
                        }
                        catch (Exception exc)
                        {
                            ACCGMainForm.accg_log.WriteLog("ERROR", "The process failed: " + exc.ToString());
                            Console.WriteLine("The process failed: {0}", exc.ToString());
                        }

                    }
                    else
                    {
                        accg_series_list.Remove(current_selected_series);
                        bs_series_datasource.ResetBindings(false);
                        rtbSeriesInfo.ResetText();

                        // Saving the accg series list
                        string accg_series_path = @"data\accg_series_list.dat";
                        accg_resource.SaveACCGSeries(accg_series_path, accg_series_list);

                        lblCurrentSeries.Text = "Current series: ";
                    }
                    
                }                
               
            }
        }


        private void btnGenerate_Click(object sender, EventArgs e)
        {
            if (current_selected_series == null)
            {
                MessageBox.Show("You have to select a series!");
            }
            else
            {                
                accg_generator.Generate(current_selected_series, ac_path);
                current_selected_series.isGenerated = true;
                
                // Saving the accg series list                
                accg_resource.SaveACCGSeries(accg_series_file_name, accg_series_list);
            }

        }

        private void syncToolStripMenuItem_Click(object sender, EventArgs e)
        {            
            accg_resource.Sync(accg_cars_file_name, accg_tracks_file_name);
            
            // re-populating Cars list                                    
            ac_cars_list = accg_resource.LoadCars(accg_cars_file_name);

            // re-populating Tracks list                        
            ac_tracks_list = accg_resource.LoadTracks(accg_tracks_file_name);
            
            MessageBox.Show("Resources synced!");
        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(accg_manual_file_name);
        }

        private void ACCGMainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.ExitThread();
        }

        private void ACCGMainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Saving the accg series list                 
            ACCGMainForm.accg_resource.SaveACCGSeries(ACCGMainForm.accg_series_file_name, ACCGMainForm.accg_series_list);  
        }
       
              
    }
}
