using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

public struct ConversationEntry
{
    public bool IsPlayer;
    public string[] Entries;

    // Should only have multiple entries for player dialogue options
    public ConversationEntry(string[] entries, bool isPlayer = true)
    {
        Entries = entries;
        IsPlayer = isPlayer;
    }

    public ConversationEntry(string entry, bool isPlayer)
    {
        Entries = new string[1] { entry };
        IsPlayer = isPlayer;
    }

    public bool IsValid()
    {
        return Entries.Length > 0;
    }
}

public class ConversationData
{
    public List<ConversationEntry> ConversationEntries = new();

    public void AddEntry(ConversationEntry entry)
    {
        ConversationEntries.Add(entry);
    }
}

public static class DialogueHelpers
{
    public static ConversationData LoadConversationFromCsvString(string csvString)
    {
        ConversationData conversation = new();
        string[] csvRows = csvString.Split('\n');

        foreach (string row in csvRows)
        {
            ConversationEntry entry = LoadEntryFromCsvRow(row);
            if (entry.IsValid())
            {
                conversation.AddEntry(entry);
            }
        }

        return conversation;
    }

    private static ConversationEntry LoadEntryFromCsvRow(string csvRow)
    {
        List<string> csvFields = new();
        bool insideQuote = false;
        string currentField = "";
        
        // Manually parse row because not all commas mean new value if wrapped inside quotes
        foreach (char character in csvRow)
        {
            if (insideQuote)
            {
                if (character == '"')
                {
                    insideQuote = false;
                }
                else
                {
                    currentField += character;
                }

                continue;
            }

            switch (character)
            {
                case '"':
                    insideQuote = true;
                    continue;
                case ',':
                    csvFields.Add(currentField);
                    currentField = "";
                    continue;
                default:
                    currentField += character;
                    break;
            }
        }

        csvFields.Add(currentField);

        if (csvFields[0].Length > 0)
        {
            // First entry populated means it's npc line
            string npcLine = csvFields[0];
            csvFields.RemoveAt(0);
            if (csvFields.Any())
            {
                Debug.LogError("DialogueHelpers::LoadEntryFromCsvRow - Invalid row has entries in first column and following column(s): " + csvRow);
                return new ConversationEntry("", false);
            }

            return new ConversationEntry(npcLine, false);
        }

        csvFields.RemoveAll(field => field.Length == 0);
        if (csvFields.Count == 0)
        {
            Debug.LogWarning("DialogueHelpers::LoadEntryFromCsvRow - Conversation contained fully empty row " + csvRow);
            return new ConversationEntry("", false);
        }

        return new ConversationEntry(csvFields.ToArray());
    }
}
