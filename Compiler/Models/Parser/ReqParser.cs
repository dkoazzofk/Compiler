using System.Collections.Generic;
using System.Linq;
using Compiler;

namespace Compiler
{
    public class ReqParser : IParser
    {
        public List<ParserError> Errors { get; set; } = new();

        public List<ParserError> Parse(List<Lexeme> tokensList)
        {
            Errors = new();
            int i = 0;

            while (i < tokensList.Count)
            {
                var currentErrors = new List<ParserError>();

                // DECLARE
                if (!Match(tokensList, ref i, LexemeType.DECLARE))
                {
                    currentErrors.Add(CreateError("Ошибка: отсутствует ключевое слово DECLARE", tokensList, i));
                    SkipUntilNext(tokensList, ref i, LexemeType.Identifier); // Пытаемся двигаться к следующему ожидаемому элементу
                }

                // Identifier
                if (!Match(tokensList, ref i, LexemeType.Identifier))
                {
                    currentErrors.Add(CreateError("Ошибка: отсутствует имя константы", tokensList, i));
                    SkipUntilNext(tokensList, ref i, LexemeType.CONSTANT);
                }

                // CONSTANT
                if (!Match(tokensList, ref i, LexemeType.CONSTANT))
                {
                    currentErrors.Add(CreateError("Ошибка: отсутствует ключевое слово CONSTANT", tokensList, i));
                    SkipUntilNext(tokensList, ref i, LexemeType.INTEGER);
                }

                // INTEGER
                if (!Match(tokensList, ref i, LexemeType.INTEGER))
                {
                    currentErrors.Add(CreateError("Ошибка: отсутствует ключевое слово INTEGER", tokensList, i));
                    SkipUntilNext(tokensList, ref i, LexemeType.AssignmentOperator);
                }

                // := или =
                if (tokensList.ElementAtOrDefault(i)?.Type == LexemeType.AssignmentOperator)
                {
                    i++;
                }
                else
                {
                    // Пропускаем, чтобы продолжить к следующему числу
                    SkipUntilNext(tokensList, ref i, LexemeType.Sign, LexemeType.UnsignedInteger, LexemeType.Number);
                }

                // optional +/-
                if (tokensList.ElementAtOrDefault(i)?.Type == LexemeType.Sign)
                {
                    i++;
                }

                // Число
                if (!Match(tokensList, ref i, LexemeType.UnsignedInteger) && !Match(tokensList, ref i, LexemeType.Number))
                {
                    currentErrors.Add(CreateError("Ошибка: отсутствует число", tokensList, i));
                    SkipUntilNext(tokensList, ref i, LexemeType.Semicolon);
                }

                // ;
                if (!Match(tokensList, ref i, LexemeType.Semicolon))
                {
                    currentErrors.Add(CreateError("Ошибка: отсутствует завершающий оператор ;", tokensList, i));
                    // Продвигаемся дальше до следующего DECLARE или конца
                    SkipUntilNext(tokensList, ref i, LexemeType.DECLARE);
                }

                Errors.AddRange(currentErrors);

                // Если вдруг мы где-то застряли, просто двигаемся дальше
                if (i < tokensList.Count && tokensList[i].Type != LexemeType.DECLARE)
                {
                    i++;
                }
            }

            return Errors;
        }

        private bool Match(List<Lexeme> tokens, ref int index, LexemeType expectedType)
        {
            while (index < tokens.Count &&
                   (tokens[index].Type == LexemeType.Whitespace || tokens[index].Type == LexemeType.NewLine))
            {
                index++;
            }

            if (index < tokens.Count && tokens[index].Type == expectedType)
            {
                index++;
                return true;
            }

            return false;
        }

        private void SkipUntilNext(List<Lexeme> tokens, ref int index, params LexemeType[] expectedTypes)
        {
            while (index < tokens.Count && !expectedTypes.Contains(tokens[index].Type))
            {
                index++;
            }
        }

        private ParserError CreateError(string message, List<Lexeme> tokens, int index)
        {
            if (index >= tokens.Count && tokens.Count > 0)
            {
                var last = tokens.Last();
                return new ParserError(message, last.StartIndex, last.EndIndex);
            }

            var token = tokens.ElementAtOrDefault(index);
            return new ParserError(message, token?.StartIndex ?? 0, token?.EndIndex ?? 0);
        }
    }
}
