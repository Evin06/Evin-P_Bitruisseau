using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using MQTTnet;
using MQTTnet.Client;
using System.Collections.Generic;
using MédiaPlayer.Models;
using MédiaPlayer.Envelopes;
using EnvelopeTypes = MédiaPlayer.Envelopes;
using ModelTypes = MédiaPlayer.Models;

namespace MédiaPlayer
{
    public partial class Form1 : Form
    {
        private IMqttClient mqttClient;
        private static List<Music> receivedMusicList = new List<Music>(); 

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

                var connectResult = await mqttClient.ConnectAsync(options);

                if (connectResult.ResultCode == MQTTnet.Client.MqttClientConnectResultCode.Success)
                {
                    MessageBox.Show("Connected to MQTT broker.");

                    await mqttClient.SubscribeAsync(new MqttTopicFilterBuilder()
                        .WithTopic("test")
                        .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                        .Build());

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
                string receivedMessage = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                GenericEnvelope envelope = JsonSerializer.Deserialize<GenericEnvelope>(receivedMessage);

                if (envelope.SenderId == mqttClient.Options.ClientId) return;

                switch (envelope.MessageType)
                {
                    case EnvelopeTypes.MessageType.DEMANDE_CATALOGUE: 
                        await RespondToCatalogRequest();
                        break;

                    case EnvelopeTypes.MessageType.ENVOIE_CATALOGUE: 
                        ProcessReceivedCatalog(envelope);
                        break;

                    case EnvelopeTypes.MessageType.ENVOIE_FICHIER: 
                        ProcessReceivedMusic(envelope);
                        break;

                    default:
                        Console.WriteLine("Message non reconnu.");
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du traitement du message : {ex.Message}");
            }
        }

        private async Task RespondToCatalogRequest()
        {
            var catalog = new SendCatalog
            {
                Content = LoadLocalMediaData()
            };

            var responseEnvelope = new GenericEnvelope
            {
                SenderId = mqttClient.Options.ClientId,
                MessageType = EnvelopeTypes.MessageType.ENVOIE_CATALOGUE, // Correspond à 0
                EnveloppeJson = catalog.ToJson()
            };

            await SendData("test", JsonSerializer.Serialize(responseEnvelope));
        }

        private void ProcessReceivedCatalog(GenericEnvelope envelope)
        {
            var receivedCatalog = JsonSerializer.Deserialize<SendCatalog>(envelope.EnveloppeJson);
            foreach (var media in receivedCatalog.Content)
            {
                receivedMusicList.Add(new Music
                {
                    Name = media.FileName,
                    Duration = media.FileDuration,
                    Extension = media.FileType
                });
            }
            MessageBox.Show("Catalogue reçu et ajouté à la liste !");
        }

        private void ProcessReceivedMusic(GenericEnvelope envelope)
        {
            var receivedMusic = JsonSerializer.Deserialize<SendMusic>(envelope.EnveloppeJson);
            MessageBox.Show($"Musique reçue : {receivedMusic.Content}");
        }

        private async Task SendData(string topic, string data)
        {
            var message = new MQTTnet.MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(data)
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .WithRetainFlag(false)
                .Build();

            await mqttClient.PublishAsync(message);
            Console.WriteLine("Message sent successfully!");
        }

        private List<MediaData> LoadLocalMediaData()
        {
            string musicFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\Music");
            var mediaList = new List<MediaData>();

            if (Directory.Exists(musicFolderPath))
            {
                var files = Directory.GetFiles(musicFolderPath, "*.mp3");

                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    mediaList.Add(new MediaData
                    {
                        FileName = Path.GetFileNameWithoutExtension(file),
                        FileArtist = "Unknown Artist",
                        FileType = fileInfo.Extension,
                        FileSize = fileInfo.Length,
                        FileDuration = "Unknown"
                    });
                }
            }

            return mediaList;
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
                    Duration = "Unknown"
                };

                listBox1.Items.Add(music.GetFormattedDetails());
            }
        }
        private void buttonMesMedias_Click(object sender, EventArgs e)
        {
            LoadMusicFiles();
        }

        private async void buttonMediaAutres_Click(object sender, EventArgs e)
        {
            var askCatalog = new AskCatalog
            {
                Content = "Qui a un catalogue ?"
            };

            var envelope = new GenericEnvelope
            {
                SenderId = mqttClient.Options.ClientId,
                MessageType = EnvelopeTypes.MessageType.DEMANDE_CATALOGUE, 
                EnveloppeJson = askCatalog.ToJson()
            };

            string serializedEnvelope = JsonSerializer.Serialize(envelope);
            await SendData("test", serializedEnvelope);

            MessageBox.Show("Demande de catalogue envoyée !");
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
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
