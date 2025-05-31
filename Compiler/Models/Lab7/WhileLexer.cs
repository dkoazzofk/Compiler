using System;
using System.Collections.Generic;

namespace Lab7
{
    public class EnglishLexer
    {
        private List<Token> tokens;

        private readonly HashSet<string> nouns = new HashSet<string> { "flight", "passenger", "trip", "morning" };
        private readonly HashSet<string> verbs = new HashSet<string> { "is", "prefers", "like", "need", "depend", "fly" };
        private readonly HashSet<string> adjectives = new HashSet<string> { "cheapest", "non-stop", "first", "latest", "other", "direct" };
        private readonly HashSet<string> pronouns = new HashSet<string> { "me", "i", "you", "it" };
        private readonly HashSet<string> properNouns = new HashSet<string> { "Alaska", "Baltimore", "Los Angeles", "Chicago" };
        private readonly HashSet<string> determiners = new HashSet<string> { "the", "a", "an", "this", "these", "that" };
        private readonly HashSet<string> prepositions = new HashSet<string> { "from", "to", "on", "near" };

        public List<Token> Analyze(string input)
        {
            tokens = new List<Token>();
            string[] words = input.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < words.Length; i++)
            {
                string word = words[i].ToLower();
                TokenType type = TokenType.Error;

                if (nouns.Contains(word)) type = TokenType.Noun;
                else if (verbs.Contains(word)) type = TokenType.Verb;
                else if (adjectives.Contains(word)) type = TokenType.Adjective;
                else if (pronouns.Contains(word)) type = TokenType.Pronoun;
                else if (properNouns.Contains(word)) type = TokenType.ProperNoun;
                else if (determiners.Contains(word)) type = TokenType.Determiner;
                else if (prepositions.Contains(word)) type = TokenType.Preposition;
                else if (word == "." || word == "?" || word == "!") type = TokenType.EndOfSentence;

                tokens.Add(new Token(type, word, i, i));
            }

            return tokens;
        }

        public EnglishLexer()
        {
            tokens = new List<Token>();
        }
    }
}