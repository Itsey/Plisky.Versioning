namespace Plisky.CodeCraft {

    public interface IHookVersioningChanges {

        object PreUpdateAllAction();

        void PreUpdateAllAction(string rootPath);

        void PostUpdateAllAction(string rootPath);

        void PreUpdateFileAction(string fl);

        void PostUpdateFileAction(string fl);
    }
}