// OOSCI Unity Sender and Reciever code by Andy Sheen 2023.
// Adapted from Github user danielbierwirth TCPTestClient Gist: https://gist.github.com/danielbierwirth/0636650b005834204cb19ef5ae6ccedb#file-tcptestclient-cs
// and https://github.com/iddi/oocsi-esp

using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using System.IO;

public class OOSCI : MonoBehaviour
{
	TcpClient client;
	#region private members
	private int MSG_SIZE = 2048;
	private TcpClient socketConnection;
	private Thread clientReceiveThread;
	private bool isConnectedToServer = false;
	private string clientList;
	private bool clientListReady = false;
	private string channelList;
	private bool channelListReady = false;
	#endregion
	#region public members
	OOSCIMessage incomingMessage;
	public OOSCIMessage outgoingMessage;
	public string serverName = "oocsi.id.tue.nl";
	public int port = 4444;
	public string OOCSIName;
	public string subscribeChannel1;
	public string subscribeChannel2;
	public string subscribeChannel3;
	public bool logging;
	#endregion

	//test variables
	int rotationX;
	int rotationY;
	int rotationZ;


	//Used as a way to know what kind of response is expected, if waiting on one from server
	enum MessageResponseType {CONNECTING,CLIENTLIST,CHANNELLIST,IDLE}
	MessageResponseType messageResponse = MessageResponseType.IDLE;

	// Use this for initialization
	void Start()
	{
		
		ConnectToServer();
	}

