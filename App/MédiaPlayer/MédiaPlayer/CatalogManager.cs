using MQTTnet;
using MQTTnet.Client;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using MédiaPlayer.Models;
using MédiaPlayer.Envelopes;

namespace MédiaPlayer
{
    public class CatalogManager
    {
        public readonly IMqttClient mqttClient;
        public List<MediaData> musics = new List<MediaData>();

        public CatalogManager(IMqttClient mqttClient)
        {
            this.mqttClient = mqttClient;
        }

        public async Task SendCatalog(string topic)
        {
            try
            {
                if (musics.Count == 0)
                {
                    MessageBox.Show("No media to send.", "Catalog Empty", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                SendCatalog sendCatalog = new SendCatalog();
                sendCatalog.Content = musics;

                GenericEnvelope envelope = new GenericEnvelope
                {
                    SenderId = mqttClient.Options.ClientId,
                    MessageType = MessageType.ENVOIE_CATALOGUE,
                    EnvelopeJson = sendCatalog.ToJson()
                };

                var message = new MqttApplicationMessageBuilder()
                    .WithTopic(topic)
                    .WithPayload(JsonSerializer.Serialize(envelope))
                    .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                    .Build();

                await mqttClient.PublishAsync(message);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending catalog: {ex.Message}", "Catalog Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public async Task SendData(string topic, string data)
        {
            try
            {
                var message = new MqttApplicationMessageBuilder()
                    .WithTopic(topic)
                    .WithPayload(data)
                    .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                    .Build();

                await mqttClient.PublishAsync(message);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending data: {ex.Message}", "Data Send Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
