using MQTTnet;
using MQTTnet.Client;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MédiaPlayer
{
    public static class MQTTConnectionManager
    {
        private static IMqttClient client;
        private static readonly string brokerAddress = "mqtt.blue.section-inf.ch";
        private static readonly int port = 1883;
        private static readonly string username = "ict";
        private static readonly string password = "321";

        public static async Task<IMqttClient> GetClientAsync()
        {
            if (client == null || !client.IsConnected)
            {
                var factory = new MqttFactory();
                client = factory.CreateMqttClient();
                var options = new MqttClientOptionsBuilder()
                    .WithTcpServer(brokerAddress, port)
                    .WithCredentials(username, password)
                    .WithClientId(Guid.NewGuid().ToString())
                    .WithCleanSession()
                    .Build();

                try
                {
                    await client.ConnectAsync(options);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error connecting to MQTT broker: {ex.Message}", "MQTT Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            return client;
        }
    }
}
