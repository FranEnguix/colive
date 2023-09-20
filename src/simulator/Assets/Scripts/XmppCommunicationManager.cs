using S22.Xmpp;
using S22.Xmpp.Client;
using S22.Xmpp.Extensions.Dataforms;
using S22.Xmpp.Im;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Xml;
using UnityEngine;

public class XmppCommunicationManager : MonoBehaviour
{
	[SerializeField] private string xmppNode = "fiveserver";
	[SerializeField] private string xmppDomain = "localhost";
	[SerializeField] private string xmppPass = "fiveserver";
    [SerializeField] private int xmppPort = 5222;
    [SerializeField] private bool xmppTls = true;

    [SerializeField] private GameObject mapLoader;
    [SerializeField] private GameObject[] agentsPrefabs;
    [SerializeField] private GameObject[] artifactsPrefabs;

    private Dictionary<string, GameObject> entities;
    private Dictionary<string, GameObject> spawners;
    private ConcurrentQueue<ICommand> commandQueue;
    private XmppClient xmppClient;

    private void Awake() {
        LoadPlayerPrefs();
        entities = new Dictionary<string, GameObject>();
        spawners = new Dictionary<string, GameObject>();
        commandQueue = new ConcurrentQueue<ICommand>();
        ConnectToXmppServer();
        // connect = Task.Run(async () => await ConnectToXmppServerAsync());
        // ConnectToXmppServerAsync();
        /*
            Debug.LogError("Ha habido un problema con la autenticaci�n.");
            Debug.LogError(ex.Message);
            Debug.LogError(ex.StackTrace);
        */
    }

    private void Start() {
        LinkSpawners();
    }

    private void Update() {
        DequeueAndProcessCommand();
        // var task = Task.Run(async () => await DequeueAndProcessCommand());
        // task.Wait();
        if (Input.GetKeyDown(KeyCode.Space)) {
            Debug.Log(xmppClient.Connected);
        }

    }

    private void OnDestroy() {
        if (xmppClient != null && xmppClient.Connected) {
            xmppClient.Close();
        }
    }

    private void LoadPlayerPrefs() {
        if (PlayerPrefs.HasKey("xmppNode"))
            xmppNode = PlayerPrefs.GetString("xmppNode");
        if (PlayerPrefs.HasKey("xmppDomain"))
            xmppDomain = PlayerPrefs.GetString("xmppDomain");
        if (PlayerPrefs.HasKey("xmppPass"))
            xmppPass = PlayerPrefs.GetString("xmppPass");
        if (PlayerPrefs.HasKey("xmppPort"))
            xmppPort = PlayerPrefs.GetInt("xmppPort");
        if (PlayerPrefs.HasKey("xmppTls"))
            xmppTls = PlayerPrefs.GetInt("xmppTls") != 0;
    }

    private void ConnectToXmppServer() {
        ConnectXmppClient();
        if (xmppClient.Connected) {
            try {
                xmppClient.Authenticate(xmppNode, xmppPass);
            } catch (AuthenticationException) {
                Debug.Log($"Username ({xmppNode}) and password not matched by any user.");
                Debug.Log("Attempting In-Band registration...");
                AttemptRegistrationInBand();
                Debug.Log("Success!");
            }
        }
    }

    private void AttemptRegistrationInBand() {
        if (!xmppClient.Connected)
            ConnectXmppClient();
        try {
            xmppClient.Register(RegisterInBandCallback);
            xmppClient.Authenticate(xmppNode, xmppPass);
        } catch (XmppErrorException ex) {
            Debug.LogError(ex.Error.Text);
            throw ex;
        }
    }

    private void ConnectXmppClient() {
        xmppClient = new XmppClient(xmppDomain, xmppPort, xmppTls);
        xmppClient.Message += OnNewXmppMessage;
        xmppClient.Connect();
    }

    private void OnNewXmppMessage(object sender, MessageEventArgs e) {
        // var agent = e.Message.From;
        // var fiveserver = e.Message.To;
        EnqueueCommandFromMessage(e.Message.From, e.Message.Body);
        // XmppCommunicator.SendXmppCommand(xmppClient, to: agent, from: fiveserver, "position 0 10 0");
    }

    private SubmitForm RegisterInBandCallback(RequestForm form) {
        if (!String.IsNullOrEmpty(form.Instructions))
            Debug.Log(form.Instructions);

        SubmitForm submitForm = new SubmitForm();
        foreach (var field in form.Fields) {
            // Debug.Log($"{field.Name} | {field.Description} | {field.Type.Value} | {field.Required} | {field.Values}");
            DataField f = null;

            if (field is TextField && field.Required)
                f = new TextField(field.Name, xmppNode);
            else if (field is PasswordField && field.Required)
                f = new PasswordField(field.Name, xmppPass);
            else if (field is JidField && field.Required)
                f = new JidField(field.Name, new Jid($"{xmppNode}@{xmppDomain}"));

            if (f != null)
                submitForm.Fields.Add(f);
        }
        return submitForm;
    }

