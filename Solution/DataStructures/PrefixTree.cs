namespace Solution.DataStructures;

public class PrefixTree
{
    public PrefixTreeNode Root { get; set; }


    public PrefixTree()
    {
        Root = new PrefixTreeNode();
    }


    public void Insert(string word, PrefixTreeNode? node = null)
    {
        if (node is null) node = Root;

        foreach (var item in node.Nodes)
        {
            if (item.Letter == word[0])
            {
                Insert(word.Substring(1), item);
                return;
            }
        }
        
        node.Nodes.Add(new PrefixTreeNode(word));
    }

    public bool Exists(ref string word, string? temp = null, PrefixTreeNode? node = null)
    {
        if (node is null) node = Root;
        if (temp is null) temp = string.Empty;

        temp += node.Letter;

        if (node.IsWord && word == temp.Trim()) return true;

        foreach (var item in node.Nodes)
            if (Exists(ref word, temp, item)) return true;

        return false;
    }

    public List<string> ToList(List<string>? words = null, string? word = null, PrefixTreeNode? node = null)
    {
        if (words is null) words = new List<string>();
        if (word is null) word = string.Empty;
        if (node is null) node = Root;

        word += node.Letter;

        if (node.IsWord) words.Add(word);
        
        foreach (var item in node.Nodes) ToList(words, word, item);

        return words;
    }


    public class PrefixTreeNode
    {
        public List<PrefixTreeNode> Nodes { get; set; }

        public char Letter { get; init; }
        public bool IsWord { get; init; }


        public PrefixTreeNode()
        {
            Nodes = new List<PrefixTreeNode>();

            Letter = ' ';
            IsWord = false;
        }

        public PrefixTreeNode(string word) : this()
        {
            Nodes = new List<PrefixTreeNode>();

            if (word.Length == 1)
            {
                Letter = char.Parse(word);
                IsWord = true;
            }
            else if (word.Length > 1)
            {
                Letter = word[0];
                Nodes.Add(new PrefixTreeNode(word.Substring(1)));
            }
        }
    }
}
