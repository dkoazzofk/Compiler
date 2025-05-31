using System;

namespace Lab7
{
    public enum TokenType
    {
        Noun,           // flight, passenger, trip, morning
        Verb,           // is, prefers, like, need, depend, fly
        Adjective,      // cheapest, non-stop, first, latest, other, direct
        Pronoun,        // me, I, you, it
        ProperNoun,     // Alaska, Baltimore, Los Angeles, Chicago
        Determiner,     // the, a, an, this, these, that
        Preposition,    // from, to, on, near
        EndOfSentence,  // . ? !
        Error
    }

    public class Token
    {
        public TokenType Type { get; set; }
        public string Value { get; set; }
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }

        public Token(TokenType type, string value, int startIndex, int endIndex)
        {
            Type = type;
            Value = value;
            StartIndex = startIndex;
            EndIndex = endIndex;
        }
    }
}