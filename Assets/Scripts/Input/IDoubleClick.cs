using System.Xml.Serialization;

namespace Common.Input
{
    public interface IDoubleClick
    {
        void OnDoubleClick();
        bool CanDoubleClick();
    }
}