namespace Flowgrate;

public class IndexBuilder
{
    private readonly IndexDefinition _index;

    internal IndexBuilder(IndexDefinition index)
    {
        _index = index;
    }

    public IndexBuilder Name(string name)
    {
        _index.Name = name;
        return this;
    }
}
