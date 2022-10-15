namespace Solution.Models;

public class Word
{
    public string Text { get; set; }

    public byte Count { get; set; }


    public Word(string text)
    {
        Text = text;
        Count = default;
    }


    public override bool Equals(object? obj)
    {
        if (obj is Word word)
            return Text == word.Text;

        return false;
    }
}
