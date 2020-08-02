using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Javelin.Tokenizers {
    /// <summary>
    /// A simple Tokenizer, barely makes ends meet 
    /// </summary>
    public class EnglishTokenizer : ITokenizer {
        
        /// <summary>
        /// Location of English stopwords file on disk
        /// </summary>
        private const string StopWordsPath = "stopwords.txt";

        /// <summary>
        /// Set of English stopwords
        /// </summary>
        private HashSet<string> _stopWords;
        
        /// <summary>
        /// Token delimeters
        /// </summary>
        private readonly char[] _delimiters = 
            { ' ', ',', ':', '\t', '\n' };
        
        /// <summary>
        /// Substrings to remove from tokens
        /// </summary>
        private readonly string[] _removeStrings = 
            { "@", ",", ".", ";", "\'", "!", "?", "^", "*", ")", "(", "-" };

        public EnglishTokenizer() {
            LoadStopWords();
        }

        /// <summary>
        /// Given a string of text, returns a tokenized array of strings
        /// Crude, moderately effective
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public string[] Tokenize(string text) {
            var rawTokens = text.Split(_delimiters);
            var tokens = rawTokens
                .SelectMany(
                    token => _removeStrings,
                    (token, c) => token.Replace(c, string.Empty));

            // Probably Inefficient
            return tokens.Except(_stopWords).ToArray();
        }
        
        private void LoadStopWords() {
            var stops = File.ReadAllLines(StopWordsPath).ToList();
            _stopWords = new HashSet<string>(stops);
        }
    }
}