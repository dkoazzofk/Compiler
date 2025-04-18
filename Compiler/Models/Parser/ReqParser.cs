using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Compiler;

public class ReqParser : IParser
{
    private List<Lexeme> tokens;
    private Lexeme CurrToken;
    private int CurrIndex;
    private int MaxIndex;

    public List<ParserError> Errors { get; set; }

    public ReqParser()
    {
        Errors = new List<ParserError>();
    }

    public List<ParserError> Parse(List<Lexeme> tokensList)
    {
        Errors.Clear();
        if (tokensList.Count <= 0)
            return Errors;

        PreprocessTokens(tokensList);

        tokens = tokensList;
        CurrIndex = 0;
        MaxIndex = tokensList.Count - 1;
        CurrToken = tokens[CurrIndex];

        try
        {
            DECLARE(false);
        }
        catch (SyntaxErrorException)
        {
            if (CurrToken.Type != LexemeType.Semicolon && !CurrToken.IsCorrupted)
                Errors.Add(new ParserError($"Выражение незакончено", CurrToken.StartIndex, tokens[MaxIndex].EndIndex, ErrorType.UnfinishedExpression));
        }

        return Errors;
    }

    private void PreprocessTokens(List<Lexeme> lexemes)
    {
        for (int i = lexemes.Count - 1; i >= 0; i--)
        {
            var lexeme = lexemes[i];
            if (lexeme.Type == LexemeType.InvalidCharacter)
            {
                Errors.Add(new ParserError($"Недопустимый символ \"{lexeme.Value}\"", lexeme.StartIndex, lexeme.EndIndex, ErrorType.UnfinishedExpression));
                lexemes.RemoveAt(i);

                if (i > 0)
                    lexemes[i - 1].IsCorrupted = true;
                if (i < lexemes.Count)
                    lexemes[i].IsCorrupted = true;
            }
        }

        for (int i = 0; i < lexemes.Count; i++)
        {
            var lexeme = lexemes[i];
            if (lexeme.IsCorrupted && IsKeywordType(lexeme.Type))
            {
                int start = i;
                int end = i;
                int startIndex = lexeme.StartIndex;
                int endIndex = lexeme.EndIndex;
                string combinedValue = lexeme.Value;

                while (end + 1 < lexemes.Count &&
                       lexemes[end + 1].IsCorrupted &&
                       lexemes[end + 1].Type == lexeme.Type)
                {
                    end++;
                    endIndex = lexemes[end].EndIndex;
                    combinedValue += lexemes[end].Value;
                }

                if (start != end)
                {
                    var combinedLexeme = new Lexeme(lexeme.Type, combinedValue, startIndex, endIndex)
                    {
                        IsCorrupted = true
                    };

                    lexemes.RemoveRange(start, end - start + 1);
                    lexemes.Insert(start, combinedLexeme);
                    i = start;
                }
            }
        }
    }

    private bool IsKeywordType(LexemeType type)
    {
        return type == LexemeType.DECLARE ||
               type == LexemeType.CONSTANT ||
               type == LexemeType.INTEGER;
    }

    private void ChangeCurrentToken()
    {
        if (CanGetNext())
        {
            CurrIndex++;
            CurrToken = tokens[CurrIndex];
        }
        else
        {
            throw new SyntaxErrorException();
        }
    }

    private LexemeType GetNextType()
    {
        return CanGetNext() ? tokens[CurrIndex + 1].Type : LexemeType.Error;
    }

    private bool CanGetNext() => CurrIndex < MaxIndex;

    private void DECLARE(bool get, bool neutralize = false)
    {
        if (get) ChangeCurrentToken();
        if (CurrToken.IsCorrupted)
        {
            HandleCorruptedToken(LexemeType.DECLARE, () => IDENTIFIER(true), () => DECLARE(true));
            return;
        }

        if (CurrToken.Type == LexemeType.DECLARE)
        {
            IDENTIFIER(true);
        }
        else
        {
            if (CurrToken.Type == LexemeType.Identifier && GetNextType() == LexemeType.CONSTANT)
            {
                Errors.Add(new ParserError($"Пропущено ключевое слово DECLARE", CurrToken.StartIndex, CurrToken.EndIndex, ErrorType.UnfinishedExpression));
                IDENTIFIER(false);
            }
            else
            {
                Errors.Add(new ParserError($"Ожидалось ключевое слово DECLARE, а встречено \"{CurrToken.Value}\"", CurrToken.StartIndex, CurrToken.EndIndex, ErrorType.UnfinishedExpression));
                SkipToNextValidToken(LexemeType.DECLARE, () => IDENTIFIER(true));
            }
        }
    }

