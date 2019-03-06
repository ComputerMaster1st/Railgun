using System;
using System.Collections.Generic;

namespace TreeDiagram.Models.Fun
{
    public class FunBite : FunBase
    {
        public List<string> Bites { get; private set; } = new List<string>();
        
        public FunBite(ulong id) : base(id) { }
        
        public void AddBite(string message) {
            if (!Bites.Contains(message)) Bites.Add(message);
        }

        public string GetBite() {
            if (Bites.Count <= 0) return null;
            
            var rand = new Random();
            return Bites[rand.Next(Bites.Count)];
        }
    }
}