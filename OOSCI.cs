// Adapted from Github user danielbierwirth TCPTestClient Gist: https://gist.github.com/danielbierwirth/0636650b005834204cb19ef5ae6ccedb#file-tcptestclient-cs

using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class OOSCI : MonoBehaviour
{
	OOSCIMessage incomingMessage;
	OOSCIMessage outgoingMessage;

	public float cubeRotation;

	#region private members
	private int MSG_SIZE = 2048;
	private TcpClient socketConnection;
	private Thread clientReceiveThread;
	private bool isConnectedToServer = false;
	private bool isSubscribedToChannel = false;
	#endregion
	#region public members 
	public string serverName = "oocsi.id.tue.nl";
	public int port = 4444;
	public string UniqueID;
	public string subscribeChannel;
	#endregion

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
			sendTestMessage();
		}
		if (Input.GetKeyDown(KeyCode.Space))
		{
			SendUniqueID();
		}
	}
	/// <summary> 	
	/// Setup socket connection. 	
	/// </summary> 	
	private void ConnectToServer()
	{
		try
		{
			clientReceiveThread = new Thread(new ThreadStart(ListenForData));
			clientReceiveThread.IsBackground = true;
			clientReceiveThread.Start();
			//SendUniqueID();
		}
		catch (Exception e)
		{
			Debug.Log("On client connect exception " + e);
		}
	}
	/// <summary> 	
	/// Subscribe to channels	
	/// </summary> 	
	private void SubscribeToChannels()
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
				string clientMessage = "subscribe " + subscribeChannel + "\n";
				// Convert string message to byte array.                 
				byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(clientMessage);
				// Write byte array to socketConnection stream.                 
				stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);
				Debug.Log("Subscribing to " + clientMessage);
			}
			isSubscribedToChannel = true;
		}


		catch (SocketException socketException)
		{
			Debug.Log("Socket exception: " + socketException);
		}
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
					// Read incomming stream into byte arrary. 					
					while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
					{
						var incommingData = new byte[length];
						Array.Copy(bytes, 0, incommingData, 0, length);
						// Convert byte array to string message. 						
						string serverMessage = Encoding.ASCII.GetString(incommingData);
						Debug.Log("server message received as: " + serverMessage);
						if(serverMessage.IndexOf("ping") >= 0 || serverMessage == " ")
						{
							SendPingAck();	
						} else
                        {
							processOOSCIMessage(serverMessage);
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
				string clientMessage = UniqueID + "(JSON)" +"\n";
				// Convert string message to byte array.                 
				byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(clientMessage);
				// Write byte array to socketConnection stream.                 
				stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);
				Debug.Log("Client sent his message - should be received by server");
			}
		}


		catch (SocketException socketException)
		{
			Debug.Log("Socket exception: " + socketException);
		}
	}
	/// <summary> 	
	/// Sends Ping Acknowledgement.	
	/// </summary>
	private void SendPingAck()
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
				string clientMessage = "." + "\n";
				// Convert string message to byte array.                 
				byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(clientMessage);
				// Write byte array to socketConnection stream.                 
				stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);
				Debug.Log("Sending client-ping");
			}
		}


		catch (SocketException socketException)
		{
			Debug.Log("Socket exception: " + socketException);
		}
	}
	/// <summary> 	
	/// Process JSON message from OOSCI server.
	/// </summary>
	private void processOOSCIMessage(string message)
    {
		if(isConnectedToServer == true)
        {
			incomingMessage = JsonUtility.FromJson<OOSCIMessage>(message);
			//Change 'incomingMessage.rotation' to match your incoming message
			Debug.Log(incomingMessage.rotation);

		} else if (message.IndexOf("{'message' : \"welcome " + UniqueID+"\"}") >= 0 && isConnectedToServer == false)
        {
			isConnectedToServer = true;
			Debug.Log("connected to OOSCI server.");
			SubscribeToChannels();
		}

	}
	/// <summary> 	
	/// send Message.
	/// </summary>
    private void sendTestMessage()
    {
		if (isSubscribedToChannel == true)
		{
			outgoingMessage.rotation = cubeRotation++;
			string outMessage = JsonUtility.ToJson(outgoingMessage);
			outMessage = outMessage + "\n";

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
					byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(outMessage);
					// Write byte array to socketConnection stream.                 
					stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);
					Debug.Log("Sending client-ping");
				}
			}


			catch (SocketException socketException)
			{
				Debug.Log("Socket exception: " + socketException);
			}

		} else
        {
			Debug.Log("Not currently subscribed to channel.");
		}
	}

}