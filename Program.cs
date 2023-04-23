
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
            // Console.WriteLine($"Waiting for {task.Status}");
            if (task.IsFaulted)
            {
                Console.WriteLine(task.Exception);
            }
        }
        Console.WriteLine(JsonSerializer.Serialize(target.Data));

    }
    public void Prehandler(string node, Source source, Target target)
    {
        // check if it exists in the target
        if (!target.Data!.ContainsKey(node))
        {
            // execute the post handler for this node
            tasks.Add(Task.Run(() => Posthandler(node, source, target)));
            // signal all threads to continue
            eventWaitHandle.Set();
            // fetch the successors 
            var successors = source.Data![node];
            foreach (var successor in successors)
            {
                // execute the pre handler for each successor
                Prehandler(successor, source, target);
            }
        }
    }
    public void Posthandler(string node, Source source, Target target)
    {
        // wait to be signaled
        while (eventWaitHandle.WaitOne())
        {
            // check if children of node is above zero(0)
            if (source.Data![node].Count > 0)
            {
                // check if all children of node are in the target
                if (source.Data![node].All(x => target.Data!.ContainsKey(x)))
                {
                    // add node with children to target
                    target.Data!.TryAdd(node, source.Data[node]);
                    // reset eventWaitHandle
                    eventWaitHandle.Set();
                    return;
                }
            }
            else
            {
                // add node with empty children to target
                target.Data!.TryAdd(node, source.Data[node]);
                // reset eventWaitHandle
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

