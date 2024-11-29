using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;

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

                // Check if the message contains exactly "hello"
                if (receivedMessage.Contains("hello"))
                {

                    // Respond with the music list
                    string musicList = GetMusicList();
                    await SendMqttMessage(musicList);

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

        private static string GetMusicList()
        {
            string musicFolderPath = @"..\..\..\Music";
            try
            {
                // Retrieve all mp3 files from the specified directory
                string[] musicFiles = Directory.GetFiles(musicFolderPath, "*.mp3");

                // Get only the file names
                var fileNames = musicFiles.Select(Path.GetFileName).ToArray();

                // Join the file names into a single string separated by commas
                return fileNames.Length > 0
                    ? "Available music: " + string.Join(", ", fileNames)
                    : "No music files found.";
            }
            catch (Exception ex)
            {
                return $"Error retrieving music files: {ex.Message}";
            }
        }
    }
}
