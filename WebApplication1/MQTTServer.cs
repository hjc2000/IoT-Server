using MQTTnet;
using MQTTnet.Server;

namespace WebApplication1
{
    public class MQTTServer
    {
        public MQTTServer()
        {
            StartServe();
        }

        IMqttServer? _server;
        public async void StartServe()
        {
            //链式编程
            var optionBuilder = new MqttServerOptionsBuilder()
                            .WithDefaultEndpoint()
                            .WithDefaultEndpointPort(1883)
                            .WithConnectionValidator((c) =>
                            {
                                //连接验证
                                c.ReasonCode = MQTTnet.Protocol.MqttConnectReasonCode.Success;
                            })
                            .WithSubscriptionInterceptor((c) =>
                            {
                                //允许订阅主题
                                c.AcceptSubscription = true;
                            })
                            .WithApplicationMessageInterceptor((c) =>
                            {
                                //允许发布主题
                                c.AcceptPublish = true;
                            });
            _server = new MqttFactory().CreateMqttServer();
            _server.UseClientConnectedHandler(c =>
            {
                Console.WriteLine("客户端已连接");
            });
            _server.UseClientDisconnectedHandler(c =>
            {
                Console.WriteLine("客户端断开连接");
            });
            await _server.StartAsync(optionBuilder.Build());
        }

    }
}
