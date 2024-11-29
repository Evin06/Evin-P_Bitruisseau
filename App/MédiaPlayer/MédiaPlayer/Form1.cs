using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MQTTnet;
using MQTTnet.Client;


namespace MédiaPlayer
{
    public partial class Form1 : Form
    {
        private IMqttClient mqttClient;

        public Form1()
        {
            InitializeComponent();
            InitializeMqttClient();
        }

        private async void InitializeMqttClient()
        {
            try
            {
                var factory = new MqttFactory();
                mqttClient = factory.CreateMqttClient();

                var options = new MqttClientOptionsBuilder()
                    .WithTcpServer("blue.section-inf.ch", 1883)
                    .WithCredentials("ict", "321")
                    .WithClientId(Guid.NewGuid().ToString())
                    .WithCleanSession()
                    .Build();

                // Connect to MQTT broker
                var connectResult = await mqttClient.ConnectAsync(options);
                if (connectResult.ResultCode == MQTTnet.Client.MqttClientConnectResultCode.Success)
                {
                    MessageBox.Show("Connected to MQTT broker.");
                }
                else
                {
                    MessageBox.Show($"Failed to connect to broker: {connectResult.ResultCode}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing MQTT client: {ex.Message}");
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (mqttClient == null || !mqttClient.IsConnected)
                {
                    MessageBox.Show("MQTT client is not connected. Cannot send the message.");
                    return;
                }

                // Create the MQTT message
                var message = new MQTTnet.MqttApplicationMessageBuilder()
                    .WithTopic("test")
                    .WithPayload("HELLO qui a des musiques ?")
                    .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                    .WithRetainFlag(false)
                    .Build();

                // Publish the message
                await mqttClient.PublishAsync(message);

                MessageBox.Show("Message sent: HELLO qui a des musiques ?");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending MQTT message: {ex.Message}");
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadMusicFiles();
        }

        private void LoadMusicFiles()
        {
            string musicFolderPath = @"..\..\..\Music";

            try
            {
                // Effacer les éléments existants dans la ListBox
                listBox1.Items.Clear();

                // Récupérer tous les fichiers mp3 du répertoire spécifié
                string[] musicFiles = Directory.GetFiles(musicFolderPath, "*.mp3");

                foreach (string musicFile in musicFiles)
                {
                    // Récupérer uniquement le nom du fichier sans le chemin complet
                    string fileName = Path.GetFileName(musicFile);
                    listBox1.Items.Add(fileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors du chargement des fichiers musicaux : " + ex.Message);
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                MessageBox.Show($"Musique sélectionnée : {listBox1.SelectedItem}");
            }
        }
    }
}
