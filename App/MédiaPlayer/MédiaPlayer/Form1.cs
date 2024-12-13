using System;
using System.IO;
using System.Windows.Forms;
using MQTTnet;
using MQTTnet.Client;
using TagLib;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace MédiaPlayer
{
    public partial class Form1 : Form
    {
        private IMqttClient mqttClient;
        private static List<string> receivedMusicList = new List<string>(); // Liste des musiques reçues via MQTT

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
                // Décoder le message reçu
                string receivedMessage = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);

                if (receivedMessage.Contains(":"))
                {
                    // Extraire la partie après les deux-points
                    string musicPart = receivedMessage.Split(':')[1].Trim();

                    // Séparer les musiques par les virgules
                    string[] musicFiles = musicPart.Split(new[] { ", " }, StringSplitOptions.None);

                    foreach (var music in musicFiles)
                    {
                        if (!string.IsNullOrWhiteSpace(music)) 
                        {
                            receivedMusicList.Add(music); // Ajouter le nom de la musique à la liste
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"Message ignoré : {receivedMessage}");
                }
            }
            catch (Exception ex)
            {
                // Gérer les erreurs de traitement
                MessageBox.Show($"Erreur lors du traitement du message : {ex.Message}");
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Charger les fichiers musicaux locaux au démarrage
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

                // Récupérer tous les fichiers musicaux (tous formats)
                string[] musicFiles = Directory.GetFiles(musicFolderPath, "*.*")
                    .Where(file => file.EndsWith(".mp3"))
                    .ToArray();

                if (musicFiles.Length == 0)
                {
                    listBox1.Items.Add("Aucun fichier musical trouvé.");
                    return;
                }

                foreach (string musicFile in musicFiles)
                {
                    try
                    {
                        // Utiliser TagLib pour extraire les métadonnées
                        var file = TagLib.File.Create(musicFile);
                        var duration = file.Properties.Duration;

                        var music = new Music
                        {
                            Name = Path.GetFileNameWithoutExtension(musicFile), // Nom sans extension
                            Extension = Path.GetExtension(musicFile), // Extension (ex: .mp3)
                            Duration = $"{(int)duration.TotalMinutes} min {duration.Seconds} sec" // Durée formatée
                        };

                        // Ajouter la description formatée à la ListBox
                        listBox1.Items.Add(music.GetFormattedDetails());
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Erreur avec le fichier {Path.GetFileName(musicFile)} : {ex.Message}");
                    }
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

            if (receivedMusicList.Count > 0)
            {
                // Afficher la liste des musiques reçues dans la ListBox
                foreach (var music in receivedMusicList)
                {
                    listBox1.Items.Add(music);
                }
            }
            else
            {
                listBox1.Items.Add("Aucune musique reçue pour le moment.");
            }
        }

        private void buttonReglage_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Réglages à définir.");
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Vérifier si un élément est sélectionné
            if (listBox1.SelectedItem != null)
            {
                MessageBox.Show($"Fichier sélectionné : {listBox1.SelectedItem}");
            }
        }
    }

    // Classe pour stocker les informations des fichiers musicaux
    public class Music
    {
        public string Name { get; set; } // Nom du fichier
        public string Extension { get; set; } // Extension du fichier (ex: .mp3)
        public string Duration { get; set; } // Durée du fichier en minutes et secondes

        // Retourne une chaîne formatée pour affichage dans la ListBox
        public string GetFormattedDetails()
        {
            return $"{Name} | {Extension} | {Duration}";
        }
    }
}
