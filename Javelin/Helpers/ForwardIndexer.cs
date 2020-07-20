using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace Javelin.Helpers {
    public class ForwardIndexer {
        
        private readonly Dictionary<long, string> _forwardIndex = new Dictionary<long, string>();

        /// <summary>
        /// Creates an in-memory forward index of text data from a zip archive
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public void BuildInMemoryForwardIndex(string filePath) {
            using var file = File.OpenRead(filePath);
            using var zip = new ZipArchive(file, ZipArchiveMode.Read);
                
            for (var docId = 1; docId < zip.Entries.Count; docId++) {
                using var stream = zip.Entries[docId].Open();
                AppendForwardIndex(stream, docId);
            }
        }

        public List<long> GetDocumentsContainingTerm(string term) {
            var result = new List<long>();
            foreach (var document in _forwardIndex) {
                if (document.Value.Contains(term, StringComparison.InvariantCulture)) {
                    result.Add(document.Key);
                }
            }
            return result;
        }
        
        
        /// <summary>
        /// Serializes the stream of text data into the forward index
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="docId"></param>
        private void AppendForwardIndex(Stream stream, long docId) {
            using var reader = new StreamReader(stream);
            var documentText = reader.ReadToEnd();
            try {
                _forwardIndex[docId] = documentText.ToLowerInvariant();
            } catch (Exception e) {
                Console.WriteLine("Error building forward index");
                Console.WriteLine(e);
            }
        }
    }
}