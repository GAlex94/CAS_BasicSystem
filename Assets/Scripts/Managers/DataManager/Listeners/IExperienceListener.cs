
namespace Casino
{
    public interface IExperienceListener
    {
        void OnExperienceChange(int newExp, int oldExp);
        void OnLevelUp(int newLevel);
    }
}