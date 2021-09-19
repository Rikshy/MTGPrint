namespace MTGPrint.Models
{
    public enum CardComponent
    {
        Token,
        MeldPart,
        MeldResult,
        ComboPiece
    }

    public enum CardLayout
    {
        Normal,
        Split,
        Flip,
        Transform,
        Meld,
        Leveler,
        Saga,
        Adventure,
        Planar,
        Scheme,
        Vanguard,
        Token,
        DoubleFacedToken,
        Emblem,
        Augment,
        Host,
        ArtSeries,
        DoubleSided,
        ModalDualface,
        Class
    }

    public enum CardBorder
    {
        With,
        Without
    }

    public enum BulkType
    {
        OracleCards,
        Rulings,
        AllCards,
        ArtWorks,
        DefaultCards,
        UniqueArtwork
    }
}
