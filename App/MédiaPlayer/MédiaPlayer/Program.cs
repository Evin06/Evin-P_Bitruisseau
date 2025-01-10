using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using MédiaPlayer.Models;
using System.Collections.Generic;
using MédiaPlayer.Envelopes;
using System.Text.Json;

namespace MédiaPlayer
{
    internal static class Program
    {
        private static IMqttClient mqttClient;
        private const string broker = "blue.section-inf.ch";
        private const int port = 1883;
        private const string topic = "tutu";
        private const string username = "ict";
        private const string password = "321";

        [STAThread]
        static async Task Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            await InitializeMqttClient();
            Application.Run(new Form1());
        }

        private static async Task InitializeMqttClient()
        {
            try
            {
                var factory = new MqttFactory();
                mqttClient = factory.CreateMqttClient();

                var options = new MqttClientOptionsBuilder()
                    .WithTcpServer(broker, port)
                    .WithCredentials(username, password)
                    .WithClientId(Guid.NewGuid().ToString())
                    .WithCleanSession()
                    .Build();

                var connectResult = await mqttClient.ConnectAsync(options);

                if (connectResult.ResultCode == MQTTnet.Client.MqttClientConnectResultCode.Success)
                {
                    MessageBox.Show("Connected to MQTT broker successfully.");

                    await mqttClient.SubscribeAsync(new MqttTopicFilterBuilder()
                        .WithTopic(topic)
                        .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                        .Build());

                    mqttClient.ApplicationMessageReceivedAsync += HandleReceivedMessage;
                }
                else
                {
                    MessageBox.Show($"Failed to connect to MQTT broker: {connectResult.ResultCode}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing MQTT client: {ex.Message}");
            }
        }

        private static async Task HandleReceivedMessage(MqttApplicationMessageReceivedEventArgs e)
        {
            try
            {
                string receivedMessage = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);
                var envelope = JsonSerializer.Deserialize<GenericEnvelope>(receivedMessage);

                if (envelope.SenderId == mqttClient.Options.ClientId)
                {
                    return;
                }

                // Si on reçoit un message de type 1 (DEMANDE_CATALOGUE)
                if (envelope.MessageType == MessageType.DEMANDE_CATALOGUE)
                {
                    await RespondToCatalogRequest();
                }
                else
                {
                    Console.WriteLine($"Unhandled message type: {envelope.MessageType}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error handling received message: {ex.Message}");
            }
        }

        private static async Task RespondToCatalogRequest()
        {
            try
            {
                // Créez le catalogue local
                var catalog = new SendCatalog
                {
                    Content = LoadLocalMediaData()
                };

                // Préparez un message enveloppe de type 0 (ENVOIE_CATALOGUE)
                var envelope = new GenericEnvelope
                {
                    SenderId = mqttClient.Options.ClientId,
                    MessageType = MessageType.ENVOIE_CATALOGUE, // Type 0
                    EnveloppeJson = JsonSerializer.Serialize(catalog)
                };

                // Sérialisez et envoyez le message
                string message = JsonSerializer.Serialize(envelope);
                await SendMqttMessage(message);

                Console.WriteLine("Catalogue envoyé en réponse au DEMANDE_CATALOGUE !");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'envoi du catalogue : {ex.Message}");
            }
        }

        private static async Task SendMqttMessage(string message)
        {
            var mqttMessage = new MQTTnet.MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(message)
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .WithRetainFlag(false)
                .Build();

            await mqttClient.PublishAsync(mqttMessage);
        }

        private static List<MediaData> LoadLocalMediaData()
        {
            string musicFolderPath = @"..\..\..\Music";
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
    }
}
