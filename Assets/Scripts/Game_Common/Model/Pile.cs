namespace Common {
    public struct PileData
    {
        public int Index;
        public PileKind Kind;
//TODO convertir en tuple
        public PileData(PileKind kind, int index) : this()
        {
            Kind = kind;
            Index = index;
        }
    }
}