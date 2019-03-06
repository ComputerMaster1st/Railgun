using System;
using System.Collections.Generic;

namespace TreeDiagram.Models.Fun
{
    public class FunRst : FunBase
    {
        public List<string> Rst { get; private set; } = new List<string>();
        
        public FunRst(ulong id) : base(id) { }
        
        public void AddRst(string message) {
            if (!Rst.Contains(message)) Rst.Add(message);
        }

        public string GetRst() {
            if (Rst.Count <= 0) return null;
            
            var rand = new Random();
            return Rst[rand.Next(Rst.Count)];
        }
    }
}