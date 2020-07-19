namespace Javelin.Tokenizers {
    public class SimpleTokenizer : ITokenizer {

        public SimpleTokenizer() {
            
        }

        public string[] Tokenize(string text) {
            return text.Split(" ");
        }
    }
}