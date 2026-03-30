using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace SnakeEvolution.Core.AI
{
    public class QTablePolicy
    {
        private readonly Dictionary<string, Dictionary<string, double>> _table = new();

        public void Load(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Q-table JSON non trovato: {filePath}");
            }

            string json = File.ReadAllText(filePath);
            List<QTableEntry>? entries = JsonSerializer.Deserialize<List<QTableEntry>>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (entries == null)
            {
                throw new InvalidOperationException("Impossibile deserializzare q_table.json");
            }

            _table.Clear();

            foreach (QTableEntry entry in entries)
            {
                _table[entry.State] = entry.Actions;
            }
        }

        public bool ContainsState(string stateKey)
        {
            return _table.ContainsKey(stateKey);
        }

        public string? GetBestAction(string stateKey)
        {
            if (!_table.TryGetValue(stateKey, out Dictionary<string, double>? actions) || actions.Count == 0)
            {
                return null;
            }

            return actions
                .OrderByDescending(x => x.Value)
                .First()
                .Key;
        }
    }
}
