using System;
using System.Collections.Generic;
using TreeDiagram.Enums;

namespace TreeDiagram.Models.Server
{
    public class ServerJoinLeave : ConfigBase
    {
        public bool SendToDM { get; set; } = false;
        public ulong ChannelId { get; set; } = 0;
        public int DeleteAfterMinutes { get; set; } = 0;

        public List<string> JoinMessages { get; private set; } = new List<string>() { "Welcome to **<server>**, **<user>**!" };
        public List<string> LeaveMessages { get; private set; } = new List<string>() { "Goodbye, **<user>**." };

        public ServerJoinLeave(ulong id) : base(id) { }

        public void AddMessage(string message, MsgType type) {
            switch (type) {
                case MsgType.Join:
                    JoinMessages.Add(message);
                    break;
                case MsgType.Leave:
                    LeaveMessages.Add(message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public void RemoveMessage(int index, MsgType type) {
            switch (type) {
                case MsgType.Join:
                    JoinMessages.RemoveAt(index);
                    break;
                case MsgType.Leave:
                    LeaveMessages.RemoveAt(index);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public string GetMessage(MsgType type) {
            var rand = new Random();
            string message = null;

            while (string.IsNullOrEmpty(message))
            {
                try
                {
                    switch (type)
                    {
                        case MsgType.Join:
                            message = JoinMessages[(rand.Next(0, JoinMessages.Count))];
                            break;
                        case MsgType.Leave:
                            if (LeaveMessages.Count < 1) return null;

                            message = LeaveMessages[(rand.Next(0, LeaveMessages.Count))];
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(type), type, null);
                    }
                }
                catch { }
            }

            return message;
        }
    }
}