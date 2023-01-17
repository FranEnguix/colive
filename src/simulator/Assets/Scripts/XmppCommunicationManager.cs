using Matrix;
using Matrix.Extensions.Client.Message;
using Matrix.Extensions.Client.Presence;
using Matrix.Network;
using Matrix.Network.Resolver;
using Matrix.Xml;
using Matrix.Xmpp;
using Matrix.Xmpp.Base;
using Matrix.Xmpp.XData;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class XmppCommunicationManager : MonoBehaviour
{
	[SerializeField] private string xmppName = "fiveserver";
	[SerializeField] private string xmppPass = "fiveserver";
	[SerializeField] private string xmppDomain = "localhost";
	[SerializeField] private string xmppHostnameResolver = "127.0.0.1";

    [SerializeField] private GameObject mapLoader;
    [SerializeField] private GameObject[] agentsPrefabs;

    private Dictionary<string, GameObject> entities;
    private Dictionary<string, GameObject> spawners;
    private ConcurrentQueue<ICommand> commandQueue;
    private XmppClient xmppClient;

    private void Awake() {
        entities = new Dictionary<string, GameObject>();
        spawners = new Dictionary<string, GameObject>();
        commandQueue = new ConcurrentQueue<ICommand>();
        ConnectToXmppServerAsync();
        SetupXmppHandlers();
    }

    private void Start() {
        LinkSpawners();
    }

    private void Update() {
        DequeueAndProcessCommand();
    }

    private void OnDestroy() {
        xmppClient.DisconnectAsync();
    }

    private async void ConnectToXmppServerAsync() {
        xmppClient = new XmppClient {
            Username = xmppName,
            Password = xmppPass,
            XmppDomain = xmppDomain,
            HostnameResolver = new StaticNameResolver(xmppHostnameResolver),
            CertificateValidator = new AlwaysAcceptCertificateValidator(),
            Tls = false
        };
        await xmppClient.ConnectAsync();
        await xmppClient.SendPresenceAsync(Show.Chat, "five server awake");
    }

    private void SetupXmppHandlers() {
        SetupCommandHandler();
        SetupPresenceHandler();
    }
    private void SetupCommandHandler() {
        xmppClient.XmppXElementStreamObserver.Where(el => {
            if (el is Message message && message.XData.Fields.Length > 0) {
                Field metadata = message.XData.Fields[0];
                bool command = metadata.Values.Length > 0 && metadata.Values[0].Equals("command");
                return metadata.Var.Equals("five") && command;
            }
            return false;
        }).Subscribe(el => {
            Message message = (Message)el;
            string agent = message.From.User;
            string content = message.Body;
            Debug.Log($"XMPP message from {agent}: {content}");
            EnqueueCommandFromMessage(agent, content);
        });
    }

    private void SetupPresenceHandler() {
        xmppClient.XmppXElementStreamObserver.Where(el => el is Presence).Subscribe(el => {
            Presence presence = (Presence)el;
            Debug.Log($"XMPP presence from {presence.From}: {presence.Name}");
        });
    }

    private void LinkSpawners() {
        var mapLoaderScript = mapLoader.GetComponent<MapLoader>();
        spawners = mapLoaderScript.GetSpawners();
    }

    private void EnqueueCommandFromMessage(string agentName, string message) {
        ICommand command = CommandParser.ParseCommand(message);
        if (command != null) {
            command.AgentName = agentName;
            commandQueue.Enqueue(command);
        } else {
            Debug.LogWarning($"{agentName} sent a missformat command: {message}"); ;
        }
        /* DEPRECATED because Unity only instantiates in main thread.
        if (command != null) {
            //command.AgentName = agentName;
            command.Execute(entities);
            if (command is CreateCommand create) {
                if (!entities.ContainsKey(create.AgentName))
                    CreateEntity(create);
                SendPositionOfAvatarAgent(entities[create.AgentName]);
            } else {
                //command.Execute(entities);
            }
            // commandQueue.Enqueue(command);
        }
        */
    }
    private void DequeueAndProcessCommand() {
        if (!commandQueue.IsEmpty)
            if (commandQueue.TryDequeue(out ICommand command)) {
                command.Execute(entities);
                if (command is CreateCommand create) {
                    if (!entities.ContainsKey(create.AgentName))
                        CreateEntity(create);
                    SendPositionOfAvatarAgent(entities[create.AgentName]);
                }
            }
    }
	private void CreateEntity(CreateCommand command) {
        GameObject agentPrefab = GetAgentPrefab(command.AgentPrefab);
        if (agentPrefab != null) {
            GameObject entity = InstantiateEntity(agentPrefab, command);
            entity.name = command.AgentName;
            entities.Add(entity.name, entity);
            // var tcpClient = tcpCommandClients.Find(x => x.AgentName == command.AgentName);
            var entityComponent = entity.GetComponent<Entity>();
            entityComponent.XmppClient = xmppClient;
            // entityComponent.TcpCommandManager = tcpClient;
            entityComponent.AgentCollision = command.AgentCollision;
        } else {
            Debug.LogError($"Agent {command.AgentName} asks for non existing prefab: {command.AgentPrefab}");
        }
    }

    private GameObject GetAgentPrefab(string agentPrefabName) {
        foreach(var agentPrefab in agentsPrefabs)
            if (agentPrefab.name.Equals(agentPrefabName, StringComparison.InvariantCultureIgnoreCase))
                return agentPrefab;
        return null;
    }

    private GameObject InstantiateEntity(GameObject agentPrefab, CreateCommand command) {
        if (command.StarterPosition != null) {
            Vector3 position = command.StarterPosition3();
            return Instantiate(agentPrefab, position, Quaternion.identity);
        } else {
            var spawner = spawners[command.SpawnerName];
            Vector3 spawnerPosition = spawner.transform.position;
            return Instantiate(agentPrefab, spawnerPosition, Quaternion.identity);
        }
    }

    private async Task SendPositionOfAvatarAgent(GameObject agent) {
        var entityComponent = agent.GetComponent<Entity>();
        await entityComponent.SendCurrentPositionAsync();
    }
}