	// Update is called once per frame
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.UpArrow))
		{
			sendTestMessage(subscribeChannel1, rotationX++%360);
		}
		if (Input.GetKeyDown(KeyCode.DownArrow))
		{
			sendTestMessage(subscribeChannel2, rotationY++% 360);
		}
		if (Input.GetKeyDown(KeyCode.LeftArrow))
		{
			sendTestMessage(subscribeChannel3, rotationZ++% 360);
		}
	}

	// Stops thread on close
	void OnApplicationQuit()
	{
		if (clientReceiveThread.IsAlive)
		{
			clientReceiveThread.Abort();
		}

	}

	/// <summary> 	
	/// Setup socket connection. 	
	/// </summary> 	
	public void ConnectToServer()
	{
		try
		{
			messageResponse = MessageResponseType.CONNECTING;
			clientReceiveThread = new Thread(new ThreadStart(ListenForData));
			clientReceiveThread.IsBackground = true;
			clientReceiveThread.Start();
		}
		catch (Exception e)
		{
			Debug.Log("On client connect exception " + e);
		}
		Invoke("SendUniqueID", 1);
	}

	/// <summary> 	
	/// Utility function to post to OOSCI server	
	/// </summary> 	
	private void sendMessageToOOSCI(string message)
    {
		if (socketConnection == null)
		{
			return;
		}
		try
		{
			// Get a stream object for writing. 			
			NetworkStream stream = socketConnection.GetStream();
			if (stream.CanWrite)
			{
				// Convert string message to byte array.                 
				byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(message);
				// Write byte array to socketConnection stream.                 
				stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);
			}
		}


		catch (SocketException socketException)
		{
			Debug.Log("Socket exception: " + socketException);
		}
	}

	/// <summary> 	
	/// Subscribe to channels	
	/// </summary> 	
	private void SubscribeToChannel(string _subscribeChannel)
	{
		sendMessageToOOSCI("subscribe " + _subscribeChannel + "\n");
		messageResponse = MessageResponseType.IDLE;
	}
	/// <summary> 	
	/// Runs in background clientReceiveThread; Listens for incomming data. 	
	/// </summary>     
	private void ListenForData()
	{
		try
		{
			socketConnection = new TcpClient(serverName, port);
			Byte[] bytes = new Byte[MSG_SIZE];
			while (true)
			{
				
				// Get a stream object for reading 				
				using (NetworkStream stream = socketConnection.GetStream())
				{
					int length;
					// Read incoming stream into byte arrary.
					//
					while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
					{
						var incomingData = new byte[length];
						Array.Copy(bytes, 0, incomingData, 0, length);

						// Convert byte array to string message. 						
						string serverMessage = Encoding.ASCII.GetString(incomingData);
						string[] messages = serverMessage.Split('\n');
						Debug.Log(messages.Length);
						foreach (string line in messages)
						{
							if(line != "")
							{ 
								//string serverMessage = line;
								print("server message received as: " + line);

								if (line.IndexOf("ping") >= 0 || line == " ")
								{
									SendPingAck();
								}
								else
								{
									if (messageResponse == MessageResponseType.CONNECTING)
									{
										if (line.IndexOf("{'message' : \"welcome " + OOCSIName + "\"}") >= 0 && isConnectedToServer == false)
										{
											isConnectedToServer = true;
											print("connected to OOSCI server.");
											SubscribeToChannel(subscribeChannel1);
											SubscribeToChannel(subscribeChannel2);
											SubscribeToChannel(subscribeChannel3);
										}
									}
									else if (messageResponse == MessageResponseType.CLIENTLIST)
									{
										clientList = line;
										clientListReady = true;
										messageResponse = MessageResponseType.IDLE;
									}
									else if (messageResponse == MessageResponseType.CHANNELLIST)
									{
										channelList = line;
										channelListReady = true;
										messageResponse = MessageResponseType.IDLE;
									}
									else
									{
										if (line != null || line.IndexOf(" ") > 0)
										{
											Debug.Log("process!");
											processOOSCIMessage(line);
										}
									}
								}
							}
						}
					}
				}
			}
		}
		catch (SocketException socketException)
		{
			Debug.Log("Socket exception: " + socketException);
		}
	}
	/// <summary> 	
	/// Connects to OOSCI server with Unique client name.	
	/// </summary> 	
	private void SendUniqueID()
	{
		sendMessageToOOSCI(OOCSIName + "(JSON)" + "\n");
		
	}
	/// <summary> 	
	/// Sends Ping Acknowledgement.	
	/// </summary>
	private void SendPingAck()
	{
		sendMessageToOOSCI("."+ "\n");
	}
	/// <summary> 	
	/// Process JSON message from OOSCI server.
	/// </summary>
	private void processOOSCIMessage(string message)
    {
		if(isConnectedToServer == true)
        {
			incomingMessage = JsonUtility.FromJson<OOSCIMessage>(message);
			//Change 'incomingMessage.rotation' to match your incoming message key
			if (incomingMessage.recipient == subscribeChannel1)
			{
				print("Channel 1: " + incomingMessage.rotation);
			}
			else if (incomingMessage.recipient == subscribeChannel2)
			{
				print("Channel 2: " + incomingMessage.rotation);
			}
			else if (incomingMessage.recipient == subscribeChannel3)
			{
				print("Channel 3: " + incomingMessage.rotation);
			}
		}

	}
	/// <summary> 	
	/// send test Message.
	/// </summary>
    private void sendTestMessage(String _subscribeChannel, int value)
    {
		//Change 'outgoingMessage.rotation' to match your JSON key message.
		outgoingMessage.rotation = value;
		outgoingMessage.recipient = _subscribeChannel;
		outgoingMessage.text = "content";
		outgoingMessage.sender = getName();
		outgoingMessage.timestamp = -1;
		string outMessage= "{}";
		outMessage = JsonUtility.ToJson(outgoingMessage);
		print("sendraw " + _subscribeChannel + " " + outMessage + "\n");
		sendMessageToOOSCI("sendraw " + _subscribeChannel + " "+ outMessage + "\n");
		
	}
	/// <summary> 	
	// return client list
	/// </summary> 	
	private void getClients()
	{
		sendMessageToOOSCI("clients" + "\n");
		messageResponse = MessageResponseType.CLIENTLIST;

	}
	/// <summary> 	
	// fetches client list
	/// </summary> 	
	private void getChannels()
	{
		sendMessageToOOSCI("channels" + "\n");
		messageResponse = MessageResponseType.CHANNELLIST;
	}
	/// <summary>
	// Returns client list when available
	/// </summary> 	
	public string getChannelList()
    {
		getChannels();
		while(channelListReady == false)
        {

        }
		channelListReady = false;
		return channelList;
	}
	/// <summary> 	
	// check whether client is included in client list
	/// </summary> 	
	public bool containsClient(String clientName) {
	//check for the client.
	getClients();
	while(clientListReady == false)
		{

        }
	if (clientList.IndexOf(clientName) == -1) {
		//not found
		clientListReady = false;
		return false;
	}
		clientListReady = false;
		return true;
	}

	/// <summary> 	
	// return the client name
	/// </summary> 	
	private String getName()
	{
		return OOCSIName;
	}

	/// <summary> 	
	// print message if logging is activated
	/// </summary> 	
	private void print(String message) {
		if (logging == true)
			Debug.Log(message);
	}

	/// <summary> 	
	// print message if logging is activated
	/// </summary> 	
	private void print(char message)
	{
		if (logging)
			Debug.Log(message);
	}

	/// <summary> 	
	// print message if logging is activated
	/// </summary> 	
	private void println()
	{
		if (logging)
			Debug.Log("\n");
	}

	/// <summary> 	
	// print message if logging is activated
	/// </summary> 	
	private void println(String message)
	{
		if (logging)
			Debug.Log(message);
	}

	/// <summary> 	
	// print message if logging is activated
	/// </summary> 	
	private void println(char message)
	{
		if (logging)
			Debug.Log(message);
	}

	/// <summary>
	// Unsubscribes from channel
	/// </summary> 	
	private void unsubscribe(String chan) {
		sendMessageToOOSCI("unsubscribe " + chan + "\n");
	}

	/// <summary>
	// Disconnects from server
	/// </summary> 	
	private void disconnect()
	{
		sendMessageToOOSCI("quit");
	}

	private string getRecipient()
    {
		return incomingMessage.recipient.ToString();
    }

}
