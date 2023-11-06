#pragma warning disable CS0436
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using Frictionless;
using NativeWebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Solana.Unity.Wallet;
using UnityEngine;
using Debug = UnityEngine.Debug;
using WebSocketState = NativeWebSocket.WebSocketState;

namespace Socket
{
    // This is just a workaround since the UnitySDK websocket is not reconnecting properly in webgl currently.
    public class SolPlayWebSocketService : MonoBehaviour
    {
        IWebSocket websocket;

        private Dictionary<PublicKey, SocketSubscription> subscriptions =
            new Dictionary<PublicKey, SocketSubscription>();

        private int reconnectTries;
        private int subcriptionCounter;
        public string socketUrl;
        public WebSocketState WebSocketState;

        private class SocketSubscription
        {
            public long Id;
            public long SubscriptionId;
            public Action<MethodResult> ResultCallback;
            public PublicKey PublicKey;
        }

        [Serializable]
        public class WebSocketErrorResponse
        {
            public string jsonrpc;
            public string error;
            public string data;
        }

        [Serializable]
        public class WebSocketResponse
        {
            public string jsonrpc;
            public long result;
            public long id;
        }

        [Serializable]
        public class WebSocketMethodResponse
        {
            public string jsonrpc;
            public string method;
            public MethodResult @params;
        }

        [Serializable]
        public class MethodResult
        {
            public AccountInfo result;
            public long subscription;
        }

        [Serializable]
        public class AccountInfo
        {
            public Context context;
            public Value value;
        }

        [Serializable]
        public class Context
        {
            public int slot;
        }

        [Serializable]
        public class ValueList
        {
            public long lamports;
            public List<string> data;
            public string owner;
            public bool executable;
            public BigInteger rentEpoch;
        }

        [Serializable]
        public class Value
        {
            public long lamports;
            public object data;
            public JObject dataObject;
            public TokenAccountData tokenAccountData;
            public byte[] dataBytes;
            public string owner;
            public bool executable;
            public BigInteger rentEpoch;
        }

        private void Awake()
        {
            if (ServiceFactory.Resolve<SolPlayWebSocketService>() == null)
            {
                ServiceFactory.RegisterSingleton(this);   
            }
        }

        public WebSocketState GetState()
        {
            if (websocket == null)
            {
                return WebSocketState.Closed;
            }

            return websocket.State;
        }
        
        private void SetSocketUrl(string rpcUrl)
        {
            socketUrl = rpcUrl.Replace("https://", "wss://");
            if (socketUrl.Contains("localhost"))
            {
                socketUrl = "ws://localhost:8900";
            }
            Debug.Log("Socket url: " + socketUrl);
        }

        public void Connect(string rpcUrl)
        {
            System.Net.ServicePointManager.ServerCertificateValidationCallback = (message, cert, chain, sslPolicyErrors) => true;
            
            if (websocket != null)
            {
                websocket.OnOpen -= websocketOnOnOpen();
                websocket.OnError -= websocketOnOnError();
                websocket.OnClose -= OnWebSocketClosed;
                websocket.OnMessage -= websocketOnOnMessage();
                websocket.Close();
            }

            SetSocketUrl(rpcUrl);
            Debug.Log("Connect Socket: " + socketUrl);
            
#if UNITY_WEBGL && !UNITY_EDITOR
            websocket = new WebSocket(socketUrl);
#else 
            websocket = new WebSocket(socketUrl);
#endif

            websocket.OnOpen += websocketOnOnOpen();
            websocket.OnError += websocketOnOnError();
            websocket.OnClose += OnWebSocketClosed;
            websocket.OnMessage += websocketOnOnMessage();
            websocket.Connect();

            Debug.Log("Socket connect done");
        }

        private WebSocketMessageEventHandler websocketOnOnMessage()
        {
            return (bytes) =>
            {
                var message = System.Text.Encoding.UTF8.GetString(bytes);
                //Debug.Log("SocketMessage:" + message);
                WebSocketErrorResponse errorResponse = JsonConvert.DeserializeObject<WebSocketErrorResponse>(message);
                if (!string.IsNullOrEmpty(errorResponse.error))
                {
                    Debug.LogError(errorResponse.error);
                    return;
                }

                if (message.Contains("spl-token"))
                {
                    WebSocketMethodResponse methodResponse =
                        JsonConvert.DeserializeObject<WebSocketMethodResponse>(message);
                    if (methodResponse != null)
                    {
                        JObject data = methodResponse.@params.result.value.data as JObject;
                        methodResponse.@params.result.value.dataObject = data;
                        TokenAccountData tokenAccountData = JsonUtility.FromJson<TokenAccountData>(data.ToString());
                        methodResponse.@params.result.value.tokenAccountData = tokenAccountData;
                        Debug.Log("Data string: " + data.ToString());
                    }

                    foreach (var subscription in subscriptions)
                    {
                        if (subscription.Value.SubscriptionId == methodResponse.@params.subscription)
                        {
                            subscription.Value.ResultCallback(methodResponse.@params);
                        }
                    }
                } else if (message.Contains("method"))
                {
                    WebSocketMethodResponse methodResponse =
                        JsonConvert.DeserializeObject<WebSocketMethodResponse>(message);
                    if (methodResponse != null)
                    {
                        JArray data = methodResponse.@params.result.value.data as JArray;
                        if (data != null)
                            methodResponse.@params.result.value.dataBytes = Convert.FromBase64String((string) data[0]);
                    }

                    foreach (var subscription in subscriptions)
                    {
                        if (subscription.Value.SubscriptionId == methodResponse.@params.subscription)
                        {
                            subscription.Value.ResultCallback(methodResponse.@params);
                        }
                    }
                }
                else
                {
                    WebSocketResponse response = JsonConvert.DeserializeObject<WebSocketResponse>(message);

                    foreach (var subscription in subscriptions)
                    {
                        if (subscription.Value.Id == response.id)
                        {
                            subscription.Value.SubscriptionId = response.result;
                        }
                    }
                }
            };
        }

