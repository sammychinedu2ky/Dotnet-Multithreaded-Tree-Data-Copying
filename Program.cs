using System.Text.Json;
var mySource = new Source("A")
{
    Data = new Dictionary<string, IList<string>>(new List<KeyValuePair<string, IList<string>>>()
{
    new KeyValuePair<string, IList<string>>("A", new List<string>(){"B", "C"}),
    new KeyValuePair<string, IList<string>>("B", new List<string>(){"E"}),
    new KeyValuePair<string, IList<string>>("C", new List<string>(){"D", "E"}),
    new KeyValuePair<string, IList<string>>("E", new List<string>(){"F", "G"}),
    new KeyValuePair<string, IList<string>>("F", new List<string>()),
    new KeyValuePair<string, IList<string>>("G", new List<string>(){"H", "I"}),
    new KeyValuePair<string, IList<string>>("H", new List<string>()),
    new KeyValuePair<string, IList<string>>("I", new List<string>()),
    new KeyValuePair<string, IList<string>>("D", new List<string>(){"Sam", "Cat"}),
    new KeyValuePair<string, IList<string>>("Sam", new List<string>()),
    new KeyValuePair<string, IList<string>>("Cat", new List<string>()),
})
};
var myTarget = new Target()
{
    Data = new()
};

mySource.CopyTo(myTarget);

public class Source
{
    public Dictionary<string, IList<string>>? Data { get; init; }
    public List<Task> Tasks { get; set; } = new List<Task>();
    private string Root { get; set; }

    public Source(string root)
    {
        this.Root = root;
    }
    public void CopyTo(Target target)
    {
        Prehandler(Root, this, target);
        Console.WriteLine(JsonSerializer.Serialize(target.Data));

    }

    private static void Prehandler(string node, Source source, Target target)
    {
        // check if it exists in the target
        if (!target.Data!.ContainsKey(node))
        {
            // check if the node has children of its own
            if (source.Data![node].Count > 0)
            {
                foreach (var child in source.Data![node])
                {
                    Prehandler(child, source, target);
                }
                Console.WriteLine($"Adding {node} with children {string.Join(" ", source.Data[node])} to target");
                target.Data.Add(node, source.Data![node]);
            }
            else
            {
                Console.WriteLine($"Adding {node} with no children to target");
                target.Data.Add(node, new List<string>());
            }

        }
    }
}
public class Target
{
    public Dictionary<string, IList<string>>? Data { get; set; }
    public Dictionary<string, bool> Visited { get; set; } = new Dictionary<string, bool>();
}

