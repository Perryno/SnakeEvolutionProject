using System.Collections.Generic;

namespace SnakeEvolution.Core.AI
{
    public class QTableEntry
    {
        public string State { get; set; } = string.Empty;
        public Dictionary<string, double> Actions { get; set; } = new();
    }
}
