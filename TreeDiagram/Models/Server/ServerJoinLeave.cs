using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TreeDiagram.Enums;

namespace TreeDiagram.Models.Server
{
    public class ServerJoinLeave
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; private set; }

        public bool SendToDM { get; set; }
        public ulong ChannelId { get; set; }
        public int DeleteAfterMinutes { get; set; }

        public List<string> JoinMessages { get; private set; } = new List<string>() { "Welcome to **<server>**, **<user>**!" };
        public List<string> LeaveMessages { get; private set; } = new List<string>() { "Goodbye, **<user>**." };

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
                            if (JoinMessages.Count < 1) return null;
                            
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
                catch
                {
                    // ignored
                }
            }

            return message;
        }
    }
}