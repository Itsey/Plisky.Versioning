namespace Plisky.CodeCraft;

public interface IHookVersioningChanges {

    void PostUpdateAllAction(string rootPath);

    void PostUpdateFileAction(string fl);

    void PreUpdateAllAction(string rootPath);

    void PreUpdateFileAction(string fl);
}