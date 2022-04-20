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

        /// <summary>
        /// 启动服务
        /// </summary>
        public async void StartServe()
        {
            //链式编程
            MqttServerOptionsBuilder optionBuilder = GetDefaultOptionBuilder();
            _server = new MqttFactory().CreateMqttServer();
            UseHook();
            await _server.StartAsync(optionBuilder.Build());
        }

        /// <summary>
        /// 获取已经用默认设置设置好的 MqttServerOptionsBuilder
        /// </summary>
        /// <returns></returns>
        MqttServerOptionsBuilder GetDefaultOptionBuilder()
        {
            return new MqttServerOptionsBuilder()
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
        }

        /// <summary>
        /// 使用钩子
        /// </summary>
        void UseHook()
        {
            _server.UseClientConnectedHandler(c =>
            {
                Console.WriteLine("客户端已连接");
            });
            _server.UseClientDisconnectedHandler(c =>
            {
                Console.WriteLine("客户端断开连接");
            });
        }
    }
}
