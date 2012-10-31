public interface IGameObject
{
    WorldGameObject world { get; set; }
    void CheckIfMovedOutsideTheWorld();
}