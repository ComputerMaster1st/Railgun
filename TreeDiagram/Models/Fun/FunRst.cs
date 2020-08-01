using System;
using System.Collections.Generic;

namespace TreeDiagram.Models.Fun
{
    public class FunRst : FunBase
    {
        private List<string> _rst;

        public List<string> Rst { 
            get {
                if (_rst == null) _rst = new List<string>();
                return _rst;
            } private set {
                _rst = value;
            }}
        
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