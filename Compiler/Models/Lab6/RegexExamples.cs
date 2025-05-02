using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Lab6;

public class RegexExamples
{
    public static List<Match> ValidatePhoneNumber(string input)
    {
        string pattern = @"(?:\b\d{3}[-\s]?\d{2}[-\s]?\d{2}\b)";
        Regex regex = new Regex(pattern);
        return GetMatchesWithPositions(input, regex);
    }

    public static List<Match> ValidateFullName(string input)
    {
        string pattern = @"\b[А-ЯЁ][а-яё]*\s+[А-ЯЁ][а-яё]*\s+[А-ЯЁ][а-яё]*\b";
        Regex regex = new Regex(pattern);
        return GetMatchesWithPositions(input, regex);
    }

    public static List<Match> ValidateLatitude(string input)
    {
        string pattern = @"(?<!\d)[-]?(?:(?:0*[0-8]\d?)|(?:90))(?:\.\d+)?(?!\d)";
        Regex regex = new Regex(pattern);
        return GetMatchesWithPositions(input, regex);
    }

    private static List<Match> GetMatchesWithPositions(string input, Regex regex)
    {
        List<Match> matches = [.. regex.Matches(input).Cast<Match>()];
        return matches;
    }
}