    private void IDENTIFIER(bool get, bool neutralize = false)
    {
        if (get) ChangeCurrentToken();
        if (CurrToken.IsCorrupted)
        {
            HandleCorruptedToken(LexemeType.Identifier, () => CONST(true), () => IDENTIFIER(true));
            return;
        }

        if (CurrToken.Type == LexemeType.Identifier)
        {
            CONST(true);
        }
        else
        {
            if (CurrToken.Type == LexemeType.CONSTANT)
            {
                Errors.Add(new ParserError($"Пропущен идентификатор", CurrToken.StartIndex, CurrToken.EndIndex, ErrorType.UnfinishedExpression));
                CONST(false);
            }
            else
            {
                Errors.Add(new ParserError($"Ожидался идентификатор, а встречено \"{CurrToken.Value}\"", CurrToken.StartIndex, CurrToken.EndIndex, ErrorType.UnfinishedExpression));
                SkipToNextValidToken(LexemeType.Identifier, () => CONST(true));
            }
        }
    }

    private void CONST(bool get, bool neutralize = false)
    {
        if (get) ChangeCurrentToken();
        if (CurrToken.IsCorrupted)
        {
            HandleCorruptedToken(LexemeType.CONSTANT, () => INT(true), () => CONST(true));
            return;
        }

        if (CurrToken.Type == LexemeType.CONSTANT)
        {
            INT(true);
        }
        else
        {
            if (CurrToken.Type == LexemeType.INTEGER)
            {
                Errors.Add(new ParserError($"Пропущено ключевое слово CONSTANT", CurrToken.StartIndex, CurrToken.EndIndex, ErrorType.UnfinishedExpression));
                INT(false);
            }
            else
            {
                Errors.Add(new ParserError($"Ожидалось ключевое слово CONSTANT, а встречено \"{CurrToken.Value}\"", CurrToken.StartIndex, CurrToken.EndIndex, ErrorType.UnfinishedExpression));
                SkipToNextValidToken(LexemeType.CONSTANT, () => INT(true));
            }
        }
    }

    private void INT(bool get, bool neutralize = false)
    {
        if (get) ChangeCurrentToken();
        if (CurrToken.IsCorrupted)
        {
            HandleCorruptedToken(LexemeType.INTEGER, () => ASSIGN(true), () => INT(true));
            return;
        }

        if (CurrToken.Type == LexemeType.INTEGER)
        {
            ASSIGN(true);
        }
        else
        {
            if (CurrToken.Type == LexemeType.AssignmentOperator)
            {
                Errors.Add(new ParserError($"Пропущено ключевое слово INTEGER", CurrToken.StartIndex, CurrToken.EndIndex, ErrorType.UnfinishedExpression));
                ASSIGN(false);
            }
            else
            {
                Errors.Add(new ParserError($"Ожидалось ключевое слово INTEGER, а встречено \"{CurrToken.Value}\"", CurrToken.StartIndex, CurrToken.EndIndex, ErrorType.UnfinishedExpression));
                SkipToNextValidToken(LexemeType.INTEGER, () => ASSIGN(true));
            }
        }
    }

    private void ASSIGN(bool get, bool neutralize = false)
    {
        if (get) ChangeCurrentToken();
        if (CurrToken.IsCorrupted)
        {
            HandleCorruptedToken(LexemeType.AssignmentOperator, () => NUMBER(true), () => ASSIGN(true));
            return;
        }

        if (CurrToken.Type == LexemeType.AssignmentOperator)
        {
            NUMBER(true);
        }
        else
        {
            Errors.Add(new ParserError($"Пропущен оператор присваивания", CurrToken.StartIndex, CurrToken.EndIndex, ErrorType.UnfinishedExpression));

            if (CurrToken.Type == LexemeType.Sign || CurrToken.Type == LexemeType.UnsignedInteger)
            {
                NUMBER(false, true);
            }
            else if (CurrToken.Type == LexemeType.Semicolon)
            {
                Errors.Add(new ParserError($"Пропущено число", CurrToken.StartIndex, CurrToken.EndIndex, ErrorType.UnfinishedExpression));
                NUMBER(false, true);
            }
            else
            {
                Errors.Add(new ParserError($"Ожидался оператор присваивания, а встречено \"{CurrToken.Value}\"", CurrToken.StartIndex, CurrToken.EndIndex, ErrorType.UnfinishedExpression));
                NUMBER(true, true);
            }
        }
    }

