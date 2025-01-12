using MQTTnet;
using MQTTnet.Client;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using MédiaPlayer.Models;
using MédiaPlayer.Envelopes;

namespace MédiaPlayer
{
    public class CatalogManager
    {
        private readonly IMqttClient mqttClient;

        public CatalogManager(IMqttClient mqttClient)
        {
            this.mqttClient = mqttClient;
        }

        public async Task SendCatalog(string topic)
        {
            var catalog = new SendCatalog { Content = LoadLocalMediaData() };
            if (!catalog.Content.Any())
            {
                MessageBox.Show("No media to send.", "Catalog Empty", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var envelope = new GenericEnvelope
            {
                SenderId = mqttClient.Options.ClientId,
                MessageType = MessageType.ENVOIE_CATALOGUE,
                EnveloppeJson = JsonSerializer.Serialize(catalog)
            };

            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(JsonSerializer.Serialize(envelope))
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .Build();

            await mqttClient.PublishAsync(message);
        }


        private List<MediaData> LoadLocalMediaData()
        {
            // Load media data from disk; placeholder for actual implementation
            return new List<MediaData>();
        }
        public async Task SendData(string topic, string data)
        {
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(data)
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .Build();

            await mqttClient.PublishAsync(message);
        }
    }
}
