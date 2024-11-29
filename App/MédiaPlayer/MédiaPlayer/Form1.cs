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
    public partial class Form1 : Form
    {
        private IMqttClient mqttClient;
        private const string broker = "blue.section-inf.ch";
        private const int port = 1883;
        private const string username = "ict";
        private const string password = "321";
        private const string topic = "test";

        public Form1()
        {
            InitializeComponent();
            InitializeMqttClient();
        }

        private async void InitializeMqttClient()
        {
            try
            {
                // Initialize MQTT client
                var factory = new MqttFactory();
                mqttClient = factory.CreateMqttClient();

                // Create MQTT client options
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

                    // Register message received handler
                    mqttClient.ApplicationMessageReceivedAsync += MqttClient_ApplicationMessageReceived;
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

        private async Task MqttClient_ApplicationMessageReceived(MqttApplicationMessageReceivedEventArgs e)
        {
            // Decode the message payload
            string receivedMessage = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);

            // Check if the message contains "HELLO qui a des musiques ?"
            if (receivedMessage.Contains("HELLO"))
            {
                // Get the list of music files
                string musicList = GetMusicList();

                // Send the list of music files back
                await SendMqttMessage(musicList);
            }
        }

        private string GetMusicList()
        {
            string musicFolderPath = @"..\..\..\Music";
            try
            {
                // Retrieve all mp3 files in the specified directory
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

        private async 
        Task
SendMqttMessage(string message)
        {
            if (mqttClient == null || !mqttClient.IsConnected)
            {
                MessageBox.Show("MQTT client is not connected. Please check the connection.");
                return;
            }

            try
            {
                var mqttMessage = new MqttApplicationMessageBuilder()
                    .WithTopic(topic)
                    .WithPayload(message)
                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                    .WithRetainFlag(false)
                    .Build();

                await mqttClient.PublishAsync(mqttMessage);
                MessageBox.Show($"Message '{message}' sent successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending MQTT message: {ex.Message}");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Send the MQTT message when the button is clicked
            SendMqttMessage("HELLO qui a des musiques ?");
        }
    }
}
