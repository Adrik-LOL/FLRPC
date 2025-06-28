using Newtonsoft.Json;
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

namespace FLRPC.Config
{
    public partial class ConfigFile : Form
    {
        public ConfigFile()
        {
            InitializeComponent();
        }

        private void numericUpDown1_Enter(object sender, EventArgs e)
        {
            toolTip1.Show("Update Interval (ms)", numericUpDown1);
        }

        private void ConfigFile_Load(object sender, EventArgs e)
        {
            ConfigSettings.LoadConfig(Program.ConfigPath);

            checkBox1.Checked = ConfigValues.SecretMode;
            checkBox2.Checked = ConfigValues.ShowTimestamp;
            checkBox3.Checked = ConfigValues.DisplayConfigInfo;
            checkBox4.Checked = ConfigValues.AccurateVersion;

            textBox1.Text = ConfigValues.ClientID;
            numericUpDown1.Minimum = Int32.MinValue;
            numericUpDown1.Maximum = Int32.MaxValue;
            numericUpDown1.Value = ConfigValues.UpdateInterval;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ConfigValues.SecretMode = checkBox1.Checked;
            ConfigValues.ShowTimestamp = checkBox2.Checked;
            ConfigValues.DisplayConfigInfo = checkBox3.Checked;
            ConfigValues.AccurateVersion = checkBox4.Checked;

            ConfigValues.ClientID = textBox1.Text;
            ConfigValues.UpdateInterval = (int)numericUpDown1.Value;

            SaveNewConfig(Program.ConfigPath);
            MessageBox.Show("Configuration saved!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static void SaveNewConfig(string filePath)
        {
            try
            {
                // Get current values from ConfigValues properties (not default attributes)
                var properties = typeof(ConfigValues)
                    .GetProperties()
                    .Where(prop => Attribute.IsDefined(prop, typeof(DefaultValueAttribute)))
                    .ToDictionary(
                        prop => prop.Name,
                        prop => prop.GetValue(null)  // current value of the property
                    );

                // Serialize properties to JSON
                string json = JsonConvert.SerializeObject(properties, Formatting.Indented);

                // Overwrite the file every time
                File.WriteAllText(filePath, json);

                Console.WriteLine("Configuration saved/overwritten successfully.\n", Color.LightSkyBlue);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving configuration: {ex.Message}", Color.Red);
                Utils.LogException(ex, "SaveNewConfig");
            }
        }

    }
}