    private void LinkSpawners() {
        var mapLoaderScript = mapLoader.GetComponent<MapLoader>();
        spawners = mapLoaderScript.GetSpawners();
    }

    private void EnqueueCommandFromMessage(Jid sender, string message) {
        ICommand command = CommandParser.ParseCommand(message);
        if (command != null) {
            if (command is CreateArtifactCommand createArtifactCommand) {
                createArtifactCommand.Jid = sender;
                command = createArtifactCommand;
            } else if (command is CreateCommand createEntity) {
                createEntity.Jid = sender;
                command = createEntity;
            } else {
                command.Name = sender.ToString();
            }
            commandQueue.Enqueue(command);
        } else {
            Debug.LogWarning($"{sender.Node} sent a missformat command: {message}"); ;
        }
    }

    private void DequeueAndProcessCommand() {
        if (!commandQueue.IsEmpty)
            if (commandQueue.TryDequeue(out ICommand command)) {
                command.Execute(entities);
                if (command is CreateCommand create) {
                    if (!entities.ContainsKey(create.Jid.ToString()))
                        CreateEntity(create);
                    SendPositionOfAvatarAgent(entities[create.Jid.ToString()]);
                } else if (command is CreateArtifactCommand createArtifact) {
                    if (!entities.ContainsKey(createArtifact.Jid.ToString()))
                        CreateArtifactEntity(createArtifact);
                }
            }
    }
    
	private void CreateEntity(CreateCommand command) {
        GameObject agentPrefab = GetAgentPrefab(command.AgentPrefab);
        if (agentPrefab != null) {
            GameObject entity = InstantiateEntity(agentPrefab, command);
            var entityScript = entity.GetComponent<Entity>();
            entityScript.SetDisplayName(command.Name);
            entityScript.Jid = command.Jid;
            entity.name = command.Jid.ToString();
            entities.Add(entity.name, entity);
            // var tcpClient = tcpCommandClients.Find(x => x.AgentName == command.AgentName);
            var entityComponent = entity.GetComponent<Entity>();
            entityComponent.XmppClient = xmppClient;
            // entityComponent.TcpCommandManager = tcpClient;
            entityComponent.AgentCollision = command.AgentCollision;
        } else {
            Debug.LogError($"Agent {command.Jid} asks for non existing prefab: {command.AgentPrefab}");
        }
    }

    private void CreateArtifactEntity(CreateArtifactCommand command) {
        GameObject artifactPrefab = GetArtifactPrefab(command.ArtifactPrefab);
        if (artifactPrefab != null) {
            GameObject entity = InstantiateEntity(artifactPrefab, command);
            var entityScript = entity.GetComponent<Artifact>();
            entityScript.Jid = command.Jid;
            entityScript.SetDisplayName(command.Name);
            entity.name = command.Jid.ToString();
            entities.Add(entity.name, entity);
        } else {
            Debug.LogError($"Artifact {command.Jid} asks for non existing prefab: {command.ArtifactPrefab}");
        }
    }


    private GameObject GetAgentPrefab(string agentPrefabName) {
        foreach(var agentPrefab in agentsPrefabs)
            if (agentPrefab.name.Equals(agentPrefabName, StringComparison.InvariantCultureIgnoreCase))
                return agentPrefab;
        return null;
    }

    private GameObject GetArtifactPrefab(string artifactPrefabName) {
        foreach(var artifactPrefab in artifactsPrefabs)
            if (artifactPrefab.name.Equals(artifactPrefabName, StringComparison.InvariantCultureIgnoreCase))
                return artifactPrefab;
        return null;
    }

    private GameObject InstantiateEntity(GameObject agentPrefab, ICommand command) {
        bool instantiatedByCoordinates = false;
        string spawnerName = null;
        Vector3 starterPosition = new Vector3();
        if (command is CreateArtifactCommand artifactCommand) {
            instantiatedByCoordinates = artifactCommand.IsInstantiatedByCoordinates();
            spawnerName = artifactCommand.SpawnerName;
            starterPosition = artifactCommand.StarterPosition;
        } else if (command is CreateCommand createCommand) {
            instantiatedByCoordinates = createCommand.IsInstantiatedByCoordinates();
            spawnerName = createCommand.SpawnerName;
            starterPosition = createCommand.StarterPosition;
        }

        if (instantiatedByCoordinates)
            return Instantiate(agentPrefab, starterPosition, Quaternion.identity);
        else {
            var spawner = spawners[spawnerName];
            Vector3 spawnerPosition = spawner.transform.position;
            return Instantiate(agentPrefab, spawnerPosition, Quaternion.identity);
        }
    }

    private void SendPositionOfAvatarAgent(GameObject agent) {
        var entityComponent = agent.GetComponent<Entity>();
        entityComponent.SendCurrentPosition();
    }

    public Dictionary<string, GameObject> Entities {
        get { return entities; }
    }
}
