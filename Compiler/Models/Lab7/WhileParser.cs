using System;
using System.Collections.Generic;

namespace Lab7
{
    public class EnglishParser
    {
        private string result = "";
        private List<Token> tokens;
        private Token currentToken;
        private int currentIndex;
        private const string separator = " → ";

        public string Parse(List<Token> tokensList)
        {
            tokens = tokensList;
            currentIndex = 0;
            currentToken = tokens[currentIndex];
            result = string.Empty;

            try
            {
                S();
                if (currentToken.Type != TokenType.EndOfSentence)
                {
                    Error("Ожидался конец предложения (., ?, !)");
                }
            }
            catch (Exception ex)
            {
                result += $"\nSyntax Error: {ex.Message}";
            }

            return result;
        }

        private void Log(string str)
        {
            result += (result == string.Empty) ? str : separator + str;
        }

        private void NextToken()
        {
            if (currentIndex < tokens.Count - 1)
            {
                currentIndex++;
                currentToken = tokens[currentIndex];
            }
            else
            {
                throw new Exception("Неожиданный конец входных данных");
            }
        }

        private void Error(string message)
        {
            throw new Exception($"{message} (на позиции {currentToken.StartIndex}, токен: '{currentToken.Value}')");
        }

        private void S()
        {
            Log("S");
            NP();
            VP();
        }

        private void NP()
        {
            Log("NP");
            switch (currentToken.Type)
            {
                case TokenType.Pronoun:
                    Log("Pro");
                    NextToken();
                    break;
                case TokenType.ProperNoun:
                    Log("PropN");
                    NextToken();
                    break;
                case TokenType.Determiner:
                    Log("Det");
                    NextToken();

                    if (currentToken.Type == TokenType.Adjective)
                    {
                        A();
                        Nom();
                    }
                    else
                    {
                        Nom();
                    }
                    break;
                default:
                    Error("Ожидалось NP (Pronoun, ProperNoun или Det)");
                    break;
            }
        }

        private void Nom()
        {
            Log("Nom");

            if (currentToken.Type == TokenType.Noun)
            {
                N();

                if (currentToken.Type == TokenType.Preposition)
                {
                    PP();
                }
            }
            else if (currentToken.Type == TokenType.Noun)
            {
                N();
                Nom();
            }
            else
            {
                N();
            }
        }

        private void VP()
        {
            Log("VP");

            if (currentToken.Type == TokenType.Verb)
            {
                V();

                if (currentToken.Type == TokenType.Pronoun ||
                    currentToken.Type == TokenType.ProperNoun ||
                    currentToken.Type == TokenType.Determiner)
                {
                    NP();

                    if (currentToken.Type == TokenType.Preposition)
                    {
                        PP();
                    }
                }
                else if (currentToken.Type == TokenType.Preposition)
                {
                    PP();
                }
            }
            else
            {
                Error("Ожидался глагол (VP)");
            }
        }

        private void PP()
        {
            Log("PP");
            if (currentToken.Type == TokenType.Preposition)
            {
                P();
                NP();
            }
            else
            {
                Error("Ожидался предлог (PP)");
            }
        }

        private void N()
        {
            if (currentToken.Type == TokenType.Noun)
            {
                Log("N");
                NextToken();
            }
            else
            {
                Error("Ожидалось существительное (N)");
            }
        }

        private void V()
        {
            if (currentToken.Type == TokenType.Verb)
            {
                Log("V");
                NextToken();
            }
            else
            {
                Error("Ожидался глагол (V)");
            }
        }

        private void A()
        {
            if (currentToken.Type == TokenType.Adjective)
            {
                Log("A");
                NextToken();
            }
            else
            {
                Error("Ожидалось прилагательное (A)");
            }
        }

        private void P()
        {
            if (currentToken.Type == TokenType.Preposition)
            {
                Log("P");
                NextToken();
            }
            else
            {
                Error("Ожидался предлог (P)");
            }
        }
    }
}