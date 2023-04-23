
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

class Source
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

        Task.WhenAny(tasks).Wait();
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
        {
            Console.WriteLine($"Key {key} not found in target");
            Console.WriteLine($"The children of {key} are {string.Join(",", source.Data![key])}");
            {
                 source.Visited.TryAdd(key, false);
                if (source.Data![key].Count == 0)
                {
                    Console.WriteLine($"Copying {key} to target in prehandler");
                    tasks.Add(Task.Run(() => Posthandler(key, source.Data![key], source, target)));
                    eventWaitHandle.Set();
                }
                else
                {
                    foreach (var child in source.Data![key])
                    {

                        source.Visited.TryAdd(child, false);
                        tasks.Add(Task.Run(() => Posthandler(key, source.Data![key], source, target)));
                        eventWaitHandle.Set();
                        Prehandler(child, source, target);


                    }
                }


            }


        }
    }
    public void Posthandler(string node, IList<string> children, Source source, Target target)
    {
        while (eventWaitHandle.WaitOne())
        {
        Console.WriteLine($"post handler for {node} with children {String.Join(",", children)}");
           // Console.WriteLine($"Waiting for node {node} with children {String.Join(",", children)}");
            if (!target.Data!.ContainsKey(node) && source.Visited[node] == false)
            {
                
                if(children.Count == 0)
                {
                    target.Data.TryAdd(node, new List<string>(){});
                }
                else
                {
                    target.Data.TryAdd(node, children);
                }
               
                source.Visited.TryAdd(node,true);
                //  Task.Delay(50).Wait();
                //target.Data![node] = children;


                
            }

            Console.WriteLine(JsonSerializer.Serialize(target.Data));
            Console.WriteLine(JsonSerializer.Serialize(source.Visited));
            if (source.Data!.Keys.All(key => target.Data!.ContainsKey(key)))
            {
                Console.WriteLine($"All nodes have been visited from task ${Task.CurrentId}");
                break;
            }
            eventWaitHandle.Reset();
           // eventWaitHandle.Set();
        }
    }
}

class Target
{
    public ConcurrentDictionary<string, IList<string>>? Data { get; set; }
    public string root { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public ConcurrentDictionary<string, bool> Visited { get; set; } = new ConcurrentDictionary<string, bool>();
}