        private static WebSocketErrorEventHandler websocketOnOnError()
        {
            return (e) =>
            {
                Debug.LogError("Socket Error! " + e + " maybe you need to use a different RPC node. For example helius or quicknode");
            };
        }

        private WebSocketOpenEventHandler websocketOnOnOpen()
        {
            return () =>
            {
                reconnectTries = 0;
                foreach (var subscription in subscriptions)
                {
                    SubscribeToPubKeyData(subscription.Value.PublicKey, subscription.Value.ResultCallback);
                }

                Debug.Log("Socket Connection open!");
                MessageRouter.RaiseMessage(new SocketServerConnectedMessage());
            };
        }

        private void OnWebSocketClosed(WebSocketCloseCode closecode)
        {
            Debug.Log("Socket disconnect: " + closecode);
            if (this)
            {
                StartCoroutine(Reconnect());
            }
        }

        public IEnumerator Reconnect()
        {
            while (true)
            {
                yield return new WaitForSeconds(reconnectTries * 0.5f + 0.1f);
                reconnectTries++;
                Debug.Log("Reconnect Socket");
                Connect(socketUrl);
                while (websocket == null || websocket.State == WebSocketState.Closed)
                {
                    yield return null;
                }
                while (websocket.State == WebSocketState.Connecting)
                {
                    yield return null;
                }
                while (websocket.State == WebSocketState.Closed || websocket.State == WebSocketState.Closing)
                {
                    yield break;
                }

                if (websocket.State == WebSocketState.Open)
                {
                    yield break;
                }
            }
        }

        void Update()
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            if (websocket != null && websocket.State == WebSocketState.Open)
            {
                websocket.DispatchMessageQueue();
            }
#endif
            if (websocket != null)
            {
                WebSocketState = websocket.State;
            }
        }

        public async void SubscribeToBlocks()
        {
            if (websocket.State == WebSocketState.Open)
            {
                string accountSubscribeParams ="{ \"jsonrpc\": \"2.0\", \"id\": \"22\", \"method\": \"blockSubscribe\", \"params\": [\"all\"] }";
                await websocket.Send(System.Text.Encoding.UTF8.GetBytes(accountSubscribeParams));
            }
        }

        public async void SubscribeToPubKeyData(PublicKey pubkey, Action<MethodResult> onMethodResult)
        {
            var subscriptionsCount = subcriptionCounter++;
            var socketSubscription = new SocketSubscription()
            {
                Id = subscriptionsCount,
                SubscriptionId = 0,
                ResultCallback = onMethodResult,
                PublicKey = pubkey
            };
            if (subscriptions.ContainsKey(pubkey))
            {
                subscriptions[pubkey].Id = subscriptionsCount;
            }
            else
            {
                subscriptions.Add(pubkey, socketSubscription);
            }
            
            if (websocket.State == WebSocketState.Open)
            {
                string accountSubscribeParams =
                    "{\"jsonrpc\":\"2.0\",\"id\":" + subscriptionsCount +
                    ",\"method\":\"accountSubscribe\",\"params\":[\"" + pubkey.Key +
                    "\",{\"encoding\":\"jsonParsed\",\"commitment\":\"processed\"}]}";
                await websocket.Send(System.Text.Encoding.UTF8.GetBytes(accountSubscribeParams));
            }
        }

        async void UnSubscribeFromPubKeyData(PublicKey pubkey, long id)
        {
            if (websocket.State == WebSocketState.Open)
            {
                string unsubscribeParameter = "{\"jsonrpc\":\"2.0\", \"id\":" + id +
                                              ", \"method\":\"accountUnsubscribe\", \"params\":[" + pubkey.Key + "]}";
                await websocket.Send(System.Text.Encoding.UTF8.GetBytes(unsubscribeParameter));
            }
        }

        private async void OnApplicationQuit()
        {
            if (websocket == null)
            {
                return;
            }

            await websocket.Close();
        }
    }

    public class SocketServerConnectedMessage
    {
    }
}