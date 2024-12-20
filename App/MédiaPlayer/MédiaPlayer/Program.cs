using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using MédiaPlayer.Models;

namespace MédiaPlayer
{
    internal static class Program
    {
        private static IMqttClient mqttClient;
        private const string broker = "blue.section-inf.ch";
        private const int port = 1883;
        private const string topic = "test";
        private const string username = "ict";
        private const string password = "321";

        [STAThread]
        static async Task Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Initialize MQTT client
            await InitializeMqttClient();

            // Run the application
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

                // Connect to MQTT broker
                var connectResult = await mqttClient.ConnectAsync(options);

                if (connectResult.ResultCode == MqttClientConnectResultCode.Success)
                {
                    MessageBox.Show("Connected to MQTT broker successfully.");

                    // Subscribe to the topic
                    await mqttClient.SubscribeAsync(new MqttTopicFilterBuilder()
                        .WithTopic(topic)
                        .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                        .Build());

                    // Register callback for received messages
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
                // Decode the message payload
                string receivedMessage = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);

                // Check if the message contains "HELLO"
                if (receivedMessage.Contains("HELLO"))
                {
                    // Respond with the serialized music list
                    string musicListJson = GetSerializedMusicList();
                    await SendMqttMessage(musicListJson);
                }
                else
                {
                    Console.WriteLine($"Ignored message: {receivedMessage}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error handling received message: {ex.Message}");
            }
        }

        private static async Task SendMqttMessage(string message)
        {
            try
            {
                var mqttMessage = new MqttApplicationMessageBuilder()
                    .WithTopic(topic)
                    .WithPayload(message)
                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                    .WithRetainFlag(false)
                    .Build();

                await mqttClient.PublishAsync(mqttMessage);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending MQTT message: {ex.Message}");
            }
        }

        private static string GetSerializedMusicList()
        {
            string musicFolderPath = @"..\..\..\Music";

            try
            {
                // Retrieve all mp3 files
                string[] musicFiles = Directory.GetFiles(musicFolderPath, "*.mp3");

                // Create a list of Music objects
                var musicList = musicFiles.Select(file => new Music
                {
                    Name = Path.GetFileNameWithoutExtension(file),
                    Extension = Path.GetExtension(file),
                    Duration = "Unknown" // Optionally extract duration using TagLib
                }).ToList();

                // Serialize the list to JSON
                return MusicManager.SerializeMusicList(musicList);
            }
            catch (Exception ex)
            {
                return $"Error retrieving music files: {ex.Message}";
            }
        }
    }
}
