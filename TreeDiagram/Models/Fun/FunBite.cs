using System;
using System.Collections.Generic;

namespace TreeDiagram.Models.Fun
{
    public class FunBite : FunBase
    {
        private List<string> _bites;

        public List<string> Bites { 
            get {
                if (_bites == null) _bites = new List<string>();
                return _bites;
            } private set {
                _bites = value;
            }}
        
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