namespace Guru
{
    public interface IHindiRichTextFixer
    {
        string BeforeFix(string text);
        
        string Fix(string text);
    }
}