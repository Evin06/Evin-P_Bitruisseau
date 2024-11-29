using System;
using System.IO;
using System.Linq;
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

        private void Form1_Load(object sender, EventArgs e)
        {
            // Charger les fichiers musicaux au démarrage
            LoadMusicFiles();
        }

        private void LoadMusicFiles()
        {
            string musicFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\Music");

            try
            {
                listBox1.Items.Clear();

                if (!Directory.Exists(musicFolderPath))
                {
                    listBox1.Items.Add("Le dossier 'Music' n'existe pas.");
                    return;
                }

                string[] musicFiles = Directory.GetFiles(musicFolderPath, "*.mp3");

                if (musicFiles.Length == 0)
                {
                    listBox1.Items.Add("Aucun fichier musical trouvé.");
                    return;
                }

                foreach (string musicFile in musicFiles)
                {
                    listBox1.Items.Add(Path.GetFileName(musicFile));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des fichiers musicaux : {ex.Message}");
            }
        }

        private async void buttonSend_Click(object sender, EventArgs e)
        {
            try
            {
                if (mqttClient == null || !mqttClient.IsConnected)
                {
                    MessageBox.Show("MQTT client is not connected.");
                    return;
                }

                var message = new MQTTnet.MqttApplicationMessageBuilder()
                    .WithTopic("test")
                    .WithPayload("HELLO qui a des musiques ?")
                    .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                    .WithRetainFlag(false)
                    .Build();

                await mqttClient.PublishAsync(message);

                MessageBox.Show("Message envoyé : HELLO qui a des musiques ?");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'envoi du message : {ex.Message}");
            }
        }

        private void buttonMesMedias_Click(object sender, EventArgs e)
        {
            LoadMusicFiles();
        }

        private void buttonMediaAutres_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
        }

        private void buttonReglage_Click(object sender, EventArgs e)
        {
           
        }
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Vérifier si un élément est sélectionné
            if (listBox1.SelectedItem != null)
            {
                // Afficher le fichier sélectionné (ou ajoutez une autre logique)
                MessageBox.Show($"Fichier sélectionné : {listBox1.SelectedItem}");
            }
        }
    }
}
