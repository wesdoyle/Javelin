# Inverted Index 

For small amounts of text data, we can simply perform linear scanning through a collection of documents to find specific information using regular expressions.

You'll hear this refered to as `grepping` after the UNIX command `grep`

However, linear scan is slow!  For larger amounts of data, we need to create an index to make our searches more efficient.

We use indices (indexes) to provide greater speed as well as flexibility - both in terms of the query as well as the resulitng set of matched documents.

For instance, we might want to "rank" results based on some criteria, and we might want to construct complex queries for which `grep` would not be well suited.

For this, we need to create an __index__ of our data that we can use to search.

With an __index__, one of the most basic fundamental types of models for information retrieval we can construct is a __Boolean retrieval model__.

An initial approach to satisfy the ability to perform Boolean searches might be to construct a `document-term matrix`, which is a way to (very sparsely) represent the occurence of terms in a collection of documents.

(See Notebook)