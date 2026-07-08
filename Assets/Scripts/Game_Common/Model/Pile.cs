namespace Common {
    public struct PileData
    {
        public int Index;
        public PileKind Kind;

        public PileData(PileKind kind, int index) : this()
        {
            Kind = kind;
            Index = index;
        }
    }
}