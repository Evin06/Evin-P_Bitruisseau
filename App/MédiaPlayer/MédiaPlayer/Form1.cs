using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MQTTnet;
using MQTTnet.Client;
using System.Collections.Generic;
using MédiaPlayer.Models;
namespace MédiaPlayer
{
    public partial class Form1 : Form
    {
        private IMqttClient mqttClient;
        private static List<Music> receivedMusicList = new List<Music>(); // List of received music objects

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

                    // Subscribe to the topic
                    await mqttClient.SubscribeAsync(new MqttTopicFilterBuilder()
                        .WithTopic("test")
                        .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                        .Build());

                    // Register callback for received messages
                    mqttClient.ApplicationMessageReceivedAsync += HandleReceivedMessage;
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

        private async Task HandleReceivedMessage(MqttApplicationMessageReceivedEventArgs e)
        {
            try
            {
                // Decode the message payload
                string receivedMessage = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);

                try
                {
                    // Deserialize the JSON into a list of Music objects
                    var musicList = MusicManager.DeserializeMusicList(receivedMessage);

                    // Add the received music to the list
                    receivedMusicList.AddRange(musicList);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deserializing message: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error handling received message: {ex.Message}");
            }
        }

        private void buttonSend_Click(object sender, EventArgs e)
        {
            // Example usage: Send a "HELLO" message
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

            mqttClient.PublishAsync(message);
        }

        private void buttonMesMedias_Click(object sender, EventArgs e)
        {
            // Load local music files
            LoadMusicFiles();
        }

        private void buttonMediaAutres_Click(object sender, EventArgs e)
        {
            // Display the received music list
            listBox1.Items.Clear();

            foreach (var music in receivedMusicList)
            {
                listBox1.Items.Add(music.GetFormattedDetails());
            }

            if (receivedMusicList.Count == 0)
            {
                listBox1.Items.Add("Aucune musique reçue pour le moment.");
            }
        }

        private void LoadMusicFiles()
        {
            string musicFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\Music");

            listBox1.Items.Clear();

            if (!Directory.Exists(musicFolderPath))
            {
                listBox1.Items.Add("Le dossier 'Music' n'existe pas.");
                return;
            }

            string[] musicFiles = Directory.GetFiles(musicFolderPath, "*.mp3");

            foreach (string musicFile in musicFiles)
            {
                var music = new Music
                {
                    Name = Path.GetFileNameWithoutExtension(musicFile),
                    Extension = Path.GetExtension(musicFile),
                    Duration = "Unknown" // Extract duration if needed
                };

                listBox1.Items.Add(music.GetFormattedDetails());
            }
        }
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Logique à exécuter lorsque l'utilisateur sélectionne un élément dans la ListBox
            if (listBox1.SelectedItem != null)
            {
                MessageBox.Show($"Fichier sélectionné : {listBox1.SelectedItem}");
            }
        }

        private void buttonReglage_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Réglages à définir.");
        }

    }
}
