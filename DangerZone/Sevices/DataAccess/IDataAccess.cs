namespace DangerZone.Sevices.DataAccess
{
    public interface IDataAccess<T>
    {
        T GetData(string id);

        void SetData(string id, T Data);
    }
}