    private void NUMBER(bool get, bool neutralize = false)
    {
        if (get) ChangeCurrentToken();
        if (CurrToken.IsCorrupted)
        {
            HandleCorruptedToken(LexemeType.UnsignedInteger, () => END(true), () => NUMBER(true));
            return;
        }

        if (neutralize)
        {
            UNSIGNEDINT(false, true);
            return;
        }

        if (CurrToken.Type == LexemeType.Sign || CurrToken.Type == LexemeType.UnsignedInteger)
        {
            UNSIGNEDINT(CurrToken.Type == LexemeType.Sign);
        }
        else
        {
            if (CurrToken.Type == LexemeType.Semicolon)
            {
                Errors.Add(new ParserError($"Пропущен знак или число", CurrToken.StartIndex, CurrToken.EndIndex, ErrorType.UnfinishedExpression));
                UNSIGNEDINT(false, true);
            }
            else
            {
                Errors.Add(new ParserError($"Ожидался знак или число, а встречено \"{CurrToken.Value}\"", CurrToken.StartIndex, CurrToken.EndIndex, ErrorType.UnfinishedExpression));
                SkipToNextValidToken(new[] { LexemeType.Sign, LexemeType.UnsignedInteger }, () => UNSIGNEDINT(true, true));
            }
        }
    }

    private void UNSIGNEDINT(bool get, bool neutralize = false)
    {
        if (get) ChangeCurrentToken();
        if (CurrToken.IsCorrupted)
        {
            HandleCorruptedToken(LexemeType.UnsignedInteger, () => END(true), () => UNSIGNEDINT(true));
            return;
        }

        if (neutralize)
        {
            END(false);
            return;
        }

        if (CurrToken.Type == LexemeType.UnsignedInteger)
        {
            END(true);
        }
        else
        {
            if (CurrToken.Type == LexemeType.Semicolon)
            {
                Errors.Add(new ParserError($"Пропущено число", CurrToken.StartIndex, CurrToken.EndIndex, ErrorType.UnfinishedExpression));
                END(false);
            }
            else
            {
                Errors.Add(new ParserError($"Ожидалось число, а встречено \"{CurrToken.Value}\"", CurrToken.StartIndex, CurrToken.EndIndex, ErrorType.UnfinishedExpression));
                SkipToNextValidToken(LexemeType.UnsignedInteger, () => END(true));
            }
        }
    }

    private void END(bool get, bool neutralize = false)
    {
        if (get) ChangeCurrentToken();
        if (CurrToken.IsCorrupted)
        {
            HandleCorruptedToken(LexemeType.Semicolon, () => DECLARE(true), () => END(true));
            return;
        }

        if (CurrToken.Type == LexemeType.Semicolon)
        {
            DECLARE(true);
        }
        else
        {
            if (CurrToken.Type == LexemeType.DECLARE || GetNextType() == LexemeType.Error)
            {
                Errors.Add(new ParserError($"Пропущен оператор конца выражения", CurrToken.StartIndex, CurrToken.EndIndex, ErrorType.UnfinishedExpression));
                if (GetNextType() != LexemeType.Error)
                    DECLARE(false);
            }
            else
            {
                Errors.Add(new ParserError($"Ожидался оператор конца выражения, а встречено \"{CurrToken.Value}\"", CurrToken.StartIndex, CurrToken.EndIndex, ErrorType.UnfinishedExpression));
                SkipToNextValidToken(LexemeType.Semicolon, () => DECLARE(true));
            }
        }
    }

    private void HandleCorruptedToken(LexemeType expectedType, Action successAction, Action failureAction)
    {
        Errors.Add(new ParserError($"Поврежденное ключевое слово \"{CurrToken.Value}\" (ожидалось {GetTypeName(expectedType)})",
            CurrToken.StartIndex, CurrToken.EndIndex, ErrorType.UnfinishedExpression));

        if (CanGetNext())
        {
            ChangeCurrentToken();
            successAction();
        }
        else
        {
            failureAction();
        }
    }

    private void SkipToNextValidToken(LexemeType expectedType, Action action)
    {
        while (CanGetNext() && CurrToken.Type != expectedType && !CurrToken.IsCorrupted)
        {
            ChangeCurrentToken();
        }

        if (CurrToken.Type == expectedType || CurrToken.IsCorrupted)
        {
            action();
        }
    }

    private void SkipToNextValidToken(LexemeType[] expectedTypes, Action action)
    {
        while (CanGetNext() && !expectedTypes.Contains(CurrToken.Type) && !CurrToken.IsCorrupted)
        {
            ChangeCurrentToken();
        }

        if (expectedTypes.Contains(CurrToken.Type) || CurrToken.IsCorrupted)
        {
            action();
        }
    }

    private string GetTypeName(LexemeType type)
    {
        return type switch
        {
            LexemeType.DECLARE => "DECLARE",
            LexemeType.CONSTANT => "CONSTANT",
            LexemeType.INTEGER => "INTEGER",
            LexemeType.Identifier => "идентификатор",
            LexemeType.AssignmentOperator => "оператор присваивания",
            LexemeType.UnsignedInteger => "число",
            LexemeType.Sign => "знак",
            LexemeType.Semicolon => "конец выражения",
            _ => type.ToString()
        };
    }
}
