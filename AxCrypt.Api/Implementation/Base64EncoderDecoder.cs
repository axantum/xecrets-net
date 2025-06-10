namespace AxCrypt.Api.Implementation
{
    public static class Base64EncoderDecoder
    {
        public static string ToBase64EncodedText(this string text)
        {
            byte[] textBytes = System.Text.Encoding.UTF8.GetBytes(text);
            return System.Convert.ToBase64String(textBytes);
        }

        public static string ToBase64DecodedText(this string base64)
        {
            base64 = base64.Replace(" ", "+");
            int mod4 = base64.Length % 4;
            if (mod4 > 0)
            {
                base64 += new string('=', 4 - mod4);
            }

            byte[] base64Bytes = System.Convert.FromBase64String(base64);
            return System.Text.Encoding.UTF8.GetString(base64Bytes, 0, base64Bytes.Length);
        }
    }
}