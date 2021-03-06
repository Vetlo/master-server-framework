﻿using Barebones.Networking;
using System.Collections.Generic;

namespace Barebones.MasterServer
{
    public delegate void RoomAccessCallback(RoomAccessPacket access, string error);

    public delegate void RoomAccessReceivedHandler(RoomAccessPacket access);

    public class MsfRoomsClient : MsfBaseClient
    {
        /// <summary>
        /// Event, invoked when an access is received
        /// </summary>
        public event RoomAccessReceivedHandler OnAccessReceivedEvent;

        /// <summary>
        /// If set to true, game server will never be started
        /// </summary>
        public bool ForceClientMode { get; set; } = false;

        public MsfRoomsClient(IClientSocket connection) : base(connection) { }

        /// <summary>
        /// An access, which was last received
        /// </summary>
        public RoomAccessPacket LastReceivedAccess { get; private set; }

        /// <summary>
        /// Tries to get an access to a room with a given room id
        /// </summary>
        /// <param name="roomId"></param>
        /// <param name="callback"></param>
        public void GetAccess(int roomId, RoomAccessCallback callback)
        {
            GetAccess(roomId, callback, "", new Dictionary<string, string>(), Connection);
        }

        /// <summary>
        /// Tries to get an access to a room with a given room id and password
        /// </summary>
        public void GetAccess(int roomId, string password, RoomAccessCallback callback)
        {
            GetAccess(roomId, callback, password, new Dictionary<string, string>(), Connection);
        }

        /// <summary>
        /// Tries to get an access to a room with a given room id and password
        /// </summary>
        public void GetAccess(int roomId, RoomAccessCallback callback, string password)
        {
            GetAccess(roomId, callback, password, new Dictionary<string, string>(), Connection);
        }

        /// <summary>
        /// Tries to get an access to a room with a given room id, password,
        /// and some other properties, which will be visible to the room (game server)
        /// </summary>
        public void GetAccess(int roomId, RoomAccessCallback callback, Dictionary<string, string> properties)
        {
            GetAccess(roomId, callback, "", properties, Connection);
        }

        /// <summary>
        /// Tries to get an access to a room with a given room id, password,
        /// and some other properties, which will be visible to the room (game server)
        /// </summary>
        public void GetAccess(int roomId, RoomAccessCallback callback, string password, Dictionary<string, string> properties)
        {
            GetAccess(roomId, callback, password, properties, Connection);
        }

        /// <summary>
        /// Tries to get an access to a room with a given room id, password,
        /// and some other properties, which will be visible to the room (game server)
        /// </summary>
        public void GetAccess(int roomId, RoomAccessCallback callback, string password, Dictionary<string, string> properties, IClientSocket connection)
        {
            if (!connection.IsConnected)
            {
                callback.Invoke(null, "Not connected");
                return;
            }

            var packet = new RoomAccessRequestPacket()
            {
                RoomId = roomId,
                Properties = properties,
                Password = password
            };

            connection.SendMessage((short)MsfMessageCodes.GetRoomAccessRequest, packet, (status, response) =>
            {
                if (status != ResponseStatus.Success)
                {
                    callback.Invoke(null, response.AsString("Unknown Error"));
                    return;
                }

                var access = response.Deserialize(new RoomAccessPacket());
                LastReceivedAccess = access;
                callback.Invoke(access, null);
                OnAccessReceivedEvent?.Invoke(access);
            });
        }

        /// <summary>
        /// This method triggers the <see cref="OnAccessReceivedEvent"/> event. Call this, 
        /// if you made some custom functionality to get access to rooms
        /// </summary>
        /// <param name="access"></param>
        public void TriggerAccessReceivedEvent(RoomAccessPacket access)
        {
            LastReceivedAccess = access;
            OnAccessReceivedEvent?.Invoke(access);
        }
    }
}