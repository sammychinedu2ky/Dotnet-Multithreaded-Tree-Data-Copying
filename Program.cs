
using System.Collections.Concurrent;
using System.Text.Json;

var mySource = new Source("A")
{

    Data = new ConcurrentDictionary<string, IList<string>>(new List<KeyValuePair<string, IList<string>>>()
{
    new KeyValuePair<string, IList<string>>("A", new List<string>(){"B", "C"}),
    new KeyValuePair<string, IList<string>>("B", new List<string>()),
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
    Data = new ConcurrentDictionary<string, IList<string>>()
    {

    }
};

mySource.CopyTo(myTarget);
interface ITree
{
    public IDictionary<string, IList<string>>? Data { get; set; }
    public string root { get; set; }
    public IDictionary<string, bool> Visited { get; set; }
}

public class Source
{
    public ConcurrentDictionary<string, IList<string>>? Data { get; set; }
    public EventWaitHandle eventWaitHandle { get; set; }
    public List<Task> tasks { get; set; } = new List<Task>();
    public string root { get; set; }
    public ConcurrentDictionary<string, bool> Visited { get; set; } = new ConcurrentDictionary<string, bool>() { };

    public Source(string root)
    {
        this.root = root;
        eventWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
    }
    public void CopyTo(Target target)
    {
        Prehandler(root, this, target);

        Task.WhenAll(tasks).Wait();
        Task.Delay(5000);
        foreach (var task in tasks)
        {
            Console.WriteLine($"Waiting for {task.Status}");
            if (task.IsFaulted)
            {
                Console.WriteLine(task.Exception);
            }
        }
        Console.WriteLine(JsonSerializer.Serialize(target.Data));

    }
    public void Prehandler(string key, Source source, Target target)
    {
        // check if it exists in the target
        if (!target.Data!.ContainsKey(key))
        {
            // fetch the successors 
            tasks.Add(Task.Run(() => Posthandler(key, target)));
            eventWaitHandle.Set();
            var successors = source.Data![key];
            foreach (var successor in successors)
            {
                Prehandler(successor, source, target);
            }
        }
    }
    public void Posthandler(string node, Target target)
    {
        while (eventWaitHandle.WaitOne())
        {
            if (this.Data![node].Count > 0)
            {
                if (this.Data![node].All(x => target.Data!.ContainsKey(x)))
                {
                    target.Data!.TryAdd(node, this.Data[node]);
                    eventWaitHandle.Set();
                    return;
                }
            }
            else{
                target.Data!.TryAdd(node, this.Data[node]);
                eventWaitHandle.Set();
                return;
            }
        }
    }
}
public class Target
{
    public ConcurrentDictionary<string, IList<string>>? Data { get; set; }
    public string root { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public ConcurrentDictionary<string, bool> Visited { get; set; } = new ConcurrentDictionary<string, bool>();
}

