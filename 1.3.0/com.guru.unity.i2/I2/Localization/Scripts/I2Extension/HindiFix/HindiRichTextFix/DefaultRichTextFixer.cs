namespace Guru
{
    public class DefaultRichTextFix: IHindiRichTextFixer
    {
        public string BeforeFix(string text)
        {
            // 颜色标签
            text = text.Replace("<color=", "￥");
            return text;
        }

        public string Fix(string text)
        {
            // 颜色标签
            text = text.Replace("￥", "<color=");
            return text;
        }
    }
}