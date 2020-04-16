using System.Net;
using System.Threading.Tasks;
using Grpc.Core;
using Subs;

namespace QubeView
{
    using UnityEngine;

    public class Subtitle : MonoBehaviour
    {
        private string _subtitle;
        
        private SubsService.SubsServiceClient _client;
        
        private Channel _channel;

        private string _hostName;
        
        private string _ip;

        public async void Start()
        { 
            _channel = new Channel("192.168.43.1:33455", ChannelCredentials.Insecure);
            
            _client = new SubsService.SubsServiceClient(_channel);
            
            _hostName = Dns.GetHostName(); 
            
            _ip = Dns.GetHostEntry(_hostName).AddressList[0].ToString();
            
            await Connect(_client, _channel, _ip);
        }

        private async Task Connect(SubsService.SubsServiceClient subsClient, Channel subsChannel, string ipAddr)
        {
            using (var reply = subsClient.SubManyTimes(new SubManyTimesRequest
            {
                IpAddr = ipAddr
            }))
            {
                while (await reply.ResponseStream.MoveNext())
                {
                    _subtitle = reply.ResponseStream.Current.Result;
                }
            }
            subsChannel.ShutdownAsync().Wait();
        }
        
        public void Update()
        {
            GetComponent<TextMesh>().text = _subtitle;
        }
    }
}
