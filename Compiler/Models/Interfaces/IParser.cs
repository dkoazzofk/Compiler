
namespace Compiler
{
    public interface IParser
    {
        List<ParserError> Errors { get; set; }

        List<ParserError> Parse(List<Lexeme> tokensList);
    }
}