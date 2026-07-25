using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct ConversationEntry
{
    public bool IsPlayer;
    public string CharacterName;
    public string[] Entries;

    // Constructor with multiple entries is only for Player
    public ConversationEntry(string[] entries, string name)
    {
        Entries = entries;
        IsPlayer = true;
        CharacterName = name;
    }

    // Constructor with one entry is meant for Npcs
    public ConversationEntry(string entry, string name, bool isPlayer = false)
    {
        Entries = new string[1] { entry };
        CharacterName = name;
        IsPlayer = isPlayer;
    }

    public bool IsValid()
    {
        return Entries.Length > 0;
    }

    public bool IsPlayerChoice()
    {
        return IsPlayer && Entries.Length > 1;
    }
}

public class ConversationData
{
    public List<ConversationEntry> ConversationEntries = new();
    public string[] Characters { get; protected set; } // All the characters present in this conversation

    public void AddEntry(ConversationEntry entry)
    {
        ConversationEntries.Add(entry);
    }

    public void SetCharacters(string[] characters)
    {
        Characters = characters;
    }

    public bool IsEmpty()
    {
        return ConversationEntries.Count == 0;
    }
}

public static class DialogueHelpers
{
    /**
     * Loads csv data for conversations in the following format:
     * 
     * NpcName1, NpcName2, ... , PlayerName,,
     * NpcLine1,, ... ,,,
     * ,, ... , PlayerChoice1, PlayerChoice2, PlayerChoice3
     * , NpcLine2, ... ,,,
     * ,, ... , PlayerResponse,,
     * ,, ... , PlayerChoice4, PlayerChoice5,
     * ...
     *
     * The first row contains all npc names and then the player's name
     * Following that there can either be 1 npc line per row or 1+ player lines per row (more than 1 means player picks an option)
     */
    public static ConversationData LoadConversationFromCsvString(string csvString)
    {
        ConversationData conversation = new();
        csvString = csvString.Replace("\r", "");
        string[] csvRows = csvString.Split('\n');

        if (csvRows.Length < 2)
        {
            Debug.LogError("DialogueHelpers::LoadConversationFromCsvString - Conversations need at least 2 rows, character row and dialogue");
            return new ConversationData();
        }

        string[] characters = LoadCharactersCsvRow(csvRows[0]);
        if (characters.Length < 1)
        {
            Debug.LogError("DialogueHelpers::LoadConversationFromCsvString - Conversations need at least 1 character");
            return new ConversationData();
        }
        
        conversation.SetCharacters(characters);

        for (int i = 1; i < csvRows.Length; ++i)
        {
            string row = csvRows[i];
            ConversationEntry entry = LoadEntryFromCsvRow(row, characters);
            if (entry.IsValid())
            {
                conversation.AddEntry(entry);
            }
        }

        return conversation;
    }

    /**
     * Load the first row of csv entries as the characters present in the conversation.
     * The last entry is assumed to be the player
     */
    private static string[] LoadCharactersCsvRow(string csvRow)
    {
        List<string> csvFields = SplitCsvRow(csvRow);
        List<string> characters = new();

        bool charactersEnded = false;
        foreach (string field in csvFields)
        {
            if (field.Length == 0)
            {
                charactersEnded = true;
                continue;
            }
            
            // if not empty but previous entry was empty then error
            if (charactersEnded)
            {
                Debug.LogError("DialogueHelpers:LoadCharactersCsvRow - Cannot have middle character name entry be empty");
                return Array.Empty<string>();
            }
            
            characters.Add(field);
        }

        return characters.ToArray();
    }

    /** Load conversation entry from row given characters present */
    private static ConversationEntry LoadEntryFromCsvRow(string csvRow, string[] characters)
    {
        List<string> csvFields = SplitCsvRow(csvRow);

        if (csvFields.Count < characters.Length)
        {
            Debug.LogError("DialogueHelpers::LoadEntryFromCsvRow - conversation row has fewer entries characters: " + csvRow);
            return new ConversationEntry("", "error");
        }

        // To see if it's player dialogue go through and test if there's any fields filled before the final character (player) column
        bool isPlayer = true;
        int characterIndex = characters.Length - 1;
        for (int i = 0; i < characters.Length - 1; ++i)
        {
            if (csvFields[i].Length > 0)
            {
                // If we already found dialogue under another character then this is saying there's text from multiple characters
                if (!isPlayer)
                {
                    Debug.LogError("DialogueHelpers::LoadEntryFromCsvRow - conversation row has multiple non-player entries: " + csvRow);
                    return new ConversationEntry("", "error");
                }
                isPlayer = false;
                characterIndex = i;
            }
        }

        // if not player, take the one line of dialogue we care about and use it
        if (!isPlayer)
        {
            string dialogueLine = csvFields[characterIndex];
            csvFields.RemoveAt(characterIndex);
            if (csvFields.Any(field => field.Length > 0))
            {
                Debug.LogError("DialogueHelpers::LoadEntryFromCsvRow - Invalid row has both npc and player columns filled: " + csvRow);
                return new ConversationEntry("", "error");
            }

            return new ConversationEntry(dialogueLine, characters[characterIndex]);
        }

        // if it is the player, remove all empty columns and just collect the dialogue lines we care about
        csvFields.RemoveAll(field => field.Length == 0);
        if (csvFields.Count == 0)
        {
            Debug.LogWarning("DialogueHelpers::LoadEntryFromCsvRow - Conversation contained fully empty row " + csvRow);
            return new ConversationEntry("", "error");
        }

        return new ConversationEntry(csvFields.ToArray(), characters.Last());
    }

    private static List<string> SplitCsvRow(string csvRow)
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

        return csvFields;
    }
}
