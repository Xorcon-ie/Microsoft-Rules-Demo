namespace Core;

// ----------------------------------------------------------------------------------------------------
// The IRulesRecord interface is used by the core engine to add context to returned results
public interface IRulesRecord {
    public string Id {get;}
    public string Descriptor {get;}
}