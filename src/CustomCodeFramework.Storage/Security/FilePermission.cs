namespace CustomCodeFramework.Storage.Security;

[Flags]
public enum FilePermission
{
    None = 0,
    Read = 1,
    Write = 2,
    Delete = 4,
    List = 8,
    FullControl = Read | Write | Delete | List,
}
