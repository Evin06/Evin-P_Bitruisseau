using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;

namespace MédiaPlayer
{
    internal static class Program
    {
        [STAThread]
        static async Task Main()
        {
            
           
            Console.WriteLine("Starting the MQTT client...");

            string broker = "inf-n510-p301";
            int port = 1883;
            string clientId = Guid.NewGuid().ToString();
            string topic = "test";
            string username = "ict";
            string password = "321";

            // Create a MQTT client factory
            var factory = new MqttFactory();

            // Create a MQTT client instance
            var mqttClient = factory.CreateMqttClient();

            // Create MQTT client options
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(broker, port) // MQTT broker address and port
                .WithCredentials(username, password) // Set username and password
                .WithClientId(clientId)
                .WithCleanSession()
                .Build();

            try
            {
                Console.WriteLine("Connecting to the MQTT broker...");
                var connectResult = await mqttClient.ConnectAsync(options);

                if (connectResult.ResultCode == MqttClientConnectResultCode.Success)
                {
                    MessageBox.Show("Connected to MQTT broker successfully.");

                    // Subscribe to a topic
                    await mqttClient.SubscribeAsync(new MqttTopicFilterBuilder()
                        .WithTopic(topic)
                        .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                        .Build());

                    Console.WriteLine($"Subscribed to topic '{topic}'.");

                    // Callback function when a message is received
                    mqttClient.ApplicationMessageReceivedAsync += e =>
                    {
                        string message = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);
                        Console.WriteLine($"Received message: {message}");
                        return Task.CompletedTask;
                    };

           
                    // Publier un message personnalisé
                    var queryMessage = new MqttApplicationMessageBuilder()
                        .WithTopic(topic)
                        .WithPayload("Salut tout le monde, quelqu'un a des musiques à partager ?")
                        .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                        .WithRetainFlag(false)
                        .Build();

                    await mqttClient.PublishAsync(queryMessage);
                    Console.WriteLine("Message 'Salut tout le monde, quelqu'un a des musiques à partager ?' published.");


                    // Unsubscribe and disconnect
                    Console.WriteLine("Unsubscribing and disconnecting...");
                    await mqttClient.UnsubscribeAsync(topic);
                    await mqttClient.DisconnectAsync();
                    Console.WriteLine("Disconnected from MQTT broker.");
                }
                else
                {
                    Console.WriteLine($"Failed to connect to MQTT broker: {connectResult.ResultCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
            Application.Run(new Form1());
        }
    }
}
