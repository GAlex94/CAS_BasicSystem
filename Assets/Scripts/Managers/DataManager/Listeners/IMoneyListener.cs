
namespace Casino
{
    public interface IMoneyListener
    {
        void OnMoneyChange(int newMoney, int oldMoney);
    }
}