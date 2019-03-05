﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TreeDiagram.Models.Server.Inactivity
{
    public class UserActivityContainer
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; private set; }

        public ulong UserId { get; }
        public DateTime LastActive { get; set; }

        public UserActivityContainer(ulong userId) => UserId = userId;
    }
}