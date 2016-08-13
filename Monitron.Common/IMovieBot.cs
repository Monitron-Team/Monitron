namespace Monitron.Common
{
    public interface IMovieBot
    {
        //[RemoteCommand(MethodName = "pause_movie")]
        string PauseMovie(Identity i_Buddy);
    }
}
